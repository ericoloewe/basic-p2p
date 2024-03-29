﻿using peer.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peer.Processors
{
    public class ClientProcessor : Processor
    {
        private Action<DownloadFileMessage> OnDownloadFile;

        private Action<string[]> OnReceiveFileList;

        public ClientProcessor(PeerConnection connection) : base(connection) { }

        public new void Send(Message peerMessage)
        {
            base.Send(peerMessage);
        }

        public async Task<byte[]> DownloadFile(string fileName)
        {
            var promise = new TaskCompletionSource<DownloadFileMessage>();

            Send(new GetFileMessage(fileName));

            OnDownloadFile = (files) => promise.SetResult(files);

            await promise.Task;

            return promise.Task.Result.FileBytes;
        }

        public async Task<string[]> GetFiles()
        {
            var promise = new TaskCompletionSource<string[]>();

            Send(new Message(PeerCommandType.GET_LIST));

            OnReceiveFileList = (files) => promise.SetResult(files);

            await promise.Task;

            return promise.Task.Result;
        }

        protected override async Task ProcessParsedCommand(Message message)
        {
            await base.ProcessParsedCommand(message);

            switch (message.Type)
            {
                case PeerCommandType.DOWNLOAD_FILE:
                    {
                        var downloadFileMessage = (DownloadFileMessage)message;

                        OnDownloadFile(downloadFileMessage);
                        break;
                    }

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
    }
}