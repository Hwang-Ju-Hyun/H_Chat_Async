using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatCore_Common;

namespace H_Chat_Async_Client
{
    public partial class Client_Async : Form
    {
        ChatCore_Common.ChatCore_Async core;
        public Client_Async()
        {
            InitializeComponent();
        }

        private void Client_Async_Load(object sender, EventArgs e)
        {            
        }

        private void BTN_ConnectServer_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient(textBox1.Text,5001);
            core = new ChatCore_Common.ChatCore_Async(client);
            core.MessageReceived += OnMessageReceived;

            IPEndPoint iPLocalEndPoint = (IPEndPoint)client.Client.LocalEndPoint;
            textBox2.Text= iPLocalEndPoint.ToString();
            core.IP = iPLocalEndPoint;


            core.StartReceive();
        }

        private void OnMessageReceived(ChatCore_Async sender,string message)
        {                                    
            string[] msg = message.Split('|');
            string from = msg[0];
            string content = msg[1];

            this.Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"{from} : {content}"+"\n");
            }));
        }

        private async void BTN_Send_Click(object sender, EventArgs e)
        {
            await core.Send_Async($"{core.IP.ToString()}|{InputBox.Text}");
            //richTextBox1.AppendText(core.IP+" : "+InputBox.Text+"\n");
            InputBox.Clear();
        }
    }
}
