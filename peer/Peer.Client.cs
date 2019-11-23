using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;

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
            var processor = new PeerProcessorServer(connection);

            processor.OnReceive = f => SaveFile(f);
            processor.OnStop = () => processors.Remove(processor);
            processors.Add(processor);

            Console.WriteLine($"Connected to endpoint: {sender.RemoteEndPoint}");
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void UploadFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath).ToList();
            var fragmentSize = (bytes.Count / processors.Count);
            var currentSlice = 0;

            var startIndex = fragmentSize * currentSlice;
            var endIndex = fragmentSize * (currentSlice + 1);
            var file = GetFileByStartAndEndIndexes(filePath, bytes, startIndex, endIndex);

            SaveFile(file);

            foreach (var peerProcessor in processors)
            {
                var numberOfConnections = peerProcessor.GetNumberOfConnections();

                startIndex = fragmentSize * currentSlice;
                currentSlice += numberOfConnections;
                endIndex = fragmentSize * (currentSlice + 1);
                file = GetFileByStartAndEndIndexes(filePath, bytes, startIndex, endIndex);

                peerProcessor.SendFile(file);
            }
        }
    }
}
