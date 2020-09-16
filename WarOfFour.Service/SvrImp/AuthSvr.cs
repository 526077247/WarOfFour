using System;
using System.Collections.Generic;
using System.Text;
using Service.SocketCore;
namespace WarOfFour.Service
{
    public delegate void ChangeUserToken(string userName,string newToken);
    public class AuthSvr : AppServiceBase, IAuthSvr
    {
        ILoginMgeSvr LoginMgeSvr;
        ILoginCallBack LoginCallBack;
        Dictionary<string, string> userToken;
        Dictionary<string, string> tokenUser;
        event ChangeUserToken changeUserTokenEvt;
        public AuthSvr()
        {
            userToken = new Dictionary<string, string>();
            tokenUser = new Dictionary<string, string>();
            LoginMgeSvr = ServiceManager.GetService<ILoginMgeSvr>("LoginMgeSvr");
            LoginCallBack = ServiceManager.GetService<ILoginCallBack>("LoginCallBack");
            
        }


        public void LoginIn(string token, string userName, string psw)
        {
            var res= LoginMgeSvr.Login(userName, psw);
            if (string.IsNullOrEmpty(res.Token))
            {
                LoginCallBack.LoginFail(new List<string>() { token }, "用户名密码不匹配");
            }
            else
            {
                tokenUser.Add(token, userName);
                if (userToken.ContainsKey(userName))
                {
                    LoginCallBack.CloseLink(new List<string>() { userToken[userName] }, "账号在其他地方登陆");
                    tokenUser.Remove(userToken[userName]);
                    userToken[userName] = token;
                    changeUserTokenEvt?.Invoke(userName, token);
                }
                else
                {
                    userToken.Add(userName, token);
                }
                LoginCallBack.LoginSuccess(new List<string>() { token });
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="token"></param>

        public void LoginOut(string token)
        {
            if (tokenUser.ContainsKey(token))
            {
                userToken.Remove(tokenUser[token]);
                tokenUser.Remove(token);
            }
        }

        public string GetUserName(string token)
        {
            if (tokenUser.TryGetValue(token,out string res))
            {
                return res;
            }
            return null;
        }

        /// <summary>
        /// 添加用户改变Token事件
        /// </summary>
        /// <param name="evt"></param>
        public void AddChangeUserTokenEvent(ChangeUserToken evt)
        {
            changeUserTokenEvt += evt;
        }

    }
}
