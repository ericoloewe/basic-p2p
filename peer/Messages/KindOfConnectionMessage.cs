using System;

namespace peer.Messages
{
    public class KindOfConnectionMessage : Message
    {
        public ConnectionType KindOfConnection { get { return (ConnectionType)body[0]; } }

        public KindOfConnectionMessage(ConnectionType type) : base(PeerCommandType.KIND_OF_CONNECTION)
        {
            body = new byte[] { (byte)type };
        }

        public KindOfConnectionMessage(Message message) : base(message) { }

        public override string ToString() => $"{Type} => {KindOfConnection}";
    }

    public enum ConnectionType
    {
        CLIENT,
        PEER,
    }
}