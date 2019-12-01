using System;

namespace peer.Messages
{
    public class ConnectionMessage : Message
    {
        public int ConnectionsAmount { get { return BitConverter.ToInt32(body, 0); } }

        public ConnectionMessage(int connectionsAmount) : base(PeerCommandType.CONNECTIONS)
        {
            body = BitConverter.GetBytes(connectionsAmount);
        }

        public ConnectionMessage(Message message) : base(message) { }

        public override string ToString() => $"{Type} => {ConnectionsAmount}";
    }
}