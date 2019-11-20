using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class Peer : IDisposable
    {
        public IEnumerable<PeerFile> Files { get; }
        public bool Stopped { get; private set; }

        private IList<PeerProcessor> peers = new List<PeerProcessor>();
        private Socket listener;
        private readonly Task cycle;
        private readonly string ip;
        private readonly int port;
        private readonly int owner;

        public Peer(string ip, int port)
        {
            Console.WriteLine($"Creating peer at endpoint {ip}:{port}");

            cycle = StartToAccept(ip, port).ContinueWith(t => HandleStop());

            this.ip = ip;
            this.port = port;
        }

        public void Connect(string nodeIp, int nodePort)
        {
            var ipAddress = IPAddress.Parse(nodeIp);
            var endpoint = new IPEndPoint(ipAddress, nodePort);
            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(endpoint);

            var connection = new PeerConnection(sender);

            CreateProcessor(connection);
            Console.WriteLine($"Connected to endpoint: {sender.RemoteEndPoint}");
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void UploadFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath).ToList();

            for (int i = 0; i < peers.Count; i++)
            {
                var peer = peers[i];
                var fragmentSize = (bytes.Count / peers.Count);
                var startIndex = fragmentSize * i;
                var endIndex = fragmentSize * (i + 1);
                var list = bytes.GetRange(startIndex, endIndex - startIndex);

                var fileName = Path.GetFileName(filePath);

                var file = new PeerFile(fileName, this, startIndex, endIndex, list.ToArray());

                peer.SendFile(file);
            }
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

            CreateProcessor(connection);
        }

        private void CreateProcessor(PeerConnection connection)
        {
            var processor = new PeerProcessor(connection, p => peers.Remove(p));

            peers.Add(processor);
        }

        private void HandleStop()
        {
            Console.WriteLine("Stop to accept!");
            Stopped = true;

            if (cycle.IsFaulted)
            {
                Console.WriteLine("There was some problems!");
                Console.WriteLine(cycle.Exception);
            }
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
}
