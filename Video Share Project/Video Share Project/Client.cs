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
        public const int CONNECTION_PORT = 8001;

        public Client() 
        {
            InitializeClient();
        }

        protected void InitializeClient()
        {
            Console.WriteLine("client init");
            UdpClient client = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, CONNECTION_PORT);

            byte[] message = Encoding.UTF8.GetBytes(Messages.AcceptClient.name());
            client.Send(message, message.Length, endPoint);
            client.Close();
        }
    }
}
