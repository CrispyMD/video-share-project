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
        public const int PORT = 8801;

        public Server()
        {
            InitializeServer();

        }

        private async Task<bool> InitializeServer() //return depends on init success
        {
            if (! await DoesServerExist()) {
                Console.WriteLine("Found a server");
                return false;
            }
            

            UdpClient listener = new UdpClient() {EnableBroadcast=true };
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, PORT);
            listener.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            
            Thread server_recieving_clients_thread = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("in da thread");
                    byte[] bytes = listener.Receive(ref endpoint); //'blocking' function
                    Console.WriteLine($"Received broadcast from {endpoint} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");

                    Thread client_thread = new Thread(() => HandleClient(endpoint));
                    //() => HandleClient(client): call the thread on a function that does not get any parameters and just calls HandleClient(client)
                    client_thread.Start();
                }
            });

            server_recieving_clients_thread.IsBackground = true;
            server_recieving_clients_thread.Start();
            Console.WriteLine("server running");
            return true;
        }

        
        private void HandleClient(IPEndPoint endpoint)
        {
            /*
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[4096];
            int bytesRead;
            string data = "";

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                // Process received data
                //byte[] response = Encoding.ASCII.GetBytes("Server response");
                //stream.Write(response, offset:0, response.Length);
                Console.WriteLine(data);
                GotMessageFromClient?.Invoke(this, data); //? ensures that if there are no subscribers invoke won't raise an error
            } */
        }
        


        private async Task<bool> DoesServerExist()
        {
            UdpClient checksForServer = new UdpClient() { EnableBroadcast = true };
            checksForServer.Client.ReceiveTimeout = 1000;
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, PORT);

            byte[] broadcastMessage = Encoding.UTF8.GetBytes("bobo");
            await checksForServer.SendAsync(broadcastMessage, broadcastMessage.Length, new IPEndPoint(IPAddress.Broadcast, PORT));
            checksForServer.Client.Bind(endpoint);


            try
            {
                string message = Encoding.UTF8.GetString(checksForServer.Receive(ref endpoint));
                Console.WriteLine($"Got message {message}");
                return true;
            }
            catch (SocketException e) //no one sent a message
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("I am the server");
                return false;
            }
            finally { checksForServer.Close(); }
        }

    }
}
