using UnityEngine;
using UnityEditor;

public static class PrintUtilities
{
	[MenuItem("GameObject/PrintAABB", false, 49)]
	static void PrintAABB()
	{
		var raw = Application.GetStackTraceLogType(LogType.Log);
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		if (Selection.activeGameObject != null)
		{
			Debug.Log(
					string.Format("Left: {0}",
					Selection.activeGameObject.transform.position.x - Selection.activeGameObject.transform.lossyScale.x / 2)
				);
			Debug.Log(
					string.Format("Right: {0}",
					Selection.activeGameObject.transform.position.x + Selection.activeGameObject.transform.lossyScale.x / 2)
				);
			Debug.Log(
					string.Format("Bottom: {0}",
					Selection.activeGameObject.transform.position.y - Selection.activeGameObject.transform.lossyScale.y / 2)
				);
			Debug.Log(
					string.Format("Top: {0}",
					Selection.activeGameObject.transform.position.y + Selection.activeGameObject.transform.lossyScale.y / 2)
				);
		}
		Application.SetStackTraceLogType(LogType.Log, raw);
	}
}
