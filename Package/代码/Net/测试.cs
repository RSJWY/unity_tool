using Client;
using Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class 测试 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ServerController.instance.AddReceiveMsgCallBack(MyProtocolEnum.MsgTest, (MsgBase _msg) =>
        {
            MsgTest msgTest = (MsgTest)_msg;
            client = msgTest.targetSocket;
            Debug.Log(
                string.Format("客户端：{0}发来一组测试消息，消息为：\n {1}",
                msgTest.targetSocket.RemoteEndPoint.ToString(), msgTest.ReContent.ToString()));
        });
        ClientController.instance.AddReceiveMsgCallBack(MyProtocolEnum.MsgTest, (MsgBase _msg) =>
        {
            MsgTest msgTest = (MsgTest)_msg;
            Debug.Log(
                string.Format("服务器：{0}发来一组测试消息，消息为：\n {1}",
                msgTest.targetSocket.RemoteEndPoint.ToString(), msgTest.ReContent.ToString()));
        });
    }

    Socket client;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            MsgTest _t = new MsgTest();
            _t.ReContent = "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsaffdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsadasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas";

            ClientController.instance.SendMsgToServer(_t);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MsgTest _t = new MsgTest();
            _t.ReContent = "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsaffdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsadasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas";

            ServerController.instance.SendMsgToClient(_t, client);
        }


        if (Input.GetKeyDown(KeyCode.Tab))
        {
            for (int i = 0; i < 3; i++)
            {
                MsgTest _t = new MsgTest();
                _t.ReContent = "开头结尾";

                ClientController.instance.SendMsgToServer(_t);
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < 3; i++)
            {
                MsgTest _t = new MsgTest();
                _t.ReContent = "开头结尾";

                ServerController.instance.SendMsgToClient(_t, client);
            }
        }
    }
}
