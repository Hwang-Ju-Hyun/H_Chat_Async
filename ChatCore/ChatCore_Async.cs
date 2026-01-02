using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatCore_Common
{
    public class ChatCore_Async
    {
        private TcpClient client;
        private NetworkStream stream;
        public event Action<string> MessageReceived;

        public delegate void DisconnectedDelegate(ChatCore_Async sender);
        public event DisconnectedDelegate Disconnected;
        IPEndPoint remoteEndPoint;
        public IPEndPoint IP
        {
            get; 
            set; 
        }

        public override string ToString()
        {
            return IP.Address.ToString() + ":" + IP.Port;
        }
        public ChatCore_Async(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();

            remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
        }

        public async Task Send_Async(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            await stream.WriteAsync(data, 0, data.Length);
        }
        
        public void StartReceive()
        {
            ReceiveLoop_Async();
        }

        private async Task ReceiveLoop_Async()
        {
            byte[] buffer = new byte[4096];
            try
            {
                while(true)
                {
                    int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytes == 0)
                        break;

                    string msg = Encoding.UTF8.GetString(buffer,0,bytes);
                    if(MessageReceived!=null)
                    {
                        MessageReceived(msg);
                    }
                }
            }
            catch(Exception ex)
            {
                if(Disconnected!=null)
                {
                    Disconnected(this);
                }
                Console.WriteLine(ex.ToString());                
            }
        }
    }
}