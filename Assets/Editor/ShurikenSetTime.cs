using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShurikenSetTime
{
	[MenuItem("GameObject/Shuriken", false, 48)]
	static void SetTime()
    {
		if (Selection.activeGameObject != null)
		{
			int index = 1;
			float time = 0;
			foreach (Transform t in Selection.activeGameObject.transform)
            {
				var s = t.GetComponent<ShurikenExplode>();
				if (index == 1)
                {
					time = s.beforeShowIdleTime;
                }
				else
                {
					time = time + 0.5f;
					s.beforeShowIdleTime = time;
                }
				index++;
            }
		}
	}
}
