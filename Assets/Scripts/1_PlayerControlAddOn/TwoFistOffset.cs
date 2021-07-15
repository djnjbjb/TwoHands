using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoFistOffset
{
    public Vector2 left { get; set; }
    public Vector2 right { get; set; }

    public HandControlTool.DirectionOf9History leftDirOf9History = new HandControlTool.DirectionOf9History();
    public HandControlTool.DirectionOf9History rightDirOf9History = new HandControlTool.DirectionOf9History();

    public TwoFistOffset()
    {
    }

    public void FixedUpdateHistoryManually()
    {
        leftDirOf9History.FixedUpdateManually(Time.fixedTime, HandControlTool.Tool.ArbitraryDirectionToNineDirection(left));
        rightDirOf9History.FixedUpdateManually(Time.fixedTime, HandControlTool.Tool.ArbitraryDirectionToNineDirection(right));
    }
}
