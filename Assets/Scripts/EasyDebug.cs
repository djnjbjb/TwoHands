using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyDebug : MonoBehaviour
{
    [SerializeField] float timeScale = 1f;
    [SerializeField] bool musicOn = true;

    void Start()
    {
        Time.timeScale = timeScale;
        GameObject.Find("Audio").transform.Find("BackGround").gameObject.SetActive(musicOn);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            HKey.debugModOn = !HKey.debugModOn;
        }
        Y.DebugPanel.Log("HKey.DebugModOn", "Debug", HKey.debugModOn);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Ludo.LogFile.LogTemp("1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Ludo.LogFile.LogTemp("2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Ludo.LogFile.LogTemp("3");
        }

    }
}
