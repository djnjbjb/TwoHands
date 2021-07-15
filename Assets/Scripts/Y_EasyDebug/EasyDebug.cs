using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyDebug : MonoBehaviour
{
    [SerializeField] float timeScale = 1f;
    [SerializeField] bool musicOn = true;
    [SerializeField] bool playerTrail = false;
    [SerializeField] GameObject TrailPrefab = null;
    GameObject player;

    void Start()
    {
        Time.timeScale = timeScale;
        GameObject.Find("Audio").transform.Find("BackGround").gameObject.SetActive(musicOn);
        player = GameObject.Find("Player");
    }

    private void FixedUpdate()
    {
        if (playerTrail && TrailPrefab != null)
        {
            var trail = Instantiate(TrailPrefab, player.transform.position, Quaternion.identity);
            if (PlayerControl.playerControl.wholeState == WholeState.WhileJumping)
            {
                trail.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
            }
        }
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
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Ludo.LogFile.Log("【EasyDebug】 ManuallyNumpad1");
        }

    }
}
