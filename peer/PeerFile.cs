namespace peer
{
    public class PeerFile
    {
        public string Name { get; private set; }

        private readonly PeerProcessor processorOwner;
        private readonly Peer owner;
        private readonly int startIndex;
        private readonly int endIndex;
        private readonly byte[] filePart;

        public PeerFile(string fileName, Peer owner, int startIndex, int endIndex, byte[] filePart)
        {
            this.Name = fileName;
            this.owner = owner;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.filePart = filePart;
        }

        public PeerFile(string fileName, PeerProcessor processorOwner, int startIndex, int endIndex, byte[] filePart)
        {
            this.Name = fileName;
            this.processorOwner = processorOwner;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.filePart = filePart;
        }
    }
}
