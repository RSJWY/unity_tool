using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// AB工具窗口
/// </summary>
public class ABToolWindow : EditorWindow
{
    [SerializeField]
    List<Object> child_obj = new List<Object>();
    SerializedObject _obj;
    SerializedProperty _prop;

    private void OnEnable()
    {
        _obj = new SerializedObject(this);
        _prop = _obj.FindProperty("child_obj");
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.Label("打包物体");
        _obj.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_prop, true);
        if (EditorGUI.EndChangeCheck())
        {
            _obj.ApplyModifiedProperties();
            Debug.Log(child_obj.Count);
        }

        GUILayout.Space(10);
        GUILayout.Label("退出时自动清空未使用标签");
        GUILayout.Space(10);

        GUILayout.EndVertical();
        if (GUILayout.Button("保存"))
        {
            ABTool.WindowsAB(child_obj.ToArray(),ABTool.ABTOOL.ABPACK);
        }
        if (GUILayout.Button("清空未使用标签"))
        {
            ABTool.WindowsAB(null,ABTool.ABTOOL.ABREMOVENOUSENAME);
        }
    }
    private void OnDisable()
    {
        ABTool.WindowsAB(null, ABTool.ABTOOL.ABREMOVENOUSENAME);
    }
}
