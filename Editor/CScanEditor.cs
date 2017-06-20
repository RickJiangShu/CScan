using System;
using System.Text.RegularExpressions;
using UnityEditor;

public class CScanEditor : Editor{

    private static string LF = @"\r?\n";//仅匹配换行

    private static string LineComment = @"//.*" + LF;//行注释
    private static string BlockComment = @"/\*[\s\S]*\*/";//块注释
    private static string NullLines = LF + @"\s*" + LF;//空白行

    /// <summary>
    /// 打印所有
    /// </summary>
    [MenuItem("CScan/All")]
    static void PrintAll()
    {
        object[] files = GetSelectedCodeFiles();
        int count = files.Length;
        int lineCount = 0;
        for (int i = 0; i < count; i++)
        {
            string fileContent = files[i].ToString();
            int fileLineCount;
            ScanLines(fileContent, out fileLineCount);
            lineCount += fileLineCount;
        }

        UnityEngine.Debug.Log("共扫描" + count + "个文件，所有行总共：" + lineCount + "行");
    }

    /// <summary>
    /// 忽略无效行
    /// </summary>
    [MenuItem("CScan/Ignore Invalid")]
    static void IgnoreInvalid()
    {
        object[] files = GetSelectedCodeFiles();
        int count = files.Length;
        int lineCount = 0;
        for (int i = 0; i < count; i++)
        {
            string fileContent = files[i].ToString();
            int fileLineCount;
            ScanValidLines(fileContent, out fileLineCount);
            lineCount += fileLineCount;
        }

        UnityEngine.Debug.Log("共扫描" + count + "个文件，有效行总共：" + lineCount + "行");
    }

    /// <summary>
    /// 扫描行数
    /// </summary>
    /// <param name="content"></param>
    /// <param name="lineCount"></param>
    public static void ScanLines(string content,out int lineCount)
    {
        MatchCollection collection = Regex.Matches(content, LF, RegexOptions.Multiline);
        lineCount = collection.Count + 1;
    }
    /// <summary>
    /// 扫描有效行
    /// </summary>
    public static string ScanValidLines(string content, out int lineCount)
    {
        string validContent = content;

        validContent = validContent.Trim();
        validContent = Regex.Replace(validContent, LineComment, "\n");
        validContent = Regex.Replace(validContent, BlockComment, "");
        validContent = Regex.Replace(validContent, NullLines, "\n");

        ScanLines(validContent,out lineCount);
        return validContent;
    }


    /// <summary>
    /// 获取选中的代码文件
    /// </summary>
    /// <returns></returns>
    static object[] GetSelectedCodeFiles()
    {
        var scripts = Selection.GetFiltered(typeof(MonoScript), SelectionMode.DeepAssets);//C#、JS、Boo
        return scripts;
    }
}