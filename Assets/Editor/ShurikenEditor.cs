using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shuriken))]
[CanEditMultipleObjects]
public class ShurikenEditor : Editor
{
    void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField($"Direction: {(target as Shuriken).ForEditor_GetDirection().ToString("N3")}");
        EditorGUILayout.LabelField($"State: {(target as Shuriken).ForEditor_GetState()}");
        EditorGUILayout.LabelField($"Env: {(target as Shuriken).ForEditor_GetRelationWithEnv()}");
    }
}