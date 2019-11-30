using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class UploadFileMessage : PeerMessage
    {
        public byte[] FileBytes { get { return body; } }
        public string FileName { get { return Encoding.UTF8.GetString(head); } }

        public UploadFileMessage(PeerNewFile file) : base(PeerCommandType.UPLOAD_FILE)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            head = fileHead.ToArray();
            body = file.FileBytes;
        }

        public UploadFileMessage(PeerMessage message) : base(message) { }
    }
}