namespace peer
{
    public class PeerFile
    {
        public string Name { get; private set; }
        public byte[] Slice { get; private set; }

        private readonly PeerProcessor processorOwner;
        private readonly Peer owner;
        private readonly int startIndex;
        private readonly int endIndex;

        public PeerFile(string fileName, Peer owner, int startIndex, int endIndex, byte[] fileSlice)
        {
            Name = fileName;
            this.owner = owner;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            Slice = fileSlice;
        }

        public PeerFile(string fileName, PeerProcessor processorOwner, int startIndex, int endIndex, byte[] fileSlice)
        {
            Name = fileName;
            this.processorOwner = processorOwner;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            Slice = fileSlice;
        }

        public override string ToString()
        {
            return $"{Name};{startIndex};{endIndex};{Slice.Length}";
        }
    }
}
