using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject player;
    public HandControl handControl;

    private void Awake()
    {
        
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (false)
        {
            PlayerOutOfRange();
        }
        
    }

    void PlayerOutOfRange()
    {
        //停止HandControl一段时间
        //渐变
    }
}
