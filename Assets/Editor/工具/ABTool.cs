using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// AssetBundle工具
/// </summary>
public class ABTool
{
#if UNITY_EDITOR
    [MenuItem("工具/ABTool/修改属性/设置为None")]
    static void SetObjectAB()
    {
        //加载选中文件夹所有文件
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//获取路径
            string fileName = GetFileName(url);//判断文件名
            //根据返回值，字符串是否是空值
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//获取路径
                //对名称设置后
                //判断获取的文件名字拼接后是否相同
                if (_ai.assetBundleName != fileName + ".assetbundle")
                {//不相同强制赋值一次
                    _ai.assetBundleName = fileName + ".assetbundle";
                }
            }
        }
    }
    #region 配套的工具函数
    /// <summary>
    /// 获取文件的名字及拓展名
    /// </summary>
    /// <param name="url">包含文件的完整路径</param>
    /// <returns></returns>
    private static string GetFileName(string _url)
    {
        string[] arr = _url.Split('/');//拆分路径,将路径以“/”区分
        string str = arr[arr.Length - 1];//定位到最后一段(包含后缀名的段)
        //是否包含后缀名常用的.
        if (str.Contains("."))
        {
            return str;//返回含有拓展名的文件名
        }
        return "";//返回空值
    }
    #endregion
#endif
}
