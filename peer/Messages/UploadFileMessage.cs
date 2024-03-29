﻿using peer.Domains;
using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class UploadFileMessage : Message
    {
        public byte[] FileBytes { get { return body; } }
        public string FileName { get { return Encoding.UTF8.GetString(head); } }

        public UploadFileMessage(PeerFile file) : base(PeerCommandType.UPLOAD_FILE)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            head = fileHead.ToArray();
            body = file.FileBytes;
        }

        public UploadFileMessage(Message message) : base(message) { }

        public override string ToString() => $"{Type} => {FileName}";
    }
}