using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
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
        public const int TCP_BUFFER_LENGTH = 512 * 1024;
        public const int CONNECTION_PORT = 8001;
        public const int DATA_PORT = 8801;
        private NetworkStream stream;
        public const int BUFFER_LENGTH = 512 * 1024; //512KB
        private byte[] videoBuffer;

        public Client()
        {
            Task.Run(() => RunInitAsync());
        }

        private async Task RunInitAsync()
        {
            await InitializeClient();
            Console.WriteLine("Finished init");
            //Thread getMessagesThread = new Thread(() =>
            //{
            //    ReceiveMessages();
            //});
            //getMessagesThread.Start();
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

                Console.WriteLine($"Got message {message} weird ahh");
            }
        }

        
        private async Task<byte[]> ReceiveMessages()
        {
            Console.WriteLine("Started receiveing a message...");
            byte[] buffer = new byte[TCP_BUFFER_LENGTH];
            int messageLength = await stream.ReadAsync(buffer, 0, TCP_BUFFER_LENGTH);
            Console.WriteLine($"Got message {Encoding.UTF8.GetString(buffer, 0, messageLength)}");

            byte[] message = new byte[messageLength];
            Array.Copy(buffer, message, messageLength);
            return message;
        }
        

        public async Task WaitForVideoBroadcastMessage(Video video)
        {
            Console.WriteLine("Client waiting for broadcast message...");

            string message = Encoding.UTF8.GetString(await ReceiveMessages());
            Console.WriteLine($"Got message {message} in WaitForVideo...");
            while(! message.Equals(Messages.StartingVideoBroadcast.name()))
            {
                message = Encoding.UTF8.GetString(await ReceiveMessages());
            }

            Console.WriteLine("Got StartingVideoBroadcast message from server");

            Thread acceptVideoChunks = new Thread(() =>
            {
                ReceiveVideoBroadcast(video);
            });

            acceptVideoChunks.Start();
        }

        private void ReceiveVideoSegment()
        {

        }

        private void ReceiveVideoBroadcast(Video video)
        {
            FileStream fileStream = null;
            try
            {
                Console.WriteLine("dawid");
                fileStream = new FileStream("buffer.mp4", FileMode.Create, FileAccess.Write, FileShare.Read);
                //TODO: CHANGE buffer.mp4 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                videoBuffer = new byte[BUFFER_LENGTH];
                int bytesRead;

                //bool startedVideo = false;
                bool foundFirstChunk = false, foundLastChunk = false;

                Console.WriteLine("About to read from stream");

                while (!foundLastChunk && (bytesRead = stream.Read(videoBuffer, 0, BUFFER_LENGTH)) > 0)
                {
                    Console.WriteLine($"Read message from server!!!!!!!!");
                    Console.WriteLine(Encoding.UTF8.GetString(videoBuffer, 0, 30) + "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

                    int writingOffset = 0, writingLength = bytesRead;

                    if (bytesRead >= 17 && Encoding.UTF8.GetString(videoBuffer, 0, 17).Equals("START_OF_SEGMENT:") && !foundFirstChunk) //string length is 17
                    {
                        foundFirstChunk = true;
                        writingOffset = 17;
                        writingLength -= 17;
                    }
                    else if (bytesRead >= 15 && Encoding.UTF8.GetString(videoBuffer, 0, 15).Equals("END_OF_SEGMENT:") && foundFirstChunk)
                    {
                        foundLastChunk = true;
                        writingOffset = 15;
                        writingLength -= 15;
                    }

                    if (writingLength > 0 && foundFirstChunk)
                    {
                        fileStream.Write(videoBuffer, writingOffset, writingLength); //writing data to the stream
                        fileStream.Flush(); //writing data to file (to memory)
                    }
                }

                fileStream.Close();
                fileStream = null; //close the stream

                if(foundFirstChunk && foundLastChunk)
                {
                    Console.WriteLine("Just before Initialize video!!!");
                    InitiateVideo(video);
                }
                else
                {
                    Console.WriteLine("Received incomplete segment");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if(fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }


        private void InitiateVideo(Video video)
        {
            Console.WriteLine("Initiate Video!!!!!!!");

            if(video.videoView.InvokeRequired)
            {
                video.videoView.Invoke(new Action(() =>
                { 
                    video.PlaySegment("buffer.mp4");
                }));
            }
            else
            {
                video.PlaySegment("buffer.mp4");
            }


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
