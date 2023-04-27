using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NaughtyAttributes;//编辑器插件，可删除，仅方便编辑一些内容
/// <summary>
/// 材质球赋值
/// </summary>
public class MatAssignmentConfig : MyConfig
{
    public override void InIt(Transform _target)
    {
        base.InIt(_target);
    }




#if UNITY_EDITOR //只有在编辑器下启用
    /// <summary>
    /// 新建一个可视化数据
    /// </summary>
    [MenuItem("Assets/Create/MyConfig/材质球移动配置", false, 0)]
    static void CreateData()
    {
        UnityEngine.Object obj = Selection.activeObject;//检查选中的物体
        if (obj)
        {//为空进入
            string path = AssetDatabase.GetAssetPath(obj);//获取当前的路径名

            ScriptableObject so = CreateInstance<MatAssignmentConfig>();//新建物体
            if (so)
            {
                string str = MyTool.TryGetName<MatAssignmentConfig>(path);//获取名字
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
