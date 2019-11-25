using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public partial class Peer : IDisposable
    {
        public bool Stopped { get; private set; }

        private readonly IList<PeerProcessor> processors = new List<PeerProcessor>();
        private readonly IList<PeerFile> files = new List<PeerFile>();
        private readonly Task cycle;
        private readonly string ip;
        private readonly int port;
        private readonly PeerInfo owner;
        private Socket listener;

        public Peer(string ip, int port)
        {
            Console.WriteLine($"Creating peer at endpoint {ip}:{port}");

            cycle = StartToAccept(ip, port).ContinueWith(t => HandleStop());

            this.ip = ip;
            this.port = port;
            owner = new PeerInfo(port);
        }

        public async Task<int> GetNumberOfConnectionsWithoutProcesor(PeerProcessor requester)
        {
            var numberOfConnections = 0;

            foreach (var processor in processors)
            {
                if (processor != requester)
                {
                    var amountOfProcessorConnections = await processor.GetNumberOfConnections();

                    numberOfConnections += amountOfProcessorConnections + 1;
                }
            }

            return numberOfConnections;
        }

        public void Dispose()
        {
            foreach (var connection in processors)
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
            var processor = new PeerProcessor(connection, this);

            processor.OnReceiveFile = f => SaveFile(f);
            processor.OnStop = () =>
            {
                processor.Dispose();
                processors.Remove(processor);
            };

            processors.Add(processor);
        }

        private PeerFile GetFileByStartAndEndIndexes(string filePath, List<byte> bytes, int startIndex, int endIndex)
        {
            var list = bytes.GetRange(startIndex, endIndex - startIndex);
            var fileName = Path.GetFileName(filePath);
            var file = new PeerFile(fileName, owner, startIndex, endIndex, list.ToArray());

            return file;
        }

        private int GetNumberOfFragments()
        {
            return 4;
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

        private void SaveFile(PeerFile file)
        {
            files.Add(file);
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
