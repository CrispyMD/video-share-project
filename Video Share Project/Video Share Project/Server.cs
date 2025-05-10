using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Video_Share_Project
{
    class Server
    {
        public event EventHandler<string> GotMessageFromClient;
        public const int CONNECTION_PORT = 8001;
        public const int DATA_PORT = 8801;
        public System.Windows.Forms.Button serverButton;

        public Server(System.Windows.Forms.Button serverButton)
        {
            this.serverButton = serverButton;
            Console.WriteLine("skib");
            Task.Run(() => InitializeServer());
            Console.WriteLine("idi");
        }

        private void setServerButtonEnabled(bool state) { serverButton.Invoke(new MethodInvoker(() => serverButton.Enabled = state)); }

        private async Task<bool> InitializeServer() //return depends on init success
        {
            setServerButtonEnabled(false);
            if (await DoesServerExist()) {
                Console.WriteLine("Found a server");
                setServerButtonEnabled(true);
                return false;
            }

            AnswerDoesServerExistsMessages();
            CreateThreadsForClients();

            setServerButtonEnabled(true);
            return true;
        }


        private void AnswerDoesServerExistsMessages()
        {
            Thread answerDSEMessages = new Thread(() =>
            {
                UdpClient reciever = new UdpClient();
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, CONNECTION_PORT);
                reciever.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                reciever.Client.Bind(endpoint);
                UdpClient sender = new UdpClient();

                while (true)
                {
                    string message = Encoding.UTF8.GetString(reciever.Receive(ref endpoint));
                    GotMessageFromClient.Invoke(endpoint, message);
                    if (message.Equals(Messages.DoesServerExist.name()))
                    {
                        byte[] serverExists = Encoding.UTF8.GetBytes(Messages.ServerExists.name());
                        sender.Send(serverExists, serverExists.Length, endpoint);
                    }
                }
            });

            answerDSEMessages.IsBackground = true;
            answerDSEMessages.Start();
        }



        private void CreateThreadsForClients()
        {
            UdpClient listener = new UdpClient() { EnableBroadcast = true };
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(new IPEndPoint(IPAddress.Any, CONNECTION_PORT));

            Thread server_recieving_clients_thread = new Thread(() =>
            {
                Console.WriteLine("in da thread");
                while (true)
                {
                    string message = Encoding.UTF8.GetString(listener.Receive(ref endpoint)); //'blocking' function
                    Console.WriteLine($"Received message from {endpoint} :");
                    Console.WriteLine(message);
                    if(message.Equals(Messages.AcceptClient.name()))
                    {
                        Console.WriteLine("Connecting a client...");
                        IPEndPoint clientEndpoint = new IPEndPoint(endpoint.Address, endpoint.Port);
                        Thread client_thread = new Thread(() => HandleClient(endpoint));
                        client_thread.Start();
                    }
                }
            });

            server_recieving_clients_thread.IsBackground = true;
            server_recieving_clients_thread.Start();
            Console.WriteLine("server running");
        }





        private void HandleClient(IPEndPoint endpoint)
        {
            UdpClient udp = new UdpClient();
            var message = Encoding.UTF8.GetBytes(Messages.ConnectionEstablished.name());
            udp.Send(message, message.Length, endpoint);
            //while (true)
            //{
            //    Console.WriteLine(endpoint);
            //    var message = udp.Receive(ref endpoint);
                
            //}
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
                checksForServer.Send(broadcastMessage, broadcastMessage.Length, new IPEndPoint(IPAddress.Broadcast, CONNECTION_PORT));

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
