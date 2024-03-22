using ProtoBuf;

/// <summary>
/// 转流，通讯规范，服务器和客户端共同规范
/// </summary>

/// <summary>
/// 心跳包
/// </summary>
[ProtoContract]
public class MsgPing: MsgBase 
{
    public MsgPing()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgPing;
    }

    [ProtoMember(1)]
    public override MyProtocolEnum ProtoType { get; set; }
}


/// <summary>
/// 测试
/// </summary>
[ProtoContract]
public class MsgTest : MsgBase
{
    public MsgTest()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgTest;
    }

    [ProtoMember(1)]
    public override MyProtocolEnum ProtoType { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [ProtoMember(2)]
    public string Content { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [ProtoMember(3)]
    public string ReContent { get; set; }
}

