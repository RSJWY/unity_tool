using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;


namespace Server
{
    /// <summary>
    /// 服务器模块管理器，用于和Unity之间交互
    /// </summary>
    public class ServerController : MonoBehaviour
    {
        static ServerController _instance;
        public static ServerController instance { get { return _instance; } }
        

        /// <summary>
        /// 接收到消息后执行的回调
        /// </summary>
        Dictionary<MyProtocolEnum, Action<MsgBase>> receiveMsgCallBack = new Dictionary<MyProtocolEnum, Action<MsgBase>>();

        // Start is called before the first frame update
        void Awake()
        {
            _instance=this;
            ServerManager.instance.Init();
        }

        // Update is called once per frame
        void Update()
        {
            ServerManager.instance.Update();
        }

        /// <summary>
        /// 向客户端发消息
        /// </summary>
        public void SendMsgToClient(MsgBase msgBase,Socket client)
        {
            ServerManager.instance.SendMessage(msgBase,client);
        }


        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="myProtocolEnum"></param>
        /// <param name="action"></param>
        public void  AddReceiveMsgCallBack(MyProtocolEnum myProtocolEnum, Action<MsgBase> action)
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
            ServerManager.instance.Quit();//关闭线程
        }
    }
}

