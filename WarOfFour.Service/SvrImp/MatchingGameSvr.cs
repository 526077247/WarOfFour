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
    public class MatchingGameSvr : AppServiceBase, IMatchingGameSvr
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

            _GameMgeSvr = ServiceManager.GetService<IGameMgeSvr>("GameMgeSvr");
            _GameCallBack = ServiceManager.GetService<IGameCallBack>("GameCallBack");
            _AuthSvr = ServiceManager.GetService<IAuthSvr>("AuthSvr");
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
            string userId = _AuthSvr.GetUserId(token);
            if (string.IsNullOrEmpty(userId))
                return;
            if (!matchQueue.Contains(token) && _GameMgeSvr.GetGame(userId) == null)
            {
                lock (matchQueueLock)
                {
                    _logger.Debug("StartMatching:" + userId);
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
            string userId = _AuthSvr.GetUserId(token);
            if (string.IsNullOrEmpty(userId))
                return;
            if (matchQueue.Contains(token))
            {
                lock (matchQueueLock)
                {
                    _logger.Debug("EndMatching:" + userId);
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
                Thread.Sleep(1);
                try
                {
                    if (matchQueue.Count >= 2 && _GameMgeSvr.GetGameCount() < 50)
                    {
                        _logger.Debug("GameCount:" + _GameMgeSvr.GetGameCount());
                        List<string> tokens = new List<string>();
                        lock (matchQueueLock)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                tokens.Add(matchQueue[0]);
                                matchQueue.RemoveAt(0);
                            }
                        }
                        Game game = MatchSuccess(tokens);
                        if (game != null)
                        {
                            _GameCallBack.MatchGame(tokens, game);
                            System.Timers.Timer timer = new System.Timers.Timer();
                            //timer.Interval = 10 * 1000;
                            //// 定义回调
                            //timer.Elapsed += (s, e) =>
                            //{
                            //    if (game.State == TYPE_OF_Game.Preparing)
                            //    {
                            //        _GameMgeSvr.DestroyGame(game);
                            //        _GameCallBack.MatchGameFail(game.Tokens);
                            //        _logger.Debug("DestroyOutOfTime:" + game.ID);
                            //    }
                            //};
                            //timer.AutoReset = false;
                            //timer.Start();
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
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
            for (int i = 0; i < tokens.Count; i++)
            {
                string userId = _AuthSvr.GetUserId(tokens[i]);
                if (!string.IsNullOrEmpty(userId))
                {
                    //Player player = new Player
                    //{
                    //    UserId = userId,
                    //    Token = tokens[i]
                    //};
                    game.JoinPlayer(tokens[i], userId);
                }
                else
                {
                    game = null;
                    break;
                }
            }
            if (game != null)
                _GameMgeSvr.AddGame(tokens, game);
            else
                _GameCallBack.MatchGameFail(tokens);
            return game;
        }

        #endregion

    }
}
