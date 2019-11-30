using peer.Messages;
using System;
using System.Threading.Tasks;

namespace peer.Processors
{
    public class ClientProcessor : Processor
    {
        public Action<PeerFile> OnReceiveFile { protected get; set; }
        public Action<int> OnReceiveNumberOfConnections { protected get; set; }

        public ClientProcessor(PeerConnection connection) : base(connection) { }

        protected override async Task ProcessParsedCommand(PeerMessage message)
        {
            await base.ProcessParsedCommand(message);

            switch (message.Type)
            {
            }
        }
    }
}