using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


namespace ServerService{
    /// <summary>
    /// 服务器模块核心，仅允许通过ServerController调用
    /// </summary>
    internal class TCPServerService : BaseInstance<TCPServerService>
    {
        #region 字段
        /// <summary>
        /// 监听端口
        /// </summary>
        static int port = 5236;

        /// <summary>
        /// 监听IP
        /// </summary>
        static IPAddress ip = IPAddress.Any;

        /// <summary>
        /// 心跳包间隔时间
        /// </summary>
        internal static long pingInterval = 3;

        /// <summary>
        /// 服务器监听Socket
        /// </summary>
        static Socket ListenSocket;

        /// <summary>
        /// 客户端Socket集合(已经建立连接的)
        /// </summary>
        internal static List<Socket> clientList = new List<Socket>();

        /// <summary>
        /// 客户端容器字典
        /// </summary>
        internal static ConcurrentDictionary<Socket, ServerClientSocket> ClientDic = new ConcurrentDictionary<Socket, ServerClientSocket>();

        /// <summary>
        /// 获取到的要处理的消息队列
        /// </summary>
        static ConcurrentQueue<MsgBase> serverMsgQueue = new ConcurrentQueue<MsgBase>();

        /// <summary>
        /// 需要交给unity处理的消息队列
        /// </summary>
        static ConcurrentQueue<MsgBase> UnityMsgQueue = new ConcurrentQueue<MsgBase>();

        /// <summary>
        /// 消息发送队列
        /// </summary>
        static ConcurrentQueue<MsgBase> msgSendQueue = new ConcurrentQueue<MsgBase>();

        /// <summary>
        /// 消息处理线程
        /// </summary>
        static Thread msgThread;
        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        static Thread pingThread;
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        static Thread msgSendThread;
        /// <summary>
        ///  消息队列发送锁
        /// </summary>
        object msgSendThreadLock = new object();

        /// <summary>
        /// 通知多线程自己跳出
        /// </summary>
        static bool isThreadOver = false;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        bool isInit = false;

        #endregion

        #region 初始化和客户端接入

        /// <summary>
        /// 使用指定IP
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_port"></param>
        internal void Init(string _ip, int _port)
        {
            ip = IPAddress.Parse(_ip);
            port = _port;
            SetInit();
        }
        /// <summary>
        /// 使用设置好的IPAddress
        /// </summary>
        /// <param name="_ipAddress"></param>
        /// <param name="_port"></param>
        internal void Init(IPAddress _ipAddress, int _port)
        {
            ip = _ipAddress;
            port = _port;
            SetInit();
        }

        internal void Init()
        {
            if (!isInit)
            {
                return;
            }
            SetInit();
        }

        /// <summary>
        /// 初始化监听
        /// </summary>
        void SetInit()
        {
            try
            {
                //监听IP
                IPAddress m_ip = ip;
                //设置监听端口
                IPEndPoint ipendpoint = new IPEndPoint(m_ip, port);
                //创建Socket并设置模式
                ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定监听Socket监听
                ListenSocket.Bind(ipendpoint);
                //链接数量限制
                ListenSocket.Listen(10);
                //开启新的异步监听
                ListenSocket.BeginAccept(AcceptCallBack, null);

                isThreadOver = false;

                //消息处理线程-后台处理
                msgThread = new Thread(MsgThread);
                msgThread.IsBackground = true;//后台运行
                msgThread.Start();
                //心跳包监测处理线程-后台处理
                pingThread = new Thread(PingThread);
                pingThread.IsBackground = true;
                pingThread.Start();
                //消息发送线程
                msgSendThread = new Thread(MsgSendListenThread);
                msgSendThread.IsBackground = true;
                msgSendThread.Start();

                isInit = true;
                //输出日志
                Debug.LogFormat("服务器启动监听：{0} 成功！！启动异步监听,当前客户端数量：{1}",
                    ListenSocket.LocalEndPoint.ToString(), clientList.Count.ToString());
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("服务器启动监听 IP:{0}，Port{1} 失败！！错误信息：\n {2}", ip,port, e.ToString());
            }

        }
        /// <summary>
        /// 有客户端链接上来执行本回调
        /// </summary>
        void AcceptCallBack(IAsyncResult ar)
        {
            if (isThreadOver)
            {
                return;
            }
            try
            {
                //结束用于连接的异步请求，获取客户端socket
                Socket client = ListenSocket.EndAccept(ar);
                //获取完成继续开启异步监听，监听下一次的链接
                ListenSocket.BeginAccept(AcceptCallBack, null);
                //创建新客户端容器存储客户端信息
                ServerClientSocket _ServerClient = new ServerClientSocket();
                //存储
                _ServerClient.socket = client;
                //存储连接时间
                _ServerClient.lastPingTime = MySocketTool.GetTimeStamp();
                //存储到字典
                //ClientDic.Add(client, _ServerClient);
                ClientDic.TryAdd(client, _ServerClient);
                //记录链接上来的客户端
                lock ((clientList as ICollection).SyncRoot)
                {
                    clientList.Add(client);
                }
                //开启异步接收消息
                client.BeginReceive(
                    _ServerClient.ReadBuff.Bytes, _ServerClient.ReadBuff.WriteIndex, _ServerClient.ReadBuff.Remain, 0, ReceiveCallBack, client);

                Debug.LogFormat("和客户端{0}建立链接成功！！并开启异步消息接收，当前客户端数量:{1},并继续监听客户端连接",
                    client.RemoteEndPoint.ToString(), ClientDic.Count);
            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("客户端建立连接失败！！错误信息：\n {0}", ex.ToString());
            }
        }

        #endregion

        #region 消息处理

        /// <summary>
        /// 接收完成回调
        /// </summary>
        /// <param name="ar"></param>
        void ReceiveCallBack(IAsyncResult ar)
        {
            //获取客户端
            Socket client = (Socket)ar.AsyncState;
            if (!ClientDic.ContainsKey(client))
            {
                Debug.LogWarning($"无法找到用于存储的客户端容器，无法执行接收,可能已经断开链接，当前数量为：{ClientDic.Count}");
                return;//找不到存储的容器
            }
            //找对应的客户端容器
            ServerClientSocket _serverClient = ClientDic[client];
            try
            {
                ByteArray readBuff = _serverClient.ReadBuff;//获取客户端容器的字节容器
                int count = client.EndReceive(ar);//获取接收到的字节长度
                if (count <= 0)
                {
                    //没有接收到数据
                    Debug.LogWarningFormat("读取客户端：{0}发来的消息出错！！数据长度小于0，客户端连可能已经断开，服务器关闭对此客户端的链接。",
                    client.RemoteEndPoint.ToString());
                    //服务器关闭链接
                    CloseClient(_serverClient);
                    return;
                }
                readBuff.WriteIndex += count;//移动已经写入索引
                OnReceiveData(_serverClient);//把数据交给数据处理函数
                ////检查是否需要扩容
                readBuff.CheckAndMoveBytes();
                //if (readBuff.Remain < 8)
                //{
                //    //完成解析或者这次接收的数据是分包数据
                //    //剩余空间不足（容量-已经写入索引
                //    readBuff.MoveBytes();//已经完成一轮解析，移动数据
                //    readBuff.ReSize(readBuff.length * 2);//扩容
                //}

                //本轮消息处理完成后或者这次处理的数据有分包情况，开启下一次的异步监听，如果分包则继续接收未接收完成的数据
                //等待下一次数据处理接收
                client.BeginReceive(readBuff.Bytes, readBuff.WriteIndex, readBuff.Remain, 0, ReceiveCallBack, client);

            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("读取客户端：{0}发来的消息出错！！错误信息： \n {1}", client.RemoteEndPoint.ToString(), ex.ToString());
                CloseClient(_serverClient);
            }
        }

        /// <summary>
        /// 接收数据处理，处理分包粘包情况
        /// </summary>
        void OnReceiveData(ServerClientSocket serverClientSocket)
        {
            //取字节流数组
            ByteArray _byteArray = serverClientSocket.ReadBuff;

            MySocketTool.DecodeMsg(_byteArray, serverMsgQueue, serverClientSocket.socket,
                () =>
                {
                    CloseClient(serverClientSocket);
                });
            /*旧接收到的数据处理方法
            //确认数据是否有误
            if (_byteArray.length <= 4 || _byteArray.ReadIndex < 0)
            {
                //前四位是记录整体数据长度
                return;
            }
            //数据无误
            int readIndex = _byteArray.ReadIndex;
            byte[] bytes = _byteArray.Bytes;
            //从数组前四位获取
            int bodyLength = BitConverter.ToInt32(bytes, readIndex);
            //判断是不是分包数据
            if (_byteArray.length < bodyLength + 4)
            {
                //如果消息长度小于读出来的消息长度
                //此为分包，不包含完整数据
                //因为产生了分包，可能容量不足，根据目标大小进行扩容到接收完整

                serverClientSocket.ReadBuff.MoveBytes();//已经完成一轮解析，移动数据
                serverClientSocket.ReadBuff.ReSize(bodyLength+8);//扩容，扩容的同时，保证长度信息也能被存入
                return;
            }
            //接收完整，存在完整数据

            //协议名解析
            _byteArray.ReadIndex += 4;//前四位存储字节流数组长度信息
            int nameCount = 0;//解析完协议名后要从哪开始读下一阶段的数据
            MyProtocolEnum protocol = MsgBase.DecodeName(_byteArray.Bytes, _byteArray.ReadIndex, out nameCount);//解析协议名
            if (protocol == MyProtocolEnum.None)
            {
                Debug.LogErrorFormat("解析协议名出错,协议名不存在！！返回的协议名为: {0}", MyProtocolEnum.None.ToString());
                CloseClient(serverClientSocket);
                return;
            }
            //读取没有问题
            _byteArray.ReadIndex += nameCount;//移动开始读位置
                                            //解析协议体-计算协议体长度
            int bodyCount = bodyLength - nameCount;//剩余数组长度
            try
            {
                //解析协议体
                MsgBase msgBase = MsgBase.Decode(protocol, _byteArray.Bytes, _byteArray.ReadIndex, bodyCount);
                if (msgBase == null)
                {
                    Debug.LogErrorFormat("解析协议名出错！！无法匹配协议基类协议名不存在！！返回的协议名为: {0}", MyProtocolEnum.None.ToString());
                    CloseClient(serverClientSocket);
                    return;
                }
                _byteArray.ReadIndex += bodyCount;
                _byteArray.CheckAndMoveBytes();//解析完成，移动数组，释放占用空间
                                            //协议解析完成，处理协议
                                            //把需要处理的信息放到消息队列中，交给线程处理
                msgBase.targetSocket = serverClientSocket.socket;//记录这组消息所属的客户端
                serverMsgQueue.Enqueue(msgBase);//添加到消息处理队列内
                
                //继续处理粘包分包
                //如果允许读长度（并非整体数组长度）有容纳消息长度的空间，说明还有数据，继续解析
                if (_byteArray.length > 4)
                {
                    OnReceiveData(serverClientSocket);
                }
            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("Socket解析协议体出错 Socket OnReceiveData Error{0}", ex.ToString());
                CloseClient(serverClientSocket);
            }
            */
        }
        #endregion

        #region 消息发送
        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        /// <param name="msgBase"></param>
        internal async UniTaskVoid SendMessage(MsgBase msgBase, Socket _client)
        {
            msgBase.targetSocket = _client;//取出目标客户端
            if (_client == null || !_client.Connected)
            {
                Debug.LogError("Socket链接不存在或者未建立链接");
                return;//链接不存在或者未建立链接
            }
            //写入数据
            try
            {
                /*旧数据序列化方法
                byte[] nameBytes = MsgBase.EncodeName(msgBase);//协议名编码
                byte[] bodyBytes = MsgBase.Encond(msgBase);//编码协议体
                int len = nameBytes.Length + bodyBytes.Length;//整体长度
                byte[] byteHead = BitConverter.GetBytes(len);//转长度为字节
                byte[] sendBytes = new byte[byteHead.Length + len];//创建发送空间
                                                                //组装字节流数据
                Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);//组装头
                Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);//组装协议名
                Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);//协议组合
                //将拼接好的信息用自定义的消息数组保存
                ByteArray ba = new ByteArray(sendBytes);
                */

                ByteArray sendBytes= await MySocketTool.EncodMsg(msgBase);

                //同步向队列放数据，这样根据队列的先进先出特性，使得数据同步，注意先后顺序
                //把消息放到目标客户端的容器内的消息队列，（需要同时向客户端发送多条消息
                ClientDic[_client].SendBuffQueue.Enqueue(sendBytes);
                //写入到队列，向客户端发送消息，根据客户端和绑定的数据发送
                msgSendQueue.Enqueue(msgBase);
                //if (msgSendQueue.Count ==1)//如果队列内只有一条待发送信息，直接在主线程内处理
                //{
                //    _client.BeginSend(sendBytes.Bytes, 0, sendBytes.length, 0, SendCallBack, _client);
                //}
            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("向客户端发送消息失败 SendMessage Error:{0}", ex.ToString());
                CloseClient(ClientDic[_client]);
            }
        }
        /// <summary>
        /// 发送消息到客户端结束后回调
        /// </summary>
        void SendCallBack(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;//获取目标客户端的socket
            MsgBase _msgbase;//存储从队列里取出来的数据
            ByteArray _ba;//存储字节流数组
            try
            {
                if (client == null || !client.Connected)
                {
                    return;
                }
                //获取已经发送的字节流长度
                int count = client.EndSend(ar);
                //判断是否发送完成(消息是否发送完整)
                //取出消息类但不移除，用作数据比较，根据消息MSG队列，获取相应的客户端内的消息数组队列
                msgSendQueue.TryPeek(out _msgbase);
                //获取存储发送数据的字节流数组
                //_ba = ClientDic[client].SendBuff;
                ClientDic[client].SendBuffQueue.TryPeek(out _ba);//取出消息字节数组但不移除，
                _ba.ReadIndex += count;//已发送索引
                if (_ba.length == 0)//代表发送完整
                {
                    MsgBase _msDelete;
                    ByteArray _baDelete;
                    //先取出客户端内的队列，后取出发送队列内的数据
                    ClientDic[client].SendBuffQueue.TryDequeue(out _baDelete);//取出但不使用，只为了从队列中移除
                    msgSendQueue.TryDequeue(out _msDelete);//取出但不使用，只为了从队列中移除
                    _ba=null;//发送完成
                    //if (msgSendQueue.Count > 0)//如果还有数据（指的是还有需要向客户端发送数据的队列
                    //{
                    //    msgSendQueue.TryPeek(out _msgbase);//取出新数据
                    //    client = _msgbase.targetSocket;//可能存在当前字节流数据不属于当前客户端情况，需要重新获取客户端信息
                    //    ClientDic[client].SendBuffQueue.TryPeek(out _ba);//获取发送消息数组
                    //}
                    //else
                    //{
                    //    _ba = null;//发送完成，数组置空
                    //}

                }
                //发送不完整，再次发送
                if (_ba != null)
                {
                    client.BeginSend(_ba.Bytes, _ba.ReadIndex, _ba.length, 0, SendCallBack, client);
                }
                else
                {
                    //本条数据发送完成，激活线程，继续处理下一条
                    lock (msgSendThreadLock)
                    {
                        Monitor.Pulse(msgSendThreadLock);
                    }

                }
            }
            catch (SocketException ex)
            {
                Debug.LogErrorFormat("向客户端发送消息失败 SendCallBack Error:{0}", ex.ToString());
                CloseClient(ClientDic[client]);
            }
        }

        #endregion

        #region 线程
        /// <summary>
        /// 消息队列发送监控线程
        /// </summary>
        void MsgSendListenThread()
        {
            while (!isThreadOver)
            {
                if (msgSendQueue.Count<=0)
                {
                    continue;
                }
                MsgBase _msgbase;
                Socket _client;
                ByteArray _sendByte;
                //取出消息队列内的消息，但不移除队列，以获取目标客户端
                msgSendQueue.TryPeek(out _msgbase);
                _client=_msgbase.targetSocket;
                //获取发送消息数组
                ClientDic[_client].SendBuffQueue.TryPeek(out _sendByte);
                _client.BeginSend(_sendByte.Bytes, 0, _sendByte.length, 0, SendCallBack, _client);
                //当前线程执行休眠，等待消息发送完成后继续
                lock (msgSendThreadLock)
                {
                    Monitor.Wait(msgSendThreadLock);
                }
            }
        }

        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        void PingThread()
        {
            List<ServerClientSocket> _tmpClose = new List<ServerClientSocket>();//存储需要断开来的客户端
            while (!isThreadOver)
            {
                Thread.Sleep(1000);//本线程可以每秒检测一次
                if (ClientDic.Count <= 0)
                {
                    continue;
                }
                //检测心跳包是否超时的计算
                //获取当前时间
                long timeNow = MySocketTool.GetTimeStamp();
                //遍历取出所有客户端
                foreach (ServerClientSocket serverClientSocket in ClientDic.Values)
                {
                    //超时，断开
                    if (timeNow - serverClientSocket.lastPingTime > pingInterval * 4)
                    {
                        //记录要断开链接的客户端
                        //防止在遍历过程中对dic操作（操作过程中，dic会被锁死）
                        _tmpClose.Add(serverClientSocket);
                    }
                }
                //逐一取出执行断开操作
                foreach (ServerClientSocket clientSocket in _tmpClose)
                {
                    Debug.LogFormat("客户端：{0}在允许时间{1}秒内发送心跳包超时，连接关闭！！", clientSocket.socket.RemoteEndPoint.ToString(), pingInterval * 4);
                    CloseClient(clientSocket);
                }
                _tmpClose.Clear();//操作完成后清除客户端
            }
        }

        /// <summary>
        /// 消息处理线程，分发消息
        /// </summary>
        void MsgThread()
        {
            while (!isThreadOver)
            {
                if (serverMsgQueue.Count <= 0)
                {
                    continue;//当前无消息，跳过进行下一次排查处理
                }
                //有待处理的消息
                MsgBase _msg = null;
                if (serverMsgQueue.Count > 0)
                {
                    //取出并移除取出来的数据
                    if (!serverMsgQueue.TryDequeue(out _msg))
                    {
                        Debug.LogError("非正常错误！在处理服务器发来信息时，取出并处理消息队列失败！！");
                        continue;
                    }
                }
                //处理取出来的数据
                if (_msg != null)
                {
                    //如果接收到是心跳包
                    if (_msg is MsgPing)
                    {
                        MsgPing _clientMsgPing = _msg as MsgPing;  
                        ServerClientSocket _socket = ClientDic[_clientMsgPing.targetSocket];
                        //更新接收到的心跳包时间（后台运行）
                        _socket.lastPingTime = MySocketTool.GetTimeStamp();
                        //创建消息并返回
                        MsgPing msgPong = new MsgPing();
                        msgPong.timeStamp = _socket.lastPingTime;//存入时间戳
                        SendMessage(msgPong, _socket.socket).Forget();//返回客户端

                        Debug.LogFormat("服务器收到客户端：{0}发来的心跳包，时间戳为：{1}，更新目标客户端时间戳为本地时间戳，同时向客户端返回一个心跳包，时间戳为：{2}",
                            _clientMsgPing.targetSocket.RemoteEndPoint.ToString(), _clientMsgPing.timeStamp.ToString(), _socket.lastPingTime.ToString());

                        //优化性能，只有在Debug下才能显示
                        if (Debug.unityLogger.logEnabled)
                        {
                            MySocketTool.TimeDifference("服务器", _socket.lastPingTime, _msg);
                        }
                    }
                    else
                    {
                        //是其他协议，交给unity处理
                        UnityMsgQueue.Enqueue(_msg);
                    }
                }
                else
                {
                    Debug.LogError("取出来空数据，请检查");
                }
            }
        }

        #endregion

        #region Unity主线程调用

        /// <summary>
        /// 需要Unity处理的消息
        /// </summary>
        internal void Update()
        {
            if (!isInit)
            {
                return;
            }
            //处理unity该处理的消息
            if (ListenSocket != null)
            {
                if (UnityMsgQueue.Count <= 0)
                {
                    return;//没有消息
                }
                //取出消息
                if (UnityMsgQueue.Count > 0)
                {
                    MsgBase msgBase = null;
                    //取出并移除数据
                    if (UnityMsgQueue.TryDequeue(out msgBase))
                    {
                        ServerController.instance.SendReceiveMsgCallBack(msgBase.ProtoType, msgBase);//交给执行回调
                    }
                    else
                    {
                        Debug.LogError("非正常错误！服务器在处理返回给unity处理的信息时，取出并处理消息队列失败！！");
                    }
                }
            }
        }
        #endregion

        #region 关闭链接


        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void CloseClient(ServerClientSocket client)
        {
            //清除客户端
            client.socket.Close();//关闭链接
            //移除已连接客户端
            ServerClientSocket _a=new();//创建用于存储返回的值，不使用
            ClientDic.TryRemove(client.socket,out _a);
            lock ((clientList as ICollection).SyncRoot)
            {
                clientList.Remove(client.socket);
            }
            Debug.LogFormat("一个客户端断开连接，当前连接总数：{0}",
                ClientDic.Count);

        }
        /// <summary>
        /// 关闭服务器，关闭所有已经链接上来的socket以及关闭多余线程
        /// </summary>
        public void Quit()
        {
            if (!isInit)
            {
                return;
            }
            //关闭所有已经链接上来的socket
            List<Socket> _tmp = new List<Socket>();//创建临时空间

            lock ((clientList as ICollection).SyncRoot)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    _tmp.Add(clientList[i]);
                }
            }
            for (int i = 0; i < _tmp.Count; i++)
            {
                CloseClient(ClientDic[_tmp[i]]);
            }
            isThreadOver = true;
            ListenSocket.Close();
            Debug.Log("已关闭所有链接上来的客户端");
        }
        #endregion
    }

}
