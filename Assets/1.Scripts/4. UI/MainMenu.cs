﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ludo.Extensions;
using Ludo.TwoHandsWar;

public class MainMenu : MonoBehaviour
{
    [System.NonSerialized] public GameObject setting;
    UnityEngine.InputSystem.Keyboard keyboard;

    public void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        setting = GameObject.Find("Canvas").transform.LudoFind("Setting", includeInactive: true).gameObject;
        keyboard = UnityEngine.InputSystem.Keyboard.current;

    }
    public void Level1()
    {
        SceneManager.LoadScene("Level1_Climb");
    }

    public void Level2()
    {
        
        SceneManager.LoadScene("Level2_Jump");
    }

    public void Level3()
    {
        SceneManager.LoadScene("Level3_River");
    }

    public void Level4()
    {
        SceneManager.LoadScene("Level4_Sword");
    }

    public void Level5()
    {
        SceneManager.LoadScene("Level5_Sword2");
    }

    public void SettingButton()
    {
        setting.SetActive(true);
        GameObject toggle_moveDirectionReverseIfGrabEnv = setting.transform.LudoFind("MoveDirectionReverseIfGrabEnv", includeInactive: true).gameObject;
        var toggle = toggle_moveDirectionReverseIfGrabEnv.GetComponent<UnityEngine.UI.Toggle>();
        toggle.isOn = 
            Ludo.TwoHandsWar.Circumstance.Setting.singleton.characterController.moveReversedGrabbing;
    }

    public void SettingReturn()
    {
        setting.SetActive(false);
    }

    public void Toggle_MoveDirectionReverseIfGrabEnv_Change()
    {
        GameObject toggle_moveDirectionReverseIfGrabEnv = setting.transform.LudoFind("MoveDirectionReverseIfGrabEnv", includeInactive: true).gameObject;
        var toggle = toggle_moveDirectionReverseIfGrabEnv.GetComponent<UnityEngine.UI.Toggle>();

        Ludo.TwoHandsWar.Circumstance.Setting.singleton.SetMoveReversedGrabbing(toggle.isOn);
    }

    public void Quit()
    {
        Application.Quit();
    }

    void Update()
    {
        UpdateDebug();
    }

    void UpdateDebug()
    {
        if (keyboard.numpad2Key.wasPressedThisFrame)
        {
            Debug.Log($"抓握时反向Static： {THWController.singleton.Out_GetMoveDirectionReverseIfGrabEnv()}");
        }
    }
}