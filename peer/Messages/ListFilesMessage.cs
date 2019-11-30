using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class ListFilesMessage : PeerMessage
    {
        public ListFilesMessage(IEnumerable<PeerFile> files) : base(PeerCommandType.LIST_FILES)
        {
            string fileList = string.Join("\n", files.Select(f => f.ToString()));

            body = Encoding.UTF8.GetBytes(fileList);
        }

        public ListFilesMessage(PeerMessage message) : base(message) { }
    }
}