using service.core;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public interface IGameMgeSvr
    {
        /// <summary>
        /// 准备开始
        /// </summary>
        /// <param name="token"></param>
        [PublishMethod]
        void ReadyGame(string token);
        /// <summary>
        /// 取消对局
        /// </summary>
        /// <param name="token"></param>
        [PublishMethod]
        void CancelGame(string token);
        /// <summary>
        /// 同步玩家信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="player"></param>
        [PublishMethod]
        void SetPlayerInfo(string token, Player player);

        /// <summary>
        /// 玩家攻击中怪物
        /// </summary>
        /// <param name="token"></param>
        /// <param name="monsterId"></param>
        [PublishMethod]
        void AttackMonster(string token, string monsterId);

        /// <summary>
        /// 怪物攻击中玩家
        /// </summary>
        /// <param name="token"></param>
        /// <param name="monsterId"></param>
        [PublishMethod]
        void MonsterAttackPlayer(string token, string monsterId );

        /// <summary>
        /// 玩家攻击中玩家
        /// </summary>
        /// <param name="token"></param>
        /// <param name="playerId"></param>
        [PublishMethod]
        void AttackPlayer(string token, string playerId);

        /// <summary>
        /// 玩家开始播放技能动画
        /// </summary>
        /// <param name="token"></param>
        /// <param name="type"></param>
        [PublishMethod]
        void PlaySkillAnimation(string token, int type);
        /// <summary>
        /// 获取账号当前对局信息
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Game GetGame(string userName);
        /// <summary>
        /// 获取进行中的对局数量
        /// </summary>
        /// <returns></returns>
        int GetGameCount();

        /// <summary>
        /// 添加对局
        /// </summary>
        void AddGame(List<string> tokens, Game game);
    }
}
