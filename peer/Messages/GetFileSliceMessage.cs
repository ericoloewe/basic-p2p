using System.Text;

namespace peer.Messages
{
    public class GetFileSliceMessage : Message
    {
        public string FileName { get; }

        public GetFileSliceMessage(string fileName) : base(PeerCommandType.GET_FILE_SLICE)
        {
            FileName = fileName;
            body = Encoding.UTF8.GetBytes(fileName);
        }

        public GetFileSliceMessage(Message message) : base(message)
        {
            FileName = Encoding.UTF8.GetString(body);
        }

        public override string ToString() => $"{Type} => {FileName}";
    }
}