using System;
using System.Collections.Generic;
using ClientService;
using UnityEngine;


/// <summary>
/// 客户端的控制器
/// </summary>
public class ClientController : MonoBehaviour
{
    static ClientController _instance;
    public static ClientController instance { get { return _instance; } }

    /// <summary>
    /// 接收到消息后执行的回调
    /// </summary>
    Dictionary<MyProtocolEnum, Action<MsgBase>> receiveMsgCallBack = new Dictionary<MyProtocolEnum, Action<MsgBase>>();
    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }
    void Update()
    {
        TCPClientService.instance.Update();
    }

    /// <summary>
    /// 向服务器发消息
    /// </summary>
    public void SendMsgToServer(MsgBase msgBase)
    {
        TCPClientService.instance.SendMessage(msgBase).Forget();
    }

    /// <summary>
    /// 增加
    /// </summary>
    /// <param name="myProtocolEnum"></param>
    /// <param name="action"></param>
    public void AddReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action<MsgBase> action)
    {
        if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
        {
            receiveMsgCallBack[myProtocolEnum] += action;
        }
        else
        {
            receiveMsgCallBack.Add(myProtocolEnum, action);
        }
    }
    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="myProtocolEnum"></param>
    /// <param name="action"></param>
    public void RemoveReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action<MsgBase> action)
    {
        if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
        {
            receiveMsgCallBack[myProtocolEnum] -= action;
        }
        else
        {
            receiveMsgCallBack.Add(myProtocolEnum, action);
        }
    }
    /// <summary>
    /// 使用
    /// </summary>
    /// <param name="myProtocolEnum"></param>
    /// <param name="msgBase"></param>
    internal void SendReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, MsgBase msgBase)
    {
        if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
        {
            receiveMsgCallBack[myProtocolEnum]?.Invoke(msgBase);
        }
    }

    public void InitClient(string ip, int port)
    {
        //客户端的
        if (ip != "127.0.0.1")
        {
            //指定目标IP
            //检查IP和Port是否合法
            if (MySocketTool.MatchIP(ip) && MySocketTool.MatchPort(port))
            {
                TCPClientService.instance.Connect(ip, port);
                //StartCoroutine(ClientManager.Instance.CheckNetThread());//检测网络状态
                TCPClientService.instance.AsyncCheckNetThread().Forget();
                return;
            }
        }
        else
        {
            //使用默认IP
            //检查Port是否合法
            if (MySocketTool.MatchPort(port))
            {
                TCPClientService.instance.Connect("127.0.0.1", port);
                //StartCoroutine(ClientManager.Instance.CheckNetThread());//检测网络状态
                TCPClientService.instance.AsyncCheckNetThread().Forget();
                return;
            }
        }
        //全部匹配失败，使用默认
        TCPClientService.instance.Connect("127.0.0.1", 9326);//开启链接服务器
        //StartCoroutine(ClientManager.Instance.CheckNetThread());//检测网络状态
        TCPClientService.instance.AsyncCheckNetThread().Forget();

    }
    private void OnEnable()
    {
        TCPClientService.instance.Connect();
    }
    private void OnDisable()
    {
        TCPClientService.instance.Quit();//关闭线程
    }
}

