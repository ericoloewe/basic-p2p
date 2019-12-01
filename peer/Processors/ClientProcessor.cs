using peer.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peer.Processors
{
    public class ClientProcessor : Processor
    {
        private Action<string[]> OnReceiveFileList;

        public ClientProcessor(PeerConnection connection) : base(connection) { }

        public new void Send(Message peerMessage)
        {
            base.Send(peerMessage);
        }

        protected override async Task ProcessParsedCommand(Message message)
        {
            await base.ProcessParsedCommand(message);

            switch (message.Type)
            {
                case PeerCommandType.GET_KIND:
                    {
                        var kindOfConnectionMessage = new KindOfConnectionMessage(ConnectionType.CLIENT);

                        Send(kindOfConnectionMessage);
                        break;
                    }

                case PeerCommandType.LIST_FILES:
                    {
                        var listFilesMessage = (ListFilesMessage)message;

                        OnReceiveFileList(listFilesMessage.Files);
                        break;
                    }
            }
        }

        public async Task<string[]> GetFiles()
        {
            var promise = new TaskCompletionSource<string[]>();

            Send(new Message(PeerCommandType.GET_LIST));

            OnReceiveFileList = (files) => promise.SetResult(files);

            await promise.Task;

            return promise.Task.Result;
        }
    }
}