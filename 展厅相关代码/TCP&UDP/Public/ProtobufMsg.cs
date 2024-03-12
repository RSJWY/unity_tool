using System;
using System.Net.Sockets;
using ProtoBuf;
using UnityEngine;

/// <summary>
/// 转流，通讯规范，服务器和客户端共同规范
/// </summary>

/// <summary>
/// 本项目自定义的协议
/// </summary>
public enum MyProtocolEnum
{
    None = 0,
    /// <summary>
    /// 心跳包协议
    /// </summary>
    MsgPing = 1,
    /// <summary>
    /// 发送图片
    /// </summary>
    SendImage,

    /// <summary>
    /// 提示消息
    /// </summary>
    TipsMsg,
    /// <summary>
    /// 请求服务器内容
    /// </summary>
    RequestServerMsg,

    /// <summary>
    /// 请求客户端内容
    /// </summary>
    RequestClientrMsg

}

/// <summary>
/// 上传图片
/// </summary>
[ProtoContract]
public class SendImage : MsgBase
{
    public SendImage()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.SendImage;
        sendImageType = SendImageType.None;
    }
    [ProtoMember(1)]
    public SendImageType sendImageType;
    /// <summary>
    /// 图片数据数组
    /// </summary>
    [ProtoMember(2)]
    public byte[] ImageByte;
    /// <summary>
    /// 图片宽度
    /// </summary>
    [ProtoMember(3)]
    public int width;
    /// <summary>
    /// 图片高度
    /// </summary>
    [ProtoMember(4)]
    public int height;
    /// <summary>
    /// 图片名字
    /// </summary>
    [ProtoMember(5)]
    public string name;

    /// <summary>
    /// 寄语
    /// </summary>
    [ProtoMember(6)]
    public string jiyu { get; set; }

    public enum SendImageType
    {
        None,
        /// <summary>
        /// 从服务器请求图片
        /// </summary>
        /// 
        [Obsolete]
        RequestImageFormTheServer,
        /// <summary>
        ///上传图片到服务器
        /// </summary>
        ClientSubmittedImage,

        /// <summary>
        /// 发送图片到客户端
        /// </summary>

        [Obsolete]
        SendImagesToTheClient,
    }

}


/// <summary>
/// 心跳包
/// </summary>
[ProtoContract]
public class MsgPing : MsgBase
{
    public MsgPing()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgPing;
    }

    [ProtoMember(1)]
    public long timeStamp;
}
/// <summary>
/// 请求服务器相关服务
/// </summary>
[ProtoContract]
public class RequestServerMsg : MsgBase
{
    public RequestServerMsg()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.RequestServerMsg;
    }

    public RequestMsg requestMsg;
    public enum RequestMsg
    {

    }
}
/// <summary>
/// 请求客户端相关服务
/// </summary>
[ProtoContract]
public class RequestClientrMsg : MsgBase
{
    public RequestClientrMsg()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.RequestClientrMsg;
        MsgGuid = Guid.NewGuid();
    }
    /// <summary>
    /// 消息GUID-响应时携带此ID返回
    /// </summary>
    [ProtoMember(1)]
    public Guid MsgGuid;

    [ProtoMember(2)]
    public RequestMsg requestMsg;

    public enum RequestMsg
    {
        None,
        /// <summary>
        /// 从服务器请求图片
        /// </summary>
        RequestImageFormTheServer
    }
}

/// <summary>
/// 服务器相关消息回复
/// </summary>
[ProtoContract]
public class TipsMsg : MsgBase
{
    public TipsMsg()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.TipsMsg;
    }

    [ProtoMember(1)]
    public string Msg;
}


/// <summary>
/// 基础协议类 序列化反序列化，服务器和客户端共用
/// </summary>
[ProtoContract]
[ProtoInclude(1, typeof(MsgPing))]
[ProtoInclude(2, typeof(SendImage))]
[ProtoInclude(3, typeof(RequestServerMsg))]
[ProtoInclude(4, typeof(RequestClientrMsg))]
[ProtoInclude(5, typeof(TipsMsg))]
public class MsgBase
{
    public MsgBase()
    {
        guid = Guid.NewGuid();
    }
    /// <summary>
    /// 协议类型
    /// </summary>
    [ProtoMember(10)]
    public MyProtocolEnum ProtoType { get; set; }
    /// <summary>
    /// 消息GUID
    /// </summary>
    [ProtoMember(11)]
    public Guid guid;
    /// <summary>
    /// 目标socket
    /// </summary>
    public Socket targetSocket;

}
