using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

        







        public Form1()
        {
            InitializeComponent();
            Core.Initialize(); //initializes libVLC package

            this.KeyPreview = true;
            //this.KeyDown += new KeyEventHandler(ShortcutEvent);
            libvlc = new LibVLC();
            mediaPlayer = new MediaPlayer(libvlc);
            videoView.MediaPlayer = mediaPlayer;
            mediaPlayer.Play(new Media(libvlc, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            
            Server server = new Server(serverButton);
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
        }


    }
}
