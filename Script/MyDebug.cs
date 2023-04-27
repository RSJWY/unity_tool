using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MyDebug : MonoBehaviour
{
    public Text debug_text;


    private void Update()
    {
        get_error();
    }
    public void get_error()
    {
        Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>     //绑定收到日志消息事件
        {
            if (type == LogType.Exception || type == LogType.Error)         //判断是否是异常或报错
            {
                debug_text.text = condition + "\n" + stackTrace;            //信息打印在屏幕上
            }
            debug_text.text = condition + "\n" + stackTrace;
        };
    }
}