using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;//线程
using System;

/// <summary>
/// 服务器代码
/// </summary>
public class MyService : MonoBehaviour
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
        btn.onClick.AddListener(() =>
        {
            //记录输入的ip
            ip = input.text;
            port = int.Parse(port_in.text);
            //开启线程
            Thread p = new Thread(MyStart);
            p.Start();
        });
        send_btn.onClick.AddListener(SendEvent);

    }

    private void SendEvent()
    {
        string str = send_str.text;
        try
        {
            byte[] b = Encoding.Unicode.GetBytes(str);
            stream.Write(b, 0, b.Length);


        }
        catch (Exception e)
        {
            t.text += "\n" + e.ToString();

        }
    }

    //新线程监听任务
    private void MyStart()
    {
        //根据ip和端口，开始监听
        TcpListener listener = new TcpListener(IPAddress.Parse(ip), port);
        listener.Start();
        t.text = "开启服务器";
        client = listener.AcceptTcpClient();//客户端链接
        stream = client.GetStream();//获取客户端流
        t.text += "\n 客户端已连接";

        //循环监听
        do
        {
            try
            {
                int size = 1024;//占位
                byte[] b = new byte[size];//数组创建
                int reader = stream.Read(b,0,size);//读取
                //如果读到的数据长度为0，链接断开
                if (reader==0)
                {
                    t.text += "\n 客户端断开";
                    break;
                }
                //获取接收到的流
                string str = Encoding.Unicode.GetString(b,0,reader);
                //长度够，打印输出
                if (str.Length>0)
                {
                    t.text += "\n"+str;
                }
            }
            catch (Exception e)
            {

                t.text += "\n"+e.ToString();
            }
        } while (true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
