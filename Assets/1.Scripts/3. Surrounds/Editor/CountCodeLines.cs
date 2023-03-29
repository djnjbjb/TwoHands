using System;
using System.IO;
using UnityEngine;
using UnityEditor;

/*
 * 来自
 * https://blog.csdn.net/LLLLL__/article/details/104288456的
*/
public class CountCodeLines
{
    [MenuItem("Tools/Code Line Count")]
    private static void PrintTotalLine()
    {
        int gameLines = 0;
        {
            string[] fileName = Directory.GetFiles("Assets/1.Scripts", "*.cs", SearchOption.AllDirectories);
            int totalLine = 0;
            foreach (var temp in fileName)
            {
                int nowLine = 0;
                StreamReader sr = new StreamReader(temp);
                while (sr.ReadLine() != null)
                {
                    nowLine++;
                }

                totalLine += nowLine;
            }
            gameLines = totalLine;
        }
        Debug.Log(String.Format("游戏代码行数：{0}", gameLines));

        int characterControllerLines = 0;
        {
            string[] fileName = Directory.GetFiles("Assets/1.Scripts/1. Game/1.CharacterController", "*.cs", SearchOption.AllDirectories);
            int totalLine = 0;
            foreach (var temp in fileName)
            {
                int nowLine = 0;
                StreamReader sr = new StreamReader(temp);
                while (sr.ReadLine() != null)
                {
                    nowLine++;
                }

                totalLine += nowLine;
            }
            characterControllerLines = totalLine;
        }
        Debug.Log(String.Format("CharacterController代码行数：{0}", characterControllerLines));
    }
}