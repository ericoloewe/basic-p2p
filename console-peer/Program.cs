using peer;
using System;

namespace console_peer
{
    class Program
    {
        private static Peer peer;
        private static bool hasExited = false;

        static void Main(string[] args)
        {
            Console.WriteLine("Feel free to type your command above: ");

            StartToAccept(args);

            while (!peer.Stopped && !hasExited)
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

        private static void StartToAccept(string[] args)
        {
            if (args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                throw new ArgumentException($"You need to fill the ip and port of the peer");
            }

            peer = new Peer(args[0], int.Parse(args[1]));
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
                case "exit":
                    {
                        Exit();
                        break;
                    }
                default:
                    {
                        throw new ArgumentException($"Invalid command => {command}");
                    }
            }
        }

        private static void Connect(string nodeIp, string nodePort)
        {
            if (string.IsNullOrEmpty(nodeIp) || string.IsNullOrEmpty(nodePort))
            {
                throw new ArgumentException($"Invalid host or ip");
            }

            peer.Connect(nodeIp, int.Parse(nodePort));
        }

        private static void Exit()
        {
            hasExited = true;
            peer.Dispose();
        }
    }
}
