namespace peer
{
    internal class PeerMessage
    {
        public bool HasFile { get { return File != null; } }
        public bool HasMessage { get { return message != null; } }
        public PeerFile File { get; }

        private string message;

        public PeerMessage(string message)
        {
            this.message = message;
        }

        public PeerMessage(PeerFile file)
        {
            this.File = file;
        }

        public override string ToString()
        {
            return message;
        }
    }
}