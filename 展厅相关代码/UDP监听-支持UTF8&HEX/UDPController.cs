using System;
using System.Net;
using UnityEngine;

public class UDPController : MonoBehaviour
{
    static UDPController _instance;
    public static UDPController instance { get { return _instance; } }

    string commandmsg;

    public Action<string> MsgActionEvent;

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        UDPService.instance.UnityUpdate();
        //取出数据
        commandmsg = UDPService.instance.GetMsgData();
        if (commandmsg != null)
        {
            //有数据，执行事件
            MsgActionEvent?.Invoke(commandmsg);
        }
    }

    public void Init(string ip, int port)
    {
        string lowerip= ip.ToLower();
        //检查是不是监听全部IP
        if (lowerip != "any")
        {
            //指定IP
            //检查IP和Port是否合法
            if (MyTool.MatchIP(ip) && MyTool.MatchPort(port))
            {
                UDPService.instance.Init(ip, port);
                return;
            }
        }
        else
        {
            //监听全部IP
            //检查Port是否合法
            if (MyTool.MatchPort(port))
            {
                UDPService.instance.Init(IPAddress.Any, port);
                return;
            }
        }
        //全部错误则使用默认参数
        UDPService.instance.Init(IPAddress.Any, 56688);
    }

    private void OnDestroy()
    {
        UDPService.instance.Close();
    }
}