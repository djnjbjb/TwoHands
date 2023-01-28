using UnityEngine;
using Ludo.Extensions;

namespace Ludo.TwoHandsWar.UI.Tutorial
{
    public class TutorialPanel : MonoBehaviour
    {
        [System.NonSerialized] public GameObject contentWindow;
        [System.NonSerialized] public GameObject keyboardWindow;
        [System.NonSerialized] public GameObject gamepadWindow;
        public void Start()
        {
            contentWindow = transform.LudoFind("Content", includeInactive: true).gameObject;
            keyboardWindow = transform.LudoFind("Content", includeInactive: true)
                                .LudoFind("Keyboard", includeInactive: true).gameObject;
            gamepadWindow = transform.LudoFind("Content", includeInactive: true)
                                .LudoFind("Gamepad", includeInactive: true).gameObject;
        }

        public void OpenOrClose()
        {
            contentWindow.SetActive(!contentWindow.activeSelf);
        }

        public void Close()
        {
            contentWindow.SetActive(false);
        }

        public void ShowKeyboardWindow()
        {
            keyboardWindow.SetActive(true);
            gamepadWindow.SetActive(false);
        }

        public void ShowGamepadWindow()
        {
            gamepadWindow.SetActive(true);
            keyboardWindow.SetActive(false);
        }
    }
}