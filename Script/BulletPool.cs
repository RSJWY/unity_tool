using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹池
/// </summary>
public class BulletPool : MonoBehaviour
{
    public static BulletPool inst;
    /// <summary>
    /// 子弹数
    /// </summary>
    public int num=100;
    
    /// <summary>
    /// 子弹池节点
    /// </summary>
    GameObject root;
    /// <summary>
    /// 子弹类型
    /// </summary>
    ButtetState state;


    GameObject bullet1_res;
    GameObject bullet2_res;
    GameObject bullet3_res;

    Dictionary<ButtetState, List<GameObject>> bullet_dic = new Dictionary<ButtetState, List<GameObject>>();

    /// <summary>
    /// 当前子弹的已用数位
    /// </summary>
    int n;


    private void Awake()
    {
        inst = this;
        //创建空节点
        root = new GameObject("RootBullet");
        root.transform.SetParent(transform);
        LoadResources();
        //遍历枚举
        foreach (ButtetState item in Enum.GetValues(typeof(ButtetState)))
        {
            BulletReady(item);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //BulletReady(Player.inst.state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    


    #region 使用
    /// <summary>
    /// 获取空闲物体
    /// </summary>
    /// <returns></returns>
    public GameObject GetBullet(ButtetState _state)
    {
        GameObject _temp;
        _temp = bullet_dic[_state][n];
        n++;
        if (n > num - 1 || n < 0)
        {
            n = 0;
        }
        return _temp;
    }


    /// <summary>
    /// 获取这个子弹的父类
    /// </summary>
    /// <returns></returns>
    public Transform GetParent(ButtetState _state)
    {
        Transform _temp;
        _temp = bullet_dic[_state][0].transform.parent;
        return _temp;

    }
    #endregion

    #region 子弹池准备

    /// <summary>
    /// 子弹类型区分
    /// </summary>
    /// <param name="_state"></param>
    void BulletReady(ButtetState _state)
    {
        switch (_state)
        {
            case ButtetState.STRAIGHT:
                CopyBullet(ButtetState.STRAIGHT, bullet1_res);
                break;
            case ButtetState.SHOT:
                CopyBullet(ButtetState.SHOT, bullet2_res);
                break;
            case ButtetState.ZHADAN:
                CopyBullet(ButtetState.ZHADAN, bullet3_res);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 复制子弹
    /// </summary>
    /// <param name="_state"></param>
    /// <param name="_obj"></param>
    void CopyBullet(ButtetState _state, GameObject _obj)
    {
        List<GameObject> _temp_lis = new List<GameObject>();
        GameObject _temp_root = new GameObject(_state.ToString());
        _temp_root.transform.SetParent(root.transform);
        for (int i = 0; i < num; i++)
        {
            GameObject _temp_obj = Instantiate(_obj);
            _temp_obj.name = i.ToString();
            _temp_obj.SetActive(false);
            _temp_obj.transform.SetParent(_temp_root.transform);
            _temp_lis.Add(_temp_obj);
        }
        bullet_dic.Add(_state, _temp_lis);

    }

    /// <summary>
    /// 加载预制体
    /// </summary>
    void LoadResources()
    {
        bullet1_res = Resources.Load<GameObject>("Bullet/bullet1");
        bullet2_res = Resources.Load<GameObject>("Bullet/bullet2");
        bullet3_res = Resources.Load<GameObject>("Bullet/bullet3");
    }
    #endregion



    
}
