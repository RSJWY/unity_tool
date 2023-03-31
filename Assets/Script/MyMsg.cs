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
    private Dictionary<string, Action<Transform>> msg_obj = new Dictionary<string, Action<Transform>>();//消息提示
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    public void Add(string _name, Action<Transform> _func)
    {
        if (msg_dic.ContainsKey(_name))
        {
            msg_obj[_name] += _func;
        }
        else
        {
            msg_obj.Add(_name, _func);
        }
    }
    /// <summary>
    /// 取消事件注册
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    public void Remove(string _name, Action<Transform> _func)
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
    /// 使用事件
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_str">想要发送的信息</param>
    public void Send(string _name, Transform _trans)
    {
        if (msg_obj.ContainsKey(_name))
        {
            msg_obj[_name]?.Invoke(_trans);
        }
    }

    #endregion

    #region 信息提示框
    private Dictionary<string, Action<string>> msg_dic = new Dictionary<string, Action<string>>();//消息提示
    /// <summary>
    /// 注册事件
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    public void Add(string _name, Action<string> _func)
    {
        if (msg_dic.ContainsKey(_name))
        {
            msg_dic[_name] += _func;
        }
        else
        {
            msg_dic.Add(_name, _func);
        }
    }
    /// <summary>
    /// 取消事件注册
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_func">事件函数</param>
    public void Remove(string _name, Action<string> _func)
    {
        if (msg_dic.ContainsKey(_name))
        {
            msg_dic[_name] -= _func;
        }
        if (msg_dic[_name] == null)
        {
            msg_dic.Remove(_name);
        }
    }
    /// <summary>
    /// 使用事件
    /// </summary>
    /// <param name="_name">事件名</param>
    /// <param name="_str">想要发送的信息</param>
    public void Send(string _name, string _str)
    {
        if (msg_dic.ContainsKey(_name))
        {
            msg_dic[_name]?.Invoke(_str);
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
    /// 手持物品——放下
    /// </summary>
    public const string OutHand = "OutHand";
}
