using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        Screen.SetResolution(1920, 1080, true);

    }
    public void Level1()
    {
        SceneManager.LoadScene("LevelZ3_1_Upward");
    }

    public void Level2()
    {
        
        SceneManager.LoadScene("LevelZ2_2_GoingRight");
    }

    public void Level3()
    {
        SceneManager.LoadScene("LevelX4");
    }

    public void Quit()
    {
        Application.Quit();
    }
}