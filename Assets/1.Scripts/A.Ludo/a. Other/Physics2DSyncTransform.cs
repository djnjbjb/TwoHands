using UnityEngine;
using System.Collections;

namespace Ludo.Other
{
    public class Physics2DSyncTransform : MonoBehaviour
    {
        static Physics2DSyncTransform instance = null;
        static bool needSync = false;

        public static void AddNeedSyncYieldReturnFixedUpdate()
        {
            CheckInstanceExistence();
            needSync = true;
        }

        static void CheckInstanceExistence()
        {
            if (instance == null)
            {
                throw new System.Exception("No Instance for YieldReturnFixedUpdate()");
            }
        }


        private void Awake()
        {
            if (instance != null)
            {
                instance = this;
            }
            else
            {
                Destroy(instance);
                instance = this;
            }
        }

        private void Start()
        {
            StartCoroutine(YieldReturnFixedUpdate());
        }

        IEnumerator YieldReturnFixedUpdate()
        {
            if (needSync)
            {
                Physics2D.SyncTransforms();
                needSync = false;
            }
            yield return new WaitForFixedUpdate();
        }

    }
}