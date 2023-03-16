using UnityEngine;

namespace Ludo.Utility
{
    public class Algebra
    {

        /// <summary>
        /// 浮点数相等
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool FloatEqual_WithIn0p001(float a, float b)
        {
            float threshold = 0.001f;
            if (Mathf.Abs(a - b) <= threshold)
                return true;
            else
                return false;
        }
    }
}