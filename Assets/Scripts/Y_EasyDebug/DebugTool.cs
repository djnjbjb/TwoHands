using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DebugTool : MonoBehaviour
{
    [SerializeField] float timeScale = 1f;
    [SerializeField] bool playerTrail = false;
    [SerializeField] GameObject TrailPrefab = null;
    [SerializeField] bool testMusicOff = false;
    [SerializeField] bool testShurikenKillOff = false;
    GameObject player;
    UnityEngine.InputSystem.Keyboard keyboard;

    void Start()
    {
        Time.timeScale = timeScale;
        GameObject.Find("Audio").transform.Find("BackGround").gameObject.SetActive(!testMusicOff);
        player = GameObject.Find("Player");
        keyboard = UnityEngine.InputSystem.Keyboard.current;

        FieldInfo fi = typeof(LevelManager).GetField("shurikenKill", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fi.GetValue(LevelManager.onlyInstance) != null)
        {
            fi.SetValue(LevelManager.onlyInstance, !testShurikenKillOff);
        }
        else
        {
            throw new System.Exception("shuirikenKill not in LevelManager");
        }
            
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
        if (keyboard.f1Key.wasPressedThisFrame)
        {
            HKey.debugModOn = !HKey.debugModOn;
        }
        Y.DebugPanel.Log("HKey.DebugModOn", "Debug", HKey.debugModOn);

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            Ludo.LogFile.LogTemp("1");
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            Ludo.LogFile.LogTemp("2");
        }
        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            Ludo.LogFile.LogTemp("3");
        }
        if (keyboard.numpad1Key.wasPressedThisFrame)
        {
            Ludo.LogFile.Log("【EasyDebug】 ManuallyNumpad1");
        }
        if (keyboard.numpad2Key.wasPressedThisFrame)
        {
            Debug.Log($"抓握时反向Static： {PlayerControlStaticSetting.GetMoveDirectionReverseIfGrabEnv()}");
            Debug.Log($"抓握时反向PlayerControl：{PlayerControl.playerControl.Out_GetMoveDirectionReverseIfGrabEnv()}");
        }
        if (keyboard.numpad3Key.wasPressedThisFrame)
        {
            Time.timeScale = timeScale;
        }

    }
}
