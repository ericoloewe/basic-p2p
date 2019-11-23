using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class PeerProcessor : IDisposable
    {
        public Action<PeerProcessor> OnStop { private get; set; }
        public Action<PeerFile> OnReceive { get; internal set; }
        private PeerConnection connection;
        private Task cycle;

        public PeerProcessor(PeerConnection connection, Action<PeerProcessor> OnStop)
        {
            this.connection = connection;
            this.OnStop = OnStop;
            cycle = StartCycle().ContinueWith(t => HandleStop());
        }

        public int GetNumberOfConnections()
        {
            throw new NotImplementedException();
        }

        public void SendFile(PeerFile file)
        {
            connection.SendFile(file);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        private void HandleStop()
        {
            OnStop.Invoke(this);

            if (cycle.Status == TaskStatus.Running)
            {
                cycle.Dispose();
            }
        }

        private async Task StartCycle()
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

        private string ReceiveAndProccessCommand()
        {
            var command = connection.Receive();
            string[] commandSplit = command.Trim().Split(';');
            var parsedCommand = commandSplit[0].Trim().ToLower();

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
                default:
                    {
                        throw new ArgumentException($"Invalid command: {command}");
                    }
            }

            return parsedCommand;
        }

        private void ReceiveFile(string fileName, int startIndex, int endIndex, int length, PeerInfo info)
        {
            var fileBytes = connection.ReceiveFile(length);

            OnReceive(new PeerFile(fileName, info, startIndex, endIndex, fileBytes));

            var endMessage = connection.Receive();

            if (endMessage.Trim() != "end-file")
            {
                throw new InvalidOperationException("File was send by a wrong way");
            }

            Console.WriteLine($"Receive file => {fileName}, {startIndex}, {endIndex}, {length}");
        }
    }
}