using Service.SocketCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public interface IMatchingGameSvr:IAppServiceBase
    {
        /// <summary>
        /// 开始匹配
        /// </summary>
        /// <param name="token"></param>
        [PublishMethod]
        void StartMatching(string token);
        /// <summary>
        /// 取消匹配
        /// </summary>
        /// <param name="token"></param>
        [PublishMethod]
        void EndMatching(string token);
        
        
    }
}
