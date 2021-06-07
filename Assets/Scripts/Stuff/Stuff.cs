using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Stuff
{
    public class Stuff : MonoBehaviour
    {
        [NonSerialized] public GameObject fist;

        void FixedUpdate()
        {

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
