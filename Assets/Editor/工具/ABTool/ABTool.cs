using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// AssetBundle����
/// </summary>
public class ABTool
{
#if UNITY_EDITOR
    [MenuItem("����/ABTool/�޸�����/����ΪNone")]
    static void SetObjectABNull()
    {
        //����ѡ���ļ��������ļ�
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//��ȡ·��
            string fileName = GetFileName(url);//�ж��ļ���
            //���ݷ���ֵ���ַ����Ƿ��ǿ�ֵ
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//��ȡ·��
                _ai.assetBundleName = null;//�ÿ�
            }
        }
    }

    [MenuItem("����/ABTool/�޸�����/����Ϊ��������")]
    static void SetObjectABMyName()
    {
        //����ѡ���ļ��������ļ�
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//��ȡ·��
            string fileName = GetFileName(url);//�ж��ļ���
            //���ݷ���ֵ���ַ����Ƿ��ǿ�ֵ
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//��ȡ·��
                _ai.assetBundleName = null;//�ÿ�
            }
        }
    }

    [MenuItem("����/ABTool/�޸�����/���")]
    static void ABpack()
    {
        string _path = Application.streamingAssetsPath;//��ȡĿ���ļ���·��
        //�ж�·���Ƿ����
        if (!Directory.Exists(_path))
        {//�������½�
            Directory.CreateDirectory(_path);
        }
        //���
        BuildPipeline.BuildAssetBundles(_path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();//ˢ�±�����
    }





    #region ���׵Ĺ��ߺ���
    /// <summary>
    /// ��ȡ�ļ������ּ���չ��
    /// </summary>
    /// <param name="url">�����ļ�������·��</param>
    /// <returns></returns>
    private static string GetFileName(string _url)
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
    #endregion
#endif
}
