using System;
using System.Net.Sockets;
using System.Text;

namespace peer
{
    internal class PeerConnection : IDisposable
    {
        private Socket socket;

        public PeerConnection(Socket socket)
        {
            this.socket = socket;
        }

        internal string Receive()
        {
            string message;
            int bytesRec;
            var bytes = new byte[10240];

            lock (socket)
            {
                bytesRec = socket.Receive(bytes);
            }

            message = Encoding.UTF8.GetString(bytes, 0, bytesRec);

            return message.Trim();
        }

        internal void Send(string message)
        {
            lock (socket)
            {
                socket.Send(Encoding.ASCII.GetBytes($"{message}\n"));
            }
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}