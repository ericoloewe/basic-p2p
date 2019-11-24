namespace peer
{
    internal class PeerMessage
    {
        private string message;

        public PeerMessage(string message)
        {
            this.message = message;
        }

        public override string ToString()
        {
            return message;
        }
    }
}