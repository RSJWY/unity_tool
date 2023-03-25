using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// һЩС����
/// </summary>
public class Gadget
{
    [MenuItem("����/С����/ѡ�����������")]
    static void GetSelectionName()
    {
        if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                Debug.Log(Selection.gameObjects[i]);
            }
        }
    }

    [MenuItem("����/С����/��ֳ���������ĸ��ӹ�ϵ")]
    static void GameObjectsName()
    {
        //����ֻ��ѡ��һ��
        if (Selection.gameObjects != null && Selection.gameObjects.Length == 1)
        {
            MatchingFunc.ObjDetech(Selection.gameObjects[0].transform);
        }
        else
        {
            Debug.LogWarning("û��ѡ���������ѡ���˶������ֻѡ��һ��������в�ֲ���");
        }
    }

    [MenuItem("����/С����/��ȡѡ�������·��")]
    static void GetSelectionPath()
    {
        if (Selection.assetGUIDs !=null&&Selection.assetGUIDs.Length>0)
        {
            for (int i = 0; i < Selection.assetGUIDs.Length; i++)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]));
            }
        }
    }

    [MenuItem("����/С����/����ѡ���ļ������������嵽����")]
    static void GetSelectionAllToSence()
    {
        if (Selection.assetGUIDs!=null&&Selection.assetGUIDs.Length==1)
        {
            string str = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);//��ȡ·��
            //��ȡ·���µ�ָ����׺�������ļ�
            string[] path = Directory.GetFiles(str, ".prefab", SearchOption.AllDirectories);
            for (int i = 0; i < path.Length; i++)
            {
                GameObject _obj = AssetDatabase.LoadAssetAtPath<GameObject>(path[i]);//����
                _obj = GameObject.Instantiate(_obj);//����
                AssetDatabase.SaveAssets();//����
                AssetDatabase.Refresh();//ˢ��

                Debug.Log("�Ѽ���" + path[i]);
            }
        }
        else
        {
            Debug.LogWarning("δѡ���ļ��л���ѡ���˶���ļ��У�������ļ���ѡ��");
        }
    }

}
