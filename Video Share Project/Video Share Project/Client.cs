using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Video_Share_Project
{
    class Client
    {
        public Client() 
        {
            InitializeClient();
        }

        protected void InitializeClient()
        {
            Console.WriteLine("client init");
            TcpClient client = new TcpClient();
            client.Connect("localhost", 8801);
            NetworkStream stream = client.GetStream();

            byte[] message = Encoding.UTF8.GetBytes("popo");
            stream.Write(message, offset: 0, message.Length);
            Console.WriteLine("end client init");
        }
    }
}
