using IBatisNet.DataAccess;
using Service.SocketCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public class GameMgeSvr:IGameMgeSvr
    {
        #region 服务描述
        List<Game> games;
        IDaoManager DaoManager;
        IGameDao _GameDao;
        IAuthSvr _AuthSvr;
        IGameCallBack _GameCallBack;
        ConcurrentDictionary<string, Game> userGame;
        readonly object userGameLock = new object();

        public GameMgeSvr()
        {
            games = new List<Game>();
            DaoManager = ServiceConfig.GetInstance().DaoManager;
            _GameDao = (IGameDao)DaoManager.GetDao(typeof(IGameDao));
            _GameCallBack = ServiceManager.GetService<IGameCallBack>("GameCallBack");
            _AuthSvr = ServiceManager.GetService<IAuthSvr>("AuthSvr");
            userGame = new ConcurrentDictionary<string, Game>();
            _AuthSvr.AddChangeUserTokenEvent((userName, token) =>
            {
                if (userGame.TryGetValue(userName, out Game res))
                {
                    for (int i = 0; i < res.Players.Count; i++)
                    {
                        if(res.Players[i].UserId== userName)
                        {
                            res.Players[i].Token = token;
                        }
                    }
                }
            });
        }

        #endregion


        #region IGameMgeSvr函数
        /// <summary>
        /// 准备游戏
        /// </summary>
        /// <param name="token"></param>
        public void ReadyGame(string token)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            var Players = game.Players;
            Player player = Players.Find(item => item.Token == token);
            player.IsReady = true;
            game.Start(30,
                (sender, e) => { _GameCallBack.GameSynchronization(game.Tokens, game); },//同步游戏数据到客户端
                () => { GameOver(game); });//游戏结束回调方法
        }
        /// <summary>
        /// 取消对局
        /// </summary>
        /// <param name="token"></param>
        public void CancelGame(string token)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            games.Remove(game);
            var tokens = game.Tokens;
            foreach (var item in tokens)
            {
                userGame.TryRemove(item, out _);
            }
            tokens.Remove(token);
            _GameCallBack.MatchGameFail(tokens);
        }
        /// <summary>
        /// 同步玩家信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="player"></param>
        public void SetPlayerInfo(string token, Player player)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            var Players = game.Players;
            Player realplayer = Players.Find(item => item.Token == token);

            realplayer.transform = player.transform;
            realplayer.Speed = player.Speed;
            
        }
        /// <summary>
        /// 玩家攻击怪物
        /// </summary>
        /// <param name="token"></param>
        /// <param name="monsterId"></param>
        [PublishMethod]
        public void AttackMonster(string token, string monsterId)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            Player player = game.Players.Find(a => a.UserId == userName);
            Monster aim = game.Monsters.Find(a => a.MonsterID == monsterId);
            if (aim != null&& aim.State!=TYPE_OF_MONSTER.Dead)
            {
                if (!aim.AimPlayers.Contains(player))
                    aim.AimPlayers.Enqueue(player);
                aim.HP -= player.Aggressive;
            }
        }
        /// <summary>
        /// 怪物攻击玩家
        /// </summary>
        /// <param name="token"></param>
        /// <param name="monsterId"></param>
        [PublishMethod]
        public void MonsterAttackPlayer(string token, string monsterId)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            Player player = game.Players.Find(a => a.UserId == userName);
            Monster aim = game.Monsters.Find(a => a.MonsterID == monsterId);
            if(aim!=null && aim.State != TYPE_OF_MONSTER.Dead)
            {
                if(aim.AimPlayers.Peek().UserId== userName)
                {
                    player.HP -= aim.Aggressive;
                    if (player.HP <= 0)
                    {
                        aim.AimPlayers.Dequeue();
                    }
                }
            }
        }
        /// <summary>
        /// 玩家攻击玩家
        /// </summary>
        /// <param name="token"></param>
        /// <param name="playerId"></param>
        [PublishMethod]
        public void AttackPlayer(string token, string playerId)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            Player attackPlayer = game.Players.Find(a => a.UserId == userName);
            Player beAttackPlayer = game.Players.Find(a => a.UserId == playerId);
            if(beAttackPlayer!=null&& beAttackPlayer.State!= TYPE_OF_PLAYER.Dead&&attackPlayer.State!= TYPE_OF_PLAYER.Dead)
            {
                beAttackPlayer.HP -= attackPlayer.Aggressive;
            }
        }

        /// <summary>
        /// 玩家播放技能动画
        /// </summary>
        /// <param name="token"></param>
        /// <param name="type"></param>
        [PublishMethod]
        public void PlaySkillAnimation(string token, int type)
        {
            string userName = _AuthSvr.GetUserName(token);
            if (!userGame.ContainsKey(userName))
            {
                return;
            }
            Game game = userGame[userName];
            List<string> tokens = new List<string>();
            tokens.AddRange(game.Tokens);
            tokens.Remove(token);
            _GameCallBack.PlaySkillAnimation(tokens, userName,type);
        }
        /// <summary>
        /// 获取账号当前对局信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Game GetGame(string userName)
        {
            if (userGame.TryGetValue(userName, out Game res))
            {
                return res;
            }
            return null;
        }

        public int GetGameCount()
        {
            return games.Count;
        }
        /// <summary>
        /// 添加对局
        /// </summary>
        public void AddGame(List<string> tokens, Game game)
        {
            Console.WriteLine("AddGame:" + Newtonsoft.Json.JsonConvert.SerializeObject(tokens));
            foreach (var item in tokens)
            {
                lock (userGameLock)
                {
                    if (!userGame.TryAdd(item, game))
                    {
                        userGame[item] = game;
                    }
                }
            }
            games.Add(game);
        }


        #endregion
        #region 私有方法
        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="game"></param>
        private void GameOver(Game game)
        {
            game.Over();
            games.Remove(game);
            foreach (var item in game.Tokens)
            {
                userGame.TryRemove(item, out _);
            }
            _GameCallBack.GameOver(game.Tokens, game);
            _GameDao.Insert(game);
        }


        #endregion
    }
}
