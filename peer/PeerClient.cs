using peer.Domains;
using peer.Messages;
using peer.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class PeerClient : IDisposable
    {
        public bool Stopped { get; private set; }
        private string ip;
        private int port;
        private ClientProcessor processor;

        public PeerClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            var ipAddress = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(ipAddress, port);
            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(endpoint);

            var connection = new PeerConnection(sender);
            processor = new ClientProcessor(connection);
        }

        public void Dispose()
        {
            processor.Dispose();
        }

        public byte[] DownloadFile(string fileName)
        {
            return processor.DownloadFile(fileName);
        }

        public async Task<string[]> GetFiles()
        {
            return await processor.GetFiles();
        }

        public void UploadFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            var peerNewFile = new PeerFile(fileName, bytes);
            var message = new UploadFileMessage(peerNewFile);

            processor.Send(message);
        }
    }
}
