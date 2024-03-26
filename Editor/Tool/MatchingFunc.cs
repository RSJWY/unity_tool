using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor�������׺���
/// </summary>
public class MatchingFunc
{
#if UNITY_EDITOR
    /// <summary>
    /// ��ȡ�ļ������ּ���չ��
    /// </summary>
    /// <param name="url">�����ļ�������·��</param>
    /// <returns></returns>
    public static string GetFileName(string _url)
    {
        string[] arr = _url.Split('/');//���·��,��·���ԡ�/������
        string str = arr[arr.Length - 1];//��λ�����һ��(������׺���Ķ�)
        //�Ƿ������׺�����õ�.
        if (str.Contains("."))
        {
            return str;//���غ�����չ�����ļ���
        }
        return "";//���ؿ�ֵ
    }
    /// <summary>
    /// �ݹ��ָ��ӹ�ϵ
    /// </summary>
    /// <param name="_obj">Ҫ��ֵ�����</param>
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
    /// ���·�����Ƿ������������ļ���������ָ��һ��û���κγ�ͻ������
    /// ����Ϊ����������+_+���+��׺
    /// </summary>
    /// <param name="_url">·��</param>
    /// <param name="_suffix">��׺����Ĭ��Ϊ "*.asset" </param>
    /// <returns></returns>
    public static string TryGetName<T>(string _url, string _suffix = ".asset")
    {
        //�½���ʼ����
        string str = "";
        int index = 0;
        UnityEngine.Object obj = null;
        do
        {
            str = _url + "/" + typeof(T).Name + "_" + index + _suffix;//ƴ��
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(str, typeof(T));//���Լ���
            index++;
        } while (obj);//û��Ҫ�ҵģ��˳�
        return str;//�ļ�������ͻ����������
    }

    /// <summary>
    /// ����ƽ̨���ض�Ӧƽ̨����Դ�洢·��
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
         * �ο�����
        https://demo.dandelioncloud.cn/article/details/1597031548094464001
         */
    }
#endif
}
