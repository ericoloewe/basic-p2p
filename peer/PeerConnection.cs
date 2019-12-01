using peer.Messages;
using System;
using System.Net.Sockets;

namespace peer
{
    public class PeerConnection : IDisposable
    {
        private Socket socket;

        public PeerConnection(Socket socket)
        {
            this.socket = socket;
        }

        public Message Receive()
        {
            var bytes = new byte[1024];

            socket.Receive(bytes);

            return Message.FromBytes(bytes);
        }

        public void Send(Message message)
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