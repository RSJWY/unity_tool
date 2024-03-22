using Server;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;


namespace Client
{
    /// <summary>
    /// 客户端的控制器
    /// </summary>
    public class ClientController : MonoBehaviour
    {
        static ClientController _instance;
        public static ClientController instance { get { return _instance; } }

        /// <summary>
        /// 接收到消息后执行的回调
        /// </summary>
        Dictionary<MyProtocolEnum, Action<MsgBase>> receiveMsgCallBack = new Dictionary<MyProtocolEnum, Action<MsgBase>>();
        private void Awake()
        {
            _instance = this;
        }
        void Start()
        {
            ClientManager.Instance.Connect("127.0.0.1", 9909);//开启链接服务器
            StartCoroutine(ClientManager.Instance.CheckNetThread());
        }


        void Update()
        {
            ClientManager.Instance.Update();
        }

        /// <summary>
        /// 向服务器发消息
        /// </summary>
        public void SendMsgToServer(MsgBase msgBase)
        {
            ClientManager.Instance.SendMessage(msgBase);
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="myProtocolEnum"></param>
        /// <param name="action"></param>
        public void AddReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action<MsgBase> action)
        {
            if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
            {
                receiveMsgCallBack[myProtocolEnum] += action;
            }
            else
            {
                receiveMsgCallBack.Add(myProtocolEnum, action);
            }
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="myProtocolEnum"></param>
        /// <param name="action"></param>
        public void RemoveReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action<MsgBase> action)
        {
            if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
            {
                receiveMsgCallBack[myProtocolEnum] -= action;
            }
            else
            {
                receiveMsgCallBack.Add(myProtocolEnum, action);
            }
        }
        /// <summary>
        /// 使用
        /// </summary>
        /// <param name="myProtocolEnum"></param>
        /// <param name="msgBase"></param>
        internal void SendReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, MsgBase msgBase)
        {
            if (receiveMsgCallBack.ContainsKey(myProtocolEnum))
            {
                receiveMsgCallBack[myProtocolEnum]?.Invoke(msgBase);
            }
        }

        private void OnApplicationQuit()
        {
            //ClientManager.Instance.Close();
        }
    }
}

