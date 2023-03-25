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

    static Object[] objarr = null;

    [MenuItem("����/ABTool/���ӻ����")]
    static void WindowABPack()
    {
        EditorWindow.GetWindowWithRect<ABToolWindow>(new Rect(200, 200, 500, 500), false, "���ӻ����");
    }

    [MenuItem("����/ABTool/�޸�����/����ΪNone,�����δʹ������")]
    static void SetObjectABNull()
    {
        //����ѡ���ļ��������ļ�
        Object[] objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//��ȡ·��
            string fileName = MatchingFunc.GetFileName(url);//�ж��ļ���
            //���ݷ���ֵ���ַ����Ƿ��ǿ�ֵ
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//��ȡ·��
                _ai.assetBundleName = null;//�ÿ�
            }
        }
        Debug.Log("�����������");
        RemoveABNmae();
    }

    [MenuItem("����/ABTool/�޸�����/����Ϊ��������")]
    static void SetObjectABMyName()
    {
        //����ѡ���ļ��������ļ�
        objarr = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        for (int i = 0; i < objarr.Length; i++)
        {
            string url = AssetDatabase.GetAssetPath(objarr[i]);//��ȡ·��
            string fileName = MatchingFunc.GetFileName(url);//�ж��ļ���
            //���ݷ���ֵ���ַ����Ƿ��ǿ�ֵ
            if (!string.IsNullOrEmpty(fileName))
            {
                AssetImporter _ai = AssetImporter.GetAtPath(url);//��ȡ·��
                _ai.assetBundleName = null;//�ÿ�
            }
        }
        Debug.Log("�������");
    }

    [MenuItem("����/ABTool/���")]
    static void ABpack()
    {
        string _path = MatchingFunc.GetPlatformPath();//��ȡĿ���ļ���·��
        //�ж�·���Ƿ����
        if (!Directory.Exists(_path))
        {//�������½�
            Directory.CreateDirectory(_path);
        }
        //���
        BuildPipeline.BuildAssetBundles(_path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();//ˢ�±�����
        Debug.Log("������");
    }

    [MenuItem("����/ABTool/���δʹ�ñ�ǩ")]
    static void RemoveABNmae()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();//���δʹ�õı�ǩ
        AssetDatabase.Refresh();//ˢ��
        Debug.Log("δʹ�ñ�ǩ������");
    }

    /// <summary>
    /// ���ӻ�������Ӻ���
    /// </summary>
    /// <param name="_objarr">�ļ�����</param>
    /// <param name="fungcname">Ҫִ�еĺ���</param>
    public static void WindowsAB(Object[] _objarr = null, ABTOOL _fungcname =default)
    {
        switch (_fungcname)
        {
            case ABTOOL.ABPACK://ִ�д������
                objarr = _objarr;
                SetObjectABMyName();
                ABpack();
                break;
            case ABTOOL.ABREMOVENOUSENAME://ɾ��δʹ�õı�ǩ
                RemoveABNmae();
                break;
            default:
                break;
        }
    }

    public enum ABTOOL
    {
        /// <summary>
        /// ���
        /// </summary>
        ABPACK,
        /// <summary>
        /// ɾ��δʹ�õ�AB��ǩ
        /// </summary>
        ABREMOVENOUSENAME
    }
#endif
}
