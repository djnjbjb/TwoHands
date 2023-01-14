using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludo.Extensions;
using UnityEngine.SceneManagement;

public class Setting : MonoBehaviour
{
    [System.NonSerialized] public GameObject settingWindow;

    public void Start()
    {
        settingWindow = this.transform.LudoFind("SettingWindow", includeInactive: true).gameObject;
    }
    public void SettingButton()
    {
        settingWindow.SetActive(true);
        GameObject toggle_moveDirectionReverseIfGrabEnv = settingWindow.transform.LudoFind("MoveDirectionReverseIfGrabEnv", includeInactive: true).gameObject;
        var toggle = toggle_moveDirectionReverseIfGrabEnv.GetComponent<UnityEngine.UI.Toggle>();
        toggle.isOn = PlayerControlStaticSetting.ForUI_GetMoveDirectionReverseIfGrabEnv();
    }

    public void SettingReturn()
    {
        settingWindow.SetActive(false);
    }

    public void Toggle_MoveDirectionReverseIfGrabEnv_Change()
    {
        GameObject toggle_moveDirectionReverseIfGrabEnv = settingWindow.transform.LudoFind("MoveDirectionReverseIfGrabEnv", includeInactive: true).gameObject;
        var toggle = toggle_moveDirectionReverseIfGrabEnv.GetComponent<UnityEngine.UI.Toggle>();
        PlayerControlStaticSetting.SetMoveDirectionReverseIfGrabEnv(toggle.isOn);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
