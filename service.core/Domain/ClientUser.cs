
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;

namespace Service.SocketCore
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
        public Socket ClientSocket { get { return clientSocket; } }
        public ClientUser(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            msg = new List<byte>();
            clientId = Guid.NewGuid().ToString();
        }
        internal void Close()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
            }
        }

        internal void ReciveMsg()
        {
            byte[] bs = new byte[5120];
            clientSocket.BeginReceive(bs, 0, 5120, SocketFlags.None, ar =>
            {
                try
                {
                    int count = clientSocket.EndReceive(ar);
                    if (count == 0) return;
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            msg.Add(bs[i]);
                        }
                        byte[] real;
                        do
                        {
                            real = StickyPackageHelper.decode(msg);
                            if (real != null)
                            {
                                SocketDataObject obj = DataUtils.BytesToObject<SocketDataObject>(real);
                                Console.WriteLine("ReciveMsgFromClient:" + clientId + " \n" + obj.ServiceName + "." + obj.MethodName + "?" + obj.Paras);
                                obj.ClientId = clientId;
                                MainServer.Instance.InvokeHandle(obj);
                            }
                        } while (real != null);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetLog("Client").Error("Client" + clientId + "Error:" + ex.ToString());
                    MainServer.Instance.RemoveClient(ClientId, out _);
                }
                ReciveMsg();
            }, null);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bs">要发送的信息</param>
        /// <returns>The number of bytes send to the socket</returns>
        internal int SendMsg(byte[] bs)
        {
            try
            {
                return clientSocket.Send(bs);
            }
            catch(Exception ex)
            {
                LogManager.GetLog("Client").Error("Client" + clientId + "Error:" + ex.ToString());
                throw ex;
            }
        }
    }
}
