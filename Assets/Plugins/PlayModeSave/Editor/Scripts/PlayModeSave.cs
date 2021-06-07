/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PluginMaster
{
    public class PlayModeSave : EditorWindow
    {
        #region CONTEXT
        private const string TOOL_NAME = "Play Mode Save";
        [MenuItem("CONTEXT/Component/Save Now", true, 2000)]
        private static bool ValidateSaveNowMenu(MenuCommand command) => PrefabUtility.GetPrefabAssetType(command.context) == PrefabAssetType.NotAPrefab && Application.IsPlaying(command.context);
        [MenuItem("CONTEXT/Component/Save Now", false, 2000)]
        private static void SaveNowMenu(MenuCommand command) => Add(command.context as Component, SaveCommand.SAVE_NOW);

        [MenuItem("CONTEXT/Component/Save When Exiting Play Mode", true, 2001)]
        private static bool ValidateSaveOnExtiMenu(MenuCommand command) => ValidateSaveNowMenu(command);
        [MenuItem("CONTEXT/Component/Save When Exiting Play Mode", false, 2001)]
        private static void SaveOnExitMenu(MenuCommand command) => Add(command.context as Component, SaveCommand.SAVE_ON_EXITING_PLAY_MODE);

        [MenuItem("CONTEXT/Component/Apply Play Mode Changes", true, 2002)]
        private static bool ValidateApplyMenu(MenuCommand command) => !Application.isPlaying && _compData.ContainsKey(GetKey(command.context));
        [MenuItem("CONTEXT/Component/Apply Play Mode Changes", false, 2002)]
        private static void ApplyMenu(MenuCommand command) => Apply(GetKey(command.context));

        [MenuItem("CONTEXT/ScriptableObject/Save Now", false, 2000)]
        private static void SaveScriptableObject(MenuCommand command)
        {
            AssetDatabase.Refresh(); 
            EditorUtility.SetDirty(command.context);
            AssetDatabase.SaveAssets();
        }
        #endregion

        #region WINDOW
        [MenuItem("Tools/Plugin Master/" + TOOL_NAME, true, int.MaxValue)]
        private static bool CheckIsNotPlaying() => !Application.isPlaying;
        [MenuItem("Tools/Plugin Master/" + TOOL_NAME, false, int.MaxValue)]
        private static void ShowWindow() => GetWindow<PlayModeSave>(TOOL_NAME);

        private const string AUTO_SAVE_PREF = "PLAY_MODE_SAVE_autoSave";
        private const string AUTO_SAVE_PERIOD_PREF = "PLAY_MODE_SAVE_autoSavePeriod";
        private const string AUTO_APPLY_PREF = "PLAY_MODE_SAVE_autoApply";

        private static void LoadPrefs()
        {
            _autoSave = EditorPrefs.GetBool(AUTO_SAVE_PREF, false);
            _autoSavePeriod = EditorPrefs.GetInt(AUTO_SAVE_PERIOD_PREF, 1);
            _autoApply = EditorPrefs.GetBool(AUTO_APPLY_PREF, true);
        }

        private void OnEnable() => LoadPrefs();
        private void OnDisable()
        {
            EditorPrefs.SetBool(AUTO_SAVE_PREF, _autoSave);
            EditorPrefs.SetInt(AUTO_SAVE_PERIOD_PREF, _autoSavePeriod);
            EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUIUtility.labelWidth = 70;
                    _autoSave = EditorGUILayout.ToggleLeft("Auto-Save Every:", _autoSave);
                    if (check.changed)
                    {
                        EditorPrefs.SetBool(AUTO_SAVE_PREF, _autoSave);
                        if (_autoSave) AutoSave();
                    }
                }
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _autoSavePeriod = EditorGUILayout.IntSlider(_autoSavePeriod, 1, 10);
                    if (check.changed) EditorPrefs.SetInt(AUTO_SAVE_PERIOD_PREF, _autoSavePeriod);
                }
                GUILayout.Label("minutes");
                GUILayout.FlexibleSpace();
            }
            if (Application.isPlaying) return;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _autoApply = EditorGUILayout.ToggleLeft("Auto-Apply All Changes When Exiting Play Mode", _autoApply);
                    if(check.changed) EditorPrefs.SetBool(AUTO_APPLY_PREF, _autoApply);
                }
                if (_autoApply)
                {
                    maxSize = minSize = new Vector2(320, 46);
                    return;
                }
                maxSize = minSize = new Vector2(320, 66);
                if (_compData.Count == 0)
                {
                    EditorGUILayout.LabelField("Nothing to apply");
                    return;
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply All Changes")) ApplyAll();
                    GUILayout.FlexibleSpace();
                }
            }
        }
        #endregion

        #region SAVE
        private static bool _autoApply = true;
        private static bool _autoSave = false;
        private static int _autoSavePeriod = 1;
        private enum SaveCommand { SAVE_NOW, SAVE_ON_EXITING_PLAY_MODE}

        private struct ComponentSaveDataKey
        {
            public int objId;
            public int compId;
            public ComponentSaveDataKey(int objId, int compId) => (this.objId, this.compId) = (objId, compId);
        }

        private class SaveDataValue
        {
            public SerializedObject serializedObj;
            public SaveCommand saveCmd;
            public SaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd) => (this.serializedObj, this.saveCmd) = (serializedObj, saveCmd);
            public virtual void Update(int componentId) => serializedObj.Update();
        }
        private class SpriteRendererSaveDataValue : SaveDataValue
        {
            public int sortingOrder;
            public int sortingLayerID;
            public SpriteRendererSaveDataValue(SerializedObject serializedObj, SaveCommand saveCmd, int sortingOrder, int sortingLayerID) : base(serializedObj, saveCmd) => (this.sortingOrder, this.sortingLayerID) = (sortingOrder, sortingLayerID);

            public override void Update(int componentId)
            {
                base.Update(componentId);
                var spriteRenderer = EditorUtility.InstanceIDToObject(componentId) as SpriteRenderer;
                sortingOrder = spriteRenderer.sortingOrder;
                sortingLayerID = spriteRenderer.sortingLayerID;
            }
        }

        private static Dictionary<ComponentSaveDataKey, SaveDataValue> _compData = new Dictionary<ComponentSaveDataKey, SaveDataValue>();
        private static ComponentSaveDataKey GetKey(Object comp) => new ComponentSaveDataKey((comp as Component).gameObject.GetInstanceID(), (comp as Component).GetInstanceID());

        private static void Add(Component component, SaveCommand cmd)
        {
            var compId = component.GetInstanceID();
            var objId = component.gameObject.GetInstanceID();
            var key = new ComponentSaveDataKey(objId, compId);
            var data = new SerializedObject(component);
            SaveDataValue GetValue()
            {
                if (component is SpriteRenderer)
                {
                    var spriteRenderer = component as SpriteRenderer;
                    return new SpriteRendererSaveDataValue(data, cmd, spriteRenderer.sortingOrder, spriteRenderer.sortingLayerID);
                }
                else return new SaveDataValue(data, cmd);
            }
            if (_compData.ContainsKey(key)) _compData[key] = GetValue();
            else _compData.Add(key, GetValue());
            var prop = new SerializedObject(component).GetIterator();
            while (prop.NextVisible(true)) data.CopyFromSerializedProperty(prop);
            EditorApplication.RepaintHierarchyWindow();
        }

        async static void AutoSave()
        {
            if (!EditorApplication.isPlaying) return;
            if (!_autoSave) return;
            foreach (var data in _compData) data.Value.Update(data.Key.compId);
            await Task.Delay(_autoSavePeriod * 60000);
            AutoSave();
        }

        private static void RemoveNewObjects()
        {
            var compIds = _compData.Keys.ToArray();
            foreach (var id in compIds)
            {
                var obj = EditorUtility.InstanceIDToObject(id.objId) as GameObject;
                if (obj != null) continue;
                _compData.Remove(id);
            }
        }

        private static void Apply(ComponentSaveDataKey key)
        {
            var obj = EditorUtility.InstanceIDToObject(key.objId) as GameObject;
            if (obj == null) return;
            var data = _compData[key].serializedObj;
            var serializedObj = new SerializedObject(data.targetObject);
            var prop = data.GetIterator();
            while (prop.NextVisible(true)) serializedObj.CopyFromSerializedProperty(prop);
            serializedObj.ApplyModifiedProperties();
            if(_compData[key] is SpriteRendererSaveDataValue)
            {
                var spriteRendererData = _compData[key] as SpriteRendererSaveDataValue;
                var spriteRenderer = EditorUtility.InstanceIDToObject(key.compId) as SpriteRenderer;
                spriteRenderer.sortingOrder = spriteRendererData.sortingOrder;
                spriteRenderer.sortingLayerID = spriteRendererData.sortingLayerID;
            }
            _compData.Remove(key);
        }

        private static void ApplyAll()
        {
            var comIds = _compData.Keys.ToArray();
            foreach (var id in comIds) Apply(id);
        }

        [InitializeOnLoadAttribute]
        private static class ApplicationEventHandler
        {
            private static GameObject autoApplyFlag = null;
            private const string AUTO_APPLY_OBJECT_NAME = "PlayModeSave_AutoApply";
            private static Texture2D _icon = Resources.Load<Texture2D>("Save");
            static ApplicationEventHandler()
            {
                EditorApplication.playModeStateChanged += OnStateChanged;
                EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCallback;
                LoadPrefs();
            }
            private static void OnStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingEditMode && _autoApply)
                {
                    autoApplyFlag = new GameObject(AUTO_APPLY_OBJECT_NAME);
                    autoApplyFlag.hideFlags = HideFlags.HideAndDontSave;
                    return;
                }
                if(state == PlayModeStateChange.ExitingPlayMode)
                {
                    foreach (var data in _compData)
                    {
                        if (data.Value.saveCmd == SaveCommand.SAVE_NOW) continue;
                        data.Value.Update(data.Key.compId);
                    }
                    return;
                }
                if(state == PlayModeStateChange.EnteredPlayMode)
                {
                    if (_autoSave) AutoSave();
                    return;
                }
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    PlayModeSave.RemoveNewObjects();
                    autoApplyFlag = GameObject.Find(AUTO_APPLY_OBJECT_NAME);
                    _autoApply = autoApplyFlag != null;
                    if (_autoApply)
                    {
                        DestroyImmediate(autoApplyFlag);
                        PlayModeSave.ApplyAll();
                    }
                }
            }

            private static void HierarchyItemCallback(int instanceID, Rect selectionRect)
            {
                var keys = _compData.Keys.Where(k => k.objId == instanceID).ToArray();
                if (keys.Length == 0) return;
                if (_icon == null) _icon = Resources.Load<Texture2D>("Save");
                GUI.Box(new Rect(selectionRect.xMax - 10, selectionRect.y + 2, 11, 11), _icon, GUIStyle.none);
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        #endregion
    }
}