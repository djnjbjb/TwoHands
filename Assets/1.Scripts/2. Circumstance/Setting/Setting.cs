using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Ludo.TwoHandsWar.SettingSubs;

namespace Ludo.TwoHandsWar.Circumstance
{
    public class Setting : MonoBehaviour
    {
        #region Type
        class AllSettings
        {
            public CharacterControllerSetting characterControllerSetting;
        }
        #endregion

        #region Static
        public static void Born()
        {
            var gameObject = new GameObject();
            gameObject.name = "Setting";
            gameObject.AddComponent<Setting>();
        }

        public static Setting singleton;

        void SetSingleton()
        {
            singleton = this;
        }
        #endregion

        #region Fields
        public CharacterControllerSetting characterController 
            { get; private set; } =  new CharacterControllerSetting();
        #endregion

        #region Public Method
        public void SetMoveReversedGrabbing(bool value)
        {
            characterController.moveReversedGrabbing = value;
        }
        #endregion

        private void Awake()
        {
            SetSingleton();
            DontDestroyOnLoad(gameObject);
            ReadFromFile();
        }

        private void ReadFromFile()
        {
            string json = ReadFromFile(GetFilePath());
            AllSettings all = JsonMapper.ToObject<AllSettings>(json);
            characterController = all.characterControllerSetting;
        }

        private void WriteToFile()
        {
            AllSettings all = new AllSettings();
            all.characterControllerSetting = characterController;

            JsonWriter writer = GetJsonWriter();
            JsonMapper.ToJson(all, writer);
            string content = writer.ToString();

            WriteToFile(GetFilePath(), content);
        }

        public string ReadFromFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath, encoding: System.Text.Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        private void WriteToFile(string filePath, string content)
        {
            StreamWriter sr = new StreamWriter(filePath, append: false, encoding: System.Text.Encoding.UTF8);
            sr.WriteLine(content);
            sr.Flush();
            sr.Close();
        }

        private string GetFilePath()
        {
            string path = "";
#if UNITY_EDITOR
            path = Application.dataPath + @"\Files\OtherFiles\setting.json";

#elif PLATFORM_STANDALONE_WIN
            path = Application.dataPath + @"\setting.json";
#else
            throw new System.Exception("Only Support Editor and Windows");
#endif
            return path;
        }

        private JsonWriter GetJsonWriter()
        {
            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = true;
            writer.IndentValue = 4;
            return writer;
        }
    }
}
