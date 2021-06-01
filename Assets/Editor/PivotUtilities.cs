using UnityEngine;
using UnityEditor;

public static class PivotUtilities
{
	[MenuItem("GameObject/Pivot/Create Pivot", false, 49)]
	static void CreatePivotObject()
	{
		if (Selection.activeGameObject != null)
		{
			var pivot = CreatePivotObject(Selection.activeGameObject);
			Selection.activeGameObject = pivot;
		}
	}

	[MenuItem("GameObject/Pivot/Create Pivot (Local Zero)", false, 49)]
	static void CreatePivotObjectAtParentPos()
	{
		if (Selection.activeGameObject != null)
		{
			var pivot = CreatePivotObjectAtParentPos(Selection.activeGameObject);
			Selection.activeGameObject = pivot;
		}
	}

	[MenuItem("GameObject/Pivot/Delete Pivot", false, 49)]
	static void DeletePivotObject()
	{
		GameObject objSelectionAfter = null;

		if (Selection.activeGameObject != null)
		{
			if (Selection.activeGameObject.transform.childCount > 0)
			{
				objSelectionAfter = Selection.activeGameObject.transform.GetChild(0).gameObject;
			}
			else if (Selection.activeGameObject.transform.parent != null)
			{
				objSelectionAfter = Selection.activeGameObject.transform.parent.gameObject;
			}

			DeletePivotObject(Selection.activeGameObject);

			Selection.activeGameObject = objSelectionAfter;
		}
	}

	[MenuItem("GameObject/Pivot/CreatePivotLeft", false, 49)]
	static void CreatePivotLeft()
	{
		if (Selection.activeGameObject != null)
		{
			var pivot = CreatePivotObjectAtOneEdge(Selection.activeGameObject, Edge.Left);
			Selection.activeGameObject = pivot;
		}
	}

	[MenuItem("GameObject/Pivot/CreatePivotRight", false, 49)]
	static void CreatePivotRight()
	{
		if (Selection.activeGameObject != null)
		{
			var pivot = CreatePivotObjectAtOneEdge(Selection.activeGameObject, Edge.Right);
			Selection.activeGameObject = pivot;
		}
	}

	[MenuItem("GameObject/Pivot/CreatePivotBottom", false, 49)]
	static void CreatePivotBottom()
	{
		if (Selection.activeGameObject != null)
		{
			var pivot = CreatePivotObjectAtOneEdge(Selection.activeGameObject, Edge.Bottom);
			Selection.activeGameObject = pivot;
		}
	}

	[MenuItem("GameObject/Pivot/CreatePivotTop", false, 49)]
	static void CreatePivotTop()
	{
		if (Selection.activeGameObject != null)
		{
			var pivot = CreatePivotObjectAtOneEdge(Selection.activeGameObject, Edge.Top);
			Selection.activeGameObject = pivot;
		}
	}


	private static GameObject CreatePivotObjectAtParentPos(GameObject current)
	{
		if (current == null)
		{
			return null;
		}

		int siblingIndex = current.transform.GetSiblingIndex();

		GameObject newObject = new GameObject("Pivot");
		newObject.transform.SetParent(current.transform.parent);

		newObject.transform.localPosition = Vector3.zero;
		newObject.transform.localScale = Vector3.one;
		newObject.transform.localRotation = Quaternion.identity;

		newObject.transform.SetSiblingIndex(siblingIndex);

		current.transform.SetParent(newObject.transform);

		return newObject;
	}

	private static GameObject CreatePivotObject(GameObject current)
	{
		if (current == null)
		{
			return null;
		}

		int siblingIndex = current.transform.GetSiblingIndex();

		GameObject newObject = new GameObject("Pivot");
		newObject.transform.SetParent(current.transform.parent);

		newObject.transform.position = current.transform.position;
		newObject.transform.localScale = current.transform.localScale;
		newObject.transform.rotation = current.transform.rotation;

		newObject.transform.SetSiblingIndex(siblingIndex);

		current.transform.SetParent(newObject.transform);

		return newObject;
	}

	private static GameObject DeletePivotObject(GameObject current)
	{
		Transform parent = current.transform.parent;
		int childrenCount = current.transform.childCount;
		int siblingIndex = current.transform.GetSiblingIndex();

		Transform[] children = new Transform[childrenCount];
		for (int i = 0; i < childrenCount; i++)
		{
			children[i] = current.transform.GetChild(i);
		}

		for (int i = 0; i < childrenCount; i++)
		{
			children[i].SetParent(parent);
			children[i].SetSiblingIndex(siblingIndex + i);
		}

		if (Application.isPlaying)
		{
			GameObject.Destroy(current);
		}
		else
		{
			GameObject.DestroyImmediate(current);
		}

		if (children.Length > 0)
		{
			return children[0].gameObject;
		}
		else
		{
			return null;
		}
	}

	private enum Edge
    {
		Left,
		Right,
		Bottom,
		Top
    };

	private static GameObject CreatePivotObjectAtOneEdge(GameObject current, Edge edge)
    {
		if (current == null)
		{
			return null;
		}

		int siblingIndex = current.transform.GetSiblingIndex();

		GameObject newObject = new GameObject("Pivot");
		newObject.transform.SetParent(current.transform.parent);
		if (edge == Edge.Left)
        {
			newObject.transform.position = new Vector3(current.transform.position.x - current.transform.localScale.x / 2, current.transform.position.y, current.transform.position.z);
		}
		else if (edge == Edge.Right)
        {
			newObject.transform.position = new Vector3(current.transform.position.x + current.transform.localScale.x / 2, current.transform.position.y, current.transform.position.z);
		}
		else if (edge == Edge.Bottom)
        {
			newObject.transform.position = new Vector3(current.transform.position.x, current.transform.position.y - current.transform.localScale.y / 2, current.transform.position.z);
		}
		else if (edge == Edge.Top)
        {
			newObject.transform.position = new Vector3(current.transform.position.x, current.transform.position.y + current.transform.localScale.y / 2, current.transform.position.z);
		}

		newObject.transform.SetSiblingIndex(siblingIndex);
		current.transform.SetParent(newObject.transform);

		return newObject;
	}
}
