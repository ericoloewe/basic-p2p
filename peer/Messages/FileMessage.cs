using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class FileMessage : PeerMessage
    {
        public FileMessage(PeerFile file) : base(PeerCommandType.FILE)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            head = fileHead.ToArray();
            body = file.Slice;
        }

        public FileMessage(PeerMessage message) : base(message) { }
    }
}