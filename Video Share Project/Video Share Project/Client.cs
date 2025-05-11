using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Video_Share_Project
{
    class Client
    {
        int sends = 0;
        public const int TCP_BUFFER_LENGTH = 256;
        public const int CONNECTION_PORT = 8001;

        public Client() 
        {
            InitializeClient();


        }

        protected async void InitializeClient()
        {
            IPAddress serverAddress = await Server.DoesServerExist();

            if (serverAddress != null)
            {
                Console.WriteLine($"Server exists in ip address {serverAddress}");

                TcpClient server = new TcpClient();
                server.Connect(serverAddress, CONNECTION_PORT);
                NetworkStream stream = server.GetStream();

                byte[] buffer = new byte[TCP_BUFFER_LENGTH];
                int messageLength = stream.Read(buffer, 0, TCP_BUFFER_LENGTH);
                string message = Encoding.UTF8.GetString(buffer, 0, messageLength);

                Console.WriteLine(message);
            }


            //UdpClient client = new UdpClient(0);
            //IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, CONNECTION_PORT);

            //byte[] acceptMessage = Encoding.UTF8.GetBytes(Messages.AcceptClient.name());
            //client.Send(acceptMessage, acceptMessage.Length, endpoint);
            
            //Console.WriteLine("Sent message");



            //client.Client.ReceiveTimeout = 1000;
            //endpoint = new IPEndPoint(IPAddress.Any, 0);

            //try
            //{
            //    Console.WriteLine("Waiting for message from server...");
            //    string message = Encoding.UTF8.GetString(client.Receive(ref endpoint));
            //    Console.WriteLine($"Got message {message} from endpoint {endpoint}");
            //    if (message.Equals(Messages.ConnectionEstablished.name()))
            //    {
            //        Console.WriteLine($"We connected to server in endpoint {endpoint}");
            //    }
            //}
            //catch (SocketException e)
            //{
            //    Console.WriteLine("no server exists");
            //}

            //sends++;
            //client.Close();

            //TcpListener listener = new TcpListener(IPAddress.Any, CONNECTION_PORT);
            //listener.Start();


            //while (true)
            //{
            //    Console.Write("Accepting server...");
            //    TcpClient server = listener.AcceptTcpClient();

            //    Console.WriteLine("Accepted the server");

            //    NetworkStream stream = server.GetStream();

            //    byte[] buffer = new byte[TCP_BUFFER_LENGTH];
            //    int messageLength = stream.Read(buffer, 0, buffer.Length);

            //    string message = Encoding.UTF8.GetString(buffer);
            //    if(message.Equals(Messages.ConnectionEstablished.name()))
            //    {
            //        Console.WriteLine("Got message Connection Established!!!!!!");
            //    }
        }
        

        private void SendMessage(byte[] message)
        {

        }


    }
}
