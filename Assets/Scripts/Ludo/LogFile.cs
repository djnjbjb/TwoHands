using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Ludo
{
    public class LogFile : MonoBehaviour
    {

        static LogFile instance = null;

        static string path;
        static string filename;
        static string fullFilename { get => path + @"\" + filename; }
        static StringBuilder buffer = new StringBuilder();
        static StreamWriter writer = null;

        static float flushTime = 0f;
        static float flushInterval = 1f;

        public bool createNewFile = true;
        public bool ifOldFileClearOldFile = false;

        static string filenameTemp;
        static string fullFilenameTemp { get => path + @"\" + filenameTemp; }
        static StringBuilder bufferTemp = new StringBuilder();
        static StreamWriter writerTemp = null;
        public void Awake()
        {
            if (instance == null)
                instance = this;
            path = Application.dataPath + @"\Log";


            //---------------------------------
            if (createNewFile)
            {
                DateTime dateTime = System.DateTime.Now;
                string filenamePrefix = "log_" + $"{dateTime:yyyyMMdd}_{dateTime:HH}h{dateTime:mm}m{dateTime:ss}s";
                filename = filenamePrefix;
                int num = 0;
                while (File.Exists(fullFilename))
                {
                    num += 1;
                    filename = filenamePrefix + "_" + num;
                }
                writer = new StreamWriter(fullFilename);
            }
            else
            {
                filename = "log";
                if (ifOldFileClearOldFile)
                {
                    writer = new StreamWriter(fullFilename, false);
                }
                else
                {
                    writer = new StreamWriter(fullFilename, true);
                }

            }
            Log($"----------------------↓↓↓----------------------");
            Log($"Log Start At {System.DateTime.Now.ToLongTimeString()}");


            //---------------------------------
            filenameTemp = "logTemp";
            writerTemp = new StreamWriter(fullFilenameTemp);
            LogTemp($"----------------------↓↓↓----------------------");
            LogTemp($"Log Start At {System.DateTime.Now.ToLongTimeString()}");

        }

        public void OnDestroy()
        {
            Log($"Log End At {System.DateTime.Now.ToLongTimeString()}");
            Log($"----------------------↑↑↑----------------------");

            LogTemp($"Log End At {System.DateTime.Now.ToLongTimeString()}");
            LogTemp($"----------------------↑↑↑----------------------");

            Flush();

            writer.Close();
            writerTemp.Close();
        }

        public void Update()
        {
            flushTime += Time.deltaTime;
            if (flushTime >= flushInterval)
            {
                Flush();
                flushTime = 0f;
            }
        }

        private void Flush()
        {
            writer.Write(buffer);
            writer.Flush();
            buffer.Clear();

            writerTemp.Write(bufferTemp);
            writerTemp.Flush();
            bufferTemp.Clear();
        }

        public static void Log(string message)
        {
            if (instance == null)
                Debug.LogError("Call Ludo.LogFile.Log, but Ludo.LogFile.instance == null.");
            buffer.Append($"{message}\n");
        }

        public static void LogTemp(string message)
        {
            if (instance == null)
                Debug.LogError("Call Ludo.LogFile.LogTemp, but Ludo.LogFile.instance == null.");
            bufferTemp.Append($"{message}\n");
        }
    }
}