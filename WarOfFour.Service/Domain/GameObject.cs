using Service.SocketCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public class GameObject:DataObject
    {
        public Transform transform { get; set; }
        public GameObject()
        {
            transform = new Transform();
        }
    }
}
