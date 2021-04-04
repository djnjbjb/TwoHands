using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MyTool
{
    public static Vector2 Vec2Rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    public static bool FloatEqual0p001(float a, float b)
    {
        float threshold = 0.001f;
        if (Mathf.Abs(a - b) <= threshold)
            return true;
        else
            return false;
    }

    public static Vector2 FindNearestPointOnLine(Vector2 lineOrigin, Vector2 lineDirection, Vector2 point)
    {
        lineDirection.Normalize();
        Vector2 lhs = point - lineOrigin;

        float dotP = Vector2.Dot(lhs, lineDirection);
        return lineOrigin + lineDirection * dotP;
    }
}

public class Logout
{
    static string buffer = "";
    public static void Log(string path, string Content)
    {
        StreamWriter sw = new StreamWriter(path + "\\HelloWorldLog.txt", true);
        //开始写入
        sw.WriteLine(Content);
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
    }
    public static void Log(string Content)
    {
        string path = Application.dataPath;
        StreamWriter sw = new StreamWriter(path + "\\HelloWorldLog.txt", true);
        //开始写入
        sw.WriteLine(Content);
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
    }

    public static void LogBuffer(string Content)
    {
        buffer += (Content + "\n");
    }

    public static void PrintBuffer()
    {
        string path = Application.dataPath;
        StreamWriter sw = new StreamWriter(path + "\\HelloWorldLog.txt", true);
        //开始写入
        sw.WriteLine(buffer);
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
    }

    public static void Clear()
    {
        string path = Application.dataPath;
        StreamWriter sw = new StreamWriter(path + "\\HelloWorldLog.txt", false);
        //开始写入
        sw.WriteLine("");
        //清空缓冲区
        sw.Flush();
        //关闭流
        sw.Close();
    }
}

public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}


