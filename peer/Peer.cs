using System;
using System.Collections.Generic;

namespace peer
{
    public class Peer
    {
        public IEnumerable<File> Files { get; set; }

        public void Connect(string nodeHost, string nodeIp)
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void UploadFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }

    public class File
    {
        public string Name { get; set; }
    }
}
