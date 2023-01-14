using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float xMin = 0;
        [SerializeField] private float xMax = 0;
        [SerializeField] private float speed = 0;
        [SerializeField] private int goingRight = 0;
        private float scaleX;
        private bool moving;
        void Start()
        {
            scaleX = this.transform.localScale.x;
            moving = true;
        }

        void FixedUpdate()
        {
            if (!moving)
            {
                return;
            }

            if (goingRight == 1)
            {
                if (this.transform.position.x >= xMax )
                {
                    goingRight = -1;
                }
                float x = Mathf.MoveTowards(this.transform.position.x, xMax, speed * Time.fixedDeltaTime);
                this.transform.position = new Vector3(x, this.transform.position.y, this.transform.position.z);
            }
            else if (goingRight == -1)
            {
                if (this.transform.position.x <= xMin)
                {
                    goingRight = 1;
                }
                float x = Mathf.MoveTowards(this.transform.position.x, xMin, speed * Time.fixedDeltaTime);
                this.transform.position = new Vector3(x, this.transform.position.y, this.transform.position.z);

            }
            if (goingRight == 1)
            {
                this.transform.localScale = new Vector3(scaleX, this.transform.localScale.y, this.transform.localScale.z);
            }
            else if (goingRight == -1)
            {
                this.transform.localScale = new Vector3(-scaleX, this.transform.localScale.y, this.transform.localScale.z);
            }
        }

        public void StopMoving()
        {
            moving = false;
        }
    }
}
