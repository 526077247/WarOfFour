using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace WarOfFour.Service
{
    public enum TYPE_OF_MONSTER{
        Alive,Dead
    }
    public class Monster: GameObject
    {
        private int hp;
        public string MonsterID { get; set; }//怪物唯一编号
        public int MaxHP { get; set; }//最大生命
        public int HP
        {
            get { return hp; }
            set
            {
                hp = value;
                if (hp <= 0)
                {
                    hp = 0;
                    State = TYPE_OF_MONSTER.Dead;
                }
            }
        }//当前生命
        public int Aggressive { get; set; }//攻击力
        public Vector3 Speed { get; set; }//速度
        public Queue<Player> AimPlayers { get; set; }//仇恨值列表
        public TYPE_OF_MONSTER State { get; set; }//怪物状态
        public Monster()
        {
            State = TYPE_OF_MONSTER.Alive;
            AimPlayers = new Queue<Player>();
            MonsterID = Guid.NewGuid().ToString();
        }

    }
}
