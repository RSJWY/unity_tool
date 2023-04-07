using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 更换材质球脚本
/// </summary>
public class ChangeMatConfig : MyConfig
{
    [Header("更换材质球脚本")]

    [Tooltip("材质球列表")]
    public List<Material> mat_lis = new List<Material>();//材质球表
    [Tooltip("材质球节点的缩放比例")]
    public Vector3 scale;
    [Tooltip("材质球弹出速度")]
    public float speed;

    public float dis;
    public MatAssignmentConfig conf;

    bool isshow;

    Transform matroot;//材质球显示的节点
    List<Transform> mat_obj_lis = new List<Transform>();//材质球物体



    public override void InIt(Transform _target)
    {
        base.InIt(_target);//存储关联物体到父物体
        isshow = false;
        mat_obj_lis.Clear();//清空旧数据
        //初始化节点参数
        GameObject mr = new GameObject("MatRoot");//空物体
        mr.transform.SetParent(_target);
        mr.transform.localPosition = Vector3.zero;
        mr.transform.localEulerAngles = Vector3.zero;
        mr.transform.localScale = Vector3.one;
        matroot = mr.transform;//记录节点位置
        //生成用于显示材质的球体
        for (int i = 0; i < mat_lis.Count; i++)
        {
            GameObject show_obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            show_obj.GetComponent<Collider>().isTrigger = true;
            show_obj.GetComponent<MeshRenderer>().material = mat_lis[i];//赋材质
            show_obj.transform.SetParent(matroot);
            //材质球显示物体大小初始化
            show_obj.transform.localPosition = Vector3.zero;
            show_obj.transform.localEulerAngles = Vector3.zero;
            show_obj.transform.localScale = scale;
            show_obj.layer = 9;

            mat_obj_lis.Add(show_obj.transform);

            if (conf!=null)
            {
                CBase c = show_obj.AddComponent<CBase>();
                c.conf = conf;
                c.SetASMat(_target);
            }

            Collider _c = show_obj.GetComponent<Collider>();
            _c.enabled = false;
        }
        matroot.gameObject.SetActive(false);
    }

    public void ShowStart()
    {
        isshow = !isshow;
        if (isshow)
        {
            matroot.gameObject.SetActive(true);

            //计算排列位置
            float x = 0;
            if (mat_obj_lis.Count % 2 == 0)
            {
                x = -dis / 2 - (mat_obj_lis.Count / 2 - 1);
            }
            else
            {
                x = -(mat_obj_lis.Count / 2) * dis;
            }

            for (int i = 0; i < mat_obj_lis.Count; i++)
            {
                Transform _t = mat_obj_lis[i];
                Tween _dtw = _t.DOLocalMove(new Vector3(0, 1, x + dis * i), speed);
                _dtw.SetEase(Ease.OutElastic);
                _dtw.onComplete = () =>
                {
                    for (int j = 0; j < mat_obj_lis.Count; j++)
                    {
                        Collider _c = mat_obj_lis[j].GetComponent<Collider>();
                        _c.enabled = true;
                    }
                };
            }
        }
        else
        {
            for (int j = 0; j < mat_obj_lis.Count; j++)
            {
                Collider _c = mat_obj_lis[j].GetComponent<Collider>();
                _c.enabled = false;
            }
            for (int i = 0; i < mat_obj_lis.Count; i++)
            {
                Transform _t = mat_obj_lis[i];
                Tween _dtw = _t.DOLocalMove(new Vector3(0, 0,0), speed);
                _dtw.SetEase(Ease.InElastic);
                _dtw.onComplete = () =>
                {
                    matroot.gameObject.SetActive(false);
                    
                };
            }
        }
    }



#if UNITY_EDITOR //只有在编辑器下启用
    /// <summary>
    /// 新建一个可视化数据
    /// </summary>
    [MenuItem("Assets/Create/MyConfig/更换材质球", false, 0)]
    static void CreateData()
    {
        UnityEngine.Object obj = Selection.activeObject;//检查选中的物体
        if (obj)
        {//为空进入
            string path = AssetDatabase.GetAssetPath(obj);//获取当前的路径名

            ScriptableObject so = CreateInstance<ChangeMatConfig>();//新建物体
            if (so)
            {
                string str = MyTool.TryGetName<ChangeMatConfig>(path);//获取名字
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
