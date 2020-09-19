using IBatisNet.Common.Utilities.Objects.Members;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public enum TYPE_OF_PLAYER
    {
        Alive,Dead
    }
    public class Player: GameObject
    {
        private int hp;
        [JsonIgnore]
        public string Token { get;set;}
        public bool IsReady { get; set; }//是否准备游戏
        public string UserId { get; set; }//用户账号
        public Vector3 Speed { get; set; }//速度
        public int MaxHP { get; set; }//最大生命
        public Transform Aim { get; set; }//准心位置
        public int HP
        {
            get { return hp; }
            set
            {
                hp = value;
                if (hp <= 0)
                {
                    hp = 0;
                    State = TYPE_OF_PLAYER.Dead;
                }
            }
        }//当前生命
        public int Aggressive { get; set; }//攻击力
        public TYPE_OF_PLAYER State { get; set; }//当前状态
        public Player()
        {
            IsReady = false;
            State = TYPE_OF_PLAYER.Alive;
            MaxHP = 100;
            HP = 100;
        }


    }
}
