using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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
    static void ScanLines(string content,out int lineCount)
    {
        MatchCollection collection = Regex.Matches(content, LF, RegexOptions.Multiline);
        lineCount = collection.Count + 1;
    }
    /// <summary>
    /// 扫描有效行
    /// </summary>
    static void ScanValidLines(string content, out int lineCount)
    {
        string validContent = content;

        validContent = validContent.Trim();
        validContent = Regex.Replace(validContent, LineComment, "\n");
        validContent = Regex.Replace(validContent, BlockComment, "");
        validContent = Regex.Replace(validContent, NullLines, "\n");

        ScanLines(validContent,out lineCount);
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

    static void TestOutput(string str)
    {
        string outputFilename = Application.dataPath + "/testoutput.txt";
        FileUtils.CreateFile(outputFilename);
        FileUtils.Write(outputFilename, str);
    }
}

#if UNITY_EDITOR
public static class FileUtils
{
    /// 检测文件是否存在Application.dataPath目录
    public static bool IsFileExists(string fileName)
    {
        if (fileName.Equals(string.Empty))
        {
            return false;
        }

        return File.Exists(GetFullPath(fileName));
    }

    /// 在Application.dataPath目录下创建文件
    public static void CreateFile(string fileName)
    {
        if (!IsFileExists(fileName))
        {
            CreateFolder(fileName.Substring(0, fileName.LastIndexOf('/')));

#if UNITY_4 || UNITY_5
            FileStream stream = File.Create(GetFullPath(fileName));
            stream.Close();
#else
            File.Create (GetFullPath (fileName));
#endif
        }
    }

    /// 写入数据到对应文件
    public static void Write(string fileName, string contents)
    {
        CreateFolder(fileName.Substring(0, fileName.LastIndexOf('/')));

        TextWriter tw = new StreamWriter(GetFullPath(fileName), false);
        tw.Write(contents);
        tw.Close();

        AssetDatabase.Refresh();
    }

    /// 从对应文件读取数据
    public static string Read(string fileName)
    {
#if !UNITY_WEBPLAYER
        if (IsFileExists(fileName))
        {
            return File.ReadAllText(GetFullPath(fileName));
        }
        else
        {
            return "";
        }
#endif

#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
    }

    /// 复制文件
    public static void CopyFile(string srcFileName, string destFileName)
    {
        if (IsFileExists(srcFileName) && !srcFileName.Equals(destFileName))
        {
            int index = destFileName.LastIndexOf("/");
            string filePath = string.Empty;

            if (index != -1)
            {
                filePath = destFileName.Substring(0, index);
            }

            if (!Directory.Exists(GetFullPath(filePath)))
            {
                Directory.CreateDirectory(GetFullPath(filePath));
            }

            File.Copy(GetFullPath(srcFileName), GetFullPath(destFileName), true);

            AssetDatabase.Refresh();
        }
    }

    /// 删除文件
    public static void DeleteFile(string fileName)
    {
        if (IsFileExists(fileName))
        {
            File.Delete(GetFullPath(fileName));

            AssetDatabase.Refresh();
        }
    }

    /// 检测是否存在文件夹
    public static bool IsFolderExists(string folderPath)
    {
        if (folderPath.Equals(string.Empty))
        {
            return false;
        }

        return Directory.Exists(GetFullPath(folderPath));
    }

    /// 创建文件夹
    public static void CreateFolder(string folderPath)
    {
        if (!IsFolderExists(folderPath))
        {
            Directory.CreateDirectory(GetFullPath(folderPath));

            AssetDatabase.Refresh();
        }
    }

    /// 复制文件夹
    public static void CopyFolder(string srcFolderPath, string destFolderPath)
    {

#if !UNITY_WEBPLAYER
        if (!IsFolderExists(srcFolderPath))
        {
            return;
        }

        CreateFolder(destFolderPath);


        srcFolderPath = GetFullPath(srcFolderPath);
        destFolderPath = GetFullPath(destFolderPath);

        // 创建所有的对应目录
        foreach (string dirPath in Directory.GetDirectories(srcFolderPath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(srcFolderPath, destFolderPath));
        }

        // 复制原文件夹下所有内容到目标文件夹，直接覆盖
        foreach (string newPath in Directory.GetFiles(srcFolderPath, "*.*", SearchOption.AllDirectories))
        {

            File.Copy(newPath, newPath.Replace(srcFolderPath, destFolderPath), true);
        }

        AssetDatabase.Refresh();
#endif

#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
    }

    /// 删除文件夹
    public static void DeleteFolder(string folderPath)
    {
#if !UNITY_WEBPLAYER
        if (IsFolderExists(folderPath))
        {

            Directory.Delete(GetFullPath(folderPath), true);

            AssetDatabase.Refresh();
        }
#endif

#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::DeleteFolder is innored under wep player platfrom");
#endif
    }

    /// <summary>
    /// 删除文件夹下所有文件
    /// </summary>
    public static void DeleteFolderFiles(string folderPath)
    {
        if (IsFolderExists(folderPath))
        {

        }
    }

    /// 返回Application.dataPath下完整目录
    public static string GetFullPath(string srcName)
    {
        return srcName;

        if (srcName.Equals(string.Empty))
        {
            return Application.dataPath;
        }

        if (srcName[0].Equals('/'))
        {
            srcName.Remove(0, 1);
        }

        return Application.dataPath + "/" + srcName;
    }

    /// 在Assets下创建目录
    public static void CreateAssetFolder(string assetFolderPath)
    {
        if (!IsFolderExists(assetFolderPath))
        {
            int index = assetFolderPath.IndexOf("/");
            int offset = 0;
            string parentFolder = "Assets";
            while (index != -1)
            {
                if (!Directory.Exists(GetFullPath(assetFolderPath.Substring(0, index))))
                {
                    string guid = AssetDatabase.CreateFolder(parentFolder, assetFolderPath.Substring(offset, index - offset));
                    // 将GUID(全局唯一标识符)转换为对应的资源路径。
                    AssetDatabase.GUIDToAssetPath(guid);
                }
                offset = index + 1;
                parentFolder = "Assets/" + assetFolderPath.Substring(0, offset - 1);
                index = assetFolderPath.IndexOf("/", index + 1);
            }

            AssetDatabase.Refresh();
        }
    }

    /// 复制Assets下内容
    public static void CopyAsset(string srcAssetName, string destAssetName)
    {
        if (IsFileExists(srcAssetName) && !srcAssetName.Equals(destAssetName))
        {
            int index = destAssetName.LastIndexOf("/");
            string filePath = string.Empty;

            if (index != -1)
            {
                filePath = destAssetName.Substring(0, index + 1);
                //Create asset folder if needed
                CreateAssetFolder(filePath);
            }


            AssetDatabase.CopyAsset(GetFullAssetPath(srcAssetName), GetFullAssetPath(destAssetName));
            AssetDatabase.Refresh();
        }
    }

    /// 删除Assets下内容
    public static void DeleteAsset(string assetName)
    {
        if (IsFileExists(assetName))
        {
            AssetDatabase.DeleteAsset(GetFullAssetPath(assetName));
            AssetDatabase.Refresh();
        }
    }

    /// 获取Assets下完整路径
    private static string GetFullAssetPath(string assetName)
    {
        if (assetName.Equals(string.Empty))
        {
            return "Assets/";
        }

        if (assetName[0].Equals('/'))
        {
            assetName.Remove(0, 1);
        }

        return "Assets/" + assetName;
    }


    public static FileInfo[] GetFiles(string filesPath, string searchPattern, SearchOption option = SearchOption.TopDirectoryOnly)
    {
        if (!FileUtils.IsFolderExists(filesPath)) return new FileInfo[0];

        string fullPath = GetFullPath(filesPath);
        DirectoryInfo dir = new DirectoryInfo(fullPath);
        FileInfo[] files = dir.GetFiles(searchPattern, option);
        return files;
    }

    public static AssetImporter GetImporter(FileInfo file)
    {
        string assetPath = GetAssetPath(file.FullName);
        return AssetImporter.GetAtPath(assetPath);
    }

    static string GetAssetPath(string fullName)
    {
        int assetIndx = fullName.IndexOf("Assets");
        string winAssetPath = fullName.Substring(assetIndx);
        return winAssetPath.Replace('\\', '/');
    }

    public static string GetNoSuffixName(FileInfo file)
    {
        return file.Name.Replace(file.Extension, "");
    }
    public static string Read(FileInfo file)
    {
        StreamReader stream = file.OpenText();
        string fileString = stream.ReadToEnd();
        stream.Close();
        return fileString;
    }

    public static string WindowsPath(string path)
    {
        return path.Replace('/','\\');
    }


    public static DirectoryInfo[] GetDirectories(string path)
    {
        string fullPath = GetFullPath(path);
        DirectoryInfo dir = new DirectoryInfo(fullPath);
        return dir.GetDirectories();
    }

}

#endif