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

    static Object[] objarr = null;

    [MenuItem("工具/ABTool/可视化打包")]
    static void WindowABPack()
    {
        EditorWindow.GetWindowWithRect<ABToolWindow>(new Rect(200, 200, 500, 500), false, "可视化打包");
    }

    [MenuItem("工具/ABTool/修改属性/设置为None,并清空未使用名字")]
    static void SetObjectABNull()
    {
        //加载选中文件夹所有文件
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//获取路径
            string fileName = MatchingFunc.GetFileName(url);//判断文件名
            //根据返回值，字符串是否是空值
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//获取路径
                _ai.assetBundleName = null;//置空
            }
        }
        Debug.Log("属性设置完成");
        RemoveABNmae();
    }

    [MenuItem("工具/ABTool/修改属性/设置为自身名字")]
    static void SetObjectABMyName()
    {
        //加载选中文件夹所有文件
        objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//获取路径
            string fileName = MatchingFunc.GetFileName(url);//判断文件名
            //根据返回值，字符串是否是空值
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//获取路径
                _ai.assetBundleName = null;//置空
            }
        }
        Debug.Log("设置完成");
    }

    [MenuItem("工具/ABTool/打包")]
    static void ABpack()
    {
        string _path = MatchingFunc.GetPlatformPath();//获取目标文件夹路径
        //判断路径是否存在
        if (!Directory.Exists(_path))
        {//不存在新建
            Directory.CreateDirectory(_path);
        }
        //打包
        BuildPipeline.BuildAssetBundles(_path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();//刷新编译器
        Debug.Log("打包完成");
    }

    [MenuItem("工具/ABTool/清空未使用标签")]
    static void RemoveABNmae()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();//清除未使用的标签
        AssetDatabase.Refresh();//刷新
        Debug.Log("未使用标签清空完成");
    }

    /// <summary>
    /// 可视化打包附加函数
    /// </summary>
    /// <param name="_objarr">文件数据</param>
    /// <param name="fungcname">要执行的函数</param>
    public static void WindowsAB(Object[] _objarr = null, ABTOOL _fungcname =default)
    {
        switch (_fungcname)
        {
            case ABTOOL.ABPACK://执行打包操作
                objarr = _objarr;
                SetObjectABMyName();
                ABpack();
                break;
            case ABTOOL.ABREMOVENOUSENAME://删除未使用的标签
                RemoveABNmae();
                break;
            default:
                break;
        }
    }

    public enum ABTOOL
    {
        /// <summary>
        /// 打包
        /// </summary>
        ABPACK,
        /// <summary>
        /// 删除未使用的AB标签
        /// </summary>
        ABREMOVENOUSENAME
    }
#endif
}
