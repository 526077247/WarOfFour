using System;
using System.Collections.Generic;
using System.Text;
using Service.SocketCore;
namespace WarOfFour.Service
{
    public interface IAuthSvr : IAppServiceBase
    {
        /// <summary>
        /// 认证
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userName">用户名</param>
        /// <param name="psw">密码</param>
        [PublishMethod]
        void LoginIn(string token, string userName, string psw);

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="token"></param>
        [PublishMethod]
        void LoginOut(string token);

        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        string GetUserId(string token);

        string GetUserToken(string userName);
        /// <summary>
        /// 添加用户改变Token事件
        /// </summary>
        /// <param name="evt"></param>
        void AddChangeUserTokenEvent(ChangeUserToken evt);
        /// <summary>
        /// 在线人数
        /// </summary>
        int OnlineCount();
    }
}
