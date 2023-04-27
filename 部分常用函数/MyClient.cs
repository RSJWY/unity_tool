using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;//线程
using System;

public class MyClient : MonoBehaviour
{
    public InputField input;
    public InputField port_in;
    public Button btn;
    public Text t;

    public InputField send_str;
    public Button send_btn;

    string ip;
    int port = 8080;
    TcpClient client;
    NetworkStream stream;



    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(ClickEvent);
        send_btn.onClick.AddListener(SendEvent);
    }
    /// <summary>
    /// 发送信息
    /// </summary>
    private void SendEvent()
    {
        string _str = send_str.text;
        try
        {
            //转换成流
            byte[] _b = Encoding.Unicode.GetBytes(_str);
            stream.Write(_b, 0, _b.Length);
        }
        catch (Exception e)
        {

            t.text = "\n" + e.ToString();
        }
    }
    /// <summary>
    /// 连接服务器
    /// </summary>
    void ClickEvent()
    {
        client = new TcpClient();
        ip = input.text;
        port = int.Parse(port_in.text);

        try
        {
            //尝试链接
            client.Connect(IPAddress.Parse(ip), port);
            t.text = "连接成功";
            stream = client.GetStream();//获取自身的流
        }
        catch (Exception e)
        {
            t.text += "\n" + e.ToString();
        }
        //监听服务器线程
        client = new TcpClient();//TCP通讯
        ip = input.text;//输入
        Thread p = new Thread(MyStart);//开线程
        p.Start();
    }

    private void MyStart(object obj)
    {
        do
        {
            try
            {
                int size = 1024;//长度
                byte[] readBuffer = new byte[size];
                int reader = stream.Read(readBuffer, 0, size);//将转换的信息流存入reader中
                if (reader == 0)
                {
                    t.text += "\n" + "服务端断开连接";
                    break;
                }//发送信息
                string str = Encoding.Unicode.GetString(readBuffer, 0, reader);
                if (str.Length > 0)
                {
                    t.text += "\n" + str;
                }

            }
            catch (Exception e)
            {
                t.text += "\n" + e.ToString();

            }

        } while (true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
