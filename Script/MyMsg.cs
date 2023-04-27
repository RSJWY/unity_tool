using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 消息管理
/// </summary>
public class MyMsg
{
    private static MyMsg _inst;

   
    public static MyMsg inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = new MyMsg();
            }
            return _inst;
        }
    }

    #region 拾取与放下
    private Dictionary<string, Action<Transform, Transform>> msg_obj = new Dictionary<string, Action<Transform, Transform>>();//事件库
    /// <summary>
    /// 注册事件-拾取与放下
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    /// <para>需要接收手柄碰到的物体和信号发出物体</para>
    public void Add(string _name, Action<Transform, Transform> _func)
    {
        if (msg_obj.ContainsKey(_name))
        {
            msg_obj[_name] += _func;
        }
        else
        {
            msg_obj.Add(_name, _func);
        }
    }
    /// <summary>
    /// 取消事件注册-拾取与放下
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param> 
    public void Remove(string _name, Action<Transform, Transform> _func)
    {
        if (msg_obj.ContainsKey(_name))
        {
            msg_obj[_name] -= _func;
        }
        if (msg_obj[_name] == null)
        {
            msg_obj.Remove(_name);
        }
    }
    /// <summary>
    /// 使用事件-拾取与放下
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_trans">目标物</param>
    /// <param name="signalsource">消息源</param>
    public void Send(string _name, Transform _trans, Transform signalsource)
    {
        if (msg_obj.ContainsKey(_name))
        {
            msg_obj[_name]?.Invoke(_trans,signalsource);
        }
    }

    #endregion

    #region 信息提示框
    private Dictionary<string, Action<string>> msg_info_dic = new Dictionary<string, Action<string>>();//消息提示
    /// <summary>
    /// 注册事件-信息提示框
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    public void Add(string _name, Action<string> _func)
    {
        if (msg_info_dic.ContainsKey(_name))
        {
            msg_info_dic[_name] += _func;
        }
        else
        {
            msg_info_dic.Add(_name, _func);
        }
    }
    /// <summary>
    /// 取消事件注册-信息提示框
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    public void Remove(string _name, Action<string> _func)
    {
        if (msg_info_dic.ContainsKey(_name))
        {
            msg_info_dic[_name] -= _func;
        }
        if (msg_info_dic[_name] == null)
        {
            msg_info_dic.Remove(_name);
        }
    }
    /// <summary>
    /// 使用事件-信息提示框
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_str">想要发送的信息</param>
    public void Send(string _name, string _str)
    {
        if (msg_info_dic.ContainsKey(_name))
        {
            msg_info_dic[_name]?.Invoke(_str);
        }
    }
    #endregion
}
/// <summary>
/// 事件名称
/// </summary>
public class MsgEventName
{
    /// <summary>
    /// 提示信息
    /// </summary>
    public const string Tips = "Tips";
    /// <summary>
    /// 手持物品-拿起
    /// </summary>
    public const string InHand = "InHand";
    /// <summary>
    /// 手持物品—放下
    /// </summary>
    public const string OutHand = "OutHand";
    /// <summary>
    /// 高亮—打开
    /// </summary>
    public const string InHigLig = "InHigLig";
    /// <summary>
    /// 高亮—关闭
    /// </summary>
    public const string OutHigLig = "OutHigLig";
    /// <summary>
    /// 点击
    /// </summary>
    public const string InOnClick = "InOnClick";
}
