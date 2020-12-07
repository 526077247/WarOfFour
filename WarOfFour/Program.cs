using Service.SocketCore;
using System;
using System.Threading;
using WarOfFour.Service;
using System.Collections;
using System.Net;

namespace WarOfFour
{
    class Program
    {
        static void Main(string[] args)
        {
            MainServer.Build(2346).Start();
            Console.ReadKey();
        }
    }
}
