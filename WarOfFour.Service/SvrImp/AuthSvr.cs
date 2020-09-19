using System;
using System.Collections.Generic;
using System.Text;
using Service.SocketCore;
namespace WarOfFour.Service
{
    public delegate void ChangeUserToken(string userName, string newToken);
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
            MainServer.Instance.LoginOutEvt = LoginOut;
        }


        public void LoginIn(string token, string userName, string psw)
        {
            var res = LoginMgeSvr.Login(userName, psw);
            _logger.Debug("userName" + userName);
            if (string.IsNullOrEmpty(res.Token))
            {
                LoginCallBack.LoginFail(new List<string>() { token }, "用户名密码不匹配");
            }
            else
            {
                if (tokenUser.ContainsKey(token))
                {
                    LoginCallBack.LoginSuccess(new List<string>() { token }, token);
                    return;
                }
                tokenUser.Add(token, userName);
                changeUserTokenEvt?.Invoke(userName, token);
                if (userToken.ContainsKey(userName))//若已存在
                {
                    LoginCallBack.CloseLink(new List<string>() { userToken[userName] }, "账号在其他地方登陆");
                    if (userToken.ContainsKey(userName))//再次确认是否在线
                    {
                        tokenUser.Remove(userToken[userName]);
                        userToken[userName] = token;
                    }
                    else
                    {
                        userToken.Add(userName, token);
                    }
                }
                else
                {
                    userToken.Add(userName, token);
                }
                LoginCallBack.LoginSuccess(new List<string>() { token },token);
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
                string userID = tokenUser[token];
                _logger.Debug("LoginOut:" + userID);
                userToken.Remove(userID);
                tokenUser.Remove(token);
                //ServiceManager.GetService<IGameMgeSvr>().BrokenWire(userID);
            }
        }

        public string GetUserName(string token)
        {
            if (tokenUser.TryGetValue(token, out string res))
            {
                return res;
            }
            LoginCallBack.CloseLink(new List<string>() { token }, "与服务器断开连接");
            return null;
        }
        public string GetUserToken(string userName)
        {
            if (userToken.TryGetValue(userName, out string res))
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
