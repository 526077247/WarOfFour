using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Client.Core
{
    public class ProxyClientService : IProxyService
    {

        public ProxyClientService()
        {


        }

        public T GetService<T>(string serviceName)
        {
            T obj = DynClientServerFactory.CreateServer<T>(serviceName);
            return obj;
        }

    }
}
