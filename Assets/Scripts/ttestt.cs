using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NameTest
{
    public class ttestt : MonoBehaviour
    {
        bool go = false;
        float speed = 0f;
        float gravity = -52f;
        public GameObject man;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Yurowm.DebugTools.DebugPanel.Log("fu", "fu", 0);
            float minY = -6.1f;

            if (!go) return;
            if (man.transform.position.y <= minY) 
            {
                man.transform.position = new Vector3(man.transform.position.x, -6.09f, man.transform.position.z);
                go = false; 
                return;
            }
            Yurowm.DebugTools.DebugPanel.Log("fu", "fu", 1);
            man.transform.position += new Vector3(0, speed * Time.fixedDeltaTime, 0);
            speed = speed + gravity * Time.fixedDeltaTime;
        }

        public void Bounce1()
        {
            speed = 24f;
            gravity = -48f;
            go = true;
            Yurowm.DebugTools.DebugPanel.Log("b1", "fu", 1);
        }

        public void Bounce2()
        {
            speed = 26f;
            gravity = -52f;
            go = true;
        }

        public void Bounce3()
        {
            speed = 28f;
            gravity = -56f;
            go = true;
        }

        public void Bounce4()
        {
            speed = 32f;
            gravity = -64f;
            go = true;
        }
        public void Bounce5()
        {
            speed = 36f;
            gravity = -72f;
            go = true;
        }

        public void Bounce6()
        {
            speed = 40f;
            gravity = -80f;
            go = true;
        }
    }
}