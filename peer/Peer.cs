using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class Peer
    {
        public IEnumerable<File> Files { get; }
        private IList<PeerConnection> peers = new List<PeerConnection>();

        public void Connect(string nodeIp, int nodePort)
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task StartToAccept(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(1000);

            var task = new Task(() =>
            {
                while (true)
                {
                    AcceptConnection(listener).Wait();
                }
            });

            task.Start();
            await task;
        }

        public void UploadFile(string filePath)
        {
            throw new NotImplementedException();
        }

        private async Task AcceptConnection(Socket listener)
        {
            Console.WriteLine("Start to accept");
            var handler = await listener.AcceptAsync();
            var connection = new PeerConnection(handler);

            peers.Add(connection);
        }
    }

    public class File
    {
        public string Name { get; set; }
    }
}
