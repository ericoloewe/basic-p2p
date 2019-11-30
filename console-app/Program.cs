using peer;
using System;
using System.IO;

namespace console_app
{
    class Program
    {
        private static PeerClient client;
        private static bool hasExited = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Feel free to type your command above: ");

            ConnectToPeer(args);

            while (!client.Stopped && !hasExited)
            {
                try
                {
                    Console.Write("> ");
                    string line = Console.ReadLine();
                    ProcessCommand(line.Split(" "));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was a problem to process command");
                    Console.WriteLine(ex);
                }
            }
        }

        private static void ConnectToPeer(string[] args)
        {
            if (args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                throw new ArgumentException($"You need to fill the ip and port of the peer");
            }

            client = new PeerClient(args[0], int.Parse(args[1]));
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
                case "exit":
                    {
                        Exit();
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

        private static void Exit()
        {
            hasExited = true;
            client.Dispose();
        }

        private static void DownloadFile(string fileName, string downloadPath)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(downloadPath))
            {
                throw new ArgumentException($"Invalid fileName or downloadPath");
            }

            var fileBytes = client.DownloadFile(fileName);

            using (var fs = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(fileBytes, 0, fileBytes.Length);
            }

            Console.WriteLine($"The file {fileName} was downloaded to {downloadPath}");
        }

        private static void ListFiles()
        {
            Console.WriteLine("Loading list");

            var task = client.GetFiles();

            task.Wait();

            Console.WriteLine("List of files: ");

            foreach (var fileName in task.Result)
            {
                Console.WriteLine($"- {fileName}");
            }
        }

        private static void UploadToNodes(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentException($"Invalid path");
            }

            string filePath = Path.GetFullPath(relativePath);

            client.UploadFile(filePath);

            Console.WriteLine($"The file {filePath} was uploaded");
        }
    }
}
