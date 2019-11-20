using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class PeerProcessor : IDisposable
    {
        public Action<PeerProcessor> OnStop { private get; set; }
        private PeerConnection connection;
        private Task cycle;
        public IList<PeerFile> Files { get; }

        public PeerProcessor(PeerConnection connection, Action<PeerProcessor> OnStop)
        {
            this.connection = connection;
            this.OnStop = OnStop;
            cycle = StartCycle().ContinueWith(t => HandleStop());
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
                        string fileName = commandSplit[1];
                        int startIndex = int.Parse(commandSplit[2]);
                        int endIndex = int.Parse(commandSplit[3]);
                        int length = int.Parse(commandSplit[4]);

                        ReceiveFile(fileName, startIndex, endIndex, length);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"Invalid command: {command}");
                    }
            }

            return parsedCommand;
        }

        private void ReceiveFile(string fileName, int startIndex, int endIndex, int length)
        {
            var fileBytes = connection.ReceiveFile(length);

            Files.Add(new PeerFile(fileName, this, startIndex, endIndex, fileBytes));

            var endMessage = connection.Receive();

            if (endMessage.Trim() != "finish-file")
            {
                throw new InvalidOperationException("File was send by a wrong way");
            }
        }
    }
}