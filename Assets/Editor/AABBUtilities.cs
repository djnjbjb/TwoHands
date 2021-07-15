using UnityEngine;
using UnityEditor;

public static class AABBUtilities
{

	static float? leftSaved = null;
	static float? rightSaved = null;
	static float? bottomSaved = null;
	static float? topSaved = null;

	[MenuItem("GameObject/AABB/PrintAABB", false, 49)]
	static void PrintAABB()
	{
		var raw = Application.GetStackTraceLogType(LogType.Log);
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		if (Selection.activeGameObject != null)
		{
			float left = Selection.activeGameObject.transform.position.x - Selection.activeGameObject.transform.lossyScale.x / 2;
			float right = Selection.activeGameObject.transform.position.x + Selection.activeGameObject.transform.lossyScale.x / 2;
			float bottom = Selection.activeGameObject.transform.position.y - Selection.activeGameObject.transform.lossyScale.y / 2;
			float top = Selection.activeGameObject.transform.position.y + Selection.activeGameObject.transform.lossyScale.y / 2;
			Debug.Log(string.Format("Left: {0}\nRight: {1}\nBottom: {2}\nTop: {3}\n",
					      left,
						  right,
						  bottom,
						  top)
                );
        }
		Application.SetStackTraceLogType(LogType.Log, raw);
	}

	[MenuItem("GameObject/AABB/SaveLeft #1", false, 50)]
	static void SaveLeft()
	{
		Save("Left");
	}

	[MenuItem("GameObject/AABB/SaveRight #2", false, 51)]
	static void SaveRight()
	{
		Save("Right");
	}

	[MenuItem("GameObject/AABB/SaveBottom #3", false, 52)]
	static void SaveBottom()
	{
		Save("Bottom");
	}

	[MenuItem("GameObject/AABB/SaveTop #4", false, 53)]
	static void SaveTop()
	{
		Save("Top");
	}

	static void Save(string which)
	{
		if (which == "Left")
        {
			leftSaved = Selection.activeGameObject.transform.position.x - Selection.activeGameObject.transform.lossyScale.x / 2;
		}
		if (which == "Right")
		{
			rightSaved = Selection.activeGameObject.transform.position.x + Selection.activeGameObject.transform.lossyScale.x / 2;
		}
		if (which == "Bottom")
		{
			bottomSaved = Selection.activeGameObject.transform.position.y - Selection.activeGameObject.transform.lossyScale.y / 2;
		}
		if (which == "Top")
		{
			topSaved = Selection.activeGameObject.transform.position.y + Selection.activeGameObject.transform.lossyScale.y / 2;
		}

	}

	[MenuItem("GameObject/AABB/RestoreLeft &1", false, 54)]
	static void RestoreLeft()
	{
		Restore("Left");
	}

	[MenuItem("GameObject/AABB/RestoreRight &2", false, 55)]
	static void RestoreRight()
	{
		Restore("Right");
	}

	[MenuItem("GameObject/AABB/RestoreBottom &3", false, 56)]
	static void RestoreBottom()
	{
		Restore("Bottom");
	}

	[MenuItem("GameObject/AABB/RestoreTop &4", false, 57)]
	static void RestoreTop()
	{
		Restore("Top");
	}

	static void Restore(string which)
	{

		if (Selection.activeGameObject == null)
        {
			return;
        }

		Vector3 p = Selection.activeGameObject.transform.position;
		if (which == "Left")
		{
			if (!leftSaved.HasValue)
            {
				return;
            }
			Selection.activeGameObject.transform.position = new Vector3(leftSaved.Value + Selection.activeGameObject.transform.lossyScale.x / 2, p.y, p.z);
		}
		if (which == "Right")
		{
			if (!rightSaved.HasValue)
			{
				return;
			}
			Selection.activeGameObject.transform.position = new Vector3(rightSaved.Value - Selection.activeGameObject.transform.lossyScale.x / 2, p.y, p.z);
		}
		if (which == "Bottom")
		{
			if (!bottomSaved.HasValue)
			{
				return;
			}
			Selection.activeGameObject.transform.position = new Vector3(p.x, bottomSaved.Value + Selection.activeGameObject.transform.lossyScale.y / 2, p.z);
		}
		if (which == "Top")
		{
			if (!topSaved.HasValue)
			{
				return;
			}
			Selection.activeGameObject.transform.position = new Vector3(p.x, topSaved.Value - Selection.activeGameObject.transform.lossyScale.y / 2, p.z);
		}
	}

	static float? scaleXSaved = null;
	static float? scaleYSaved = null;
	static void SaveScaleX()
	{
		scaleXSaved = Selection.activeGameObject.transform.localScale.x;
	}

	static void SaveScaleY()
	{
		scaleYSaved = Selection.activeGameObject.transform.localScale.y;
	}
	static void RestoreScaleX()
	{
		if (!scaleXSaved.HasValue)
		{
			return;
		}
		Vector3 s = Selection.activeGameObject.transform.localScale;
		Selection.activeGameObject.transform.localScale = new Vector3(scaleXSaved.Value, s.y, s.z);
	}
	static void RestoreScaleY()
	{
		if (!scaleYSaved.HasValue)
		{
			return;
		}
		Vector3 s = Selection.activeGameObject.transform.localScale;
		Selection.activeGameObject.transform.localScale = new Vector3(s.x, scaleYSaved.Value, s.z);
	}
	


	[MenuItem("GameObject/AABB/MaintainTopLeftSacleY_Start #q", false, 60)]
	static void MaintainTopLeftSacleY_Start()
	{
		SaveScaleY();
		SaveLeft();
		SaveTop();
	}

	[MenuItem("GameObject/AABB/MaintainTopLeftSacleY_End &q", false, 61)]
	static void MaintainTopLeftSacleY_End()
	{
		RestoreScaleY();
		RestoreLeft();
		RestoreTop();
		
	}

	[MenuItem("GameObject/AABB/MaintainLeftBottomSacleX_Start #e", false, 62)]
	static void MaintainLeftBottomSacleX_Start()
	{
		SaveScaleX();
		SaveLeft();
		SaveBottom();
	}

	[MenuItem("GameObject/AABB/MaintainLeftBottomSacleX_End &e", false, 63)]
	static void MaintainLeftBottomSacleX_End()
	{
		RestoreScaleX();
		RestoreLeft();
		RestoreBottom();
	}
}
