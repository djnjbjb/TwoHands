using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Ludo.TwoHandsWar.Circumstance;
using UnityEngine;
using Logger = Ludo.TwoHandsWar.Circumstance.Logger;

namespace Ludo.TwoHandsWar.Utilities.DebugHelper
{
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
        [SerializeField] GameObject guiSpawnPrefab = null;
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

            if (GUI.Button(new Rect(100, 100, 100, 100), "Spawn"))
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

            PlayerInput.debugModOn = hkeyDebugMode;

        }

        private void FixedUpdate()
        {
            if (playerTrail && TrailPrefab != null)
            {
                var trail = Instantiate(TrailPrefab, player.transform.position, Quaternion.identity);
                if (THWController.singleton.wholeState == WholeState.WhileJumping)
                {
                    trail.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                }
            }
        }

        void Update()
        {
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                PlayerInput.debugModOn = !PlayerInput.debugModOn;
            }
            Y.DebugPanel.Log("HKey.DebugModOn", "Debug", PlayerInput.debugModOn);

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                Logger.Log("1", "Temp");
            }
            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                Logger.Log("2", "Temp");
            }
            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                Logger.Log("3", "Temp");
            }
            if (keyboard.numpad1Key.wasPressedThisFrame)
            {
                Logger.Log("【EasyDebug】 ManuallyNumpad1");
            }
            if (keyboard.numpad2Key.wasPressedThisFrame)
            {
                Debug.Log($"抓握时反向PlayerControl：{THWController.singleton.Out_GetMoveDirectionReverseIfGrabEnv()}");
            }
            if (keyboard.numpad3Key.wasPressedThisFrame)
            {
                Time.timeScale = timeScale;
            }

        }
    }
}