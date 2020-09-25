using System;
using System.Collections.Generic;
using System.Text;
using Service.SocketCore;
namespace WarOfFour.Service
{
    public delegate void ChangeUserToken(string userName, string newToken);
    public class AuthSvr : AppServiceBase, IAuthSvr
    {
        ILoginMgeSvr _LoginMgeSvr;
        ILoginCallBack _LoginCallBack;
        Dictionary<string, string> userToken;
        Dictionary<string, string> tokenUser;
        event ChangeUserToken changeUserTokenEvt;
        public AuthSvr()
        {
            userToken = new Dictionary<string, string>();
            tokenUser = new Dictionary<string, string>();
            _LoginMgeSvr = ServiceManager.GetService<ILoginMgeSvr>("LoginMgeSvr");
            _LoginCallBack = ServiceManager.GetService<ILoginCallBack>("LoginCallBack");
            MainServer.Instance.LoginOutEvt = LoginOut;
        }


        public void LoginIn(string token, string userName, string psw)
        {
            var res = _LoginMgeSvr.Login(userName, psw);
            if (string.IsNullOrEmpty(res.Token))
            {
                _LoginCallBack.LoginFail(token, "用户名密码不匹配");
            }
            else
            {
                if (tokenUser.ContainsKey(token))
                {
                    _LoginCallBack.LoginSuccess(token, token);
                    return;
                }
                tokenUser.Add(token, userName);
                changeUserTokenEvt?.Invoke(userName, token);
                if (userToken.ContainsKey(userName))//若已存在
                {
                    _LoginCallBack.CloseLink(userToken[userName], "账号在其他地方登陆");
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
                _LoginCallBack.LoginSuccess(token, token);
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

        public string GetUserId(string token)
        {
            if (tokenUser.TryGetValue(token, out string res))
            {
                return res;
            }
            _LoginCallBack.CloseLink(token, "与服务器断开连接");
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


        /// <summary>
        /// 在线人数
        /// </summary>
        public int OnlineCount()
        {
            return tokenUser.Count;
        }
    }
}
