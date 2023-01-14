using Ludo.TwoHandsWar.PlayerInput;
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
    [SerializeField] bool testYurowmDebugPenalOn = false;

    [Header("-----------------------------------")]
    [SerializeField] bool guiSpawn = false;
    [SerializeField] GameObject guiSpawnPrefab = null ;
    [Header("-----------------------------------")]
    [SerializeField] bool hkeyDebugMode = false;

    public static DebugTool instance = null;
    GameObject player;
    UnityEngine.InputSystem.Keyboard keyboard;

    public static bool IsFunctioning()
    {
        return !(instance == null);
    }

    private void Awake()
    {
        instance = this;
        Y.DebugPanel.initActive = testYurowmDebugPenalOn;
    }

    private void OnGUI()
    {
        if (guiSpawn == false)
            return;
        
        if (GUI.Button(new Rect(100,100, 100, 100), "Spawn"))
        {
            Instantiate(guiSpawnPrefab, guiSpawnPrefab.transform.position, guiSpawnPrefab.transform.rotation);
        }

    }

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

        UniformInput.debugModOn = hkeyDebugMode;
            
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
            UniformInput.debugModOn = !UniformInput.debugModOn;
        }
        Y.DebugPanel.Log("HKey.DebugModOn", "Debug", UniformInput.debugModOn);

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
