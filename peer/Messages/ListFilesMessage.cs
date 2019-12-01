using peer.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class ListFilesMessage : Message
    {
        private static readonly string SEPARATOR = "\n";

        public string[] Files
        {
            get
            {
                var parsedBody = Encoding.UTF8.GetString(body);

                return parsedBody.Split(SEPARATOR.ToCharArray());
            }
        }

        public ListFilesMessage(IEnumerable<PeerFileSlice> files) : base(PeerCommandType.LIST_FILES)
        {
            string fileList = string.Join(SEPARATOR, files.Select(f => f.Name));

            body = Encoding.UTF8.GetBytes(fileList);
        }

        public ListFilesMessage(Message message) : base(message) { }

        public override string ToString() => $"{Type} => {Files}";
    }
}