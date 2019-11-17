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
            var ipAddress = IPAddress.Parse(nodeIp);
            var endpoint = new IPEndPoint(ipAddress, nodePort);
            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(endpoint);
            peers.Add(new PeerConnection(sender));
            Console.WriteLine($"Connected to endpoint: {sender.RemoteEndPoint}");
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task StartToAccept(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(endpoint);
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
            Console.WriteLine($"Start to accept at: {listener.RemoteEndPoint}");
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
