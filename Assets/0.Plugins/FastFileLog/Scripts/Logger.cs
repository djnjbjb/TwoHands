using UnityEngine;
using System.Collections;
using System.IO;

namespace FastFileLog {
    [System.Serializable]
    public class LoggerConfig {
        public bool enabled;
        public bool printToScreen;
        public bool printToFile;
        public object key;
    }


    [System.Serializable]
    public class Logger{
        public string name;
        public object key;

        public bool enabled;
        public bool printToScreen = false;
        public bool printToFile = false;
        
        private MemoryStream data;
        private FileStream fileWriter;
        private bool debug {
            get { return enabled && LogManager.debug; }
        }

        public void Configure(bool printToScreen, bool printToFile, object gameObject) {
            this.printToScreen = printToScreen;
            this.printToFile = printToFile;
            this.key = gameObject;
        }

        public void Log(object logMsg) {
            if (debug) {
                string msgString = LogFormat(logMsg.ToString());
                WriteToStream(data, msgString + "\n");
                
                if (printToScreen) {
                    Flush(msgString);
                }
            }
        }

        
        public void Flush(string msg) {
            if (printToScreen && debug) {
                Debug.Log(msg);
            }
        }

        public void Save() {
            if (debug && printToFile) {
                string filePath = GetFileFullPath(name);
                fileWriter = File.Open(filePath, FileMode.Append);
                data.WriteTo(fileWriter);
                fileWriter.Close();
            }            
        }


       

        #region virtual functions
        protected virtual string LogFormat(string msg) {
            return System.DateTime.Now.ToLongTimeString() + ":\t" + msg;
        }

        protected virtual string GetFileFullPath(string name) {
            return Application.dataPath + "/" + LogManager.Instance.savePath + "/" + name + "__" + System.DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
        }
        #endregion

        #region Null Instance
        private static Logger _nullLogger;
        public static Logger NullLogger {
            get {
                if (_nullLogger == null) {
                    _nullLogger = new Logger("NullLogger", false);
                    _nullLogger.enabled = false;
                }
                return _nullLogger;
            }
        }
        public static bool IsNull(Logger l) {
            return l == NullLogger;
        }
        #endregion


        public Logger(string name, bool enabled) {
            this.name = name;
            this.enabled = enabled;

            data = new MemoryStream();
            WriteToStream(data, " ----- Log: " + System.DateTime.Now.ToString("dd-MM-yyyy") + "  ----- \n");
        }

        #region Helper
        private System.Text.UnicodeEncoding uniEncoding = new System.Text.UnicodeEncoding();
        private void WriteToStream(Stream stream, string msg) {
            if (stream != null) {
                stream.Write(uniEncoding.GetBytes(msg),
                0, uniEncoding.GetByteCount(msg));
            }
        }
        #endregion
    }
}