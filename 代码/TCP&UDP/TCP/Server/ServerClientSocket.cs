using System.Net.Sockets;
using System.Collections.Concurrent;
using System;

namespace ServerService
{
    /// <summary>
    /// 服务器模块 单独客户端容器，存储客户端的相关信息等
    /// </summary>
    public class ClientSocket
    {
        /// <summary>
        /// 存储连接的客户端
        /// </summary>
        public Socket socket { get; set; }
        /// <summary>
        /// 初次连接时间（心跳包）
        /// </summary>
        public long lastPingTime { get; set; }
        /// <summary>
        /// 存储数据-接收到的数据暂存容器
        /// </summary>
        public ByteArray ReadBuff;
    }
    /// <summary>
    /// 服务器模块 消息发送数据容器
    /// </summary>
    public class ServerToClientMsg
    {
        /// <summary>
        /// 消息目标服务器
        /// </summary>
        public Socket msgTargetSocket;
        /// <summary>
        /// 消息
        /// </summary>
        public MsgBase msg;
        /// <summary>
        /// 已转换完成的消息数组
        /// </summary>
        public ByteArray sendBytes;
    }
}
