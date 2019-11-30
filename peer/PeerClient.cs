using peer.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace peer
{
    public class PeerClient : IDisposable
    {
        public bool Stopped { get; private set; }
        private string ip;
        private int port;
        private PeerConnection connection;

        public PeerClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            var ipAddress = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(ipAddress, port);
            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(endpoint);

            connection = new PeerConnection(sender);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public IList<PeerFile> GetFiles()
        {
            throw new NotImplementedException();
        }

        public void UploadFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            var peerNewFile = new PeerNewFile(fileName, bytes);
            var message = new UploadFileMessage(peerNewFile);

            connection.Send(message);
        }
    }
}
