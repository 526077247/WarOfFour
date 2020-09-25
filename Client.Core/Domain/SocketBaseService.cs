using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
namespace Client.Core
{
    public class SocketBaseService
    {
        string serviceName;
        public SocketBaseService()
        {
            serviceName = GetType().Name;
        }
        protected void SendMsg(string method, string para)
        {
            SocketDataObject socketDataObject = new SocketDataObject();
            socketDataObject.ServiceName = serviceName;
            socketDataObject.MethodName = method;
            socketDataObject.Paras = para;
            socketDataObject.Time = DateTime.Now.ToFormatString();
            socketDataObject.Version = MainClient.version;
            MainClient.Instance.SendMsgToServer(socketDataObject);
        }
       
    }
}
