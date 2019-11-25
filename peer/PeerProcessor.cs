using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace peer
{
    public class PeerProcessor : IDisposable
    {
        public Action OnStop { protected get; set; }
        public Action<PeerFile> OnReceiveFile { protected get; set; }
        public Action<int> OnReceiveNumberOfConnections { protected get; set; }

        private const int TIME_TO_WAIT_FOR_NEXT_MESSAGE = 5000;
        private readonly PeerConnection connection;
        private readonly Task cycle;
        private readonly Peer serverInstance;
        private readonly Queue<PeerMessage> messages = new Queue<PeerMessage>();

        public PeerProcessor(PeerConnection connection, Peer serverInstance)
        {
            this.connection = connection;
            this.serverInstance = serverInstance;
            cycle = StartCycle().ContinueWith(t => HandleStop());
        }

        public async Task<int> GetNumberOfConnections()
        {
            var promise = new TaskCompletionSource<int>();

            messages.Enqueue(new PeerMessage("connections"));

            OnReceiveNumberOfConnections = (numberOfConnections) => promise.SetResult(numberOfConnections);

            await promise.Task;

            return promise.Task.Result;
        }

        public void SendFile(PeerFile file)
        {
            messages.Enqueue(new PeerMessage(file));
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        protected void HandleStop()
        {
            OnStop();
        }

        protected async Task StartCycle()
        {
            var task = new Task(() =>
            {
                var command = "";

                messages.Enqueue(new PeerMessage("welcome"));

                while (!command.StartsWith("exit") || !command.StartsWith("stop"))
                {
                    command = SafeAndSyncReceiveAndProccessCommand();
                }
            });

            task.Start();
            await task;
        }

        protected string SafeAndSyncReceiveAndProccessCommand()
        {
            var command = "";

            try
            {
                Task<string> task = ReceiveAndProccessCommand();

                task.Wait();

                command = task.Result;

                if (command == "no-message")
                {
                    Thread.Sleep(TIME_TO_WAIT_FOR_NEXT_MESSAGE);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Stack: ");
                Console.WriteLine(ex);
                command = "stop";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Stack: ");
                Console.WriteLine(ex);
            }

            return command;
        }
        protected async Task<string> ReceiveAndProccessCommand()
        {
            var nextMessage = GetNextMessage();

            SendMessage(nextMessage);

            var command = connection.Receive();
            string[] commandSplit = command.Trim().Split(';');
            var parsedCommand = commandSplit[0].Trim().ToLower();

            Console.WriteLine($"Receive command {parsedCommand}");

            switch (parsedCommand)
            {
                case "exit":
                    {
                        HandleStop();
                        break;
                    }
                case "begin-file":
                    {
                        var fileName = commandSplit[1];
                        var startIndex = int.Parse(commandSplit[2]);
                        var endIndex = int.Parse(commandSplit[3]);
                        var length = int.Parse(commandSplit[4]);
                        var info = PeerInfo.FromString(commandSplit[5]);

                        ReceiveFile(fileName, startIndex, endIndex, length, info);
                        messages.Enqueue(new PeerMessage("upload-file-ok"));
                        break;
                    }
                case "connections":
                    {
                        if (commandSplit.Length > 1)
                        {
                            var numberOfConnections = commandSplit[1];

                            OnReceiveNumberOfConnections(int.Parse(numberOfConnections));
                        }
                        else
                        {
                            var numberOfConnections = await serverInstance.GetNumberOfConnectionsWithoutProcesor(this);

                            messages.Enqueue(new PeerMessage($"connections;{numberOfConnections}"));
                        }
                        break;
                    }
                case "welcome":
                case "no-message":
                    break;
                default:
                    throw new ArgumentException($"Invalid command {parsedCommand}");
            }

            return parsedCommand;
        }

        private PeerMessage GetNextMessage()
        {
            var message = new PeerMessage("no-message");

            if (messages.Count > 0)
            {
                message = messages.Dequeue();
            }

            return message;
        }

        private void SendMessage(PeerMessage message)
        {
            if (message.HasFile)
            {
                connection.SendFile(message.File);
            }
            else
            {
                connection.Send(message.ToString());
            }
        }

        protected void ReceiveFile(string fileName, int startIndex, int endIndex, int length, PeerInfo info)
        {
            var fileBytes = connection.ReceiveFile(length);

            OnReceiveFile(new PeerFile(fileName, info, startIndex, endIndex, fileBytes));

            var endMessage = connection.Receive();

            if (endMessage.Trim() != "end-file")
            {
                throw new InvalidOperationException("File was send by a wrong way");
            }

            Console.WriteLine($"Receive file => {fileName}, {startIndex}, {endIndex}, {length}");
        }
    }
}