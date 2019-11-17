using peer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace console_app
{
    class Program
    {
        private static Peer peer = new Peer();
        private static bool hasStopped = false;

        static void Main(string[] args)
        {
            StartToAccept(args);

            Console.WriteLine("Type your command: ");

            string line;

            while ((line = Console.ReadLine()) != "exit" || !hasStopped)
            {
                try
                {
                    ProcessCommand(line.Split(" "));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("There was a problem to process command");
                }
            }
        }

        private static void StartToAccept(string[] args)
        {
            if (args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                throw new ArgumentException($"You need to fill the ip and port of the peer");
            }

            peer.StartToAccept(args[0], int.Parse(args[1])).ContinueWith(t => HandleStopToAccept());
        }

        private static void HandleStopToAccept()
        {
            Console.WriteLine("Stop to accept!");

            hasStopped = true;
        }

        private static void ProcessCommand(string[] args)
        {
            var command = args[0];

            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException($"Invalid command => {command}");
            }

            var parsedCommand = command.Trim().ToLower();

            switch (parsedCommand)
            {
                case "connect":
                    {
                        Connect(args[1], args[2]);
                        break;
                    }
                case "list":
                    {
                        ListFiles();
                        break;
                    }
                case "upload":
                    {
                        UploadToNodes(args[1]);
                        break;
                    }
                case "download":
                    {
                        DownloadFile(args[1], args[2]);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"Invalid command => {command}");
                    }
            }
        }

        private static void Connect(string nodeHost, string nodeIp)
        {
            if (string.IsNullOrEmpty(nodeHost) || string.IsNullOrEmpty(nodeIp))
            {
                throw new ArgumentException($"Invalid host or ip");
            }

            peer.Connect(nodeHost, int.Parse(nodeIp));
            Console.WriteLine($"Connected to {nodeHost} {nodeIp}");
        }

        private static void ListFiles()
        {
            Console.WriteLine("List of files: ");

            foreach (var file in peer.Files)
            {
                Console.WriteLine($"- {file.Name}");
            }
        }

        private static void DownloadFile(string fileName, string downloadPath)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(downloadPath))
            {
                throw new ArgumentException($"Invalid fileName or downloadPath");
            }

            var fileBytes = peer.DownloadFile(fileName);

            using (var fs = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileBytes, 0, fileBytes.Length);
            }

            Console.WriteLine($"The file {fileName} was downloaded to {downloadPath}");
        }

        private static void UploadToNodes(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentException($"Invalid path");
            }

            string filePath = Path.GetFullPath(relativePath);

            peer.UploadFile(filePath);
            Console.WriteLine($"The file {filePath} was uploaded");
        }
    }
}
