﻿using service.core;
using System;
using System.Threading;
using WarOfFour.Service;
using System.Collections;
namespace WarOfFour
{
    class Program
    {
        static void Main(string[] args)
        {
            MainServer.Build("172.16.150.142", 2345).Start();
            //IGameDao gameDao = (IGameDao)ServiceConfig.GetInstance().DaoManager.GetDao(typeof(IGameDao));
            //Console.WriteLine( gameDao.QueryCount(new Hashtable()));
            //Game game = new Game();
            //Player player = new Player { Token = "1" };

            //game.North.Add(player);
            //player.IsReady = true;
            //Console.WriteLine(game.JsonText);
            //game.Start(30, (sender, e) => { Console.WriteLine(123); });
            //Thread.Sleep(100);
            //game.Over();
            //Console.WriteLine(game.JsonText);
            //gameDao.Insert(game);
            //Console.WriteLine(gameDao.QueryCount(new Hashtable()));
            Console.ReadKey();
        }
    }
}
