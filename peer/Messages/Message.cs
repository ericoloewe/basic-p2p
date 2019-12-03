using System.Text;
using System.Linq;

namespace peer.Messages
{
    public class Message
    {
        public static readonly byte[] END_DELIMITER = Encoding.UTF8.GetBytes(@"\end");
        private static readonly byte[] END_HEAD_DELIMITER = Encoding.UTF8.GetBytes(@"\endhead");

        public PeerCommandType Type { get; set; }

        protected byte[] head;
        protected byte[] body;

        public static Message FromBytes(byte[] bytes)
        {
            var message = new Message(bytes);

            switch (message.Type)
            {
                case PeerCommandType.CONNECTIONS:
                    message = new ConnectionMessage(message);
                    break;
                case PeerCommandType.DOWNLOAD_FILE:
                    message = new DownloadFileMessage(message);
                    break;
                case PeerCommandType.DOWNLOAD_FILE_SLICE:
                    message = new DownloadFileSliceMessage(message);
                    break;
                case PeerCommandType.GET_FILE:
                    message = new GetFileMessage(message);
                    break;
                case PeerCommandType.GET_FILE_SLICE:
                    message = new GetFileSliceMessage(message);
                    break;
                case PeerCommandType.KIND_OF_CONNECTION:
                    message = new KindOfConnectionMessage(message);
                    break;
                case PeerCommandType.LIST_FILES:
                    message = new ListFilesMessage(message);
                    break;
                case PeerCommandType.PEER_INFO:
                    message = new PeerInfoMessage(message);
                    break;
                case PeerCommandType.UPLOAD_FILE:
                    message = new UploadFileMessage(message);
                    break;
                case PeerCommandType.UPLOAD_FILE_SLICE:
                    message = new UploadFileSliceMessage(message);
                    break;
                case PeerCommandType.EXIT:
                case PeerCommandType.GENERIC_ERROR:
                case PeerCommandType.GET_CONNECTIONS:
                case PeerCommandType.GET_LIST:
                case PeerCommandType.STOP:
                    break;
            }

            return message;
        }

        public Message(PeerCommandType type)
        {
            Type = type;
            head = new byte[0];
            body = new byte[0];
        }

        protected Message(Message message)
        {
            Type = message.Type;
            head = message.head;
            body = message.body;
        }

        private Message(byte[] bytes)
        {
            var bytesAsList = bytes.ToList();
            Type = (PeerCommandType)bytes[0];

            const int startHeadIndex = 1;
            var endHeadIndex = FindBytesIndex(bytes, END_HEAD_DELIMITER);
            var endIndex = FindBytesIndex(bytes, END_DELIMITER);
            var startBodyIndex = endHeadIndex + END_HEAD_DELIMITER.Length;

            head = bytesAsList.GetRange(startHeadIndex, endHeadIndex - startHeadIndex).ToArray();
            body = bytesAsList.GetRange(startBodyIndex, endIndex - startBodyIndex).ToArray();
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

        public override string ToString() => $"{Type}";
    }
}