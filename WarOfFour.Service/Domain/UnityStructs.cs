using System;
using System.Collections.Generic;
using System.Text;

namespace WarOfFour.Service
{
    public struct Vector3
    {
        //
        // 摘要:
        //     X component of the vector.
        public float x;
        //
        // 摘要:
        //     Y component of the vector.
        public float y;
        //
        // 摘要:
        //     Z component of the vector.
        public float z;
    }

    public struct Quaternion
    {
        //
        // 摘要:
        //     X component of the Quaternion. Don't modify this directly unless you know quaternions
        //     inside out.
        public float x;
        //
        // 摘要:
        //     Y component of the Quaternion. Don't modify this directly unless you know quaternions
        //     inside out.
        public float y;
        //
        // 摘要:
        //     Z component of the Quaternion. Don't modify this directly unless you know quaternions
        //     inside out.
        public float z;
        //
        // 摘要:
        //     W component of the Quaternion. Do not directly modify quaternions.
        public float w;
    }
}
