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
using LibVLCSharp.Shared;


namespace Video_Share_Project
{
    public partial class Form1 : Form
    {
        int x = 0;
        public LibVLC libvlc;
        public MediaPlayer mediaPlayer;
        public bool fullscreen = false;
        public bool playing = false;
        public const int BUFFER_LENGTH = 512 * 1024; //512KB
        private byte[] buffer;
        private Server server;

        private Media currentMedia;



        public Form1()
        {
            InitializeComponent();
            Core.Initialize(); //initializes libVLC package


            libvlc = new LibVLC();
            mediaPlayer = new MediaPlayer(libvlc);
            videoView.MediaPlayer = mediaPlayer;

            mediaPlayer.EndReached += EndReached;

            mediaPlayer.EncounteredError += (s, e) =>
            {
                Console.WriteLine("VLC encountered an error!");
            };

            currentMedia = new Media(libvlc, "C:\\Users\\mdond\\Downloads\\zerotofive.mp4", FromType.FromPath);
            mediaPlayer.Play(currentMedia);
            //mediaPlayer.Play(new Media(libvlc, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
        }

        private void EndReached(object sender, EventArgs e)
        {

            Console.WriteLine("END REACHED");
            Console.WriteLine($"InvokeRequired in EndReached: {InvokeRequired}");
            if(InvokeRequired)
            {
                Invoke(new Action(async () => await PlayNextSegment()));
            }
            
        }

        private async Task PlayNextSegment()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("skib");
                Console.WriteLine($"InvokeRequired in PlayNextSegment: {InvokeRequired}");
                currentMedia = new Media(libvlc, "C:\\Users\\mdond\\Downloads\\fivetoten.mp4", FromType.FromPath);
                try
                {
                    Console.WriteLine("dsa");
                    mediaPlayer.Media = currentMedia;
                    Console.WriteLine("sdf");
                    mediaPlayer.Play(currentMedia);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("idi");
            });
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
            Client c = new Client();
            Thread.Sleep(100);
            c.WaitForVideoBroadcastAndPlay(libvlc, mediaPlayer, videoView);
            
        }




        private void Play_Click(object sender, EventArgs e) //only server must use this function
        {
            using (FileStream stream = new FileStream("C:\\Users\\mdond\\Downloads\\sd.mp4", FileMode.Open, FileAccess.Read))
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
            }
        }
    }
}
