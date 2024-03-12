using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;


namespace ClientService
{
    /// <summary>
    ///TCP 客户端服务
    /// </summary>
    internal class TCPClientService : BaseInstance<TCPClientService>
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
            ReConnect = 4
        }

        #region 字段

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
        /// 消息处理线程
        /// </summary>
        Thread m_msgThread;
        /// <summary>
        /// 心跳包线程
        /// </summary>
        Thread m_HeartThread;
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        Thread msgSendThread;
        /// <summary>
        ///  消息队列发送锁
        /// </summary>
        object msgSendThreadLock = new object();

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
        static long m_PingInterval = 3;
        /// <summary>
        /// 掉线重连
        /// </summary>
        static bool m_DiaoXian = false;
        /// <summary>
        /// 是否链接成功过（只要连接成功过，就是true，不再置为false）
        /// </summary>
        bool m_IsConnentSuccessed = false;
        /// <summary>
        /// 状态——连接中
        /// </summary>
        bool m_Connecting = false;
        /// <summary>
        /// 状态——关闭中
        /// </summary>
        bool m_Closing = false;
        /// <summary>
        /// 是否是重连
        /// </summary>
        bool m_ReConnect = false;
        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        static bool isThreadOver = false;

        /// <summary>
        /// 消息发送队列
        /// </summary>
        ConcurrentQueue<ByteArray> m_WriteQueue;

        /// <summary>
        /// 网络连接事件监听字典
        /// </summary>
        Dictionary<NetEvent, Action> m_ListenerDic = new Dictionary<NetEvent, Action>();

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
        /// 是否已经初始化
        /// </summary>
        bool isInit = false;

        #endregion

        #region 事件相关
        /// <summary>
        /// 增加监听链接事件
        /// </summary>
        /// <param name="netEvent"></param>
        /// <param name="listener"></param>
        public void AddEventListener(NetEvent netEvent, Action listener)
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
        public void RemoveEventListener(NetEvent netEvent, Action listener)
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
        void ConnectEvent(NetEvent netEvent)
        {
            if (m_ListenerDic.ContainsKey(netEvent))
            {
                m_ListenerDic[netEvent]?.Invoke();
            }
        }
        #endregion

        #region 连接服务器

        public void Connect()
        {
            if (!isInit)
            {
                return;
            }
            Connect(m_IP, m_Port);
        }

        /// <summary>
        /// 链接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            Reset();

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
            //存储初始设置的信息，用于重连
            m_IP = ip;
            m_Port = port;
            isInit = true;

            Debug.Log($"客户端消息：开始链接服务器，目标：IP:{m_IP}，Port：{m_Port}");
        }
        /// <summary>
        /// 初始状态，初始化变量
        /// </summary>
        void InitState()
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//配置连接模式
            m_ReadBuff = new ByteArray();//开信息收数组
            m_WriteQueue = new ConcurrentQueue<ByteArray>();//消息发送队列
            m_Connecting = false;//状态关闭
            m_Closing = false;
            msgQueue = new();//接收消息处理队列（所有接收到要处理的消息
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
                ConnectEvent(NetEvent.ConnectSucc);//网络连接事件广播
                m_IsConnentSuccessed = true;
                //心跳包时间初始化
                lastPingTime = MySocketTool.GetTimeStamp();
                lastPongTime = MySocketTool.GetTimeStamp();

                isThreadOver = false;
                //创建消息处理线程-后台处理
                m_msgThread = new Thread(MsgThread);
                m_msgThread.IsBackground = true;//设置为后台可运行
                m_msgThread.Start();//启动线程
                m_Connecting = false;//注意此处位置，后续请求服务器数据依赖此变量
                //心跳包线程-后台处理
                m_HeartThread = new Thread(PingThread);
                m_HeartThread.IsBackground = true;//后台运行
                m_HeartThread.Start();

                //消息发送线程
                msgSendThread = new Thread(MsgSendListenThread);
                msgSendThread.IsBackground = true;
                msgSendThread.Start();

                //开始接收
                m_Socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIndex, m_ReadBuff.Remain, 0, ReceiveCallBack, m_Socket);

                Debug.Log($"客户端消息：Socket连接成功 成功连接上服务器！Socket：{m_Socket.RemoteEndPoint.ToString()}");
            }
            catch (SocketException ex)
            {
                //会无限重连，直到重新连接成功
                Debug.LogWarning($"Socket连接失败,等待3秒后重新尝试链接 和服务器连接失败原因:{ex.ToString()}");
                Thread.Sleep(3000);//延时处理
                //开始接收
                m_Socket.BeginConnect(m_IP, m_Port, ConnectCallBack, m_Socket);//异步连接服务器，以免堵塞unity主线程
                Debug.LogWarning($"开始重连，配置信息为：IP:{m_IP.ToString()},Port：{m_Port.ToString()}");
                m_Connecting = true;
            }
        }
        #endregion

        #region 消息接收
        /// <summary>
        /// 接收完成回调
        /// </summary>
        /// <param name="ar"></param>
        void ReceiveCallBack(IAsyncResult ar)
        {
            //接收完成处理
            Socket socket = (Socket)ar.AsyncState;//获取socket
            if (m_Closing || !socket.Connected)
            {
                Debug.LogWarning($"客户端消息：和服务器的连接关闭中,或服Socket连接状态为False，不执行消息处理回调");
                return;
            }
            try
            {
                int count = socket.EndReceive(ar);//获取接收到的字节长度
                if (count <= 0)
                {
                    Debug.LogWarning($"接收服务器：{socket.RemoteEndPoint},发来的字节长度为0，可能服务器已经断开了链接，本地Socket执行连接关闭");
                    Close();
                    //服务器关闭链接
                    return;
                }

                m_ReadBuff.WriteIndex += count;
                //
                OnReceiveData();
                ////检查是否需要扩容
                m_ReadBuff.CheckAndMoveBytes();
                m_Socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIndex, m_ReadBuff.Remain, 0, ReceiveCallBack, m_Socket);

            }
            catch (SocketException ex)
            {
                Debug.LogError($"Socket数据接收完成处理失败 接收服务器数据失败:{ex.ToString()}");
                Close();
            }
        }

        /// <summary>
        /// 对数据进行处理
        /// </summary>
        void OnReceiveData()
        {
            MySocketTool.DecodeMsg(m_ReadBuff, msgQueue, m_Socket,
                () =>
                {
                    Close();
                });
        }
        #endregion

        #region 消息发送处理
        /// <summary>
        /// 发送信息到服务器
        /// </summary>
        /// <param name="msgBase"></param>
        internal async UniTaskVoid SendMessage(MsgBase msgBase)
        {
            if (m_Socket == null || !m_Socket.Connected)
            {
                return;//链接不存在或者未建立链接
            }
            if (m_Connecting)
            {
                Debug.LogWarning("正在链接服务器中，无法发送消息");
                return;
            }
            if (m_Closing)
            {
                Debug.LogWarning("正在关闭链接服务器中，无法发送消息");
                return;
            }
            //写入数据
            try
            {
                ByteArray sendBytes = await MySocketTool.EncodMsg(msgBase);
                //写入到队列，向服务器发送消息
                m_WriteQueue.Enqueue(sendBytes);//放入队列
            }
            catch (SocketException ex)
            {
                Debug.LogError($"向服务器发送消息失败 SendMessage Error:{ex.ToString()}");
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
                ByteArray ba;
                Socket socket = (Socket)ar.AsyncState;
                if (socket == null || !socket.Connected)
                {
                    return;
                }
                int count = socket.EndSend(ar);
                //判断是否发送完成(消息是否发送完整)
                m_WriteQueue.TryPeek(out ba);
                ba.ReadIndex += count;
                if (ba.length == 0)//代表发送完整
                {
                    ByteArray _bDelete;
                    m_WriteQueue.TryDequeue(out _bDelete);//取出但不使用，只为了从队列中移除
                    ba = null;//发送完成，置空
                }
                //发送不完整，再次发送
                if (ba != null)
                {
                    socket.BeginSend(ba.Bytes, ba.ReadIndex, ba.length, 0, SendCallBack, m_Socket);
                }
                else if (m_Closing)
                {
                    Debug.LogWarning("正在断开链接");
                    //如果正在断开，最后一条也发送完整，则执行断开指令
                    RealClose();
                }
                else
                {
                    //本条消息发送完成，激活线程
                    lock (msgSendThreadLock)
                    {
                        Monitor.Pulse(msgSendThreadLock);
                    }
                }

            }
            catch (SocketException ex)
            {
                Debug.LogError("向服务器发送消息失败 SendCallBack Error:" + ex.ToString());
                Close();
            }
        }
        #endregion

        #region 线程方法

        /// <summary>
        /// 消息处理线程回调
        /// </summary>
        void MsgThread()
        {
            while (!isThreadOver)
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
                        MsgPing _ServerMsg = msgBase as MsgPing;
                        //更新接收到的心跳包时间（后台运行）
                        lastPongTime = MySocketTool.GetTimeStamp();
                        //Debug.LogFormat("收到服务器返回的心跳包！！时间戳为：{0}，同时更新本地时间戳，时间戳为：{1}", _ServerMsg.timeStamp.ToString(),lastPongTime.ToString());
                    }
                    else
                    {
                        //其他消息交给unity消息队列处理
                        unityMsgQueue.Enqueue(msgBase);
                    }
                }
                // else
                // {
                //     Debug.LogError($"取出来空数据，请检查");
                // }
            }
        }

        /// <summary>
        /// 心跳包处理
        /// </summary>
        void PingThread()
        {
            while (!isThreadOver)
            {
                Thread.Sleep(1000);//本线程可以每秒检测一次
                if (m_ReConnect)
                {
                    //正在重连，结束或者跳过？？
                    break;
                }

                long timeNow = MySocketTool.GetTimeStamp();
                if (timeNow - lastPingTime > m_PingInterval)
                {
                    //规定时间到，发送心跳包到服务器
                    MsgPing msgPing = SendMsgMethod.SendMsgPing(timeNow);
                    SendMessage(msgPing).Forget();
                    lastPingTime = timeNow;

                }
                //如果心跳包过长时间没收到，关闭链接
                if (timeNow - lastPongTime > m_PingInterval * 4)
                {
                    Close();
                    Debug.LogWarning("服务器返回心跳包超时");
                }
            }
        }
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        void MsgSendListenThread()
        {
            while (!isThreadOver)
            {
                if (m_WriteQueue.Count <= 0)
                {
                    continue;
                }
                //队列里有消息等待发送
                ByteArray _sendByte;
                //取出消息队列内的消息，但不移除队列，以获取目标客户端
                m_WriteQueue.TryPeek(out _sendByte);
                //当前线程执行休眠，等待消息发送完成后继续
                lock (msgSendThreadLock)
                {
                    if (m_Socket != null && m_Socket.Connected)
                    {
                        m_Socket.BeginSend(_sendByte.Bytes, 0, _sendByte.length, 0, SendCallBack, m_Socket);
                    }
                     bool istimeout=Monitor.Wait(msgSendThreadLock, 10000);
                    if (!istimeout)
                    {
                        Debug.LogWarning($"客户端消息：消息发送时间超时（超过10s），请检查网络质量，关闭本客户端的链接");
                        Close();
                    }
                }
            }
        }

        #endregion

        #region Unity主线程调用

        /// <summary>
        /// 需要Unity处理的内容
        /// </summary>
        internal void TCPUpdate()
        {
            if (!isInit)
            {
                return;
            }
            if (m_DiaoXian && m_IsConnentSuccessed)
            {
                //弹框，确定是否重连
                //重新链接
                ReConnect();//只要掉线就立马重连
                //退出游戏
                m_DiaoXian = false;
            }
            if (m_Socket.Connected && m_ReConnect)//有链接，是重新连接
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
                    //取出并移除数据
                    if (unityMsgQueue.TryDequeue(out msgBase))
                    {
                        ClientController.instance.SendReceiveMsgCallBack(msgBase.ProtoType, msgBase);
                    }
                    else
                    {
                        Debug.LogError("非正常错误！客户端在处理返回给unity处理的信息时，取出并处理消息队列失败！！");
                    }

                }
            }
            /*
            //线程是否出错
            if (m_msgThread != null)
            {
                if (m_msgThread.IsAlive == false && isInit == true)
                {

                    Debug.LogWarning("消息处理线程无效");
                    //创建消息处理线程-后台处理
                    m_msgThread = new Thread(MsgThread);
                    m_msgThread.IsBackground = true;//设置为后台可运行
                    m_msgThread.Start();//启动线程
                }
            }
            //线程是否出错
            if (m_HeartThread != null)
            {
                if (m_HeartThread.IsAlive == false && isInit == true)
                {

                    Debug.LogWarning("心跳包处理线程无效");
                    //重新创建本线程
                    //心跳包线程-后台处理
                    m_HeartThread = new Thread(PingThread);
                    m_HeartThread.IsBackground = true;//后台运行
                    m_HeartThread.Start();
                }
            }
            //线程是否出错
            if (msgSendThread != null)
            {
                if (msgSendThread.IsAlive == false && isInit == true)
                {

                    Debug.LogWarning("消息发送处理线程无效");
                    //重新创建本线程

                    //消息发送线程
                    msgSendThread = new Thread(MsgSendListenThread);
                    msgSendThread.IsBackground = true;
                    msgSendThread.Start();
                }
            }*/

        }

        /// <summary>
        /// 检查当前网络类型，是否切换了网络
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckNetThread()
        {
            m_CurNetWork = Application.internetReachability;
            while (m_Socket != null)
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
        }/// <summary>
         /// 检查当前网络类型，是否切换了网络
         /// </summary>
         /// <returns></returns>
        internal async UniTaskVoid AsyncCheckNetThread()
        {
            m_CurNetWork = Application.internetReachability;
            while (m_Socket != null)
            {
                await UniTask.Delay(1000);//等待1s
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
        #endregion

        #region 关闭连接
        /// <summary>
        /// 关闭链接
        /// </summary>
        public void Close()
        {
            if (m_Socket == null || m_Closing)
            {
                return;
            }
            if (m_Connecting)
            {
                //正在连接服务器，拒绝中断
                return;
            }
            if (m_WriteQueue.Count > 0)
            {
                //消息数组存在未发送完成的消息，
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
        void RealClose()
        {
            m_Socket.Close();
            ConnectEvent(NetEvent.Close);
            Debug.Log("客户端消息：连接关闭 Close Socket");
            m_DiaoXian = true;
            isThreadOver = true;
        }
        /// <summary>
        /// 重连服务器
        /// </summary>
        public void ReConnect()
        {
            Connect(m_IP, m_Port);
            m_ReConnect = true;
            ConnectEvent(NetEvent.ReConnect);
            Debug.LogWarning("客户端消息：检测到服务器断开！！开始重新连接服务器");
            
        }
        internal void Quit()
        {
            if (!isInit)
            {
                return;
            }
            Close();
            Reset();
        }

        void Reset()
        {
            m_Connecting = false;
            m_IsConnentSuccessed = false;
            m_ReConnect = false;
            m_Closing = false;
            isThreadOver = true;
            m_Socket = default;
            m_DiaoXian = false;
        }
        #endregion

        #region 功能函数
        /// <summary>
        /// 重新链接服务器
        /// </summary>
        void ReConnectToServer()
        {
            if (!isInit)
            {
                return;
            }

        }
        #endregion
    }
}
