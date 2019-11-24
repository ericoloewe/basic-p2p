using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class PeerProcessor : IDisposable
    {
        public Action OnStop { protected get; set; }
        public Action<PeerFile> OnReceiveFile { protected get; set; }

        private readonly PeerConnection connection;
        private readonly Task cycle;
        private readonly Peer serverInstance;

        public PeerProcessor(PeerConnection connection, Peer serverInstance)
        {
            this.connection = connection;
            this.serverInstance = serverInstance;
            cycle = StartCycle().ContinueWith(t => HandleStop());
        }

        public int GetNumberOfConnections()
        {
            connection.Send("connections");

            return int.Parse(connection.Receive());
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

                while (!command.StartsWith("exit") || !command.StartsWith("stop"))
                {
                    try
                    {
                        command = ReceiveAndProccessCommand();
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
            }

            return parsedCommand;
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