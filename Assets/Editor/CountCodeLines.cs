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
        int game_lines = 0;
        {
            string[] fileName = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
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
            game_lines = totalLine;
        }
        int editor_lines;
        {
            string[] fileName = Directory.GetFiles("Assets/Editor", "*.cs", SearchOption.AllDirectories);
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
            editor_lines = totalLine;
            
        }
        Debug.Log(String.Format("游戏代码行数：{0}", game_lines));
        Debug.Log(String.Format("Editor代码行数：{0}", editor_lines));
        Debug.Log(String.Format("总代码行数：{0}", game_lines + editor_lines));

    }
}