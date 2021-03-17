using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stuff : MonoBehaviour
{
    public float speed = 0;
    public Vector2 direction = new Vector2(0,0);
    public float decrease = 0.03f;

    // Update is called once per frame
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
