using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Stuff
{
    public class Stuff : MonoBehaviour
    {
        //每个stuff有一个单独的z，用于检测碰撞时区分
        static int nextZ = 1;

        //用一个Dict作为观点的fist列表。string只有2种可能，“LeftFist”和“RightFist”。
        protected List<string> fists = new List<string>();
        
        protected string firstFist 
        {
            get => fists.Count > 0 ? fists[0] : null; 
        }



        public void Start()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, nextZ);
            nextZ += 1;
        }

        //Stuff这里的实现可能有问题。因为目前也没有Stuff。所以就不管了。具体只看Sword的实现。
        public virtual void AddAssociatedFist(string fist)
        {
            if (fist != "LeftFist" && fist != "RightFist")
                throw new System.Exception("fist should be LeftFist or RightFist");
            foreach(var fistElement in fists)
            {
                if (fistElement == fist)
                {
                    throw new System.Exception("fist already exist");
                }
            }
            fists.Add(fist);
        }
        public virtual void RemoveAssociatedFist(string fist)
        {
            if (fist != "LeftFist" && fist != "RightFist")
                throw new System.Exception("fist should be LeftFist or RightFist");
            int fistCount = -1;
            for(int i = 0; i < fists.Count; i++)
            {
                if (fists[i] == fist)
                {
                    fistCount = i;
                }
            }
            if (fistCount == -1)
            {
                throw new System.Exception("fist not exist");
            }
            fists.RemoveAt(fistCount);
        }
    }


    /*
    //legacy
    public class Stuff : MonoBehaviour
    {
        [NonSerialized] public float speed = 0;
        [NonSerialized] public Vector2 direction = new Vector2(0, 0);
        [NonSerialized] public float decrease = 0.03f;

        void FixedUpdate()
        {
            float speedPre = speed;
            speed = Mathf.Lerp(speed, 0, decrease);
            float distance = (speed + speedPre) / 2f * Time.fixedDeltaTime;
            gameObject.transform.position += distance * (Vector3)direction;

            if (speed <= 0.1)
                speed = 0;
        }
    }
    */
}
