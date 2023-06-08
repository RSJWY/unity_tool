using log4net;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace RSJWY.Log
{
    internal static class ThreadLogger
    {
        private static ILog log = LogManager.GetLogger("FileLogger"); //获取记录器
        /// <summary>
        /// 是否开启unityDebug监听
        /// </summary>
        static bool isUnityDebugOpen;
        /// <summary>
        /// 消息队列
        /// </summary>
        static ConcurrentQueue<LogMessage> logMsgQue;
        /// <summary>
        /// 信号
        /// </summary>
        static ManualResetEvent mre;
        static ThreadLogger()
        {
            isUnityDebugOpen = false;
            string str_filePath = Application.streamingAssetsPath + "/Log/LogOutPut/";
            //判断要输出的路径是否存在，如果不存在，则创建
            if (!Directory.Exists(str_filePath))
            {
                Directory.CreateDirectory(str_filePath);//目录不存在，创建
            }

            //ApplicationLogPath和LogFileName在log4net.config中使用
            FileInfo file = new System.IO.FileInfo(Application.streamingAssetsPath + "/Log/log4net.config"); //获取log4net配置文件
            GlobalContext.Properties["ApplicationLogPath"] = Path.Combine(str_filePath); //日志生成的路径
            GlobalContext.Properties["LogFileName"] = "日志"; //生成日志的文件名
            log4net.Config.XmlConfigurator.ConfigureAndWatch(file); //加载log4net配置文件

            Log("构造初始化完成，模式：多线程，时间：{0}", GetNowTime());
        }

        /// <summary>
        /// 设置UnityDebug事件监听
        /// </summary>
        internal static void SetUnityDebugOutPut(bool _isOpen = true)
        {
            if (_isOpen == true && isUnityDebugOpen == false)
            {
                Application.logMessageReceivedThreaded += onLogMessageReceived; //添加unity日志监听
                Log("开始UnityDebug事件监听，时间：{0}", GetNowTime());
                isUnityDebugOpen = true;
            }
            if (_isOpen == false && isUnityDebugOpen == true)
            {
                Application.logMessageReceivedThreaded -= onLogMessageReceived; //添加unity日志监听
                Log("关闭UnityDebug事件监听，时间：{0}", GetNowTime());
                isUnityDebugOpen = false;
            }
        }

        /// <summary>
        /// 事件监听
        /// </summary>
        /// <param name="condition">消息内容</param>
        /// <param name="stackTrace">堆栈跟踪</param>
        /// <param name="type">事件类型</param>
        static void onLogMessageReceived(string condition, string stackTrace, LogType type)
        {

            switch (type)
            {
                case LogType.Error:
                    log.ErrorFormat("事件类型：" + type.ToString() + "{0}\r\n{1}",
                        "状态：" + condition, "堆栈跟踪：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Assert:
                    log.DebugFormat("事件类型：" + type.ToString() + "{0}\r\n{1}",
                        "状态：" + condition, "堆栈跟踪：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Exception:
                    log.FatalFormat("事件类型：" + type.ToString() + "{0}\r\n{1}",
                        "状态：" + condition, "堆栈跟踪：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Warning:
                    log.WarnFormat("事件类型：" + type.ToString() + "{0}\r\n{1}",
                        "状态：" + condition, "堆栈跟踪：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Log:
                    log.Info("事件类型：" + type.ToString() + "   " + condition);
                    break;
                default:
                    log.Info("LogType类型无法匹配！" + condition);//这个情况基本不会发生
                    break;
            }
        }
        /// <summary>
        /// 处理消息线程
        /// </summary>
        static void ThreadLog()
        {
            LogMessage logMsg;

            //while (true)
            //{
            //    // 等待信号通知
            //    mre.WaitOne();
            //    // 判断是否有内容需要如磁盘 从列队中获取内容，并删除列队中的内容
            //    while (logMsgQue.Count > 0 && logMsgQue.TryDequeue(out logMsg))
            //    {
            //        // 判断日志等级，然后写日志
            //        switch (msg.Level)
            //        {
            //            case FlashLogLevel.Debug:
            //                log.Debug(msg.Message, msg.Exception);
            //                break;
            //            case FlashLogLevel.Info:
            //                log.Info(msg.Message, msg.Exception);
            //                break;
            //            case FlashLogLevel.Error:
            //                log.Error(msg.Message, msg.Exception);
            //                break;
            //            case FlashLogLevel.Warn:
            //                log.Warn(msg.Message, msg.Exception);
            //                break;
            //            case FlashLogLevel.Fatal:
            //                log.Fatal(msg.Message, msg.Exception);
            //                break;
            //        }
            //    }
            //    // 重新设置信号
            //    mre.Reset();
            //    Thread.Sleep(1);
            //}

        }

        static string GetNowTime()
        {
            string time = string.Format(
               "{3:D4}_{4:D2}_{5:D2}_{0:D2}_{1:D2}_{2:D2}",
               DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
               DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//获取当前时间
            return time;
        }

        #region 单独输出错误信息

        /// <summary>
        /// 正常Debug
        /// </summary>
        public static void Log(object message)
        {
            log.Debug(message);
        }
        /// <summary>
        /// 正常Debug
        /// </summary>
        public static void Log(string format, params object[] args)
        {
            log.DebugFormat(format, args);
        }
        /// <summary>
        /// 信息
        /// </summary>
        public static void LogInfo(object message)
        {
            log.Info(message);
        }
        /// <summary>
        /// 信息
        /// </summary>
        public static void LogInfo(string format, params object[] args)
        {
            log.InfoFormat(format, args);
        }

        /// <summary>
        /// 警告
        /// </summary>
        public static void Logwarn(object message)
        {
            log.Warn(message);
        }
        /// <summary>
        /// 警告
        /// </summary>
        public static void LogWarn(string format, params object[] args)
        {
            log.WarnFormat(format, args);
        }

        /// <summary>
        /// 错误
        /// </summary>
        public static void LogError(object message)
        {
            log.Error(message);
        }
        /// <summary>
        /// 错误
        /// </summary>
        public static void LogError(string format, params object[] args)
        {
            log.ErrorFormat(format, args);
        }

        /// <summary>
        /// 致命错误
        /// </summary>
        public static void LogFatal(object message)
        {
            log.Fatal(message);
        }
        /// <summary>
        /// 致命错误
        /// </summary>
        public static void LogFatal(string format, params object[] args)
        {
            log.FatalFormat(format, args);
        }
        #endregion
    }


    /// <summary>
    /// 消息
    /// </summary>
    public struct LogMessage
    {
        public bool isUse;
        /// <summary>
        /// 消息
        /// </summary>
        public string format;
        /// <summary>
        /// 消息数组
        /// </summary>
        public object[] args;
        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception ex;
        /// <summary>
        /// 类型
        /// </summary>
        public LogType type;
        
        /// <summary>
        /// 添加消息
        /// </summary>
        public void Set(LogType _type,string _format)
        {
            InternalSet(_type, _format,null,null);
        }
        /// <summary>
        /// 添加消息
        /// </summary>
        public void Set(LogType _type, string _format, Exception _ex)
        {
            InternalSet(_type, _format,_ex,null);
        }
        /// <summary>
        /// 添加消息
        /// </summary>
        public void Set(LogType _type, string _format, params object[] _args)
        {
            InternalSet(_type, _format,null,_args);
        }
        /// <summary>
        /// 添加消息
        /// </summary>
        public void Set(LogType _type, string _format, Exception _ex, params object[] _args)
        {
            InternalSet(_type, _format, _ex,_args);
        }

        /// <summary>
        /// 内部调用存储
        /// </summary>
        void InternalSet(LogType _type, string _format, Exception _ex,object[] _args)
        {
            isUse= true;
            type= _type;
            format = "";
            ex = _ex;
            args = _args;
        }
    }
}
