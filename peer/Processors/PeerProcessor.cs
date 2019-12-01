using peer.Domains;
using peer.Messages;
using System;
using System.Threading.Tasks;

namespace peer.Processors
{
    public class PeerProcessor : Processor
    {
        public Action<PeerFile> OnReceiveFile { protected get; set; }
        public Action<int> OnReceiveNumberOfConnections { protected get; set; }

        private readonly Peer serverInstance;

        public PeerProcessor(PeerConnection connection, Peer serverInstance) : base(connection)
        {
            this.serverInstance = serverInstance;
        }

        public async Task<int> GetNumberOfConnections()
        {
            var promise = new TaskCompletionSource<int>();

            Send(new Message(PeerCommandType.GET_CONNECTIONS));

            OnReceiveNumberOfConnections = (numberOfConnections) => promise.SetResult(numberOfConnections);

            await promise.Task;

            return promise.Task.Result;
        }

        public void SendFile(PeerFile file)
        {
            Send(new FileMessage(file));
        }

        protected override async Task ProcessParsedCommand(Message message)
        {
            await base.ProcessParsedCommand(message);

            switch (message.Type)
            {
                case PeerCommandType.CONNECTIONS:
                    {
                        var conectionsMessage = (ConnectionMessage)message;

                        OnReceiveNumberOfConnections(conectionsMessage.ConnectionsAmount);
                        break;
                    }
                case PeerCommandType.FILE:
                    {
                        var fileMessage = (FileMessage)message;
                        var file = fileMessage.File;

                        Console.WriteLine($"Receive file to save and share => {file.Name}, {file.Owner}, {file.Slice.Length}");

                        await serverInstance.SaveAndShare(file);
                        break;
                    }
                case PeerCommandType.GET_CONNECTIONS:
                    {
                        var numberOfConnections = await serverInstance.GetNumberOfConnectionsWithoutProcesor(this);

                        var connectionPeerMessage = new ConnectionMessage(numberOfConnections);

                        Send(connectionPeerMessage);
                        break;
                    }
                case PeerCommandType.GET_KIND:
                    {
                        var kindOfConnectionMessage = new KindOfConnectionMessage(ConnectionType.PEER);

                        Send(kindOfConnectionMessage);
                        break;
                    }
                case PeerCommandType.GET_LIST:
                    {
                        var files = serverInstance.GetFiles();
                        var listFilesMessage = new ListFilesMessage(files);

                        Send(listFilesMessage);
                        break;
                    }
                case PeerCommandType.UPLOAD_FILE:
                    {
                        var uploadFileMessage = (UploadFileMessage)message;

                        Console.WriteLine($"Receive file to upload => {uploadFileMessage.FileName}, {uploadFileMessage.FileBytes.Length}");

                        await serverInstance.UploadFile(uploadFileMessage.FileName, uploadFileMessage.FileBytes);
                        break;
                    }
            }
        }
    }
}