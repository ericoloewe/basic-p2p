namespace peer.Domains
{
    public class PeerFileSlice
    {
        public string Name { get; }
        public byte[] Slice { get; }
        public PeerInfo Owner { get; }
        public int StartIndex { get; }
        public int EndIndex { get; }

        public PeerFileSlice(string fileName, PeerInfo owner, int startIndex, int endIndex, byte[] fileSlice)
        {
            Name = fileName;
            Owner = owner;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            Slice = fileSlice;
        }

        public override string ToString()
        {
            return $"{Name};{StartIndex};{EndIndex};{Owner}";
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
