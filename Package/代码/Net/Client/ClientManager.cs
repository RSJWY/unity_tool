using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;


namespace Client
{
    /// <summary>
    /// 客户端模块总调
    /// </summary>
    public class ClientManager
    {
        /// <summary>
        /// 网络链接事件枚举
        /// </summary>
        public enum NetEvent
        {
            /// <summary>
            /// 连接成功
            /// </summary>
            ConnectSucc = 1,
            /// <summary>
            /// 链接失败
            /// </summary>
            ConnectFail = 2,
            /// <summary>
            /// 连接关闭
            /// </summary>
            Close = 3,
            /// <summary>
            /// 网络重连
            /// </summary>
            ReConnect=4
        }
        /// <summary>
        /// 本机连接的socket
        /// </summary>
        Socket m_Socket;
        /// <summary>
        /// 数据
        /// </summary>
        ByteArray m_ReadBuff;
        /// <summary>
        /// IP
        /// </summary>
        string m_IP;
        /// <summary>
        /// 端口
        /// </summary>
        int m_Port;
        /// <summary>
        /// 状态——连接中
        /// </summary>
        bool m_Connecting = false;
        /// <summary>
        /// 状态——关闭中
        /// </summary>
        bool m_Closing = false;

        /// <summary>
        /// 消息处理线程
        /// </summary>
        Thread m_msgThread;
        /// <summary>
        /// 心跳包线程
        /// </summary>
        Thread m_HeartThread;

        /// <summary>
        /// 最后一次发送时间
        /// </summary>
        static long lastPingTime;
        /// <summary>
        /// 最后一次接收到信息的时间
        /// </summary>
        static long lastPongTime;
        /// <summary>
        /// 心跳包间隔时间
        /// </summary>
        static long m_PingInterval = 60;
        /// <summary>
        /// 掉线重连
        /// </summary>
        static bool m_DiaoXian = false;
        /// <summary>
        /// 是否链接成功过（只要连接成功过，就是true，不再置为false）
        /// </summary>
        bool m_IsConnentSuccessed = false;

        /// <summary>
        /// 是否是重连
        /// </summary>
        bool m_ReConnect = false;

        /// <summary>
        /// 消息发送队列
        /// </summary>
        ConcurrentQueue<ByteArray> m_WriteQueue;

        /// <summary>
        /// 事件监听
        /// </summary>
        /// <param name="str"></param>
        public delegate void EventListener(string str);
        /// <summary>
        /// 网络连接事件监听字典
        /// </summary>
        Dictionary<NetEvent, EventListener> m_ListenerDic = new Dictionary<NetEvent, EventListener>();

        /// <summary>
        /// 接收的MsgBase——要处理的消息
        /// </summary>
        static ConcurrentQueue<MsgBase> msgQueue;

        /// <summary>
        /// 需要在Unity内处理的信息
        /// </summary>
        static ConcurrentQueue<MsgBase> unityMsgQueue = new ConcurrentQueue<MsgBase>();


        /// <summary>
        /// 记录当前网络
        /// </summary>
        NetworkReachability m_CurNetWork = NetworkReachability.NotReachable;

        

        /// <summary>
        /// 增加监听链接事件
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="listener"></param>
        public void AddEventListener(NetEvent netEvent, EventListener listener)
        {
            if (m_ListenerDic.ContainsKey(netEvent))
            {
                m_ListenerDic[netEvent] += listener;
            }
            else
            {
                m_ListenerDic.Add(netEvent, listener);
            }
        }

        /// <summary>
        /// 移除监听链接事件
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="listener"></param>
        public void RemoveEventListener(NetEvent netEvent, EventListener listener)
        {
            if (m_ListenerDic.ContainsKey(netEvent))
            {
                m_ListenerDic[netEvent] -= listener;
                if (m_ListenerDic[netEvent] == null)
                {
                    m_ListenerDic.Remove(netEvent);
                }
            }
        }
        /// <summary>
        /// 执行事件
        /// </summary>
        void ConnectEvent(NetEvent netEvent, string str)
        {
            if (m_ListenerDic.ContainsKey(netEvent))
            {
                m_ListenerDic[netEvent]?.Invoke(str);
            }
        }


        /// <summary>
        /// 链接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            //链接不为空并且链接成功
            if (m_Socket != null && m_Socket.Connected)
            {
                Debug.LogError("链接失败，已经有链接服务器");
                return;
            }
            //链接状态
            if (m_Connecting)
            {
                Debug.LogError("链接失败，正在链接中");
                return;
            }
            InitState();//初始
            m_Socket.NoDelay = true;//没有延时
            m_Connecting = true;//通知正在连接
            //完成后调用第三个参数回调，第四个是往第三个参数内的传参
            m_Socket.BeginConnect(ip, port, ConnectCallBack, m_Socket);//异步连接服务器，以免堵塞unity主线程
            m_IP = ip;
            m_Port = port;
        }
        /// <summary>
        /// 初始状态，初始化变量
        /// </summary>
        void InitState()
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//配置连接模式
            m_ReadBuff = new ByteArray();//开信息收发数组
            m_WriteQueue = new ConcurrentQueue<ByteArray>();//消息发送队列
            m_Connecting = false;//状态关闭
            m_Closing = false;
            msgQueue=new ConcurrentQueue<MsgBase>();//接收消息处理队列（所有接收到要处理的消息
            lastPingTime = GetTimeStamp();//心跳包时间初始化
            lastPongTime = GetTimeStamp();
        }
        /// <summary>
        /// 连接完成回调函数
        /// </summary>
        void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                //连接完成处理
                Socket socket = (Socket)ar.AsyncState;//拆箱获取用于连接的socket
                socket.EndConnect(ar);//结束用于连接的异步请求（必须
                ConnectEvent(NetEvent.ConnectSucc, "成功");//网络连接事件广播
                m_IsConnentSuccessed = true;
                //创建消息处理线程-后台处理
                m_msgThread = new Thread(MsgThread);
                m_msgThread.IsBackground = true;//设置为后台可运行
                m_msgThread.Start();//启动线程
                m_Connecting = false;//注意此处位置，后续请求服务器数据依赖此变量
                                     //心跳包线程-后台处理
                m_HeartThread = new Thread(PingThread);
                m_HeartThread.IsBackground = true;//后台运行
                m_HeartThread.Start();
                //开始接收
                m_Socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIndex, m_ReadBuff.Remain, 0, ReceiveCallBack, m_Socket);
                
                Debug.Log("Socket连接成功 Socket connect success");
            }
            catch (SocketException ex)
            {
                Debug.LogError("Socket连接失败 Socket conect fail:" + ex.ToString());
                m_Connecting = false;
            }
        }
        /// <summary>
        /// 接收完成回调
        /// </summary>
        /// <param name="ar"></param>
        void ReceiveCallBack(IAsyncResult ar)
        {
            //接收完成处理
            Socket socket = (Socket)ar.AsyncState;//获取socket
            try
            {
                int count = socket.EndReceive(ar);//获取接收到的字节长度
                if (count <= 0)
                {
                    Close();
                    //服务器关闭链接
                    return;
                }

                m_ReadBuff.WriteIndex += count;
                OnReceiveData();
                if (m_ReadBuff.Remain < 8)
                {
                    m_ReadBuff.MoveBytes();
                    m_ReadBuff.ReSize(m_ReadBuff.length * 2);
                }
                //等待下一次数据处理接收
                m_Socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIndex, m_ReadBuff.Remain, 0, ReceiveCallBack, m_Socket);

            }
            catch (SocketException ex)
            {
                Debug.LogError("Socket数据接收完成处理失败 Socket ReceiveCallBack fail:" + ex.ToString());
                Close();
            }
        }

        /// <summary>
        /// 对数据进行处理
        /// </summary>
        void OnReceiveData()
        {
            //确认数据是否有误
            if (m_ReadBuff.length <= 4 || m_ReadBuff.ReadIndex < 0)
            {
                return;
            }
            //数据无误
            int readIndex = m_ReadBuff.ReadIndex;
            byte[] bytes = m_ReadBuff.Bytes;
            int bodyLength = BitConverter.ToInt32(bytes, readIndex);
            if (m_ReadBuff.length < bodyLength + 4)
            {
                //如果消息长度小于读出来的消息长度
                //此为分包，不包含完整数据
                return;
            }
            //存在完整数据
            //协议名解析
            m_ReadBuff.ReadIndex += 4;
            int nameCount = 0;
            MyProtocolEnum protocol = MsgBase.DecodeName(m_ReadBuff.Bytes, m_ReadBuff.ReadIndex, out nameCount);//计算协议名长度
            if (protocol == MyProtocolEnum.None)
            {
                Debug.LogError("解析协议名出错  OnReceiveData MsgBase.DecodeNmae fail");
                Close();
                return;
            }
            //读取没有问题
            m_ReadBuff.ReadIndex += nameCount;
            //解析协议体-计算协议体长度
            int bodyCount = bodyLength - nameCount;
            try
            {
                MsgBase msgBase = MsgBase.Decode(protocol, m_ReadBuff.Bytes, m_ReadBuff.ReadIndex, bodyCount);
                if (msgBase == null)
                {
                    Debug.LogError("接收数据协议内容解析出错");
                    Close();
                    return;
                }
                m_ReadBuff.ReadIndex += bodyCount;
                m_ReadBuff.CheckAndMoveBytes();
                //协议解析完成，协议具体操作
                //放入消息处理队列
                msgBase.targetSocket = m_Socket;
                msgQueue.Enqueue(msgBase);
                //继续处理粘包
                if (m_ReadBuff.length > 4)
                {
                    OnReceiveData();
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("Socket解析协议体出错 Socket OnReceiveData Error" + ex.ToString());
                Close();
            }
        }
        /// <summary>
        /// 发送信息到服务器
        /// </summary>
        /// <param name="msgBase"></param>
        internal void SendMessage(MsgBase msgBase)
        {
            if (m_Socket == null || !m_Socket.Connected)
            {
                return;//链接不存在或者未建立链接
            }
            if (m_Connecting)
            {
                Debug.LogError("正在链接服务器中，无法发送消息");
                return;
            }
            if (m_Closing)
            {
                Debug.LogError("正在关闭链接服务器中，无法发送消息");
                return;
            }
            //写入数据
            try
            {
                byte[] nameBytes = MsgBase.EncodeName(msgBase);//协议名编码
                byte[] bodyBytes = MsgBase.Encond(msgBase);//编码协议体
                int len = nameBytes.Length + bodyBytes.Length;//整体长度
                byte[] byteHead = BitConverter.GetBytes(len);//转长度为字节
                byte[] sendBytes = new byte[byteHead.Length + len];//创建发送空间
                                                                   //组合
                Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);//组装头
                Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);//组装协议名
                Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);//协议组合
                ByteArray ba = new ByteArray(sendBytes);
                //写入到队列，向服务器发送消息
                m_WriteQueue.Enqueue(ba);//放入队列
                if (m_WriteQueue.Count == 1)//如果队列内只有一条待发送信息，直接在主线程内处理
                {
                    m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, m_Socket);
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("向服务器发送消息失败 SendMessage Error:" + ex.ToString());
                Close();
            }
        }
        /// <summary>
        /// 发送消息到服务器结束后回调
        /// </summary>
        void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                if (socket == null || !socket.Connected)
                {
                    return;
                }
                int count = socket.EndSend(ar);
                //判断是否发送完成(消息是否发送完整)
                ByteArray ba;
                m_WriteQueue.TryPeek(out ba);
                ba.ReadIndex += count;
                if (ba.length == 0)//代表发送完整
                {
                    ByteArray _bDelete;
                    m_WriteQueue.TryDequeue(out _bDelete);//取出但不使用，只为了从队列中移除
                    if (m_WriteQueue.Count > 0)//如果还有数据
                    {
                        m_WriteQueue.TryPeek(out ba);//取出新数据
                    }
                    else
                    {
                        ba = null;
                    }
                }
                //发送不完整或发送完整且存在第二条数据
                //再次发送
                if (ba != null)
                {
                    socket.BeginSend(ba.Bytes, ba.ReadIndex, ba.length, 0, SendCallBack, m_Socket);
                }
                else if (m_Closing)//确保关闭连接前，先把消息发送出去
                {
                    RealClose();
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("向服务器发送消息失败 SendCallBack Error:" + ex.ToString());
                Close();
            }
        }

        /// <summary>
        /// 关闭链接
        /// </summary>
        /// <param name="normal"></param>
        public void Close(bool normal = true)
        {
            //关闭链接
            if (m_Socket == null || m_Closing)
            {
                return;
            }
            if (m_Connecting)
            {
                return;
            }
            if (m_WriteQueue.Count > 0)
            {
                m_Closing = true;
            }
            else
            {
                RealClose();
            }
        }
        /// <summary>
        /// 最终关闭链接处理
        /// </summary>
        /// <param name="normal"></param>
        void RealClose(bool normal = true)
        {
            m_Socket.Close();
            ConnectEvent(NetEvent.Close, normal.ToString());
            m_DiaoXian = true;
            //关闭所有消息处理线程
            if (m_HeartThread != null && m_HeartThread.IsAlive)
            {
                m_HeartThread.Abort();
                m_HeartThread = null;
            }
            if (m_msgThread != null && m_msgThread.IsAlive)
            {
                m_msgThread.Abort();
                m_msgThread = null;
            }
            Debug.Log("连接关闭 Close Socket");
        }
        /// <summary>
        /// 重连服务器
        /// </summary>
        public void ReConnect()
        {
            Connect(m_IP, m_Port);
            m_ReConnect = true;
            ConnectEvent(NetEvent.ReConnect, "重连服务器");
            Debug.Log("重新连接服务器");
        }
        /// <summary>
        /// 外部MonoBehaivor类内调用,
        /// </summary>
        public void Update()
        {
            if (m_DiaoXian && m_IsConnentSuccessed)
            {
                //弹框，确定是否重连
                //重新链接
                ReConnect();//只要掉线就立马重连
                            //退出游戏
                m_DiaoXian = false;
            }
            if ( m_Socket.Connected && m_ReConnect)//密钥不为空,有链接，是重新连接
            {
                //重连处理
                m_ReConnect = false;
            }
            //处理unity该处理的消息
            if (m_Socket != null && m_Socket.Connected)
            {
                if (unityMsgQueue.Count <= 0)
                {
                    return;//没有消息
                }
                //取出消息
                if (unityMsgQueue.Count > 0)
                {
                    MsgBase msgBase = null;
                    unityMsgQueue.TryDequeue(out msgBase);//取出并移除数据
                    ClientController.instance.SendReceiveMsgCallBack(msgBase.ProtoType, msgBase);
                }
            }
        }

        /// <summary>
        /// 消息处理线程回调
        /// </summary>
        void MsgThread()
        {
            while (m_Socket != null && m_Socket.Connected)
            {
                if (msgQueue.Count <= 0)
                {
                    continue;//当前无消息，跳过进行下一次排查处理
                }
                //有待处理的消息
                MsgBase msgBase = null;
                //取出并移除取出来的数据
                msgQueue.TryDequeue(out msgBase);
                //处理取出来的数据
                if (msgBase != null)
                {
                    //如果接收到是心跳包
                    if (msgBase is MsgPing)
                    {
                        //更新接收到的心跳包时间（后台运行）
                        lastPongTime = GetTimeStamp();
                        //Debug.Log("收到心跳包！！");
                    }
                    else
                    {
                        //其他消息交给unity消息队列处理
                        unityMsgQueue.Enqueue(msgBase);
                    }
                }
                else
                {
                    Debug.LogError("取出来空数据，请检查");
                }
            }
        }

        /// <summary>
        /// 检查当前网络类型，是否切换了网络
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckNetThread()
        {
            m_CurNetWork = Application.internetReachability;
            while (true)
            {
                yield return new WaitForSeconds(1);
                if (m_IsConnentSuccessed)
                {
                    if (m_CurNetWork != Application.internetReachability)
                    {
                        ReConnect();
                        m_CurNetWork = Application.internetReachability;
                    }
                }
            }
        }

        /// <summary>
        /// 心跳包发送函数处理
        /// </summary>
        void PingThread()
        {
            while (m_Socket != null && m_Socket.Connected)
            {
                long timeNow = GetTimeStamp();
                if (timeNow - lastPingTime > m_PingInterval)
                {
                    MsgPing msgPing = new MsgPing();
                    SendMessage(msgPing);
                    lastPingTime = GetTimeStamp();
                }
                //如果心跳包过长时间没收到，关闭链接
                if (timeNow - lastPongTime > m_PingInterval * 4)
                {
                    Close(false);
                }
            }
        }



        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            //基础时间计算
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }


        #region 单例
        static ClientManager _instance = null;
        public static ClientManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClientManager();
                }
                return _instance;
            }
        }
        private ClientManager()
        {
        }
        #endregion
    }
}

