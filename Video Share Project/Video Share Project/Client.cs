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
        public const int PORT = 8801;

        public Client() 
        {
            InitializeClient();
        }

        protected void InitializeClient()
        {
            Console.WriteLine("client init");
            UdpClient client = new UdpClient(PORT);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);

            byte[] message = Encoding.UTF8.GetBytes("popo");
            //client.SendTo(message, endPoint);
            client.Close();
        }
    }
}
