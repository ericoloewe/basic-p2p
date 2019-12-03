using peer.Domains;
using peer.Messages;
using System;
using System.Threading.Tasks;

namespace peer.Processors
{
    public class PeerProcessor : Processor
    {
        public PeerInfo ConnectedPeerInfo { get; }

        private readonly Peer serverInstance;

        private Action<int> OnReceiveNumberOfConnections;
        private Action<PeerInfo> OnReceivePeerInfo;

        public PeerProcessor(PeerConnection connection, Peer serverInstance) : base(connection)
        {
            this.serverInstance = serverInstance;
            ConnectedPeerInfo = GetConnectedPeerInfo();
        }

        public async Task<int> GetNumberOfConnections()
        {
            var promise = new TaskCompletionSource<int>();

            Send(new Message(PeerCommandType.GET_CONNECTIONS));

            OnReceiveNumberOfConnections = (numberOfConnections) => promise.SetResult(numberOfConnections);

            await promise.Task;

            return promise.Task.Result;
        }

        public PeerInfo GetConnectedPeerInfo()
        {
            var promise = new TaskCompletionSource<PeerInfo>();

            Send(new Message(PeerCommandType.GET_INFO));

            OnReceivePeerInfo = (info) => promise.SetResult(info);

            promise.Task.Wait();

            return promise.Task.Result;
        }

        public void SendFile(PeerFileSlice file)
        {
            Send(new UploadFileSliceMessage(file));
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
                case PeerCommandType.UPLOAD_FILE_SLICE:
                    {
                        var fileMessage = (UploadFileSliceMessage)message;
                        var file = fileMessage.File;

                        await serverInstance.SaveAndShare(file, ConnectedPeerInfo.Id);
                        break;
                    }
                case PeerCommandType.GET_CONNECTIONS:
                    {
                        var numberOfConnections = await serverInstance.GetNumberOfConnectionsWithoutProcesor(ConnectedPeerInfo.Id);

                        var connectionPeerMessage = new ConnectionMessage(numberOfConnections);

                        Send(connectionPeerMessage);
                        break;
                    }
                case PeerCommandType.GET_FILE:
                    {
                        var fileMessage = (GetFileMessage)message;
                        var file = await serverInstance.GetAllSlicesOfFile(fileMessage.FileName);
                        var downloadFileMessage = new DownloadFileMessage(file);

                        Send(downloadFileMessage);
                        break;
                    }
                case PeerCommandType.GET_FILE_SLICE:
                    {
                        var fileMessage = (GetFileMessage)message;
                        var file = await serverInstance.GetSlicesOfFile(fileMessage.FileName);
                        var downloadFileMessage = new DownloadFileMessage(file);

                        Send(downloadFileMessage);
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
                case PeerCommandType.GET_INFO:
                    {
                        Send(new PeerInfoMessage(serverInstance.Info));
                        break;
                    }
                case PeerCommandType.PEER_INFO:
                    {
                        var infoMessage = (PeerInfoMessage)message;

                        OnReceivePeerInfo(infoMessage.Info);
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