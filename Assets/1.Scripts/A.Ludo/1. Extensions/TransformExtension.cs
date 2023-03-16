using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.Extensions
{
    public static class TransformExtension
    {
        public static Transform LudoFind(this Transform t, string n, bool includeInactive = false, bool recursive = false)
        {
            /*
            0 ©¥©Ó©¥ 1. not recursive
               ©¹©¥ 2. recusive
            */

            //1. not recursive
            {
                if (!recursive && includeInactive)
                {
                    return t.Find(n);
                }

                if (!recursive && !includeInactive)
                {
                    foreach (Transform child in t)
                    {
                        if (child.gameObject.activeSelf && child.name == n)
                        {
                            return child;
                        }
                    }
                }
            }
            
            //2. recursive
            Transform[] transforms = t.GetComponentsInChildren<Transform>(includeInactive);
            foreach (Transform child in transforms)
            {
                if (child.name == n)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
