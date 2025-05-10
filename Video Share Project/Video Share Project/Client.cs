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
        UdpClient server;
        int sends = 0;
        public const int CONNECTION_PORT = 8001;

        public Client() 
        {
            InitializeClient();


        }

        protected void InitializeClient()
        {
            UdpClient client = new UdpClient(0);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Broadcast, CONNECTION_PORT);

            byte[] acceptMessage = Encoding.UTF8.GetBytes(Messages.AcceptClient.name());
            client.Send(acceptMessage, acceptMessage.Length, endpoint);

            client.Client.ReceiveTimeout = 1000;
            endpoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                Console.WriteLine("Waiting for message from server...");
                string message = Encoding.UTF8.GetString(client.Receive(ref endpoint));
                Console.WriteLine($"Got message {message} from endpoint {endpoint}");
                if(message.Equals(Messages.ConnectionEstablished.name()))
                {
                    Console.WriteLine($"We connected to server in endpoint {endpoint}");
                }
            }
            catch(SocketException e)
            {
                Console.WriteLine("no server exists");
            }

            sends++;
            client.Close();

        }

        private void SendMessage(byte[] message)
        {

        }


    }
}
