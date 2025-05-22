using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFMpegCore;
using LibVLCSharp.Shared;
using FFMpegCore.Enums;
using FFMpegCore.Arguments;
using System.Diagnostics;

namespace Video_Share_Project
{
    public partial class Form1 : Form
    {
        int x = 0;
        
        public const int BUFFER_LENGTH = 512 * 1024; //512KB
        private byte[] buffer;
        private Server server = null;
        private Video video;
        



        public Form1()
        {
            InitializeComponent();
            Core.Initialize(); //initializes libVLC package

            video = new Video(videoView);

            string ffmpegCommand =
                "ffmpeg -i C:\\Users\\mdond\\Downloads\\sd.mp4 -t 5 -c:v libx264 -c:a aac -y testingfunc3.mp4";

            Console.WriteLine("starting ffmpeg");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + ffmpegCommand,
                //RedirectStandardOutput = true,
                //RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();

            }
            Console.WriteLine("finished ffmpeg");

        }

        



        private void Form1_Load(object sender, EventArgs e)
        {

        }




        private void serverButton_Click(object sender, EventArgs e)
        {
            
            server = new Server(serverButton);

            server.sendMessage(textBox1.Text);
            EventHandler<string> onGotMessageChangeTextBox = (messageSender, text) =>
            {
                //method is called on the handleClient thread, so InvokeRequired is required
                if (textBox1.InvokeRequired)
                {
                    textBox1.Invoke(new Action<string>(t => textBox1.Text = (t + (++x).ToString())), args: text);
                    //.Invoke: on the control's thread...
                    //Action is a way of writing a method with no return type
                }

            };

            server.GotMessageFromClient += onGotMessageChangeTextBox;
        }




        private void clientButton_Click(object sender, EventArgs e)
        {
            Client client = new Client();
            Thread.Sleep(100); //Required, to enable initialization to end in time
            Task.Run(() => client.WaitForVideoBroadcastMessage(video));
        }




        private void Play_Click(object sender, EventArgs e) //only server must use this function
        {
            if(server ==  null)
            {
                return;
            }

            video.PlayVideo();
            server.sendMessage(Messages.StartingVideoBroadcast.name());
            server.SendSegment(video, "C:\\Users\\mdond\\Downloads\\sd.mp4");

            /*using (FileStream stream = new FileStream("C:\\Users\\mdond\\Downloads\\sd.mp4", FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[BUFFER_LENGTH];
                int bytesRead;

                while((bytesRead = stream.Read(buffer, 0, BUFFER_LENGTH)) > 0)
                {
                    byte[] message = new byte[bytesRead];
                    Array.Copy(buffer, 0, message, 0, bytesRead);
                    server.sendMessage(message);
                    Thread.Sleep(100); //TODO: FIX THIS!!!!!!!! THIS IS IN THE UI THREAD!!!!!!!!!!!!!!!!!!!!!!!
                }
            }*/
        }
    }
}
