using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MakePaperCutFromPlayerClass
{
	[MenuItem("GameObject/MakePaperCutFromPlayer", false, 49)]
	static void MakePaperCutFromPlayer()
	{
		if (Selection.activeGameObject == null)
		{
			return;
		}
		var obj = Selection.activeGameObject;
		var t = obj.transform;
		//删掉不可见的东西
		DeleteInactiveChildenRecursively(t);

		//删掉Collidder组件
		var colliders = t.GetComponentsInChildren<Collider2D>();
		foreach (var c in colliders)
		{
			Object.DestroyImmediate(c);
		}

		//删掉Point
		DeleteChildrenNamedPoint(t);

		//设置颜色
		var sprites = t.GetComponentsInChildren<SpriteRenderer>();
		foreach (var s in sprites)
		{			
			if (ColorUtility.TryParseHtmlString("#2C6D74", out Color c))
            {
				s.color = c;
            }
			else
            {
				throw new System.Exception("Impossible");
            }

		}
		//设置图层
		foreach (var s in sprites)
		{
			s.sortingLayerName = "Env";
			s.sortingOrder = 1;
		}
	}

	static void DeleteInactiveChildenRecursively(Transform t)
    {
		List<GameObject> list = new List<GameObject>();
		foreach (Transform tt in t)
		{
			list.Add(tt.gameObject);
		}
		foreach(var l in list)
        {
			if (l.activeSelf == false)
            {
				Object.DestroyImmediate(l);
            }
			else
            {
				DeleteInactiveChildenRecursively(l.transform);
            }
        }
	}

	static void DeleteChildrenNamedPoint(Transform t)
	{
		List<string> nameList = new List<string>();
		nameList.Add("BottomLeftPoint");
		nameList.Add("BottomMiddlePoint");
		nameList.Add("BottomRightPoint");

		List<GameObject> list = new List<GameObject>();
		foreach (Transform tt in t)
		{
			list.Add(tt.gameObject);
		}
		foreach (var l in list)
		{
			if (nameList.Contains(l.name))
			{
				Object.DestroyImmediate(l);
			}
		}
	}
}
