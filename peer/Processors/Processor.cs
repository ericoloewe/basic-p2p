﻿using peer.Domains;
using peer.Messages;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace peer.Processors
{
    public class Processor : IDisposable
    {
        public Action OnStop { protected get; set; }

        private readonly Task cycle;
        private readonly PeerConnection connection;

        private Action<ConnectionType> OnReceiveKindOfConnection;

        public Processor(PeerConnection connection)
        {
            this.connection = connection;
            cycle = StartCycle().ContinueWith(t => HandleStop());
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public bool IsClient()
        {
            var promise = new TaskCompletionSource<ConnectionType>();

            Send(new Message(PeerCommandType.GET_KIND));

            OnReceiveKindOfConnection = (type) => promise.SetResult(type);

            promise.Task.Wait();

            return promise.Task.Result == ConnectionType.CLIENT;
        }

        protected virtual async Task ProcessParsedCommand(Message message)
        {
            switch (message.Type)
            {
                case PeerCommandType.EXIT:
                    {
                        HandleStop();
                        break;
                    }
                case PeerCommandType.KIND_OF_CONNECTION:
                    {
                        var kindOfConnectionMessage = (KindOfConnectionMessage)message;

                        Console.WriteLine($"Received KindOfConnection message => {kindOfConnectionMessage.KindOfConnection}");

                        OnReceiveKindOfConnection(kindOfConnectionMessage.KindOfConnection);
                        break;
                    }
            }
        }

        protected void Send(Message message)
        {
            connection.Send(message);
        }

        private void HandleStop()
        {
            OnStop();
        }

        private async Task StartCycle()
        {
            var task = new Task(() =>
            {
                Message command;

                do
                {
                    command = SafeAndSyncReceiveAndProccessCommand();
                } while (command.Type != PeerCommandType.EXIT && command.Type != PeerCommandType.STOP);
            });

            task.Start();
            await task;
        }

        private Message SafeAndSyncReceiveAndProccessCommand()
        {
            Message command = new Message(PeerCommandType.GENERIC_ERROR);

            try
            {
                var task = ReceiveAndProccessCommand();

                task.Wait();

                if (task.Exception != null)
                {
                    throw task.Exception;
                }

                command = task.Result;
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("Stack: ");
                Console.WriteLine(ex);

                if (ex.InnerExceptions.Any(e => e is SocketException))
                {
                    command = new Message(PeerCommandType.STOP);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Stack: ");
                Console.WriteLine(ex);
            }

            return command;
        }

        private async Task<Message> ReceiveAndProccessCommand()
        {
            var command = connection.Receive();

            Console.WriteLine($"Receive command {command}");
            await ProcessParsedCommand(command);

            return command;
        }
    }
}