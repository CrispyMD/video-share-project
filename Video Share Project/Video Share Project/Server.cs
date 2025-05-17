using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
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
        public const int TCP_BUFFER_LENGTH = 512 * 1024;
        public System.Windows.Forms.Button serverButton;
        List<NetworkStream> clientsStreams = new List<NetworkStream>();

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
            if (await DoesServerExist() != null) {
                Console.WriteLine("Found a server");
                setServerButtonEnabled(true);
                return false;
            }

            AnswerDoesServerExistsMessages();
            CreateThreadsForClients();

            setServerButtonEnabled(true);
            return true;
        }


        private void AnswerDoesServerExistsMessages() //udp
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
                        Console.WriteLine($"Answered DSE in endpoint {endpoint}");
                    }
                }
            });

            answerDSEMessages.IsBackground = true;
            answerDSEMessages.Start();
        }



        private void CreateThreadsForClients() //tcp
        {
            TcpListener listener = new TcpListener(IPAddress.Any, DATA_PORT);
            listener.Start();
            Console.WriteLine("TCP Server listening...");

            Thread createHandlingThreads = new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread handleClientThread = new Thread(() => HandleClient(client));
                    handleClientThread.Start();
                }
            });
            createHandlingThreads.IsBackground = true;
            createHandlingThreads.Start();
        }



        private void HandleClient(TcpClient client) //tcp
        {
            NetworkStream stream = client.GetStream();
            sendMessage(Messages.ConnectionEstablished.name(), stream);
            Console.WriteLine($"Sent message to client in endpoint {client.Client.RemoteEndPoint}");
            clientsStreams.Add(stream);
            

            while (true)
            {
                string message = getMessage(stream);
                GotMessageFromClient.Invoke(client.Client.RemoteEndPoint, message);
                //Console.WriteLine($"Sending message {message}");
                //sendMessage(message);
                Console.WriteLine($"Server got message {message} from endpoint {client.Client.RemoteEndPoint}");
            }

        }



        public void sendMessage(byte[] message)
        {
            foreach(NetworkStream stream in clientsStreams)
            {
                sendMessage(message, stream);
            }
        }

        public void sendMessage(string message)
        {
            foreach(NetworkStream stream in clientsStreams)
            {
                sendMessage(message, stream);
            }
        }

        public void sendMessage(string message, NetworkStream stream)
        {
            byte[] byteMessage = Encoding.UTF8.GetBytes(message);
            stream.Write(byteMessage, 0, byteMessage.Length);
        }

        public void sendMessage(byte[] message, NetworkStream stream)
        {
            stream.Write(message, 0, message.Length);
        }



        public string getMessage(NetworkStream stream)
        {
            byte[] message = new byte[TCP_BUFFER_LENGTH];
            int messageLength = stream.Read(message, 0, TCP_BUFFER_LENGTH);
            Console.WriteLine($"Got message {Encoding.UTF8.GetString(message, 0, messageLength)}");
            return Encoding.UTF8.GetString(message, 0, messageLength);
        }



        public static async Task<IPAddress> DoesServerExist() //udp
        {
            return await Task.Run(() =>
            {
                const int broadcastTimeout = 1000;
                UdpClient checksForServer = new UdpClient() { EnableBroadcast = true };
                checksForServer.Client.ReceiveTimeout = broadcastTimeout;
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);


                byte[] broadcastMessage = Encoding.UTF8.GetBytes(Messages.DoesServerExist.name());
                checksForServer.Send(broadcastMessage, broadcastMessage.Length, new IPEndPoint(IPAddress.Broadcast, CONNECTION_PORT));

                IPAddress recieveMessage()
                {
                    try
                    {
                        string message = Encoding.UTF8.GetString(checksForServer.Receive(ref endpoint));
                        Console.WriteLine(endpoint.Address);
                        if (message.Equals(Messages.ServerExists.name()))
                        {
                            checksForServer.Close();
                            return endpoint.Address;
                        }

                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("I am the server");
                        checksForServer.Close();
                        return null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR!!!!!!!!!!!!!! " + e.Message);
                    }

                    return endpoint.Address; //shouldn't get here
                }

                return Task.FromResult(recieveMessage());
            });
        }




        public void StartVideoBroadcast(Video video, string path)
        {
            //TODO: CREATE CHUNKS
            //Currently just sending chunks manually

            List<byte[]> chunksList = new List<byte[]>();
            chunksList.Add(
            //List<byte[]> chunksList = Video.CreateChunks(path);
        }
    }
}
