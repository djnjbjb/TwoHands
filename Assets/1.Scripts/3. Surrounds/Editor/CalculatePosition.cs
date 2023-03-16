using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CalculatePosition
{
    [MenuItem("GameObject/CalculatePosition/Diff", false, 47)]
	static void CalculatePositionDiff(MenuCommand menuCommand)
	{
		if (menuCommand.context != Selection.objects[0])
		{
			return;
		}

		if (Selection.gameObjects.Length != 2)
		{
			Debug.Log("it should be 2 GameObjects");
		}
		else
        {
			var g1 = Selection.gameObjects[0];
			var g2 = Selection.gameObjects[1];
			Vector3 diff = g2.transform.position - g1.transform.position;
			Debug.Log($"g1 siblingIndex: {g1.transform.GetSiblingIndex()}");
			Debug.Log($"g2 siblingIndex: {g2.transform.GetSiblingIndex()}");
			Debug.Log($"Diff: {diff.ToString("N3")}");
		}
	}
}
