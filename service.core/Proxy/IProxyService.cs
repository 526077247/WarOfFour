using System;
using System.Collections.Generic;
using System.Text;

namespace Service.SocketCore
{
    internal interface IProxyService
    {
        public object GetService();
        public T GetService<T>();
    }
}
