using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;

/// <summary>
/// 单例构造器，注：不能用于monobeavior
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    private static T _instance;
    private static readonly object mutex = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                //线程安全保障
                Monitor.Enter(mutex);
                _instance=new T();
                Monitor.Exit(mutex);
            }
            return _instance;
        }
    }
}