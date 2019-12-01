using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class FileMessage : PeerMessage
    {
        public PeerFile File { get; }

        public FileMessage(PeerFile file) : base(PeerCommandType.FILE)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            head = fileHead.ToArray();
            body = file.Slice;
            File = file;
        }

        public FileMessage(PeerMessage message) : base(message)
        {
            var fileInfoCommandSplit = Encoding.UTF8.GetString(head).Split(';');
            var fileName = fileInfoCommandSplit[1];
            var startIndex = int.Parse(fileInfoCommandSplit[2]);
            var endIndex = int.Parse(fileInfoCommandSplit[3]);
            var info = PeerInfo.FromString(fileInfoCommandSplit[4]);

            File = new PeerFile(fileName, info, startIndex, endIndex, body);
        }
    }
}