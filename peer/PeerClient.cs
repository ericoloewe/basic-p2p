using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace peer
{
    public class PeerClient : IDisposable
    {
        public bool Stopped { get; private set; }
        private string ip;
        private int port;

        public PeerClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public IList<PeerFile> GetFiles()
        {
            throw new NotImplementedException();
        }

        public async Task UploadFile(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
