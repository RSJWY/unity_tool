using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace Server
{
    /// <summary>
    /// 服务器模块 单独客户端容器，存储客户端的相关信息等
    /// </summary>
    public class ServerClientSocket
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
        /// 存储数据
        /// </summary>
        public ByteArray ReadBuff = new ByteArray();

        ///// <summary>
        ///// 发送数据
        ///// </summary>
        //public ByteArray SendBuff = new ByteArray();

        /// <summary>
        /// 需要发送的数据队列
        /// </summary>
        public ConcurrentQueue<ByteArray> SendBuffQueue =new ConcurrentQueue<ByteArray>();
    }

}
