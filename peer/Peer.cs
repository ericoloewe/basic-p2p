using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using peer.Messages;

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

        public void Dispose()
        {
            foreach (var connection in processors)
            {
                connection.Dispose();
            }

            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PeerFile> GetFiles()
        {
            return files;
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

        public async Task UploadFile(string fileName, byte[] bytes)
        {
            var bytesAsList = bytes.ToList();
            int peersAmount = GetNumberOfFragments();
            var fragmentSize = (bytesAsList.Count / peersAmount);
            var currentSlice = 0;

            var startIndex = fragmentSize * currentSlice;
            var endIndex = fragmentSize * (currentSlice + 1);
            var file = GetFileByStartAndEndIndexes(fileName, bytesAsList, startIndex, endIndex);

            SaveFile(file);

            foreach (var peerProcessor in processors)
            {
                var numberOfConnections = await peerProcessor.GetNumberOfConnections();

                Console.WriteLine($"numberOfConnections {numberOfConnections}");

                startIndex = fragmentSize * currentSlice;
                currentSlice += numberOfConnections;
                endIndex = fragmentSize * (currentSlice + 1);
                file = GetFileByStartAndEndIndexes(fileName, bytesAsList, startIndex, endIndex);

                peerProcessor.SendFile(file);
            }
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

        internal Task SaveFileAndUploadToOther(FileMessage fileMessage)
        {
            throw new NotImplementedException();
        }

        private PeerFile GetFileByStartAndEndIndexes(string fileName, List<byte> bytes, int startIndex, int endIndex)
        {
            var list = bytes.GetRange(startIndex, endIndex - startIndex);
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
