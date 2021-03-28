using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Service.SocketCore
{
    internal class ReciveService
    {
        public ReciveService(ConcurrentDictionary<string, ClientUser>  _cSockets)
        {
            ThreadPool.QueueUserWorkItem(ReciveMsg, _cSockets);
        }




        public void ReciveMsg(object o)
        {
            var cSockets = o as ConcurrentDictionary<string, ClientUser>;
            while (true)
            {
                Thread.Sleep(1);
                var keys = cSockets.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    try
                    {
                        cSockets[keys[i]].ReciveMsg();
                    }
                    catch(Exception ex)
                    {
                        MainServer.Instance.RemoveClient(keys[i],out _);
                    }
                }
            }
        }

    }
}
