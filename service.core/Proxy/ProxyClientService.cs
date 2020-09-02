using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace service.core
{
    internal class ProxyClientService : IProxyService
    {
        private string _service;
        private string _serviceName;
        public ProxyClientService(string service,string serviceName)
        {
            _service = service;
            _serviceName = serviceName;

        }

        public object GetService()
        {
            string IntfName = _service.Trim().Split(",")[0];
            string IntfAssembly = _service.Trim().Split(",")[1];
            Type intf = ServiceManager.GetTypeFromAssembly(IntfName, Assembly.Load(IntfAssembly));
            object obj= DynClientServerFactory.CreateServer(_serviceName, intf);
            return obj;
        }
        public T GetService<T>()
        {
            T obj = DynClientServerFactory.CreateServer<T>(_serviceName);
            return obj;
        }

    }
}
