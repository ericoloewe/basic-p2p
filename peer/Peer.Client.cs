using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peer
{
    public partial class Peer : IDisposable
    {
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

        public IEnumerable<PeerFile> GetFiles()
        {
            return files;
        }

        public async Task UploadFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath).ToList();
            int peersAmount = GetNumberOfFragments();
            var fragmentSize = (bytes.Count / peersAmount);
            var currentSlice = 0;

            var startIndex = fragmentSize * currentSlice;
            var endIndex = fragmentSize * (currentSlice + 1);
            var file = GetFileByStartAndEndIndexes(filePath, bytes, startIndex, endIndex);

            SaveFile(file);

            foreach (var peerProcessor in processors)
            {
                var numberOfConnections = await peerProcessor.GetNumberOfConnections();

                Console.WriteLine($"numberOfConnections {numberOfConnections}");

                startIndex = fragmentSize * currentSlice;
                currentSlice += numberOfConnections;
                endIndex = fragmentSize * (currentSlice + 1);
                file = GetFileByStartAndEndIndexes(filePath, bytes, startIndex, endIndex);

                peerProcessor.SendFile(file);
            }
        }
    }
}
