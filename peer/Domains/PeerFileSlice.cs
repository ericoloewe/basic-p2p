namespace peer.Domains
{
    public class PeerFileSlice
    {
        public string Name { get; }
        public byte[] Slice { get; }
        public PeerInfo Owner { get; }

        private readonly int startIndex;
        private readonly int endIndex;

        public PeerFileSlice(string fileName, PeerInfo owner, int startIndex, int endIndex, byte[] fileSlice)
        {
            Name = fileName;
            Owner = owner;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            Slice = fileSlice;
        }

        public override string ToString()
        {
            return $"{Name};{startIndex};{endIndex};{Owner}";
        }
    }

    public class PeerFile
    {
        public string Name { get; }
        public byte[] FileBytes { get; }

        public PeerFile(string fileName, byte[] bytes)
        {
            Name = fileName;
            FileBytes = bytes;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
