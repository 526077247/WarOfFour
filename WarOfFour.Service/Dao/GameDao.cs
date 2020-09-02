
/* * * * * * * * * * * * * * * * * * * * * * * *
    <!--游戏-->
    <dao interface="WarOfFour.Service.IGameDao, WarOfFour.Service" implementation="WarOfFour.Service.GameDao,WarOfFour.Service"/>
    
    <!--游戏-->
    <sqlMap embedded="WarOfFour.Service.Maps.Game.xml, WarOfFour.Service" />
* * * * * * * * * * * * * * * * * * * * * * * */
using IBatisNet.DataAccess.Interfaces;
using service.core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    /// <summary>
    /// 游戏Idao
    /// </summary>
    public interface IGameDao : IBaseDao, IDao
    {

    }

    /// <summary>
    /// 游戏dao
    /// </summary>
    public class GameDao : BaseDao, IGameDao
    {
        #region IGameDao函数

        #endregion

        #region IBaseDao函数
        /// <summary>
        /// 删除游戏
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int Delete(object obj)
        {
            return Delete(obj, "DeleteGame");
        }

        /// <summary>
        /// 取游戏
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Get(object obj)
        {
            return Get(obj, "GetGame");
        }

        /// <summary>
        /// 条件取游戏
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public object GetByPara(Hashtable para)
        {
            return Get(para, "GetGameByPara");
        }

        /// <summary>
        /// 插入游戏
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Insert(object obj)
        {
            return Insert(obj, "InsertGame");
        }

        /// <summary>
        /// 条件查游戏数量
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public int QueryCount(Hashtable map)
        {
            return QueryCount(map, "QueryGameCount");
        }

        /// <summary>
        /// 条件查游戏列表
        /// </summary>
        /// <param name="map"></param>
        /// <param name="start"></param>
        /// <param name="paseSize"></param>
        /// <returns></returns>
        public IList QueryList(Hashtable map, int start, int paseSize)
        {
            return QueryList(map, "QueryGameList", start, paseSize);
        }

        /// <summary>
        /// 更新游戏
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object Update(object obj)
        {
            return Update(obj, "UpdateGame");
        }

        #endregion
    }
}