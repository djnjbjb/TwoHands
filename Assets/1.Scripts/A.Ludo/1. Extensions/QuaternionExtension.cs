using UnityEngine;

namespace Ludo.Extensions
{
    public static class QuaternionExtension
    {
        public static float SignedAngle2D(Quaternion from, Quaternion to)
        {
            Vector3 vFrom = from * Vector3.right;
            Vector3 vTo = to * Vector3.right;
            return Vector3.SignedAngle(vFrom, vTo, new Vector3(0, 0, 1));
        }

        public static float Angle2D(this Quaternion quaternion)
        {
            return SignedAngle2D(Quaternion.identity, quaternion);
        }
    }
}
