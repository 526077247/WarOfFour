﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Service.SocketCore
{
    public interface IAppServiceBase
    {
        /// <summary>
        /// 取版本号
        /// </summary>
        /// <returns></returns>
        [PublishMethod]
        string GetVersion();
    }
}
