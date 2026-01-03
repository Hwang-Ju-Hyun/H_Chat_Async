using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatCore_Common;

namespace H_Chat_Async_Server
{
    public partial class Server_Async : Form
    {
        private TcpListener server;
        private NetworkStream stream;
        private string ip_addr = "172.18.171.66";
        private int port = 5001;
        private bool isRunning = false;
        ChatCore_Common.ChatCore_Async server_chatCore;
        List<ChatCore_Common.ChatCore_Async> clients = new List<ChatCore_Common.ChatCore_Async>();        
        public Server_Async()
        {            
            InitializeComponent();
        }
        private void AllClose()
        {
            isRunning = false;
            foreach(ChatCore_Common.ChatCore_Async client in clients)
            {
                client.Close();
            }
            server.Stop();
        }
        private void Server_Async_Load(object sender, EventArgs e)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                IPHostEntry host = Dns.GetHostEntry(ip_addr);
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        textBox1.Text = ip.ToString();
                        server_chatCore = new ChatCore_Async(null);
                        server_chatCore.IP = new IPEndPoint(IPAddress.Parse(ip.ToString()), port);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DisoconnectClient(ChatCore_Async client)
        {            
            this.Invoke(new Action(() =>
            {
                string target = client.ToString();
                List_ConnectedIP.Items.Remove(target);
                clients.Remove(client);
                MessageBox.Show("클라 나감");
            }));            
        }

        private async void OnClientMessage(ChatCore_Async sender,string msg)
        {
            string senderInfo = sender.ToString(); // IP:PORT

            string[] str_arr = msg.Split('|');
            string from = str_arr[0];
            string content = str_arr[1];


            this.Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"{from} : {content}" + "\n");
            }));

            await BroadCastingAsync(sender, $"{from}|{content}");
        }
        private void BTN_ServerStart_Click(object sender, EventArgs e)
        {
            isRunning = true;            
            MessageBox.Show("서버 활성화");
            AcceptLoopAsync();
        }

        private async Task AcceptLoopAsync()
        {
            while(isRunning)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                ChatCore_Async core = new ChatCore_Async(client);
                
                clients.Add(core);                             

                IPEndPoint client_Remote = (IPEndPoint)client.Client.RemoteEndPoint;//클라 ip
                IPEndPoint client_Local = (IPEndPoint)client.Client.LocalEndPoint;//서버 ip
                this.Invoke(new Action(() =>
                {
                    core.IP = client_Remote;
                    List_ConnectedIP.Items.Add(client_Remote.ToString());
                }));

                core.MessageReceived +=OnClientMessage;                

                core.Disconnected += DisoconnectClient;

                //await ReceiveLoopAsync(client); 왜 안되는지 설명할 줄 알아야함
                //ReceiveLoopAsync(client);
                core.StartReceive();
            }
        }

       
        private async Task BroadCastingAsync(ChatCore_Async server, string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);

            foreach(var client in clients)
            {
                try
                {
                    await client.Send_Async(msg);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void BTN_Send_Click(object sender, EventArgs e)
        {
            
            richTextBox1.AppendText($"{(server_chatCore).IP.ToString()} : { InputBox.Text}\n");
            BroadCastingAsync(server_chatCore, (server_chatCore).IP.ToString()+"|"+InputBox.Text);
            this.Invoke(new Action(() =>
            {
                InputBox.Clear();
            }));
        }

        private void Server_Async_FormClosing(object sender, FormClosingEventArgs e)
        {
            AllClose();
        }
    }
}