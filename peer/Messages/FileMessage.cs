﻿using peer.Domains;
using System.Linq;
using System.Text;

namespace peer.Messages
{
    public class FileMessage : Message
    {
        public PeerFileSlice File { get; }

        public FileMessage(PeerFileSlice file) : base(PeerCommandType.FILE)
        {
            var fileHead = Encoding.UTF8.GetBytes(file.ToString());

            head = fileHead.ToArray();
            body = file.Slice;
            File = file;
        }

        public FileMessage(Message message) : base(message)
        {
            var fileInfoCommandSplit = Encoding.UTF8.GetString(head).Split(';');
            var fileName = fileInfoCommandSplit[0];
            var startIndex = int.Parse(fileInfoCommandSplit[1]);
            var endIndex = int.Parse(fileInfoCommandSplit[2]);
            var info = PeerInfo.FromString(fileInfoCommandSplit[3]);

            File = new PeerFileSlice(fileName, info, startIndex, endIndex, body);
        }

        public override string ToString() => $"{Type} => {File}";
    }
}