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

        public Server()
        {
            InitializeServer();
        }

        private void InitializeServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 8801);
            listener.Start();

            Thread server_recieving_clients_thread = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("in da thread");
                    TcpClient client = listener.AcceptTcpClient(); //'blocking' function
                    Console.WriteLine("client connected!!!");

                    Thread client_thread = new Thread(() => HandleClient(client));
                    //() => HandleClient(client): call the thread on a function that does not get any parameters and just calls HandleClient(client)
                    client_thread.Start();
                }
            });

            server_recieving_clients_thread.IsBackground = true;
            server_recieving_clients_thread.Start();
            Console.WriteLine("server running");
        }


        private void HandleClient(TcpClient client)
        {
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
            }
        }

    }
}
