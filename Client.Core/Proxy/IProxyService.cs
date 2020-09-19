using System;
using System.Collections.Generic;
using System.Text;

namespace Client.Core
{
    public interface IProxyService
    {

        T GetService<T>(string serviceName);
    }
}
