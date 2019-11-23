using System;

namespace peer
{
    public class PeerProcessorClient : PeerProcessor
    {
        public PeerProcessorClient(PeerConnection connection) : base(connection)
        {
        }

        protected override string ReceiveAndProccessCommand()
        {
            string parsedCommand = base.ReceiveAndProccessCommand();

            switch (parsedCommand)
            {
                default:
                    {
                        throw new ArgumentException($"Invalid command: {parsedCommand}");
                    }
            }

            return parsedCommand;
        }
    }
}