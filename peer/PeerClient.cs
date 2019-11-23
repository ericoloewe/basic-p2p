using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer
{
    public class PeerClient : IDisposable
    {
        public void Connect(string nodeIp, int nodePort)
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
