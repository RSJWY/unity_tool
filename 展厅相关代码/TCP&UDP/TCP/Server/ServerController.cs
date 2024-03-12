using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using ServerService;
using UnityEngine;


/// <summary>
/// 服务器模块管理器，用于和Unity之间交互
/// </summary>
public class  ServerController : MonoBehaviour
{
    public static ServerController instance { get;private set; }
    /// <summary>
    /// 接收到消息后执行的回调
    /// </summary>
    Dictionary<MyProtocolEnum, Action<ClientSocket,MsgBase>> receiveMsgCallBack = new();

    // Start is called before the first frame update
    void Awake()
    {
        instance=this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        TCPServerService.instance.TCPUpdate();
    }

    /// <summary>
    /// 向客户端发消息
    /// </summary>
    /// <param name="msgBase">消息类</param>
    /// <param name="client">客户端信息</param>
    public void SendMsgToClient(MsgBase msgBase,Socket client)
    {
        TCPServerService.instance.SendMessage(msgBase, client).Forget();
    }


    /// <summary>
    /// 增加协议事件
    /// </summary>
    /// <param name="myProtocolEnum"></param>
    /// <param name="action"></param>
    public void  AddReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action< ClientSocket,MsgBase> action)
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
    /// 移除协议事件
    /// </summary>
    /// <param name="myProtocolEnum"></param>
    /// <param name="action"></param>
    public void RemoveReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action<ClientSocket, MsgBase> action)
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
    /// 响应事件
    /// </summary>
    /// <param name="myProtocolEnum"></param>
    /// <param name="msgBase"></param>
    internal void SendReceiveMsgCallBack(MyProtocolEnum myProtocolEnum,ClientSocket serverClientSocket, MsgBase msgBase)
    {
        if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
        {
            receiveMsgCallBack[myProtocolEnum]?.Invoke(serverClientSocket, msgBase);
        }
    }

    public void InitServer(string ip,int port)
    {
        //检查是不是监听全部IP
        if (ip!="any")
        {
            //指定IP
            //检查IP和Port是否合法
            if (MySocketTool.MatchIP(ip)&&MySocketTool.MatchPort(port))
            {
                TCPServerService.instance.Init(ip, port);
                return;
            }
        }
        else
        {
            //监听全部IP
            //检查Port是否合法
            if (MySocketTool.MatchPort(port))
            {
                TCPServerService.instance.Init(IPAddress.Any, port);
                return;
            }
        }
        //全部错误则使用默认参数
        TCPServerService.instance.Init(IPAddress.Any, 9326);
    }

    private void OnEnable()
    {
        TCPServerService.instance.Init();
    }
    private void OnDisable()
    {
        TCPServerService.instance.Quit();//关闭线程
    }
}
