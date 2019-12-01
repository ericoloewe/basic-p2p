using System;
using System.Text;

namespace peer.Messages
{
    public class GetFileMessage : Message
    {
        public string FileName { get; }

        public GetFileMessage(string fileName) : base(PeerCommandType.GET_FILE)
        {
            FileName = fileName;
            body = Encoding.UTF8.GetBytes(fileName);
        }

        public GetFileMessage(Message message) : base(message)
        {
            FileName = Encoding.UTF8.GetString(body);
        }

        public override string ToString() => $"{Type} => {FileName}";
    }
}