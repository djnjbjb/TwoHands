using UnityEngine;
using System.Collections;

public class TransformSync : MonoBehaviour
{
    static TransformSync instance = null;
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
