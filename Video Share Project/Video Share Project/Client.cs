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
    class Client
    {
        public const int TCP_BUFFER_LENGTH = 256;
        public const int CONNECTION_PORT = 8001;
        public const int DATA_PORT = 8801;
        private NetworkStream stream;

        public Client()
        {
            RunInitAsync();
        }

        private async void RunInitAsync()
        {
            await InitializeClient();
            Thread getMessagesThread = new Thread(() =>
            {
                receiveMessages();
            });
            getMessagesThread.Start();
        }

        private async Task InitializeClient()
        {
            IPAddress serverAddress = await Server.DoesServerExist();

            if (serverAddress != null)
            {
                Console.WriteLine($"Server exists in ip address {serverAddress}");

                TcpClient server = new TcpClient();
                server.Connect(serverAddress, DATA_PORT);
                stream = server.GetStream();

                byte[] buffer = new byte[TCP_BUFFER_LENGTH];
                int messageLength = stream.Read(buffer, 0, TCP_BUFFER_LENGTH);
                string message = Encoding.UTF8.GetString(buffer, 0, messageLength);

                Console.WriteLine($"Got message {message}");
            }
        }

        
        private void receiveMessages()
        {
            SendMessage("popo");
            Console.WriteLine("Started receiveing");
            byte[] buffer = new byte[TCP_BUFFER_LENGTH];
            int messageLength = stream.Read(buffer, 0, TCP_BUFFER_LENGTH);
            Console.Write($"Got message {Encoding.UTF8.GetString(buffer, 0, messageLength)}");

            string message = Encoding.UTF8.GetString(buffer, 0, messageLength);
            SendMessage(message + " popo");
        }
        

        private void SendMessage(byte[] message)
        {
            stream.Write(message, 0, message.Length);
        }

        private void SendMessage(string message)
        {
            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            stream.Write(byteMessage, 0, byteMessage.Length);
        }


    }
}
