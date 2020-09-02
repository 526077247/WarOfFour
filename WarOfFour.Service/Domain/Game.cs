using Newtonsoft.Json;
using service.core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace WarOfFour.Service
{
    public enum TYPE_OF_Game
    {
        Preparing,
        Underway,
        Over

    }
    public class Game : DataObject
    {
        #region 基础属性
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 当前状态
        /// </summary>
        public TYPE_OF_Game State { get; private set; }
        /// <summary>
        /// 东队
        /// </summary>
        public DataList<Player> East { get; set; }
        /// <summary>
        /// 南队
        /// </summary>
        public DataList<Player> South { get; set; }
        /// <summary>
        /// 西队
        /// </summary>
        public DataList<Player> West { get; set; }
        /// <summary>
        /// 北队
        /// </summary>
        public DataList<Player> North { get; set; }
        /// <summary>
        /// 怪物
        /// </summary>
        public DataList<Monster> Monsters { get; set; }
        /// <summary>
        /// 胜利者
        /// </summary>
        public int Winner { get; set; }
        #endregion

        #region 扩展属性

        [JsonIgnore]
        Timer time;
        [JsonIgnore]
        private List<string> tokens;
        [JsonIgnore]
        private List<string> eastTokens;
        [JsonIgnore]
        private List<string> southTokens;
        [JsonIgnore]
        private List<string> westTokens;
        [JsonIgnore]
        private List<string> northTokens;
        /// <summary>
        /// 取当前对局玩家Token
        /// </summary>
        [JsonIgnore]
        public List<string> Tokens
        {
            get
            {
                if (tokens == null)
                {
                    tokens = new List<string>();
                    tokens.AddRange(EastTokens);
                    tokens.AddRange(SouthTokens);
                    tokens.AddRange(WestTokens);
                    tokens.AddRange(NorthTokens);
                }
                return tokens;
            }
        }
        /// <summary>
        /// 取当前对局East玩家Token
        /// </summary>
        [JsonIgnore]
        public List<string> EastTokens
        {
            get
            {
                if (eastTokens == null)
                {
                    eastTokens = new List<string>();
                    foreach (var item in East)
                    {
                        eastTokens.Add(item.Token);
                    }
                }
                return eastTokens;
            }
        }
        /// <summary>
        /// 取当前对局South玩家Token
        /// </summary>
        [JsonIgnore]
        public List<string> SouthTokens
        {
            get
            {
                if (southTokens == null)
                {
                    southTokens = new List<string>();
                    foreach (var item in South)
                    {
                        southTokens.Add(item.Token);
                    }
                }
                return southTokens;
            }
        }
        /// <summary>
        /// 取当前对局West玩家Token
        /// </summary>
        [JsonIgnore]
        public List<string> WestTokens
        {
            get
            {
                if (westTokens == null)
                {
                    westTokens = new List<string>();
                    foreach (var item in West)
                    {
                        westTokens.Add(item.Token);
                    }
                }
                return westTokens;
            }
        }
        /// <summary>
        /// 取当前对局North玩家Token
        /// </summary>
        [JsonIgnore]
        public List<string> NorthTokens
        {
            get
            {
                if (northTokens == null)
                {
                    northTokens = new List<string>();
                    foreach (var item in North)
                    {
                        northTokens.Add(item.Token);
                    }
                }
                return northTokens;
            }
        }
        [JsonIgnore]
        public List<Player> Players { 
            get
            {
                List<Player> res = new List<Player>();
                res.AddRange(East);
                res.AddRange(South);
                res.AddRange(West);
                res.AddRange(North);
                return res;
            }
        }
        [JsonIgnore]
        public int[] ALiveCount
        {
            get
            {
                int[] res = new int[4];
                for (int i = 0; i < East.Count; i++)
                {
                    if (East[i].State == TYPE_OF_PLAYER.Alive) res[0]++;
                }
                for (int i = 0; i < South.Count; i++)
                {
                    if (South[i].State == TYPE_OF_PLAYER.Alive) res[1]++;
                }
                for (int i = 0; i < West.Count; i++)
                {
                    if (West[i].State == TYPE_OF_PLAYER.Alive) res[2]++;
                }
                for (int i = 0; i < North.Count; i++)
                {
                    if (North[i].State == TYPE_OF_PLAYER.Alive) res[3]++;
                }
                return res;
            }
        }
        #endregion

        public Game()
        {
            Id = Guid.NewGuid().ToString();
            StartTime = DateTime.Now.ToFormatString();
            State = TYPE_OF_Game.Preparing;
            East = new DataList<Player>();
            South = new DataList<Player>();
            West = new DataList<Player>();
            North = new DataList<Player>();
            Monsters = new DataList<Monster>();
            
        }
        public void Start(int Interval, ElapsedEventHandler handler, Action overCallBack)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (!Players[i].IsReady)
                {
                    return;
                }
            } 
            time = new Timer();
            time.Interval = Interval;
            // 定义回调
            time.Elapsed += handler;
            time.Elapsed += (sender,e)=> {
                int[] vs = ALiveCount;
                int count = 0;
                foreach (var item in vs)
                {
                    if (item == 0)
                    {
                        count++;
                    }
                }
                if (count >= 3)
                {
                    overCallBack();
                }
            };
            // 定义多次循环
            time.AutoReset = true;
            State = TYPE_OF_Game.Underway;
            // 允许Timer执行
            time.Enabled = true;
        }

        public void Over()
        {
            if (State != TYPE_OF_Game.Underway)
            {
                return;
            }
            time.Enabled = false;
            State = TYPE_OF_Game.Over;
            EndTime = DateTime.Now.ToFormatString();
            for (int i=0;i<ALiveCount.Length;i++)
            {
                if (ALiveCount[i] > 0)
                {
                    Winner = i + 1;
                    return;
                }
            }
        }

        public void JoinPlayer(string token,string userId,int index=-1)
        {
            if (index < 0)
                index = Players.Count;
            Player player = new Player { Token = token, UserId = userId };
            if (index % 4 == 0)
                East.Add(player);
            if (index % 4 == 1)
                North.Add(player);
            if (index % 4 == 2)
                West.Add(player);
            if (index % 4 == 3)
                South.Add(player);
        }

    }
}
