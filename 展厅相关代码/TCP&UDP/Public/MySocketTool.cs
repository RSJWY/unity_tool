using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Linq;

/// <summary>
/// Socket共用工具
/// </summary>
public static partial class MySocketTool
{
    /// <summary>
    /// 设置时区的信息
    /// </summary>
    static TimeZoneInfo timeZoneInfo;


    /// <summary>
    /// 消息加密密钥
    /// </summary>
    internal static readonly string MsgKey = "Wqu6Rp@+UE7svIuTaI^Phtiu=Ze*1^NW%Tn6iFn+#GN#5qmxs5Gl%8$98wPulGp1";


    static MySocketTool()
    {
        ////获取所支持的时区
        //var timeZones = TimeZoneInfo.GetSystemTimeZones();
        //foreach (TimeZoneInfo timeZone in timeZones)
        //{
        //    Debug.Log(timeZone.Id);
        //}
        //时区设置
        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
    }

    /// <summary>
    /// 获取当前时间戳
    /// </summary>
    /// <returns>时间戳</returns>
    public static long GetTimeStamp()
    {

        //根据当前时区计算时间戳
        //TimeSpan ts = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo) - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); 
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;  
        long timestamp = nowUtc.ToUnixTimeSeconds();
        return Convert.ToInt64(timestamp);
    }
    /// <summary>
    /// 时间误差检测，仅用作提示
    /// </summary>
    /// <param name="ServetOrClinet">服务器还是客户端，用作打印输出区分</param>
    /// <param name="NowTimeStamp">当前的时间戳</param>
    /// <param name="msgPing">心跳包类</param>
    public static void TimeDifference(string ServetOrClinet, long LocalTimeStamp,MsgBase msgPing)
    {
        MsgPing _msgPing= msgPing as MsgPing;
        long RemoteTimeStamp = _msgPing.timeStamp;//获取远端传来的时间戳
        if (RemoteTimeStamp <= 0)
        {
            Debug.LogErrorFormat("传入的值小于等于0，处理无效");
            return;
        }
        //根据时间戳计算时间
        DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime RemoteTime = startTime.AddSeconds(RemoteTimeStamp);//远端时间戳
        DateTime LocalTime = startTime.AddSeconds(LocalTimeStamp);//当前时间戳

        //计算误差
        long pingPongTimeStamp = LocalTimeStamp - RemoteTimeStamp;

        Debug.LogFormat($"{ServetOrClinet}消息：\n本地时间：{LocalTime}，远端时间：{RemoteTime}，时间戳时间差为{pingPongTimeStamp}");
        //Debug.LogFormat("{0}消息：\n本地时间：{3}远端时间为：{1}，\n和远端时间误差为：{2}s，请注意时间误差！！", ServetOrClinet, Remotetime, pingPongTime, TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo));
    }

    /// <summary>
    /// 反序列化本次的消息
    /// </summary>
    /// <param name="byteArray">存储消息的自定义数组</param>
    /// <param name="msgQueue">消息处理完后放入的队列</param>
    /// <param name="targetSocket">本条消息所属客户端</param>
    /// <param name="errorAction">发生错误时的回调</param>
    internal static void DecodeMsg(ByteArray byteArray,ConcurrentQueue<MsgBase> msgQueue,Socket targetSocket,Action errorAction)
    {
        //确认数据是否有误
        if (byteArray.length <= 4 || byteArray.ReadIndex < 0)
        {
            //前四位是记录整体数据长度
            Debug.LogError($"发生异常，允许读数据长度小于等于4或者开始读索引小于0，无法对数据处理");
            return;
        }
        //数据无误
        int readIndex = byteArray.ReadIndex;
        byte[] bytes = byteArray.Bytes;
        //从数组前四位获取数据长度
        int msgLength = BitConverter.ToInt32(bytes, readIndex);
        //判断是不是分包数据
        if (byteArray.length < msgLength + 4)
        {
            //如果消息长度小于读出来的消息长度
            //此为分包，不包含完整数据
            //因为产生了分包，可能容量不足，根据目标大小进行扩容到接收完整
            //扩容后，retun，继续接收

            byteArray.MoveBytes();//已经完成一轮解析，移动数据
            byteArray.ReSize(msgLength + 8);//扩容，扩容的同时，保证长度信息也能被存入
            return;
        }
        //接收完整，存在完整数据

        //协议名解析
        byteArray.ReadIndex += 4;//前四位存储字节流数组长度信息
        int nameCount = 0;//解析完协议名后要从哪开始读下一阶段的数据
        MyProtocolEnum protocol = MsgBase.DecodeName(byteArray.Bytes, byteArray.ReadIndex, out nameCount);//解析协议名
        if (protocol == MyProtocolEnum.None)
        {
            Debug.LogErrorFormat("解析协议名出错,协议名不存在！！返回的协议名为: {0}", MyProtocolEnum.None.ToString());
            errorAction.Invoke();
            return;
        }
        //读取没有问题
        byteArray.ReadIndex += nameCount;//移动开始读位置，开始解析协议体
        //解析协议体-计算协议体长度
        int bodyCount = msgLength - nameCount-4;//剩余数组长度（剩余的长度就是协议体长度）要去掉校验码

        //检查校验码
        byte[] remoteCRC32Bytes =  byteArray.Bytes.Skip(byteArray.ReadIndex + bodyCount).Take(4).ToArray();//取校验码
        byte[] bodyBytes = byteArray.Bytes.Skip(byteArray.ReadIndex ).Take(bodyCount).ToArray();//取数据体
        uint localCRC32 = GetCrc32(bodyBytes);//获取协议体的CRC32校验码
        uint remoteCRC32 = ReverseBytesToCRC32(remoteCRC32Bytes); // 运算获取远端计算的验码
        if (localCRC32 != remoteCRC32)
        {
            Debug.LogError($"CRC32校验失败！！远端CRC32：{remoteCRC32}，本机计算的CRC32：{localCRC32}。协议体的一致性遭到破坏，和远端的数据不对应");
            errorAction.Invoke();
        }

        //校验码检测通过
        try
        {
            //解析协议体
            MsgBase msgBase = MsgBase.Decode(protocol, byteArray.Bytes, byteArray.ReadIndex, bodyCount);
            if (msgBase == null)
            {
                Debug.LogErrorFormat("解析协议名出错！！无法匹配协议基类协议名不存在！！返回的协议名为: {0}", MyProtocolEnum.None.ToString());
                errorAction.Invoke();
                return;
            }
            byteArray.ReadIndex += bodyCount+4;//要去掉校验码
            //解析完成，移动数组，释放占用空间
            //byteArray.CheckAndMoveBytes();
            byteArray.MoveBytes();
            //协议解析完成，处理协议
            //把需要处理的信息放到消息队列中，交给线程处理
            msgBase.targetSocket = targetSocket;//记录这组消息所属的客户端
            msgQueue.Enqueue(msgBase);//添加到消息处理队列内

            //继续处理
            //如果允许读长度（并非整体数组长度）有容纳消息长度的空间，说明还有数据，
            //这个为粘包，将所有数据传入，继续解析
            if (byteArray.length > 4)
            {
                DecodeMsg(byteArray,msgQueue,targetSocket,errorAction);
            }
        }
        catch (SocketException ex)
        {
            Debug.LogErrorFormat("Socket解析协议体出错 Socket OnReceiveData Error{0}", ex.ToString());
            errorAction.Invoke();
        }
        finally
        {
        }
    }
    /// <summary>
    /// 将消息进行序列化
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns>序列化后的消息</returns>
    internal static async UniTask<ByteArray> EncodMsg(MsgBase msgBase)
    {
        await UniTask.SwitchToThreadPool();
        
        //协议体编码
        byte[] nameBytes = MsgBase.EncodeName(msgBase);//协议名编码
        byte[] bodyBytes = MsgBase.Encond(msgBase);//编码协议体

        //获取校验码
        uint crc32 = GetCrc32(bodyBytes);//获取协议体的CRC32校验码
        byte[] bodyCrc32Bytes = GetCrc32Bytes(crc32);//获取协议体的CRC32校验码数组

        //长度转数组
        int len = nameBytes.Length + bodyBytes.Length+ bodyCrc32Bytes.Length;//整体长度
        byte[] byteHead = BitConverter.GetBytes(len);//转长度为字节
        byte[] sendBytes = new byte[byteHead.Length + len];//创建发送空间

        //组装字节流数据
        Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);//组装头
        Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);//组装协议名
        Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);//组装协议体
        Array.Copy(bodyCrc32Bytes, 0, sendBytes, byteHead.Length + nameBytes.Length+ bodyBytes.Length, bodyCrc32Bytes.Length);//组装校验码

        //将拼接好的信息用自定义的消息数组保存
        ByteArray ba = new ByteArray(sendBytes);
        await UniTask.SwitchToMainThread();
        return ba;
    }
   
}
