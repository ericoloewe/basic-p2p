using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class Peer : IDisposable
    {
        public IEnumerable<File> Files { get; }
        public bool Stopped { get; private set; }

        private IList<PeerConnection> peers = new List<PeerConnection>();
        private Socket listener;

        public Peer(string ip, int port)
        {
            Console.WriteLine($"Creating peer at endpoint {ip}:{port}");

            StartToAccept(ip, port).ContinueWith(t =>
            {
                Console.WriteLine("Stop to accept!");
                Stopped = true;

                if (t.IsFaulted)
                {
                    Console.WriteLine("There was some problems!");
                    Console.WriteLine(t.Exception);
                }
            });
        }

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

        public void UploadFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var connection in peers)
            {
                connection.Dispose();
            }

            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }

        private async Task AcceptConnection()
        {
            Console.WriteLine($"Start to accept at: {listener.LocalEndPoint}");
            var handler = await listener.AcceptAsync();
            var connection = new PeerConnection(handler);

            peers.Add(connection);
        }

        private async Task StartToAccept(string ip, int port)
        {
            var ipAddress = IPAddress.Parse(ip);
            var endpoint = new IPEndPoint(ipAddress, port);
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(endpoint);
            listener.Listen(1000);

            var task = new Task(() =>
            {
                while (true)
                {
                    AcceptConnection().Wait();
                }
            });

            task.Start();
            await task;
        }
    }

    public class File
    {
        public string Name { get; set; }
    }
}
