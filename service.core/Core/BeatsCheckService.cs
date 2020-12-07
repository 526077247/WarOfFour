using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Service.SocketCore
{
    internal class BeatsCheckService
    {
        public BeatsCheckService(ConcurrentDictionary<string, ClientUser> _cSockets)
        {
            ThreadPool.QueueUserWorkItem(SendBeats, _cSockets);
        }

        public void SendBeats(object o)
        {
            var cSockets = o as ConcurrentDictionary<string, ClientUser>;
            byte[] buffer = DataUtils.ObjectToBytes(new SocketDataObject());
            byte[] bs = StickyPackageHelper.encode(buffer);
            while (true)
            {
                Thread.Sleep(5000);
                var keys = cSockets.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    cSockets[keys[i]].SendMsg(bs);
                }
            }
        }
    }
}
