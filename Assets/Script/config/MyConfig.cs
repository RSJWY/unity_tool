using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 配置文件总类
/// </summary>
public class MyConfig : ScriptableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    public string name;
    public bool isHighlight;//高亮
    public bool isInHandPos;//手持
    public Vector3 inHandPos;
    public bool isInHandAngle;
    public Vector3 inHandAngle;

#if UNITY_EDITOR //只有在编辑器下启用
    /// <summary>
    /// 新建一个可视化数据
    /// </summary>
    [MenuItem("Assets/Create/MyConfig", false, 0)]
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
