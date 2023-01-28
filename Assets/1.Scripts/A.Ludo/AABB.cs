using UnityEngine;

namespace Ludo
{
    public class AABB
    {
        public float left;
        public float right;
        public float bottom;
        public float top;

        public Bounds ToBounds()
        {
            return new Bounds(new Vector3((left+right)/2, (bottom+top)/2, 0), new Vector3(right - left, top-bottom, 0));
        }
    }
}

