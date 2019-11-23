namespace peer
{
    public class PeerFile
    {
        public string Name { get; }
        public byte[] Slice { get; }
        public PeerInfo Owner { get; }

        private readonly PeerProcessor processorOwner;
        private readonly int startIndex;
        private readonly int endIndex;

        public PeerFile(string fileName, PeerInfo owner, int startIndex, int endIndex, byte[] fileSlice)
        {
            Name = fileName;
            this.Owner = owner;
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
