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
        Scan(false);
    }

    /// <summary>
    /// 忽略无效行
    /// </summary>
    [MenuItem("CScan/Ignore Invalid")]
    static void IgnoreInvalid()
    {
        Scan(true);
    }


    static void Scan(bool ignore)
    {
        //选取
        EditorUtility.DisplayProgressBar("CScan", "正在选取脚本文件...", 0f);
        long startTime = Timestamp();
        object[] files = GetSelectedCodeFiles();
        float selectSeconds = (Timestamp() - startTime) * 0.001f;

        //扫描
        startTime = Timestamp();
        int count = files.Length;
        int lineCount = 0;
        for (int i = 0; i < count; i++)
        {
            MonoScript src = files[i] as MonoScript;
            string fileContent = src.text;
            int fileLineCount;
            if (!ignore)
                ScanLines(fileContent, out fileLineCount);
            else
                ScanValidLines(fileContent, out fileLineCount);

            lineCount += fileLineCount;
            EditorUtility.DisplayProgressBar("CScan", string.Format("正在扫描{0}...{1}/{2}", src.name, i, count), (float)i / count);

        }
        EditorUtility.ClearProgressBar();

        float scanSeconds = (Timestamp() - startTime) * 0.001f;

        string scanName = !ignore ? "所有行" : "有效行";
        UnityEngine.Debug.Log(string.Format("本次共扫描{0}个文件，{1}总共：{2}行\n选取文件耗时：{3}s，扫描耗时：{4}s", count.ToString("N0"), scanName, lineCount.ToString("N0"), selectSeconds.ToString("0.000"), scanSeconds.ToString("0.000")));
    }

    /// <summary>
    /// 扫描行数
    /// </summary>
    /// <param name="content"></param>
    /// <param name="lineCount"></param>
    public static void ScanLines(string content,out int lineCount)
    {
        
        lineCount = 1;
        for (int i = 0, l = content.Length; i < l; i++)
        {
            if (content[i] == '\n')
            {
                lineCount++;
            }
        }
         
        
        /*
        MatchCollection collection = Regex.Matches(content, LF);
        lineCount = collection.Count + 1;
         */
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

    /// <summary>
    /// 获取1970.01.01到现在的时间戳（毫秒）
    /// </summary>
    /// <returns></returns>
    static long Timestamp()
    {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
    }
}