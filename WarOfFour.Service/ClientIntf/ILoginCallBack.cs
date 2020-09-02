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
        /// <param name="tokens"></param>
        void LoginSuccess(List<string> tokens);
        /// <summary>
        /// 登录失败
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="msg"></param>
        void LoginFail(List<string> tokens,string msg);
        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="msg"></param>
        void CloseLink(List<string> tokens,string msg);
    }
}
