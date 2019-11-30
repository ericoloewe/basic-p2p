using peer.Messages;
using System;
using System.Net.Sockets;
using System.Text;

namespace peer
{
    public class PeerConnection : IDisposable
    {
        private Socket socket;

        public PeerConnection(Socket socket)
        {
            this.socket = socket;
        }

        public PeerMessage Receive()
        {
            var bytes = new byte[1024];

            socket.Receive(bytes);

            return new PeerMessage(bytes);
        }

        public void Send(PeerMessage message)
        {
            socket.Send(message.ToBytes());
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}