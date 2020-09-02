using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public class Transform
    {
        //
        // 摘要:
        //     A Quaternion that stores the rotation of the Transform in world space.
        public Quaternion rotation { get; set; }
        //
        // 摘要:
        //     The world space position of the Transform.
        public Vector3 position { get; set; }
    }
}
