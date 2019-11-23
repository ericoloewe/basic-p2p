using System;

namespace peer
{
    public class PeerInfo
    {
        public int Id { get; }

        public PeerInfo(int id)
        {
            Id = id;
        }

        internal static PeerInfo FromString(string v)
        {
            throw new NotImplementedException();
        }
    }
}
