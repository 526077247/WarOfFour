
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;

namespace service.core
{
    /// <summary>
    /// 客户端
    /// </summary>
    public class ClientUser
    {
        private Socket clientSocket;
        private string clientId;
        private List<byte> msg;
        public string Endpoint { get { if (clientSocket != null) return clientSocket.RemoteEndPoint.ToString();return "" ; } }
        public string ClientId { get { return clientId; } }
        public ClientUser(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            msg = new List<byte>();
            ThreadPool.QueueUserWorkItem(ReciveMsg, clientSocket);
            clientId = Guid.NewGuid().ToString();
        }
        public void Close()
        {
            if (clientSocket != null)
            {
                MainServer.Instance.CloseLink(ClientId);
                clientSocket.Close();
            }
        }

        public void ReciveMsg(object o)
        {
            Socket cSocket = o as Socket;
            while (true&& cSocket != null)
            {
                byte[] bs = new byte[5120];
                int count;
                try
                {
                    count = cSocket.Receive(bs);
                }
                catch (Exception ex)
                {
                    Close();
                    return;
                }
                if (count == 0) break;
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        msg.Add(bs[i]);
                    }
                    byte[] real = StickyPackageHelper.decode(msg);
                    if (real != null)
                    {
                        SocketDataObject obj = DataUtils.BytesToObject<SocketDataObject>(real);
                        obj.ClientId = clientId;
                        MainServer.Instance.AddHandleEvt(obj);
                    }
                }
            }

        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bs">要发送的信息</param>
        /// <returns>The number of bytes send to the socket</returns>
        public int SendMsg(byte[] bs)
        {
            try
            {
                return clientSocket.Send(bs);
            }
            catch(Exception ex)
            {
                Close();
                return 0;
            }
        }
    }
}
