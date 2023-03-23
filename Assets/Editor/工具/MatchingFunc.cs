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
    public static MatchingFunc inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = new MatchingFunc();
            }
            return _inst;
        }
    }
    static MatchingFunc _inst;

#if UNITY_EDITOR
    /// <summary>
    /// ��ȡ�ļ������ּ���չ��
    /// </summary>
    /// <param name="url">�����ļ�������·��</param>
    /// <returns></returns>
    public string GetFileName(string _url)
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
    public void ObjDetech(Transform _obj)
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
#endif
}
