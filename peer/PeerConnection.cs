﻿using System;
using System.Collections.Generic;
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

        public string Receive()
        {
            lock (socket)
            {
                string message;
                int bytesRec;
                var bytes = new byte[1024];

                bytesRec = socket.Receive(bytes);
                message = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                return message.Trim();
            }
        }

        public byte[] ReceiveFile(int length)
        {
            lock (socket)
            {
                int bytesRec;
                var bytes = new byte[length];

                bytesRec = socket.Receive(bytes);

                return bytes;
            }
        }

        public void Send(string message)
        {
            lock (socket)
            {
                socket.Send(Encoding.ASCII.GetBytes($"{message}\n"));
            }
        }

        public string SendAndReceive(string message)
        {
            lock (socket)
            {
                Send(message);

                return Receive();
            }
        }

        public void SendFile(PeerFile file)
        {
            lock (socket)
            {
                Send($"begin-file;{file}");
                socket.Send(file.Slice);
                Send("end-file");
            }
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}