using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Video_Share_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            Server server = new Server();
            EventHandler<string> onGotMessageChangeTextBox = (messageSender, text) =>
            {
                //method is called on the handleClient thread, so InvokeRequired is required
                if (textBox1.InvokeRequired)
                {
                    textBox1.Invoke(new Action<string>(t => textBox1.Text = t), args:text);
                    //.Invoke: on the control's thread...
                    //Action is a way of writing a method with no return type
                }
                else
                {
                    textBox1.Text = text;
                }
                
            };

            server.GotMessageFromClient += onGotMessageChangeTextBox;
        }

        private void clientButton_Click(object sender, EventArgs e)
        {
            Client c = new Client();
        }

        private void changeTextBoxText()
        {

        }

    }
}
