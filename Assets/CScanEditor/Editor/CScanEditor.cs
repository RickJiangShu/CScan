using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class CScanEditor : Editor{

    /// <summary>
    /// 打印所有
    /// </summary>
    [MenuItem("CScan/All")]
    static void PrintAll()
    {
        object[] files = GetSelectedCodeFiles();
    }

    /// <summary>
    /// 获取选中的代码文件
    /// </summary>
    /// <returns></returns>
    static object[] GetSelectedCodeFiles()
    {
        object[] files = Selection.GetFiltered(typeof(FileInfo), SelectionMode.DeepAssets);

        var obj = Selection.activeObject;

        UnityEngine.Debug.Log("activeObject:" + Selection.activeObject);
        return null;
    }
}
