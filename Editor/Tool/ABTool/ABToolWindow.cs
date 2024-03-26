using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// AB���ߴ���
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
        GUILayout.Label("�������");
        _obj.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_prop, true);
        if (EditorGUI.EndChangeCheck())
        {
            _obj.ApplyModifiedProperties();
            Debug.Log(child_obj.Count);
        }

        GUILayout.Space(10);
        GUILayout.Label("�˳�ʱ�Զ����δʹ�ñ�ǩ");
        GUILayout.Space(10);

        GUILayout.EndVertical();
        if (GUILayout.Button("����"))
        {
            ABTool.WindowsAB(child_obj.ToArray(),ABTool.ABTOOL.ABPACK);
        }
        if (GUILayout.Button("���δʹ�ñ�ǩ"))
        {
            ABTool.WindowsAB(null,ABTool.ABTOOL.ABREMOVENOUSENAME);
        }
    }
    private void OnDisable()
    {
        ABTool.WindowsAB(null, ABTool.ABTOOL.ABREMOVENOUSENAME);
    }
}
