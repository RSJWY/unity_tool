using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class UDPController : MonoBehaviour
{
    public static UDPController instance { get;private set; }

    string commandmsg;

    public Action<string> MsgAction;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        UDPService.instance.UnityUpdate();
        //取出数据
        commandmsg = UDPService.instance.GetMsgData();
        if (commandmsg != null)
        {
            MsgAction?.Invoke(commandmsg);
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
            if (MatchIP(ip) && MatchPort(port))
            {
                UDPService.instance.Init(ip, port);
                return;
            }
        }
        else
        {
            //监听全部IP
            //检查Port是否合法
            if (MatchPort(port))
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

    //版权声明：本文为CSDN博主「牛奶咖啡13」的原创文章，遵循CC 4.0 BY-SA版权协议，转载请附上原文出处链接及本声明。
    //原文链接：https://blog.csdn.net/xiaochenXIHUA/article/details/106199209

    /// <summary>
    /// 匹配IP地址是否合法
    /// </summary>
    /// <param name="ip">当前需要匹配的IP地址</param>
    /// <returns>true:表示合法</returns>
    public static bool MatchIP(string ip)
    {
        bool success = false;
        if (!string.IsNullOrEmpty(ip))
        {
            //判断是否为IP
            success = Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        return success;
    }

    /// <summary>
    /// 匹配端口是否合法
    /// </summary>
    /// <param name="port"></param>
    /// <returns>true：表示合法</returns>
    public static bool MatchPort(int port)
    {
        bool success = false;
        if (port >= 1000 && port <= 65535)
        {
            success = true;
        }
        return success;
    }


    /// <summary>
    /// 检查IP是否可ping通
    /// </summary>
    /// <param name="strIP">要检查的IP</param>
    /// <returns>是否可连通【true:表示可以连通】</returns>
    public static bool CheckIPIsPing(string strIP)
    {
        if (!string.IsNullOrEmpty(strIP))
        {
            if (!MatchIP(strIP))
            {
                return false;
            }
            // Windows L2TP VPN和非Windows VPN使用ping VPN服务端的方式获取是否可以连通
            System.Net.NetworkInformation.Ping pingSender = new ();
            PingOptions options = new PingOptions();

            // 使用默认的128位值
            options.DontFragment = true;

            //创建一个32字节的缓存数据发送进行ping
            string data = "testtesttesttesttesttesttesttest";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(strIP, timeout, buffer, options);

            return (reply.Status == IPStatus.Success);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 连续几次查看是否某个IP可以PING通
    /// </summary>
    /// <param name="strIP">ping的IP地址</param>
    /// <param name="waitMilliSecond">每次间隔时间，单位：毫秒</param>
    /// <param name="testNumber">测试次数</param>
    /// <returns>是否可以连通【true:表示可以连通】</returns>
    public static bool MutiCheckIPIsPing(string strIP, int waitMilliSecond, int testNumber)
    {
        for (int i = 0; i < testNumber - 1; i++)
        {
            if (CheckIPIsPing(strIP))
            {
                return true;
            }
            Thread.Sleep(waitMilliSecond);
        }

        return CheckIPIsPing(strIP);
    }
}