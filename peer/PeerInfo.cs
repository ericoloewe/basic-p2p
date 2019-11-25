using System;

namespace peer
{
    public class PeerInfo
    {
        public int Id { get; }

        public static PeerInfo FromString(string str)
        {
            return new PeerInfo(int.Parse(str));
        }

        public PeerInfo(int id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}
