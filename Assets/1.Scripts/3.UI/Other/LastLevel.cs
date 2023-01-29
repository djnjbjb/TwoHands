using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ludo.TwoHandsWar.UI.Other
{
    public class LastLevel : MonoBehaviour
    {
        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}