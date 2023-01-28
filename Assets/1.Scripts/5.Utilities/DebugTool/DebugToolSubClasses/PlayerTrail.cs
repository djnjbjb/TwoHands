using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.TwoHandsWar.Utilities.DebugHelper.DebugToolSubClasses
{
    public class PlayerTrail : MonoBehaviour
    {
        const float lifeTime = 4;
        void Start()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}