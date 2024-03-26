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

namespace Server
{
    /// <summary>
    /// 服务器模块核心，仅允许通过ServerController调用
    /// </summary>
    internal class ServerManager
    {

        #region 服务器模块核心
        /// <summary>
        /// 监听端口
        /// </summary>
        static int port = 9909;

        /// <summary>
        /// 监听IP
        /// </summary>
        static IPAddress ip = IPAddress.Any;

        /// <summary>
        /// 心跳包间隔时间
        /// </summary>
        internal static long pingInterval = 60;

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
        internal static Dictionary<Socket, ServerClientSocket> ClientDic = new Dictionary<Socket, ServerClientSocket>();

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
        static ConcurrentQueue<MsgBase> msgSendQueue=new ConcurrentQueue<MsgBase>();

        /// <summary>
        /// 消息处理线程
        /// </summary>
        static Thread msgThread;
        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        static Thread pingThread;

        /// <summary>
        /// 使用默认参数
        /// </summary>
        internal void Init()
        {
            SetInit();
        }
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
                ListenSocket.Listen(1);
                //开启异步监听
                ListenSocket.BeginAccept(AcceptCallBack, null);

                //消息处理线程-后台处理
                msgThread = new Thread(MsgThread);
                msgThread.IsBackground = true;//后台运行
                msgThread.Start();
                //心跳包监测处理线程-后台处理
                pingThread=new Thread(PingThread);
                pingThread.IsBackground = true;
                pingThread.Start();

                //输出日志
                Debug.Log(
                    string.Format("服务器启动监听：{0} 成功！！启动异步监听,当前客户端数量：{1}",
                    ListenSocket.LocalEndPoint.ToString(),clientList.Count.ToString()));
            }
            catch (Exception e)
            {
                Debug.LogError(
                    string.Format("服务器启动监听 {0} 失败！！错误信息：\n {1}", 
                    ListenSocket.LocalEndPoint.ToString(), e.ToString()));
            }

        }
        /// <summary>
        /// 有客户端链接上来执行本回调
        /// </summary>
        void AcceptCallBack(IAsyncResult ar)
        {
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
                _ServerClient.lastPingTime = GetTimeStamp();
                //存储到字典
                ClientDic.Add(client, _ServerClient);
                //记录链接上来的客户端
                clientList.Add(client);

                //开启异步接收消息
                client.BeginReceive(
                    _ServerClient.ReadBuff.Bytes, _ServerClient.ReadBuff.WriteIndex, _ServerClient.ReadBuff.Remain, 0, ReceiveCallBack, client);

                Debug.Log(
                    string.Format("和客户端{0}建立链接成功！！并开启异步消息接收，当前客户端数量:{1},并继续监听客户端连接", 
                    client.RemoteEndPoint.ToString(), ClientDic.Count));
            }
            catch (SocketException ex)
            {
                Debug.LogError(string.Format("客户端建立连接失败！！错误信息：\n {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// 接收完成回调
        /// </summary>
        /// <param name="ar"></param>
        void ReceiveCallBack(IAsyncResult ar)
        {
            /*
            //拆包
            Socket _client = (Socket)ar.AsyncState;
            //找对应的客户端容器
            ServerClientSocket _serverClient = ClientDic[_client];
            try
            {
                //接收信息，根据信息解析协议，根据协议内容处理消息再下发到客户端
                int count = _client.EndReceive(ar);
                //每一个客户端有单独的数据读取存储空间用来存数据
                ByteArray readBuff = _serverClient.ReadBuff;
                //如果上次接收包刚好沾满1024的数组（表示存在分包），没有可用空间
                if (readBuff.Remain <= 0)
                {
                    //先处理数据
                    OnReceiveData(_serverClient);
                    //处理完成后再移动数据
                    readBuff.CheckAndMoveBytes();
                    //如果接收的数据过大
                    while (readBuff.Remain <= 0)
                    {
                        //保证如果数据长度大于默认长度，扩充数据，保证信息的正常接收
                        int expandSize = readBuff.length < ByteArray.default_Size ? ByteArray.default_Size : readBuff.length;
                        readBuff.ReSize(expandSize * 2);
                    }
                }
                //代表客户端断开连接
                if (count <= 0)
                {
                    CloseClient(_serverClient);
                    return;
                }
                //下一个读取位置
                readBuff.WriteIndex += count;
                //解析信息
                OnReceiveData(_serverClient);
                //是否移动
                readBuff.CheckAndMoveBytes();
            }
            catch (SocketException ex)
            {
                Debug.LogError(
                    string.Format("读取客户端：{0}发来的消息出错！！错误信息： \n {{1}}",
                    _client.RemoteEndPoint.ToString(),ex.ToString()));
                CloseClient(_serverClient);
                return;
            }
            */
            //接收完成处理
            Socket client = (Socket)ar.AsyncState;//获取客户端
                                                  //找对应的客户端容器
            ServerClientSocket _serverClient = ClientDic[client];
            try
            {
                ByteArray readBuff = _serverClient.ReadBuff;//获取客户端容器的字节容器
                int count = client.EndReceive(ar);//获取接收到的字节长度
                if (count <= 0)
                {
                    //没有接收到数据
                    Debug.LogError(string.Format("读取客户端：{0}发来的消息出错！！数据长度小于0",
                    client.RemoteEndPoint.ToString()));
                    //服务器关闭链接
                    CloseClient(_serverClient);
                    return;
                }
                readBuff.WriteIndex += count;//移动已经写入索引
                OnReceiveData(_serverClient);//把数据交给数据处理函数
                if (readBuff.Remain < 8)
                {
                    //完成解析或者这次接收的数据是分包数据
                    //剩余空间不足（容量-已经写入索引
                    readBuff.MoveBytes();//已经完成一轮解析，移动数据
                    readBuff.ReSize(readBuff.length * 2);//扩容
                }
                //本轮消息处理完成后或者这次处理的数据有分包情况，开启下一次的异步监听，如果分包则继续接收未接收完成的数据
                //等待下一次数据处理接收
                client.BeginReceive(readBuff.Bytes, readBuff.WriteIndex, readBuff.Remain, 0, ReceiveCallBack, client);

            }
            catch (SocketException ex)
            {
                Debug.LogError(string.Format("读取客户端：{0}发来的消息出错！！错误信息： \n {1}",
                    client.RemoteEndPoint.ToString(), ex.ToString()));
                CloseClient(_serverClient);
            }
        }

        /// <summary>
        /// 接收数据处理，处理分包粘包情况
        /// </summary>
        void OnReceiveData(ServerClientSocket serverClientSocket)
        {
            /* 源处理方式
            ByteArray readBuff = clientSocket.ReadBuff;//取出
            //确认消息完整性
            //基本消息长度判断
            if (readBuff.length <= 4 || readBuff.ReadIndex < 0)
            {
                return;//消息不完整，不是一个完整数据（消息头不存在或者读取起点下标意外为负数
            }
            //取出数据，以免影响元数据
            int readIndex = readBuff.ReadIndex;
            byte[] bytes = readBuff.Bytes;
            int bodyLegth = BitConverter.ToInt32(bytes, readIndex);//计算长度
            if (readBuff.length < bodyLegth + 4)
            {
                //判断接收到的信息长度是否小于包体长度+包头长度，
                //如果小于，带表信息不全
                //大于带表信息全了，但可能有粘包存在
                return;
            }
            //信息全了
            readBuff.ReadIndex += 4;//移四位(前四位为包头信息
            //解析协议名
            int nameCount = 0;
            MyProtocolEnum proto = MyProtocolEnum.None;
            try
            {
                //协议名解析并存储解析出来的协议名
                proto = MsgBase.DecodeName(readBuff.Bytes, readBuff.ReadIndex, out nameCount);
            }
            catch (Exception ex)
            {
                Debug.LogError("解析协议名出错: " + ex.ToString());
                CloseClient(clientSocket);
                return;
            }
            //解析协议内容
            if (proto == MyProtocolEnum.None)
            {
                Debug.LogError(
                    string.Format("解析协议名出错,协议名不存在！！返回的协议名为: "+ MyProtocolEnum.None.ToString()));
                CloseClient(clientSocket);
                return;
            }
            //协议名无误
            readBuff.ReadIndex += nameCount;
            //解析协议体
            int bodyCount = bodyLegth - nameCount;//协议体长度（剩下的未处理的字节流就是协议体
            MsgBase msgBase = null;//创建基础协议类
            try
            {
                msgBase = MsgBase.Decode(proto, readBuff.Bytes, readBuff.ReadIndex, bodyCount);
                if (msgBase == null)
                {
                    Debug.LogError(
                        string.Format("从客户端：{0}接收上来的信息解析协议体出错，无法转换基类", clientSocket.socket.RemoteEndPoint.ToString()));
                    CloseClient(clientSocket);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    string.Format("接收客户端：{0}数据协议内容解析错误：{1}",
                    clientSocket.socket.RemoteEndPoint.ToString(),ex.ToString()));
                CloseClient(clientSocket);
                return;
            }
            //协议体解析完成
            readBuff.ReadIndex += bodyCount;
            readBuff.CheckAndMoveBytes();
            //分发消息（反射消息，发送到某一个类）
            //寻找方法，通过反射分发消息，寻找在ServerMsgHandler的函数
            MethodInfo mi = typeof(ServerMsgHandler).GetMethod(proto.ToString());
            //装箱，把需要传给ServerMsgHandler进行处理的数据装箱
            object[] _o = { clientSocket, msgBase };
            if (mi != null)
            {
                //找到
                mi.Invoke(null, _o);
            }
            else
            {
                //没找到
                Debug.LogError(
                    string.Format("通过反射在ServerMsgHandler找不到对应函数，需要函数名为{0}，发送消息的客户端为{1}",
                    proto.ToString(), clientSocket.socket.RemoteEndPoint.ToString()));
            }

            //继续读取消息（粘包消息
            if (readBuff.length > 4)
            {
                //大于头的位置，还有消息，继续读取
                //如果信息长度不够，我们需要再次读取信息
                OnReceiveData(clientSocket);
            }
            */
            //取字节流数组
            ByteArray _byteArray=serverClientSocket.ReadBuff;

            //确认数据是否有误
            if (_byteArray.length <= 4 || _byteArray.ReadIndex < 0)
            {
                //前四位是记录整体数据长度
                return;
            }
            //数据无误
            int readIndex = _byteArray.ReadIndex;
            byte[] bytes = _byteArray.Bytes;
            int bodyLength = BitConverter.ToInt32(bytes, readIndex);//获取整体字节流长度，占用四位数组长度
            if (_byteArray.length < bodyLength + 4)
            {
                //如果消息长度小于读出来的消息长度
                //此为分包，不包含完整数据
                return;
            }
            //存在完整数据
            //协议名解析
            _byteArray.ReadIndex += 4;//前四位存储字节流数组长度信息
            int nameCount = 0;//解析完协议名后要从哪开始读下一阶段的数据
            MyProtocolEnum protocol = MsgBase.DecodeName(_byteArray.Bytes, _byteArray.ReadIndex, out nameCount);//解析协议名
            if (protocol == MyProtocolEnum.None)
            {
                Debug.LogError(
                    string.Format("解析协议名出错,协议名不存在！！返回的协议名为: " + MyProtocolEnum.None.ToString()));
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
                    Debug.LogError(
                   string.Format("解析协议名出错！！无法匹配协议基类协议名不存在！！返回的协议名为: " + MyProtocolEnum.None.ToString()));
                    CloseClient(serverClientSocket);
                    return;
                }
                _byteArray.ReadIndex += bodyCount;
                _byteArray.CheckAndMoveBytes();//解析完成，移动数组，释放占用空间
                //协议解析完成，处理协议
                //把需要处理的信息放到消息队列中，交给线程处理
                msgBase.targetSocket = serverClientSocket.socket;//记录这组消息所属的客户端
                serverMsgQueue.Enqueue(msgBase);//添加到队列内
                //继续处理粘包分包
                if (_byteArray.length > 4)
                {
                    OnReceiveData(serverClientSocket);
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("Socket解析协议体出错 Socket OnReceiveData Error" + ex.ToString());
                CloseClient(serverClientSocket);
            }

        }

        /// <summary>
        /// 发送信息到客户端
        /// </summary>
        /// <param name="msgBase"></param>
        internal void SendMessage(MsgBase msgBase,Socket _client)
        {
            msgBase.targetSocket= _client;//取出目标客户端
            if (_client == null || !_client.Connected)
            {
                Debug.LogError("链接不存在或者未建立链接");
                return;//链接不存在或者未建立链接
            }
            //写入数据
            try
            {
                byte[] nameBytes = MsgBase.EncodeName(msgBase);//协议名编码
                byte[] bodyBytes = MsgBase.Encond(msgBase);//编码协议体
                int len = nameBytes.Length + bodyBytes.Length;//整体长度
                byte[] byteHead = BitConverter.GetBytes(len);//转长度为字节
                byte[] sendBytes = new byte[byteHead.Length + len];//创建发送空间
                //组装字节流数据
                Array.Copy(byteHead, 0, sendBytes, 0, byteHead.Length);//组装头
                Array.Copy(nameBytes, 0, sendBytes, byteHead.Length, nameBytes.Length);//组装协议名
                Array.Copy(bodyBytes, 0, sendBytes, byteHead.Length + nameBytes.Length, bodyBytes.Length);//协议组合
                ByteArray ba = new ByteArray(sendBytes);//开新空间
                //ClientDic[_client].SendBuff=ba;//存储到客户端容器发送数据内

                //同步向队列放数据，这样根据队列的先进先出特性，使得数据同步
                //把消息放到目标客户端的容器内的消息队列，（需要同时向客户端发送多条消息
                ClientDic[_client].SendBuffQueue.Enqueue(ba);
                //写入到队列，向客户端发送消息，根据客户端和绑定的数据发送
                msgSendQueue.Enqueue(msgBase);
                if (msgSendQueue.Count == 1)//如果队列内只有一条待发送信息，直接在主线程内处理
                {
                    _client.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, _client);
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("向客户端发送消息失败 SendMessage Error:" + ex.ToString());
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
                int count = client.EndSend(ar);//获取已经发送的字节流长度
                //判断是否发送完成(消息是否发送完整)
                msgSendQueue.TryPeek(out _msgbase);//取出消息类但不移除，用作数据比较
                //_ba = ClientDic[client].SendBuff;//获取存储发送数据的字节流数组
                ClientDic[client].SendBuffQueue.TryPeek(out _ba);//取出消息字节数组但不移除，
                _ba.ReadIndex += count;//已发送索引
                if (_ba.length == 0)//代表发送完整
                {
                    MsgBase _msDelete;
                    ByteArray _baDelete;
                    msgSendQueue.TryDequeue(out _msDelete);//取出但不使用，只为了从队列中移除
                    ClientDic[client].SendBuffQueue.TryDequeue(out _baDelete);//取出但不使用，只为了从队列中移除
                    if (msgSendQueue.Count > 0)//如果还有数据（指的是还有需要向客户端发送数据的队列
                    {
                        msgSendQueue.TryPeek(out _msgbase);//取出新数据
                        client = _msgbase.targetSocket;//可能存在当前字节流数据不属于当前客户端情况，需要重新获取客户端信息
                        ClientDic[client].SendBuffQueue.TryPeek(out _ba);//获取发送消息数组
                    }
                    else
                    {
                        _ba = null;//发送完成，数组置空
                    }
                }
                //发送不完整或发送完整且存在第二条数据
                //再次发送
                if (_ba != null)
                {
                    client.BeginSend(_ba.Bytes, _ba.ReadIndex, _ba.length, 0, SendCallBack, client);
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("向客户端发送消息失败 SendCallBack Error:" + ex.ToString());
                CloseClient(ClientDic[client]);
            }
        }
        /// <summary>
        /// 心跳包监测线程
        /// </summary>
        void PingThread()
        {
            List<ServerClientSocket> _tmpClose = new List<ServerClientSocket>();//存储需要断开来的客户端
            while (true)
            {
                Thread.Sleep(1000);//本线程可以每秒检测一次
                if (ClientDic.Count<=0)
                {
                    continue;
                }
                //检测心跳包是否超时的计算
                //获取当前时间
                long timeNow = GetTimeStamp();
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
                    CloseClient(clientSocket);
                    Debug.Log(
                        string.Format("客户端：{0}心跳包超时，连接关闭！！" , clientSocket.socket.RemoteEndPoint.ToString()));
                }
                _tmpClose.Clear();//操作完成后清除客户端
            }
        }

        /// <summary>
        /// 消息处理线程，分发消息
        /// </summary>
        void MsgThread()
        {
            while (ListenSocket != null )
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
                    serverMsgQueue.TryDequeue(out _msg);
                }
                //处理取出来的数据
                if (_msg != null)
                {
                    //如果接收到是心跳包
                    if (_msg is MsgPing)
                    {
                        ServerClientSocket _socket = ClientDic[_msg.targetSocket];
                        //更新接收到的心跳包时间（后台运行）
                        _socket.lastPingTime = GetTimeStamp();
                        MsgPing msgPong = new MsgPing();
                        //Debug.Log(
                        //    string.Format("服务器收到客户端：{0}发来的心跳包，时间为：{1}",
                        //    _msg.targetSocket.RemoteEndPoint.ToString(), _socket.lastPingTime.ToString()));
                        SendMessage(msgPong, _socket.socket);//返回客户端
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

        internal void Update()
        {
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
                    UnityMsgQueue.TryDequeue(out msgBase);//取出并移除数据
                    ServerController.instance.SendReceiveMsgCallBack(msgBase.ProtoType, msgBase);//交给执行回调
                }
            }
        }



        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void CloseClient(ServerClientSocket client)
        {
            //清除客户端
            client.socket.Close();//关闭链接
            //移除已连接客户端
            ClientDic.Remove(client.socket);
            clientList.Remove(client.socket);
            Debug.Log(
                string.Format("一个客户端断开连接，当前连接总数：{0}",
                ClientDic.Count));
        }
        /// <summary>
        /// 关闭服务器，关闭所有已经链接上来的socket以及关闭多余线程
        /// </summary>
        public void Quit()
        {
            //关闭线程
            if (msgThread != null && msgThread.IsAlive)
            {
                msgThread.Abort();
            }
            if (pingThread != null && pingThread.IsAlive)
            {
                pingThread.Abort();
            }
            /*
            ////关闭所有已经链接上来的socket
            //List<Socket> _tmp = new List<Socket>();//创建临时空间
            //for (int i = 0; i < clientList.Count; i++)
            //{
            //    _tmp.Add(clientList[i]);
            //}
            //for (int i = 0; i < _tmp.Count; i++)
            //{
            //    CloseClient(ClientDic[_tmp[i]]);
            //}
            //Debug.Log("已关闭所有链接上来的客户端");
            */
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
        #endregion



        #region 单例
        static ServerManager _instance=null;
        public static ServerManager instance
        {
            get
            {
                if (_instance==null)
                {
                    _instance=new ServerManager();
                }
                return _instance;
            }
        }
        private ServerManager()
        {
        }
        #endregion
    }
}
