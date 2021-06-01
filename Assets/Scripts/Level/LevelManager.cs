using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ludo.Extensions;

public class LevelManager : MonoBehaviour
{
    static LevelManager onlyInstance = null;

    class AABB
    {
        public float left;
        public float right;
        public float bottom;
        public float top;
    }

    //设计用变量
    float fadeOffTime = 0.5f;
    float litUpTime = 0.5f;
    bool playerOutOfRangeOn = false;

    GameObject playerHead;
    AABB region;
    void Awake()
    {
        if (onlyInstance == null)
        {
            onlyInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void OnDestroy()
    {
        if (onlyInstance == this)
        {
            onlyInstance = null;
        }
    }

    void Start()
    {
        region = new AABB();
        {
            Transform levelRegionTransform = transform.LudoFind("LevelRegion", includeInactive: true, recursive: false);
            if (levelRegionTransform == null)
            {
                throw new Exception("Can not find LevelRegion");
            }
            region.left = levelRegionTransform.position.x - levelRegionTransform.localScale.x / 2;
            region.right = levelRegionTransform.position.x + levelRegionTransform.localScale.x / 2;
            region.bottom = levelRegionTransform.position.y - levelRegionTransform.localScale.x / 2;
            region.top = levelRegionTransform.position.y + levelRegionTransform.localScale.x / 2;
        }

        playerHead = null;
        {
            playerHead = GameObject.Find("Player").transform.Find("Body").Find("Head").gameObject;
            if (playerHead == null)
            {
                throw new Exception("Can not find playerHead");
            }
        }
    }


    void Update()
    {

    }

    void FixedUpdate()
    {
        if (playerOutOfRangeOn)
            return;
        Vector3 p = playerHead.transform.position;
        if (p.x > region.right || p.x < region.left || p.y < region.bottom)
        {
            PlayerOutOfRange();
        }
    }

    void PlayerOutOfRange()
    {
        StartCoroutine(PlayerOutOfRangeCoroutine());
    }

    IEnumerator PlayerOutOfRangeCoroutine()
    {
        playerOutOfRangeOn = true;
        {
            //
            //1. Before Scene Load
            //
            GameObject out_backGroundMusicOld = null;
            GameObject out_deathSoundOld = null;
            {
                //获取需要的变量
                HandControl handControl = GameObject.Find("HandControl").GetComponent<HandControl>();
                GameObject blackScreen = GameObject.Find("Canvas").transform.LudoFind("BlackScreen", includeInactive: true, recursive: false).gameObject;
                var image = blackScreen.GetComponent<UnityEngine.UI.Image>();
                GameObject backGroundMusic = GameObject.Find("Audio").transform.Find("BackGround").gameObject;
                GameObject deathSound = GameObject.Find("Audio").transform.Find("Death").gameObject;
                if (handControl == null)
                {
                    throw new Exception("Can not find HandControl");
                }
                if (blackScreen == null)
                {
                    throw new Exception("Can not find BlackScreen");
                }
                if (image == null)
                {
                    throw new Exception("Can not find image from BlackScreen");
                }
                if (backGroundMusic == null)
                {
                    throw new Exception("Can not find BackGround(BackGroundMusic)");
                }
                if (deathSound == null)
                {
                    throw new Exception("Can not find Death(deathSound)");
                }

                //播放音效
                handControl.hcAudio.PlayDeath();

                //停止玩家操作
                handControl.gameObject.SetActive(false);


                //设置NotDestroy
                DontDestroyOnLoad(this.gameObject);

                backGroundMusic.transform.parent = null;
                deathSound.transform.parent = null;
                DontDestroyOnLoad(backGroundMusic);
                DontDestroyOnLoad(deathSound);

                //停止所有声音
                var sounds = GameObject.Find("Audio").GetComponentsInChildren<AudioSource>();
                foreach (var sound in sounds)
                {
                    sound.Stop();
                }

                //颜色渐变
                blackScreen.SetActive(true);
                float startFadeTime = Time.time;
                while (Time.time - startFadeTime <= fadeOffTime)
                {
                    image.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), (Time.time - startFadeTime) / fadeOffTime);
                    yield return null;
                }

                //设置需要传出的值
                out_backGroundMusicOld = backGroundMusic;
                out_deathSoundOld = deathSound;
            }


            //
            //2. Scene Loading
            //
            HandControl out_handControl = null;
            {
                //Prepare for OnSceneLoaded
                GameObject backGroundMusicOld = out_backGroundMusicOld;
                GameObject deathSoundOld = out_deathSoundOld;
                SceneManager.sceneLoaded += OnSceneLoaded;
                void OnSceneLoaded(Scene scene1, LoadSceneMode mode)
                {
                    Start();
                    //获取需要的变量
                    HandControl handControl = GameObject.Find("HandControl").GetComponent<HandControl>();
                    GameObject blackScreen = GameObject.Find("Canvas").transform.LudoFind("BlackScreen", includeInactive: true, recursive: false).gameObject;
                    var image = blackScreen.GetComponent<UnityEngine.UI.Image>();
                    GameObject backGroundMusicNew = GameObject.Find("Audio").transform.Find("BackGround").gameObject;
                    GameObject deathSoundNew = GameObject.Find("Audio").transform.Find("Death").gameObject;
                    if (handControl == null)
                    {
                        throw new Exception("Can not find HandControl");
                    }
                    if (blackScreen == null)
                    {
                        throw new Exception("Can not find BlackScreen");
                    }
                    if (image == null)
                    {
                        throw new Exception("Can not find image from BlackScreen");
                    }
                    if (backGroundMusicNew == null)
                    {
                        throw new Exception("Can not find BackGround(backGroundMusicNew)");
                    }
                    if (deathSoundNew == null)
                    {
                        throw new Exception("Can not find Death(deathSoundNew)");
                    }

                    //停止玩家操作、停止游戏
                    handControl.gameObject.SetActive(false);
                    Time.timeScale = 0f;

                    //NotDestroy的东西归位
                    Destroy(backGroundMusicNew);
                    backGroundMusicOld.transform.parent = GameObject.Find("Audio").transform;
                    Destroy(deathSoundNew);
                    deathSoundOld.transform.parent = GameObject.Find("Audio").transform;
                    SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());

                    //-=OnSceneLoaded
                    SceneManager.sceneLoaded -= OnSceneLoaded;

                    out_handControl = handControl;
                }

                //Load Scene
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            //
            //3. After Scene Load
            //
            {
                //获取需要的变量
                HandControl handControl = out_handControl;
                GameObject blackScreen = GameObject.Find("Canvas").transform.LudoFind("BlackScreen", includeInactive: true, recursive: false).gameObject;
                var image = blackScreen.GetComponent<UnityEngine.UI.Image>();
                if (blackScreen == null)
                {
                    throw new Exception("Can not find BlackScreen");
                }
                if (image == null)
                {
                    throw new Exception("Can not find image from BlackScreen");
                }
                //颜色渐变
                blackScreen.SetActive(true);
                float startLitUpTime = Time.unscaledTime;
                image = blackScreen.GetComponent<UnityEngine.UI.Image>();
                while (Time.unscaledTime - startLitUpTime <= litUpTime)
                {
                    image.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), (Time.unscaledTime - startLitUpTime) / litUpTime);
                    yield return null;
                }

                //结束
                //timeScale恢复，handControl恢复
                Time.timeScale = 1f;
                handControl.gameObject.SetActive(true);
                blackScreen.SetActive(false);
            }


        }
        playerOutOfRangeOn = false;
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
