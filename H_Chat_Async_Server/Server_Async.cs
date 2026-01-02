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
        private string ip_addr = "192.168.45.195";
        private int port = 5000;
        private bool isRunning = false;
        List<ChatCore_Common.ChatCore_Async> clients = new List<ChatCore_Common.ChatCore_Async>();        
        public Server_Async()
        {
            InitializeComponent();
        }

        private void Server_Async_Load(object sender, EventArgs e)
        {
            server=new TcpListener(IPAddress.Any, port);
            server.Start();
            IPHostEntry host = Dns.GetHostEntry(ip_addr);
            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily==AddressFamily.InterNetwork)
                {
                    textBox1.Text = ip.ToString();
                    break;
                }
            }
        }

        private void DisoconnectClient(ChatCore_Async client)
        {            
            this.Invoke(new Action(() =>
            {
                MessageBox.Show("클라 나감");
                string target = client.ToString();
                for(int i=0;i<clients.Count;i++)
                {
                    if (clients[i].ToString()== target)
                    {
                        List_ConnectedIP.Items.Remove(target);
                        clients.Remove(client);
                    }                        
                }                
            }));            
        }

        private async void OnClientMessage(ChatCore_Async sender,string msg)
        {
            this.Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"{msg}" + "\n");
            }));            
            
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

                IPEndPoint remote = (IPEndPoint)client.Client.RemoteEndPoint;

                this.Invoke(new Action(() =>
                {
                    core.IP = remote;                    
                    List_ConnectedIP.Items.Add(remote.ToString());
                }));

                core.MessageReceived += (msg) =>
                {
                    OnClientMessage(core,msg);
                };

                core.Disconnected += DisoconnectClient;

                //await ReceiveLoopAsync(client); 왜 안되는지 설명할 줄 알아야함
                //ReceiveLoopAsync(client);
                core.StartReceive();
            }
        }

        //private async Task ReceiveLoopAsync(TcpClient client)
        //{
        //    NetworkStream stream = client.GetStream();
        //    byte[] buffer = new byte[4096];
        //    try
        //    {
        //        while(true)
        //        {
        //
        //
        //            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
        //
        //            if (bytes == 0)
        //                break;
        //
        //            string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
        //
        //            IPEndPoint remote = (IPEndPoint)client.Client.RemoteEndPoint;
        //
        //            this.Invoke(new Action(() =>
        //            {
        //                richTextBox1.AppendText($"{remote} :{msg}\n");
        //            }));
        //
        //            await BroadCastingAsync(client, msg);
        //        }                
        //    }
        //    catch(Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

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
            richTextBox1.AppendText(InputBox.Text +"\n");
            BroadCastingAsync(null,InputBox.Text);
            this.Invoke(new Action(() =>
            {
                InputBox.Clear();
            }));
        }
    }
}