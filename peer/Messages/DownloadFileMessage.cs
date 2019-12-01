using peer.Domains;
using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class DownloadFileMessage : Message
    {
        public byte[] FileBytes { get { return body; } }
        public string FileName { get { return Encoding.UTF8.GetString(head); } }

        public DownloadFileMessage(PeerFile file) : base(PeerCommandType.DOWNLOAD_FILE)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            head = fileHead.ToArray();
            body = file.FileBytes;
        }

        public DownloadFileMessage(Message message) : base(message) { }

        public override string ToString() => $"{Type} => {FileName}";
    }
}