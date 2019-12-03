using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using peer.Domains;
using peer.Processors;

namespace peer
{
    public class Peer : IDisposable
    {
        public bool Stopped { get; private set; }
        public PeerInfo Info { get; }

        private readonly IList<PeerProcessor> connectedPeers = new List<PeerProcessor>();
        private readonly IList<PeerProcessor> connectedClients = new List<PeerProcessor>();
        private readonly IList<PeerFileSlice> files = new List<PeerFileSlice>();
        private readonly Task cycle;
        private readonly string ip;
        private readonly int port;
        private Socket listener;

        public Peer(string ip, int port)
        {
            Console.WriteLine($"Creating peer at endpoint {ip}:{port}");

            cycle = StartToAccept(ip, port).ContinueWith(t => HandleStop());

            this.ip = ip;
            this.port = port;
            Info = new PeerInfo(port);
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
            foreach (var connection in connectedPeers)
            {
                connection.Dispose();
            }

            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
        }

        public async Task<PeerFile> GetAllSlicesOfFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PeerFileSlice> GetFiles() => files;

        public async Task<int> GetNumberOfConnectionsWithoutProcesor(int requesterPeerId)
        {
            var numberOfConnections = 0;

            foreach (var processor in GetConnectedPeersWithoutId(requesterPeerId))
            {
                var amountOfProcessorConnections = await processor.GetNumberOfConnections();

                numberOfConnections += amountOfProcessorConnections + 1;
            }

            return numberOfConnections;
        }

        public async Task UploadFile(string fileName, byte[] bytes)
        {
            var bytesAsList = bytes.ToList();
            var file = GetFileByStartAndEndIndexes(fileName, bytesAsList, 0, bytesAsList.Count, Info);

            await SaveAndShareForPeers(file, connectedPeers);
        }

        public async Task SaveAndShare(PeerFileSlice file, int requesterPeerId)
        {
            var peersToShare = GetConnectedPeersWithoutId(requesterPeerId);

            await SaveAndShareForPeers(file, peersToShare);
        }

        public async Task SaveAndShareForPeers(PeerFileSlice file, IList<PeerProcessor> peersToShare)
        {
            var bytesAsList = file.Slice.ToList();
            int peersAmount = await GetNumberOfFragments(peersToShare) + 1;
            var fragmentSize = (bytesAsList.Count / peersAmount);
            var currentSlice = 0;

            var startIndex = fragmentSize * currentSlice;
            var endIndex = fragmentSize * (currentSlice + 1);

            SaveFile(GetFileByStartAndEndIndexes(file, startIndex, endIndex));

            foreach (var peerProcessor in peersToShare)
            {
                currentSlice++;
                var numberOfConnections = await peerProcessor.GetNumberOfConnections();

                Console.WriteLine($"numberOfConnections: {peerProcessor.ConnectedPeerInfo} => {numberOfConnections}");

                startIndex = fragmentSize * currentSlice;
                currentSlice += numberOfConnections;
                if (peerProcessor == peersToShare.Last())
                    endIndex = bytesAsList.Count;
                else
                    endIndex = fragmentSize * (currentSlice + 1) - 1;
                file = GetFileByStartAndEndIndexes(file, startIndex, endIndex);

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
            bool isClient = processor.IsClient();

            processor.OnStop = () =>
            {
                processor.Dispose();

                if (isClient)
                    connectedClients.Remove(processor);
                else
                    connectedPeers.Remove(processor);
            };

            if (isClient)
                connectedClients.Add(processor);
            else
                connectedPeers.Add(processor);
        }

        private List<PeerProcessor> GetConnectedPeersWithoutId(int peerId) => connectedPeers.Where(cp => cp.ConnectedPeerInfo.Id != peerId).ToList();

        private PeerFileSlice GetFileByStartAndEndIndexes(PeerFileSlice file, int startIndex, int endIndex)
        {
            var realStartIndex = file.StartIndex + startIndex;
            var realEndIndex = realStartIndex + (endIndex - startIndex);
            var list = file.Slice.ToList().GetRange(startIndex, endIndex - startIndex);
            var slice = new PeerFileSlice(file.Name, file.Owner, realStartIndex, realEndIndex, list.ToArray());

            return slice;
        }

        private PeerFileSlice GetFileByStartAndEndIndexes(string fileName, List<byte> bytes, int startIndex, int endIndex, PeerInfo owner)
        {
            var list = bytes.GetRange(startIndex, endIndex - startIndex);
            var file = new PeerFileSlice(fileName, owner, startIndex, endIndex, list.ToArray());

            return file;
        }

        private async Task<int> GetNumberOfFragments(IList<PeerProcessor> peersToShare)
        {
            var amount = 0;

            foreach (var peerProcessor in peersToShare)
            {
                var numberOfConnections = await peerProcessor.GetNumberOfConnections();

                amount += 1 + numberOfConnections;
            }

            return amount;
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

        private void SaveFile(PeerFileSlice file)
        {
            Console.WriteLine($"Saving file => {file}");
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
