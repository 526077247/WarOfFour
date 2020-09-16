using IBatisNet.DataAccess;
using Service.SocketCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WarOfFour.Service
{
    /// <summary>
    /// 游戏匹配服务
    /// </summary>
    public class MatchingGameSvr:AppServiceBase,IMatchingGameSvr
    {
        #region 服务描述:游戏匹配服务
        List<string> matchQueue;
        IGameMgeSvr _GameMgeSvr;
        IAuthSvr _AuthSvr;
        IGameCallBack _GameCallBack;
        readonly object matchQueueLock = new object();
        
        public MatchingGameSvr()
        {
            matchQueue = new List<string>();
            
            _GameMgeSvr= ServiceManager.GetService<IGameMgeSvr>("GameMgeSvr");
            _GameCallBack = ServiceManager.GetService<IGameCallBack>("GameCallBack");
            _AuthSvr= ServiceManager.GetService<IAuthSvr>("AuthSvr");
            ThreadPool.QueueUserWorkItem(MatchGameThread);
        }

        #endregion

        #region IMatchingGameSvr函数

        /// <summary>
        /// 开始匹配
        /// </summary>
        /// <param name="token"></param>
        public void StartMatching(string token)
        {
            string userId = _AuthSvr.GetUserName(token);
            if (string.IsNullOrEmpty(userId))
                return;
            if (!matchQueue.Contains(token)&& _GameMgeSvr.GetGame(_AuthSvr.GetUserName(token))==null)
            {
                lock(matchQueueLock)
                {
                    Console.WriteLine("StartMatching:"+token);
                    matchQueue.Add(token);
                }
               
            }
        }
        /// <summary>
        /// 结束匹配
        /// </summary>
        /// <param name="token"></param>
        public void EndMatching(string token)
        {
            if (matchQueue.Contains(token))
            {
                lock (matchQueueLock)
                {
                    Console.WriteLine("EndMatching:" + token);
                    matchQueue.Remove(token);
                }
            }
        }

        
        
        #endregion


        #region 私有方法
        /// <summary>
        /// 匹配线程
        /// </summary>
        /// <param name="o"></param>
        private void MatchGameThread(object o)
        {
            while (true)
            {
                
                try
                {
                    if (matchQueue.Count >= 2 && _GameMgeSvr.GetGameCount() < 50)
                    {
                        List<string> tokens = matchQueue.Take(2).ToList();
                        Game game = MatchSuccess(tokens);
                        if (game != null)
                        {
                            _GameCallBack.MatchGame(tokens, game);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        /// <summary>
        /// 匹配成功
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        private Game MatchSuccess(List<string> tokens)
        {
            Game game = new Game();
            for (int i = 0; i<tokens.Count;i++)
            {
                string userId = _AuthSvr.GetUserName(tokens[i]);
                if (!string.IsNullOrEmpty(userId))
                {    
                    game.JoinPlayer(tokens[i], userId , i);
                }
                else
                {
                    game = null;
                    tokens.RemoveAt(i);
                    break;
                }
            }
            if (game == null)
            {
                lock (matchQueueLock)
                {
                    matchQueue.AddRange(tokens);
                }
            }
            else
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    matchQueue.Remove(tokens[i]);
                }
                _GameMgeSvr.AddGame(tokens, game);
            }
            return game;
        }
        
        #endregion

    }
}
