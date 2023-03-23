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
    static void SetObjectABNull()
    {
        //加载选中文件夹所有文件
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//获取路径
            string fileName = MatchingFunc.inst.GetFileName(url);//判断文件名
            //根据返回值，字符串是否是空值
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//获取路径
                _ai.assetBundleName = null;//置空
            }
        }
    }
    [MenuItem("工具/ABTool/修改属性/设置为自身名字")]
    static void SetObjectABMyName()
    {
        //加载选中文件夹所有文件
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//获取路径
            string fileName = MatchingFunc.inst.GetFileName(url);//判断文件名
            //根据返回值，字符串是否是空值
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//获取路径
                _ai.assetBundleName = null;//置空
            }
        }
    }
    [MenuItem("工具/ABTool/修改属性/打包")]
    static void ABpack()
    {
        string _path = Application.streamingAssetsPath;//获取目标文件夹路径
        //判断路径是否存在
        if (!Directory.Exists(_path))
        {//不存在新建
            Directory.CreateDirectory(_path);
        }
        //打包
        BuildPipeline.BuildAssetBundles(_path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();//刷新编译器
    }


#endif
}
