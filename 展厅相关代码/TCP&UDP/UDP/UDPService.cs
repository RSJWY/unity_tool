using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class UDPService
{
    static object thisLock = new();
    static UDPService _instance;
    public static UDPService instance
    {
        get
        {
            if (_instance == null)
            {
                lock (thisLock)
                {
                    if (_instance == null)
                    {
                        _instance = new();
                    }
                }
            }
            return _instance;
        }
    }
    /// <summary>
    /// 监听端口
    /// </summary>
    int port = 6003;
    /// <summary>
    /// 监听IP
    /// </summary>
    IPAddress ip = IPAddress.Any;
    /// <summary>
    /// UDP Socket
    /// </summary>
    Socket udpClient;
    /// <summary>
    /// 存储消息来源客户端信息
    /// </summary>
    EndPoint clientEndPoint;
    /// <summary>
    /// 存储接收到的数据
    /// </summary>
    byte[] ReceiveData = new byte[1024];
    /// <summary>
    /// 接收到的消息数组队列
    /// </summary>
    ConcurrentQueue<byte[]> reciveByteMsgQueue = new ConcurrentQueue<byte[]>();

    /// <summary>
    /// 处理完毕的待执行的消息队列
    /// </summary>
    ConcurrentQueue<string> MsgQueue = new();
    /// <summary>
    /// 消息发送队列
    /// </summary>
    ConcurrentQueue<byte[]> SendMsgQueue = new ConcurrentQueue<byte[]>();
    /// <summary>
    /// 通知多线程自己跳出
    /// </summary>
    bool isThreadOver;
    /// <summary>
    /// 是否初始化过
    /// </summary>
    bool isInit;
    /// <summary>
    /// 消息处理线程
    /// </summary>
    Thread MsgThread;
    /// <summary>
    /// 消息发送线程
    /// </summary>
    Thread sendMsgThread;

    /// <summary>
    /// 传入指定IP
    /// </summary>
    /// <param name="_ip"></param>
    /// <param name="_port"></param>
    public void Init(string _ip, int _port)
    {
        ip = IPAddress.Parse(_ip);
        port = _port;
        Init();
    }
    /// <summary>
    /// 使用设置好的IPAddress
    /// </summary>
    /// <param name="_ipAddress"></param>
    /// <param name="_port"></param>
    public void Init(IPAddress _ipAddress, int _port)
    {
        ip = _ipAddress;
        port = _port;
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_port"></param>
    public void Init()
    {
        if (isInit)
        {
            return;
        }

        try
        {
            isThreadOver = false;
            udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //配置监听
            IPEndPoint ipendpoint = new IPEndPoint(ip, port);
            udpClient.Bind(ipendpoint);


            clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            //开启异步监听
            udpClient.BeginReceiveFrom(ReceiveData, 0, ReceiveData.Length,
                SocketFlags.None, ref clientEndPoint, ReceiveMessageToQueue, clientEndPoint);

            MsgThread = new Thread(Msghandle);
            MsgThread.IsBackground = true;//后台运行
            MsgThread.Start();
            //sendMsgThread = new Thread(SendMsgHandle);
            //sendMsgThread.IsBackground = true;//后台运行
            //sendMsgThread.Start();

            isInit = true;
            Debug.Log($"UDP启动监听{udpClient.LocalEndPoint.ToString()}成功");
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP启动监听 {udpClient.LocalEndPoint.ToString()} 失败！！错误信息：\n {e.ToString()}");
            isThreadOver = true;
        }
    }

    /// <summary>
    /// 接收消息并存放 
    /// </summary>
    /// <param name="iar"></param>
    void ReceiveMessageToQueue(IAsyncResult iar)
    {
        if (isThreadOver)
        {
            return;
        }
        try
        {
            int receiveDataLength = udpClient.EndReceiveFrom(iar, ref clientEndPoint);
            //如果有数据
            if (receiveDataLength > 0)
            {
                byte[] data = new byte[receiveDataLength];
                Buffer.BlockCopy(ReceiveData, 0, data, 0, receiveDataLength);
                reciveByteMsgQueue.Enqueue(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP启动监听 {udpClient.LocalEndPoint.ToString()} 失败！！错误信息：\n {e.ToString()}");
        }
        finally
        {
            //继续监听
            udpClient.BeginReceiveFrom(ReceiveData, 0, ReceiveData.Length,
                SocketFlags.None, ref clientEndPoint, ReceiveMessageToQueue, clientEndPoint);
        }
    }
    /// <summary>
    /// 关闭监听
    /// </summary>
    public void Close()
    {
        isThreadOver = true;
        if (udpClient != null)
        {
            udpClient.Close();
        }
        isInit = false;
    }
    /// <summary>
    /// 数据处理
    /// </summary>
    internal void Msghandle()
    {
        while (true)
        {
            if (isThreadOver)
            {
                return;
            }
            if (!reciveByteMsgQueue.IsEmpty)
            {
                //取出数据
                byte[] _data;
                reciveByteMsgQueue.TryDequeue(out _data);
                //处理数据，分别尝试不同协议
                //string _strASCII = Encoding.ASCII.GetString(_data);
                string _strUTF8 = Encoding.UTF8.GetString(_data);
                string _strHex = BitConverter.ToString(_data);

                string _utf8 = _strUTF8.Replace(" ", "");
                string _hex = _strHex.Replace("-", " ");
                //Debug.Log($"UTF8:{IsHex(_utf8)}，HEX:{IsHex(_hex)}");
                //优先检查UTF8
                if (IsHex(_utf8))
                {
                    //UTF8是正确的指令
                    MsgQueue.Enqueue(_strUTF8);
                }
                else
                {
                    //Hex任何时刻都是16进值，则传出，交给使用者判断
                    MsgQueue.Enqueue(_hex);
                }
            }
        }
    }

    public string GetMsgData()
    {
        if (MsgQueue.Count <= 0)
        {
            return null;
        }
        string msg;
        MsgQueue.TryDequeue(out msg);
        return msg;
    }
    public void UnityUpdate()
    {
        //线程是否出错
        if (MsgThread != null)
        {
            if (MsgThread.IsAlive == false && isInit == true)
            {
                //重新创建本线程
                MsgThread = new Thread(Msghandle);
                MsgThread.IsBackground = true;//后台运行
                MsgThread.Start();
            }
        }
    }
    void SendMsgHandle()
    {
        if (isThreadOver)
        {
            return;
        }
    }
    public static bool IsHex(string input)
    {
        // 正则表达式匹配16进制值  
        string hexPattern = @"^[0-9A-Fa-f]+$";
        return Regex.IsMatch(input, hexPattern);
    }

}
