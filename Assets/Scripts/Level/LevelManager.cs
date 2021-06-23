using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ludo.Extensions;

public class LevelManager : MonoBehaviour
{
    public static LevelManager onlyInstance = null;

    public class AABB
    {
        public float left;
        public float right;
        public float bottom;
        public float top;
    }

    //设计用变量
    float fadeOffTime = 0.5f;
    float litUpTime = 0.5f;
    float collideEnemyTime = 0.5f;
    bool playerRestartOn = false;
    bool playerMeetEnemyOn = false;

    GameObject playerHead;
    Collider2D playerCollider;
    public AABB region;
    void Awake()
    {
        if (onlyInstance == null)
        {
            onlyInstance = this;
            InitFields();

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

    void InitFields()
    {
        region = new AABB();
        {
            Transform levelRegionTransform = transform.LudoFind("LevelRegion", includeInactive: true, recursive: true);
            if (levelRegionTransform == null)
            {
                throw new Exception("Can not find LevelRegion");
            }
            region.left = levelRegionTransform.position.x - levelRegionTransform.lossyScale.x / 2;
            region.right = levelRegionTransform.position.x + levelRegionTransform.lossyScale.x / 2;
            region.bottom = levelRegionTransform.position.y - levelRegionTransform.lossyScale.y / 2;
            region.top = levelRegionTransform.position.y + levelRegionTransform.lossyScale.y / 2;
        }

        playerHead = null;
        {
            playerHead = GameObject.Find("Player").transform.Find("Body").Find("Head").gameObject;
            if (playerHead == null)
            {
                throw new Exception("Can not find playerHead");
            }
        }
        playerCollider = null;
        {
            playerCollider = GameObject.Find("Player").GetComponent<Collider2D>();
            if (playerCollider == null)
            {
                throw new Exception("Can not find playerCollider");
            }
        }
    }


    void Update()
    {

    }

    void FixedUpdate()
    {
        if (playerRestartOn)
            return;

        //out of range
        Vector3 p = playerHead.transform.position;
        Vector3 scale = playerHead.transform.lossyScale;
        if (p.x-scale.x/2 > region.right || p.x+scale.x/2 < region.left || p.y+scale.y/2 < region.bottom)
        {
            PlayerOutOfRange();
        }

        //enemy
        if (playerMeetEnemyOn == false)
        {
            LayerMask env = LayerMask.GetMask("Enemy");
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(env);
            Collider2D[] result = new Collider2D[1];
            if (Physics2D.OverlapCollider(playerCollider, filter, result) > 0)
            {
                playerMeetEnemyOn = true;
                GameObject enemy = result[0].gameObject;
                StartCoroutine(PlayerCollideEnemy(enemy));
            }
        }
    }

    void PlayerDiePre()
    {
        HandControl handControl = GameObject.Find("HandControl").GetComponent<HandControl>();
        if (handControl == null)
        {
            throw new Exception("Can not find HandControl");
        }
        //播放音效
        handControl.hcAudio.PlayDeath();

        //停止玩家操作
        handControl.gameObject.SetActive(false);
    }

    void PlayerOutOfRange()
    {
        PlayerDiePre();
        StartCoroutine(GameRestartCoroutine());
    }

    IEnumerator PlayerCollideEnemy(GameObject enemyObj)
    {
        PlayerDiePre();
        GameObject crying = playerHead.transform.LudoFind("Crying", includeInactive: true, recursive: false).gameObject;
        crying.SetActive(true);
        GameObject enemy_evil = enemyObj.transform.LudoFind("Evil", includeInactive: true, recursive: false).gameObject;
        enemy_evil.SetActive(true);
        Enemy.Enemy enemy = enemyObj.GetComponent<Enemy.Enemy>();
        enemy.StopMoving();

        yield return new WaitForSeconds(collideEnemyTime);

        yield return StartCoroutine(GameRestartCoroutine());

        playerMeetEnemyOn = false;
    }

    IEnumerator GameRestartCoroutine()
    {
        playerRestartOn = true;
        {
            //
            //1. Before Scene Load
            //
            GameObject out_backGroundMusicOld = null;
            GameObject out_deathSoundOld = null;
            {
                //获取需要的变量
                var objs = SceneManager.GetActiveScene().GetRootGameObjects();
                HandControl handControl = null;
                foreach (var obj in objs)
                {
                    if (obj.name == "HandControl")
                    {
                        handControl = obj.GetComponent<HandControl>();
                    }
                }
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
                    InitFields();
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
        playerRestartOn = false;
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
