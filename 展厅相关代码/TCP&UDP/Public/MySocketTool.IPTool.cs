using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;//声明当前的ping是C#的ping，不是unity的ping

/// <summary>
/// IP工具
/// </summary>
public static partial class MySocketTool
{

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
            Ping pingSender = new Ping();
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

