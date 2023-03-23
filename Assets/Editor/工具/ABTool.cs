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
    static void SetObjectAB()
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
                //���������ú�
                //�жϻ�ȡ���ļ�����ƴ�Ӻ��Ƿ���ͬ
                if (_ai.assetBundleName != fileName + ".assetbundle")
                {//����ͬǿ�Ƹ�ֵһ��
                    _ai.assetBundleName = fileName + ".assetbundle";
                }
            }
        }
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
