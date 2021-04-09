using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Velocity
{
    public float speed { get; set; }
    public Vector2 direction { get; set; }

    public Vector2 velocity 
    { 
        get
        {
            return speed * direction;
        }
    }

    static public implicit operator Vector2(Velocity velocity) => velocity.velocity;

}

