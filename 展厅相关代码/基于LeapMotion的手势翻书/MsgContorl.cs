using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class MsgContorl
{
    static object thislock = new();
    static MsgContorl _instance;
    public static MsgContorl instance
    {
        get
        {
            if (_instance == null)
            {
                lock (thislock)
                {
                    if (_instance == null)
                    {
                        _instance = new();
                    }
                }
            }
            return _instance;
        }
    }

    Dictionary<BookPageEvent, Action<int>> pageMsgDic = new();

    public void AddEvent(BookPageEvent bookPageEvent, Action<int> action)
    {
        if (pageMsgDic.ContainsKey(bookPageEvent))
        {
            pageMsgDic[bookPageEvent] += action;
        }
        else
        {
            pageMsgDic.Add(bookPageEvent,action);
        }
    }

    public void RemoveEvent(BookPageEvent bookPageEvent, Action<int> action)
    {
        if (pageMsgDic.ContainsKey(bookPageEvent))
        {
            pageMsgDic[bookPageEvent] -= action;
        }
    }
    public void SendEvent(BookPageEvent bookPageEvent,int pageNum){
        if(pageMsgDic.ContainsKey(bookPageEvent)){
            pageMsgDic[bookPageEvent]?.Invoke(pageNum);
        }

    }

}

public enum BookPageEvent
{
    NowPage

}