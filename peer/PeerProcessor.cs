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

        public int GetNumberOfConnections()
        {
            var response = connection.SendAndReceive("connections");

            return int.Parse(response);
        }

        public void SendFile(PeerFile file)
        {
            connection.SendFile(file);
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

                connection.Send("welcome");

                while (!command.StartsWith("exit") || !command.StartsWith("stop"))
                {
                    try
                    {
                        command = ReceiveAndProccessCommand();

                        if (command == "no-message")
                        {
                            Thread.Sleep(TIME_TO_WAIT_FOR_NEXT_MESSAGE);
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Stack: ");
                        Console.WriteLine(ex);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Stack: ");
                        Console.WriteLine(ex);
                    }
                }

                connection.Dispose();
            });

            task.Start();
            await task;
        }

        protected virtual string ReceiveAndProccessCommand()
        {
            lock (connection)
            {
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
                            break;
                        }
                    case "connections":
                        {
                            var numberOfConnections = serverInstance.GetNumberOfConnectionsWithoutProcesor(this);

                            connection.Send($"{numberOfConnections}");
                            break;
                        }
                    case "welcome":
                    case "no-message":
                        {
                            var message = GetNextMessage();

                            connection.Send(message.ToString());

                            break;
                        }
                    default:
                        throw new ArgumentException($"Invalid command {parsedCommand}");
                }

                return parsedCommand;
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