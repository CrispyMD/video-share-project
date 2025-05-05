using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Video_Share_Project
{
    class Server
    {
        public event EventHandler<string> GotMessageFromClient;
        public const int CHECK_FOR_SERVER_SEND_PORT = 8001;
        //public const int CHECK_FOR_SERVER_RECIEVE_PORT = 8002;
        //public const int PORT = 8801;

        public Server()
        {
            InitializeServer();
        }

        private async Task<bool> InitializeServer() //return depends on init success
        {
            if (await DoesServerExist()) {
                Console.WriteLine("Found a server");
                return false;
            }

            AnswerDoesServerExistsMessages();
            //CreateThreadsForClients();

            
            return true;
        }



        /*private void CreateThreadsForClients()
        {
            UdpClient listener = new UdpClient() { EnableBroadcast = true };
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, PORT);
            listener.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            Thread server_recieving_clients_thread = new Thread(() =>
            {
                Console.WriteLine("in da thread");
                while (true)
                {
                    string message = Encoding.UTF8.GetString(listener.Receive(ref endpoint)); //'blocking' function
                    Console.WriteLine($"Received message from {endpoint} :");
                    Console.WriteLine(message);
                    Thread client_thread = new Thread(() => HandleClient(endpoint));

                    client_thread.Start();
                }
            });

            server_recieving_clients_thread.IsBackground = true;
            server_recieving_clients_thread.Start();
            Console.WriteLine("server running");
        }*/



        private void AnswerDoesServerExistsMessages()
        {
            Thread answerDSEMessages = new Thread(() =>
            {
                UdpClient reciever = new UdpClient();
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, CHECK_FOR_SERVER_SEND_PORT);
                reciever.Client.Bind(endpoint);
                UdpClient sender = new UdpClient();

                while (true)
                {
                    string message = Encoding.UTF8.GetString(reciever.Receive(ref endpoint));
                    if(message.Equals(Messages.DoesServerExist.name()))
                    {
                        byte[] serverExists = Encoding.UTF8.GetBytes(Messages.ServerExists.name());
                        sender.Send(serverExists, serverExists.Length, endpoint);
                    }
                }
            });
            
            answerDSEMessages.IsBackground = true;
            answerDSEMessages.Start();
        }

        

        
        private void HandleClient(IPEndPoint endpoint)
        {
            
        }



        private async Task<bool> DoesServerExist()
        {
            return await Task.Run(() =>
            {
                const int broadcastTimeout = 1000;
                UdpClient checksForServer = new UdpClient() { EnableBroadcast = true };
                checksForServer.Client.ReceiveTimeout = broadcastTimeout;
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);


                byte[] broadcastMessage = Encoding.UTF8.GetBytes(Messages.DoesServerExist.name());
                checksForServer.Send(broadcastMessage, broadcastMessage.Length, new IPEndPoint(IPAddress.Broadcast, CHECK_FOR_SERVER_SEND_PORT));

                bool recieveMessage()
                {
                    try
                    {
                        string message = Encoding.UTF8.GetString(checksForServer.Receive(ref endpoint));
                        Console.WriteLine(endpoint.Address);
                        if (message.Equals(Messages.ServerExists.name()))
                        {
                            checksForServer.Close();
                            return true;
                        }

                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("I am the server");
                        checksForServer.Close();
                        return false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR!!!!!!!!!!!!!! " + e.Message);
                    }

                    return true; //shouldn't get here
                }

                return Task.FromResult(recieveMessage());
            });
        }

    }
}
