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

        static string filenameTemp2;
        static string fullFilenameTemp2 { get => path + @"\" + filenameTemp2; }
        static StringBuilder bufferTemp2 = new StringBuilder();
        static StreamWriter writerTemp2 = null;

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

            //---------------------------------
            filenameTemp2 = "logTemp2";
            writerTemp2 = new StreamWriter(fullFilenameTemp2);
            LogTemp2($"----------------------↓↓↓----------------------");
            LogTemp2($"Log Start At {System.DateTime.Now.ToLongTimeString()}");

        }

        public void OnDestroy()
        {
            Log($"Log End At {System.DateTime.Now.ToLongTimeString()}");
            Log($"----------------------↑↑↑----------------------");

            LogTemp($"Log End At {System.DateTime.Now.ToLongTimeString()}");
            LogTemp($"----------------------↑↑↑----------------------");

            LogTemp2($"Log End At {System.DateTime.Now.ToLongTimeString()}");
            LogTemp2($"----------------------↑↑↑----------------------");

            Flush();

            writer.Close();
            writerTemp.Close();
            writerTemp2.Close();
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

            writerTemp2.Write(bufferTemp2);
            writerTemp2.Flush();
            bufferTemp2.Clear();
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
                throw new Exception("instanc == null");
            bufferTemp.Append($"{message}\n");
        }

        public static void LogTemp2(string message)
        {
            if (instance == null)
                throw new Exception("instanc == null");
            bufferTemp2.Append($"{message}\n");
        }
    }
}