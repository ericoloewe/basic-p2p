using System.Text;
using System.Linq;

namespace peer.Messages
{
    public class PeerMessage
    {
        public static readonly byte[] END_DELIMITER = Encoding.UTF8.GetBytes(@"\end");
        private static readonly byte[] END_HEAD_DELIMITER = Encoding.UTF8.GetBytes(@"\endhead");
        
        public PeerCommandType Type { get; set; }

        protected byte[] head;
        protected byte[] body;

        public PeerMessage(byte[] bytes)
        {
            var bytesAsList = bytes.ToList();
            Type = (PeerCommandType)bytes[0];

            var endHeadIndex = FindBytesIndex(bytes, END_HEAD_DELIMITER);
            var endIndex = FindBytesIndex(bytes, END_DELIMITER);
            int startBodyIndex = endHeadIndex + END_HEAD_DELIMITER.Length;

            head = bytesAsList.GetRange(1, endHeadIndex).ToArray();
            body = bytesAsList.GetRange(startBodyIndex, endIndex).ToArray();
        }

        public PeerMessage(PeerFile file)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            Type = PeerCommandType.FILE;
            head = fileHead.ToArray();
            body = file.Slice;
        }

        public PeerMessage(PeerCommandType type)
        {
            this.Type = type;
            head = END_HEAD_DELIMITER;
            body = new byte[1024];
        }

        public byte[] ToBytes()
        {
            return new byte[] { (byte)Type }
            .Concat(head)
            .Concat(END_HEAD_DELIMITER)
            .Concat(body)
            .Concat(END_DELIMITER).ToArray();
        }

        private int FindBytesIndex(byte[] haystack, byte[] needle)
        {
            // iterate backwards, stop if the rest of the array is shorter than needle (i >= needle.Length)
            for (var i = haystack.Length - 1; i >= needle.Length - 1; i--)
            {
                var found = true;
                // also iterate backwards through needle, stop if elements do not match (!found)
                for (var j = needle.Length - 1; j >= 0 && found; j--)
                {
                    // compare needle's element with corresponding element of haystack
                    found = haystack[i - (needle.Length - 1 - j)] == needle[j];
                }

                if (found)
                    // result was found, i is now the index of the last found element, so subtract needle's length - 1
                    return i - (needle.Length - 1);
            }
            // not found, return -1
            return -1;
        }
    }

    public enum PeerCommandType
    {
        CONNECTIONS,
        EXIT,
        FILE,
        GENERIC_ERROR,
        GET_CONNECTIONS,
        STOP,
        UPLOAD_FILE,
    }
}