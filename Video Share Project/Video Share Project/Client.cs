using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
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
            Thread getMessagesThread = new Thread(() =>
            {
                ReceiveMessages();
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

        
        private byte[] ReceiveMessages()
        {
            SendMessage("popo");
            Console.WriteLine("Started receiveing");
            byte[] buffer = new byte[TCP_BUFFER_LENGTH];
            int messageLength = stream.Read(buffer, 0, TCP_BUFFER_LENGTH);
            Console.Write($"Got message {Encoding.UTF8.GetString(buffer, 0, messageLength)}");

            byte[] message = new byte[messageLength];
            Array.Copy(buffer, message, messageLength);
            return message;
        }
        

        public void WaitForVideoBroadcastAndPlay(LibVLC libvlc, MediaPlayer mediaPlayer, VideoView videoView)
        {
            string message = Encoding.UTF8.GetString(ReceiveMessages());
            while(! message.Equals(Messages.StartingVideoBroadcast.name()))
            {
                message = Encoding.UTF8.GetString(ReceiveMessages());
            }

            //now expecting video broadcast



            using (FileStream stream = new FileStream("buffer.mp4", FileMode.Create, FileAccess.Write))
            {
                videoBuffer = new byte[BUFFER_LENGTH];
                int bytesRead;
                bool startedVideo = false;

                while((bytesRead = stream.Read(videoBuffer, 0, BUFFER_LENGTH)) > 0)
                {
                    stream.Write(videoBuffer, 0, bytesRead); //writing data to the file
                    stream.Flush();

                    if(!startedVideo && stream.Length > 1024 * 1024) //waiting for 1MB of data before starting the video
                    {
                        InitiateVideo(libvlc, mediaPlayer, videoView);
                    }
                }
            }
        }

        private void InitiateVideo(LibVLC libvlc, MediaPlayer mediaPlayer, VideoView videoView)
        {
            mediaPlayer.EndReached += ((sender, args) =>
            {
                File.Delete("buffer.mp4");
            });

            mediaPlayer.Play(new Media(libvlc, "buffer.mp4", FromType.FromPath));
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
