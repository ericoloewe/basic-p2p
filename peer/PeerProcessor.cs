using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool IsWaitingForFile { get { return lastFileInfoCommandSplit != null; } }

        private const int TIME_TO_WAIT_FOR_NEXT_MESSAGE = 5000;
        private readonly PeerConnection connection;
        private readonly Task cycle;
        private readonly Peer serverInstance;
        private readonly Queue<PeerMessage> messages = new Queue<PeerMessage>();
        private string[] lastFileInfoCommandSplit;

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
            messages.Enqueue(new PeerMessage($"begin-file;{file}"));
            messages.Enqueue(new PeerMessage(file));
            messages.Enqueue(new PeerMessage("end-file"));
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

                while (!command.StartsWith("exit") && !command.StartsWith("stop"))
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

                if (task.Exception != null)
                {
                    throw task.Exception;
                }

                command = task.Result;

                if (command == "no-message")
                {
                    Thread.Sleep(TIME_TO_WAIT_FOR_NEXT_MESSAGE);
                }
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Stack: ");
                Console.WriteLine(ex);

                if (ex.InnerExceptions.Any(e => e is SocketException))
                {
                    command = "stop";
                }
            }

            return command;
        }
        protected async Task<string> ReceiveAndProccessCommand()
        {
            var nextMessage = GetNextMessage();
            string parsedCommand;

            if (IsWaitingForFile)
            {
                var fileName = lastFileInfoCommandSplit[1];
                var startIndex = int.Parse(lastFileInfoCommandSplit[2]);
                var endIndex = int.Parse(lastFileInfoCommandSplit[3]);
                var length = int.Parse(lastFileInfoCommandSplit[4]);
                var info = PeerInfo.FromString(lastFileInfoCommandSplit[5]);

                ReceiveFile(fileName, startIndex, endIndex, length, info);
                parsedCommand = "file";
            }
            else
            {
                SendMessage(nextMessage);

                var command = connection.Receive();
                string[] commandSplit = command.Trim().Split(';');
                parsedCommand = commandSplit[0].Trim().ToLower();

                Console.WriteLine($"Receive command {parsedCommand}");

                await ProcessParsedCommand(commandSplit, parsedCommand);
            }

            return parsedCommand;
        }

        private async Task ProcessParsedCommand(string[] commandSplit, string parsedCommand)
        {
            switch (parsedCommand)
            {
                case "exit":
                    {
                        HandleStop();
                        break;
                    }
                case "begin-file":
                    {
                        lastFileInfoCommandSplit = commandSplit;
                        break;
                    }
                case "end-file":
                    {
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
                case "upload-file-ok":
                    break;
                default:
                    throw new ArgumentException($"Invalid command {parsedCommand}");
            }
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
                connection.SendBytes(message.File.Slice);
            }
            else
            {
                connection.Send(message.ToString());
            }
        }

        protected void ReceiveFile(string fileName, int startIndex, int endIndex, int length, PeerInfo info)
        {
            var fileBytes = connection.ReceiveBytes(length);

            OnReceiveFile(new PeerFile(fileName, info, startIndex, endIndex, fileBytes));
            Console.WriteLine($"Receive file => {fileName}, {startIndex}, {endIndex}, {length}");
            lastFileInfoCommandSplit = null;
        }
    }
}