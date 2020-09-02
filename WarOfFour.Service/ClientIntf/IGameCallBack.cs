using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public interface IGameCallBack
    {

        /// <summary>
        /// 匹配到游戏
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="gameInfo"></param>
        void MatchGame(List<string> tokens, Game gameInfo);
        /// <summary>
        /// 同步游戏
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="gameInfo"></param>
        void GameSynchronization(List<string> tokens,Game gameInfo);
        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="gameInfo"></param>
        void GameOver(List<string> tokens, Game gameInfo);

        /// <summary>
        /// 玩家播放技能动画
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="playerId"></param>
        /// <param name="type"></param>
        void PlaySkillAnimation(List<string> tokens,string playerId, int type);
    }
}
