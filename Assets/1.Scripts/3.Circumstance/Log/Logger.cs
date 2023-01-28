using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Ludo.TwoHandsWar.Circumstance.Log
{
    public class Logger : MonoBehaviour
    {
        #region Singleton
        static Logger singleton = null;
        void SetSingleton()
        {
            if (singleton == null) singleton = this;
        }
        #endregion

        //Setting
        float s_flushTime = 1f;
        string s_path = @"[ApplicationDataPath]\Log";

        //Runtime
        string fileName;
        string fullName { get => GetFullName(fileName); }
        float unFlushedTime = 0f;
        StringBuilder buffer = new StringBuilder();
        StreamWriter writer = null;

        #region Static
        public static void Log(string message, string tag = null)
        {
            if (singleton == null) Debug.LogError("singleton == null");

            string tagString = "";
            if (tag != null) tagString = $"[tag:{tag}]";
            singleton.buffer.Append($"{tagString}{message}\n");
        }
        #endregion

        #region Unity
        public static void Born()
        {
            var gameObject = new GameObject();
            gameObject.name = "Logger";
            gameObject.AddComponent<Logger>();
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SetSingleton();

            s_path = s_path.Replace("[ApplicationDataPath]", Application.dataPath);
            CreateFolderIfNotExist();
            fileName = FindFileName();
            writer = new StreamWriter(fullName);
            LogPrefix();
        }

        public void Update()
        {
            unFlushedTime += Time.deltaTime;
            if (unFlushedTime >= s_flushTime)
            {
                Flush();
                unFlushedTime = 0f;
            }
        }

        public void OnDestroy()
        {
            LogSuffix();
            Flush();
            writer.Close();
        }
        #endregion
        
        private void Flush()
        {
            writer.Write(buffer);
            writer.Flush();
            buffer.Clear();
        }

        private string FindFileName()
        {
            DateTime dateTime = DateTime.Now;
            string name = "log_" + $"{dateTime:yyyyMMdd}_{dateTime:HH}h{dateTime:mm}m{dateTime:ss}s";

            int num = 0;
            while (File.Exists(GetFullName(name)))
            {
                num += 1;
                name = name + "_" + num;
            }

            return name;
        }

        private void CreateFolderIfNotExist()
        {
            Directory.CreateDirectory(s_path);
        }

        private string GetFullName(string name)
        {
            return s_path + @"\" + name;
        }

        private void LogPrefix()
        {
            Log($"----------------------↓↓↓----------------------");
            Log($"Log Start At {DateTime.Now.ToLongTimeString()}");
        }

        private void LogSuffix()
        {
            Log($"Log End At {DateTime.Now.ToLongTimeString()}");
            Log($"----------------------↑↑↑----------------------");
        }
    }
}