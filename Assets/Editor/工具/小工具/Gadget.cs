using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 一些小工具
/// </summary>
public class Gadget
{
    [MenuItem("工具/小工具/选中物体的名字")]
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

    [MenuItem("工具/小工具/拆分场景内物体的父子关系")]
    static void GameObjectsName()
    {
        //限制只能选择一个
        if (Selection.gameObjects != null && Selection.gameObjects.Length == 1)
        {
            MatchingFunc.ObjDetech(Selection.gameObjects[0].transform);
        }
        else
        {
            Debug.LogWarning("没有选择物体或者选择了多个，请只选择一个物体进行拆分操作");
        }
    }

    [MenuItem("工具/小工具/获取选中物体的路径")]
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

    [MenuItem("工具/小工具/加载选中文件夹内所有物体到场景")]
    static void GetSelectionAllToSence()
    {
        if (Selection.assetGUIDs!=null&&Selection.assetGUIDs.Length==1)
        {
            string str = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);//获取路径
            //获取路径下的指定后缀的所有文件
            string[] path = Directory.GetFiles(str, ".prefab", SearchOption.AllDirectories);
            for (int i = 0; i < path.Length; i++)
            {
                GameObject _obj = AssetDatabase.LoadAssetAtPath<GameObject>(path[i]);//加载
                _obj = GameObject.Instantiate(_obj);//复制
                AssetDatabase.SaveAssets();//保存
                AssetDatabase.Refresh();//刷新

                Debug.Log("已加载" + path[i]);
            }
        }
        else
        {
            Debug.LogWarning("未选择文件夹或者选择了多个文件夹，请逐个文件夹选择");
        }
    }

}
