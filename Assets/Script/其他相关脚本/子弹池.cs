using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏对象池
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool inst;


    /// <summary>
    /// 对象池数量
    /// </summary>
    int num=50;
    /// <summary>
    /// 子弹池节点
    /// </summary>
    GameObject bullet_root;
    /// <summary>
    /// 敌人池节点
    /// </summary>
    GameObject enemy_root;
    /// <summary>
    /// 工具池节点
    /// </summary>
    GameObject tool_root;

    /// <summary>
    /// 读取到的resources数据
    /// </summary>
    GameObject bullet1_res;
    GameObject bullet2_res;
    GameObject bullet3_res;
    GameObject enemy1_res;
    GameObject enemy2_res;
    GameObject boom;
    GameObject ScoreXING;

    Dictionary<ButtetState, List<GameObject>> bullet_dic = new Dictionary<ButtetState, List<GameObject>>();

    Dictionary<EnemyState, List<GameObject>> enemy_dic = new Dictionary<EnemyState, List<GameObject>>();

    Dictionary<ToolState, List<GameObject>> tool_dic = new Dictionary<ToolState, List<GameObject>>();

    /// <summary>
    /// 共用位数
    /// </summary>
    int n;


    private void Awake()
    {
        inst = this;
        LoadResources();
        //创建空节点
        bullet_root = new GameObject("子弹对象池");
        bullet_root.transform.SetParent(transform);
        
        //遍历枚举
        foreach (ButtetState item in Enum.GetValues(typeof(ButtetState)))
        {
            BulletReady(item);
        }
        enemy_root = new GameObject("敌人对象池");
        enemy_root.transform.SetParent(transform);
        foreach (EnemyState item in Enum.GetValues(typeof(ButtetState)))
        {
            BulletReady(item);
        }
        tool_root = new GameObject("Tool对象池");
        tool_root.transform.SetParent(transform);
        foreach (ToolState item in Enum.GetValues(typeof(ToolState)))
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

    public void SetYinCang()
    {
        foreach (KeyValuePair<ButtetState,List<GameObject>> item in bullet_dic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].SetActive(false);
            }
        }
        foreach (var item in enemy_dic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].SetActive(false);
            }
        }
        foreach (var item in tool_dic)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                item.Value[i].SetActive(false);
            }
        }
    }
    


    #region 使用
    /// <summary>
    /// 获取空闲物体
    /// </summary>
    /// <returns></returns>
    public GameObject Get(ButtetState _state)
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
    /// 获取空闲物体
    /// </summary>
    /// <returns></returns>
    public GameObject Get(EnemyState _state)
    {
        GameObject _temp;
        _temp = enemy_dic[_state][n];
        n++;
        if (n > num - 1 || n < 0)
        {
            n = 0;
        }
        return _temp;
    }
    /// <summary>
    /// 获取空闲物体
    /// </summary>
    /// <returns></returns>
    public GameObject Get(ToolState _state)
    {
        GameObject _temp;
        _temp = tool_dic[_state][n];
        n++;
        if (n > num - 1 || n < 0)
        {
            n = 0;
        }
        return _temp;
    }

    ///// <summary>
    ///// 获取这个子弹的父类
    ///// </summary>
    ///// <returns></returns>
    //public Transform GetParent(ButtetState _state)
    //{
    //    Transform _temp;
    //    _temp = bullet_dic[_state][0].transform.parent;
    //    return _temp;

    //}
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
    /// 子弹类型区分
    /// </summary>
    /// <param name="_state"></param>
    void BulletReady(EnemyState _state)
    {
        switch (_state)
        {
            case EnemyState.BIGFLLY:
                CopyBullet(EnemyState.BIGFLLY, enemy2_res);
                break;
            case EnemyState.SMALLFLY:
                CopyBullet(EnemyState.SMALLFLY, enemy1_res);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 子弹类型区分
    /// </summary>
    /// <param name="_state"></param>
    void BulletReady(ToolState _state)
    {
        switch (_state)
        {
            case ToolState.BOOM:
                CopyBullet(ToolState.BOOM, boom);
                break;
            case ToolState.ScoreXING:
                CopyBullet(ToolState.ScoreXING, ScoreXING);
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
        _temp_root.transform.SetParent(bullet_root.transform);
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
    /// 复制子弹
    /// </summary>
    /// <param name="_state"></param>
    /// <param name="_obj"></param>
    void CopyBullet(EnemyState _state, GameObject _obj)
    {
        List<GameObject> _temp_lis = new List<GameObject>();
        GameObject _temp_root = new GameObject(_state.ToString());
        _temp_root.transform.SetParent(enemy_root.transform);
        for (int i = 0; i < num; i++)
        {
            GameObject _temp_obj = Instantiate(_obj);
            _temp_obj.name = i.ToString();
            _temp_obj.SetActive(false);
            _temp_obj.transform.SetParent(_temp_root.transform);
            _temp_lis.Add(_temp_obj);
        }
        enemy_dic.Add(_state, _temp_lis);

    }
    /// <summary>
    /// 复制子弹
    /// </summary>
    /// <param name="_state"></param>
    /// <param name="_obj"></param>
    void CopyBullet(ToolState _state, GameObject _obj)
    {
        List<GameObject> _temp_lis = new List<GameObject>();
        GameObject _temp_root = new GameObject(_state.ToString());
        _temp_root.transform.SetParent(tool_root.transform);
        for (int i = 0; i < num; i++)
        {
            GameObject _temp_obj = Instantiate(_obj);
            _temp_obj.name = i.ToString();
            _temp_obj.SetActive(false);
            _temp_obj.transform.SetParent(_temp_root.transform);
            _temp_lis.Add(_temp_obj);
        }
        tool_dic.Add(_state, _temp_lis);

    }

    /// <summary>
    /// 加载预制体
    /// </summary>
    void LoadResources()
    {
        bullet1_res = Resources.Load<GameObject>("Bullet/bullet1");
        bullet2_res = Resources.Load<GameObject>("Bullet/bullet2");
        bullet3_res = Resources.Load<GameObject>("Bullet/bullet3");

        enemy1_res = Resources.Load<GameObject>("Enemy/Enemy1");
        enemy2_res = Resources.Load<GameObject>("Enemy/Enemy2");

        boom = Resources.Load<GameObject>("Tool/boom");
        ScoreXING = Resources.Load<GameObject>("Tool/Score");
    }
    #endregion




}
