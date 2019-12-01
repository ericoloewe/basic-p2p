using System;

namespace peer.Messages
{
    public class KindOfConnectionMessage : PeerMessage
    {
        public ConnectionType KindOfConnection { get { return (ConnectionType)body[0]; } }

        public KindOfConnectionMessage(ConnectionType type) : base(PeerCommandType.KIND_OF_CONNECTION)
        {
            body = new byte[] { (byte)type };
        }

        public KindOfConnectionMessage(PeerMessage message) : base(message) { }
    }

    public enum ConnectionType
    {
        CLIENT,
        PEER,
    }
}