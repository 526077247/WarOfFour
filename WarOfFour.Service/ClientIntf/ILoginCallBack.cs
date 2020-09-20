using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public interface ILoginCallBack
    {
        /// <summary>
        /// 登录成功
        /// </summary>
        /// <param name="token"></param>
        void LoginSuccess(string token, string userToken);
        /// <summary>
        /// 登录失败
        /// </summary>
        /// <param name="token"></param>
        /// <param name="msg"></param>
        void LoginFail(string token, string msg);
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="token"></param>
        /// <param name="msg"></param>
        void CloseLink(string token, string msg);
    }
}
