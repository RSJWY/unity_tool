using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor工具配套函数
/// </summary>
public class MatchingFunc
{
#if UNITY_EDITOR
    /// <summary>
    /// 获取文件的名字及拓展名
    /// </summary>
    /// <param name="url">包含文件的完整路径</param>
    /// <returns></returns>
    public static string GetFileName(string _url)
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
    /// <summary>
    /// 递归拆分父子关系
    /// </summary>
    /// <param name="_obj">要拆分的物体</param>
    public static void ObjDetech(Transform _obj)
    {
        if (_obj==null)
        {
            return;
        }
        for (int i= 0;  i< _obj.childCount; i++)
        {
            ObjDetech(_obj.GetChild(i));
        }
        _obj.DetachChildren();
    }

    /// <summary>
    /// 检查路径下是否有重命名的文件，并重新指定一个没有任何冲突的名字
    /// 规则为：类型名字+_+编号+后缀
    /// </summary>
    /// <param name="_url">路径</param>
    /// <param name="_suffix">后缀名，默认为 "*.asset" </param>
    /// <returns></returns>
    public static string TryGetName<T>(string _url, string _suffix = ".asset")
    {
        //新建初始参数
        string str = "";
        int index = 0;
        UnityEngine.Object obj = null;
        do
        {
            str = _url + "/" + typeof(T).Name + "_" + index + _suffix;//拼接
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(str, typeof(T));//尝试加载
            index++;
        } while (obj);//没有要找的，退出
        return str;//文件名不冲突，返回名字
    }

    /// <summary>
    /// 根据平台返回对应平台的资源存储路径
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformPath()
    {
        string strReturenPlatformPath = string.Empty;
#if UNITY_STANDALONE_WIN
        strReturenPlatformPath = Application.streamingAssetsPath;
#elif UNITY_IPHONE
            strReturenPlatformPath = Application.persistentDataPath;
#elif UNITY_ANDROID
            strReturenPlatformPath = Application.persistentDataPath;
#endif
        return strReturenPlatformPath;
        /*
         * 参考链接
        https://demo.dandelioncloud.cn/article/details/1597031548094464001
         */
    }
#endif
}
