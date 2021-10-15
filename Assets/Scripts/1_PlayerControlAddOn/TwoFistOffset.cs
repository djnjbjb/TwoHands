using PlayerControlTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoFistOffset
{
    public Vector2 left { get; set; }
    public Vector2 right { get; set; }

    public DirectionOf9History leftDirOf9History = new PlayerControlTool.DirectionOf9History();
    public DirectionOf9History rightDirOf9History = new PlayerControlTool.DirectionOf9History();

    public TwoFistOffset()
    {
    }

    public void FixedUpdateHistoryManually()
    {
        leftDirOf9History.FixedUpdateManually(Time.fixedTime, Tool.ArbitraryDirectionToNineDirection(left));
        rightDirOf9History.FixedUpdateManually(Time.fixedTime, Tool.ArbitraryDirectionToNineDirection(right));
    }
}
