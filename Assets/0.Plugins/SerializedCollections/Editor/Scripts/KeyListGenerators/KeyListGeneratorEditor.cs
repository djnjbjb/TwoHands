using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Plugins.SerializedCollections.KeysGenerators
{
    [CustomEditor(typeof(KeyListGenerator), true)]
    public class KeyListGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var iterator = serializedObject.GetIterator();
            if (iterator.Next(true))
            {
                // skip script name
                iterator.NextVisible(true);
                while (iterator.NextVisible(true))
                {
                    EditorGUILayout.PropertyField(iterator);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}