using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NaughtyAttributes;//编辑器插件，可删除，仅方便编辑一些内容

/// <summary>
/// 配置文件总类
/// </summary>
public class MyConfig : ScriptableObject
{
    [Header("配置文件")]

    [Tooltip("名字")]
    public string name;


    [Tooltip("是否高亮")]
    public bool isHighlight;//高亮
    [Tooltip("是否手持")]
    public bool isInHandPos;//手持
    [ShowIf("isInHandPos")][Tooltip("手持位置")]
    public Vector3 inHandPos;
    [Tooltip("是否旋转")]
    public bool isInHandAngle;
    [ShowIf("isInHandAngle")][Tooltip("手持旋转角")]
    public Vector3 inHandAngle;
    [Tooltip("是否可以被拾取")]
    public bool isCanHand;//是否可以被拾取
    [Tooltip("是否可以被点击")]
    public bool isCanClick;//是否可以被点击

    Transform target;//关联的物体
    public virtual void InIt(Transform _target)
    {
        target = _target;
    }


#if UNITY_EDITOR //只有在编辑器下启用
    /// <summary>
    /// 新建一个可视化数据
    /// </summary>
    [MenuItem("Assets/Create/MyConfig/基础配置文件", false, 0)]
    static void CreateData()
    {
        UnityEngine.Object obj = Selection.activeObject;//检查选中的物体
        if (obj)
        {//为空进入
            string path = AssetDatabase.GetAssetPath(obj);//获取当前的路径名

            ScriptableObject so = CreateInstance<MyConfig>();//新建物体
            if (so)
            {
                string str = MyTool.TryGetName<MyConfig>(path);//获取名字
                AssetDatabase.CreateAsset(so, str);//实例化物体
                AssetDatabase.SaveAssets();//保存
            }
            else
            {
                Debug.LogError(typeof(MyConfig) + "is null" + "\n" + "要新建的模板为空，请检查");
            }
        }
    }


#endif
}
