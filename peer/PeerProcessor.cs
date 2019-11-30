using peer.Messages;
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

        private readonly PeerConnection connection;
        private readonly Task cycle;
        private readonly Peer serverInstance;

        public PeerProcessor(PeerConnection connection, Peer serverInstance)
        {
            this.connection = connection;
            this.serverInstance = serverInstance;
            cycle = StartCycle().ContinueWith(t => HandleStop());
        }

        public async Task<int> GetNumberOfConnections()
        {
            var promise = new TaskCompletionSource<int>();

            connection.Send(new PeerMessage(PeerCommandType.GET_CONNECTIONS));

            OnReceiveNumberOfConnections = (numberOfConnections) => promise.SetResult(numberOfConnections);

            await promise.Task;

            return promise.Task.Result;
        }

        public void SendFile(PeerFile file)
        {
            connection.Send(new FileMessage(file));
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
                PeerMessage command;

                do
                {
                    command = SafeAndSyncReceiveAndProccessCommand();
                } while (command.Type != PeerCommandType.EXIT && command.Type != PeerCommandType.STOP);
            });

            task.Start();
            await task;
        }

        protected PeerMessage SafeAndSyncReceiveAndProccessCommand()
        {
            PeerMessage command = new PeerMessage(PeerCommandType.GENERIC_ERROR); ;

            try
            {
                var task = ReceiveAndProccessCommand();

                task.Wait();

                if (task.Exception != null)
                {
                    throw task.Exception;
                }

                command = task.Result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Stack: ");
                Console.WriteLine(ex);

                if (ex.InnerExceptions.Any(e => e is SocketException))
                {
                    command = new PeerMessage(PeerCommandType.STOP);
                }
            }

            return command;
        }

        protected async Task<PeerMessage> ReceiveAndProccessCommand()
        {
            var command = connection.Receive();

            Console.WriteLine($"Receive command {command}");

            await ProcessParsedCommand(command);

            return command;
        }

        private async Task ProcessParsedCommand(PeerMessage message)
        {
            switch (message.Type)
            {
                case PeerCommandType.CONNECTIONS:
                    {
                        var conectionsMessage = (ConnectionMessage)message;

                        OnReceiveNumberOfConnections(conectionsMessage.ConnectionsAmount);
                        break;
                    }
                case PeerCommandType.EXIT:
                    {
                        HandleStop();
                        break;
                    }
                case PeerCommandType.FILE:
                    {
                        var fileMessage = (FileMessage)message;

                        //Console.WriteLine($"Receive file to upload => {uploadFileMessage.FileName}, {uploadFileMessage.FileBytes.Length}");

                        await serverInstance.SaveFileAndUploadToOther(fileMessage);
                        break;
                    }
                case PeerCommandType.GET_CONNECTIONS:
                    {
                        var numberOfConnections = await serverInstance.GetNumberOfConnectionsWithoutProcesor(this);

                        PeerMessage connectionPeerMessage = new ConnectionMessage(numberOfConnections);

                        connection.Send(connectionPeerMessage);
                        break;
                    }
                case PeerCommandType.UPLOAD_FILE:
                    {
                        var uploadFileMessage = (UploadFileMessage)message;

                        Console.WriteLine($"Receive file to upload => {uploadFileMessage.FileName}, {uploadFileMessage.FileBytes.Length}");

                        await serverInstance.UploadFile(uploadFileMessage.FileName, uploadFileMessage.FileBytes);
                        break;
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}