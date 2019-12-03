using peer.Domains;
using System.Text;

namespace peer.Messages
{
    public class PeerInfoMessage : Message
    {
        public PeerInfo Info { get; }

        public PeerInfoMessage(PeerInfo info) : base(PeerCommandType.PEER_INFO)
        {
            Info = info;
            body = Encoding.UTF8.GetBytes(info.ToString());
        }

        public PeerInfoMessage(Message message) : base(message)
        {
            Info = PeerInfo.FromString(Encoding.UTF8.GetString(body));
        }

        public override string ToString() => $"{Type} => {Info}";
    }
}