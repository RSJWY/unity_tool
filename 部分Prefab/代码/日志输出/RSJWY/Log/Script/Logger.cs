using System;
using System.IO;
using log4net;
using UnityEngine;

namespace RSJWY.Log
{
    /// <summary>
    /// 日志配置，不要直接引用，已做调用范围限制处理，仅 RSJWY.Log命名空间可调用
    /// 引用：https://passion.blog.csdn.net/article/details/127886423?spm=1001.2101.3001.6650.5&utm_medium=distribute.pc_relevant.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-5-127886423-blog-119851412.235%5Ev36%5Epc_relevant_anti_vip_base&depth_1-utm_source=distribute.pc_relevant.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-5-127886423-blog-119851412.235%5Ev36%5Epc_relevant_anti_vip_base&utm_relevant_index=10
    /// </summary>
    internal class Logger
    {
        private static ILog log = LogManager.GetLogger("FileLogger"); //获取记录器

        private static Logger _Instance;
        /// <summary>
        /// 是否开启日志监听
        /// </summary>
        bool isOpen;

        internal static Logger Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Logger();
                }
                return _Instance;
            }
        }

        Logger()
        {
            isOpen = false;
        }

        /// <summary>
        /// 初始化，需要从外部调用
        /// </summary>
        internal void Init()
        {
            if (isOpen)
            {
                return;
            }
            isOpen = true;

            Application.logMessageReceivedThreaded += onLogMessageReceived; //添加unity日志监听
            string time = string.Format(
                "{3:D4}_{4:D2}_{5:D2}_{0:D2}_{1:D2}_{2:D2}",
                DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//获取当前时间

            //ApplicationLogPath和LogFileName在log4net.config中使用
            FileInfo file = new System.IO.FileInfo(Application.streamingAssetsPath + "/Log/log4net.config"); //获取log4net配置文件
            GlobalContext.Properties["ApplicationLogPath"] = Path.Combine(Application.streamingAssetsPath, "Log"); //日志生成的路径
            GlobalContext.Properties["LogFileName"] = "log_" + time; //生成日志的文件名
            log4net.Config.XmlConfigurator.ConfigureAndWatch(file); //加载log4net配置文件

            Debug.Log("日志初始化，时间为：" + time);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        internal void Close()
        {
            string time = string.Format(
               "{3:D4}_{4:D2}_{5:D2}_{0:D2}_{1:D2}_{2:D2}",
               DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
               DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//获取当前时间

            Debug.Log("日志关闭，时间为：" + time);
            Application.logMessageReceivedThreaded -= onLogMessageReceived; //添加unity日志监听
            isOpen = false;
        }

        /// <summary>
        /// 事件监听
        /// </summary>
        /// <param name="condition">状态</param>
        /// <param name="stackTrace">线程</param>
        /// <param name="type">事件类型</param>
        private void onLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    log.ErrorFormat("事件类型：" + type.ToString() + "{ 0}\r\n{1}",
                        "状态：" + condition, "线程：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Assert:
                    log.DebugFormat("事件类型：" + type.ToString() + "{ 0}\r\n{1}",
                        "状态：" + condition, "线程：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Exception:
                    log.FatalFormat("事件类型：" + type.ToString() + "{ 0}\r\n{1}",
                        "状态：" + condition, "线程：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Warning:
                    log.WarnFormat("事件类型：" + type.ToString() + "{ 0}\r\n{1}",
                        "状态：" + condition, "线程：" + stackTrace.Replace("\n", "\r\n"));
                    break;
                case LogType.Log:
                    log.Info("事件类型：" + type.ToString() + "   " + condition);
                    break;
                default:
                    log.Info("LogType类型无法匹配！" + condition);//这个情况基本不会发生
                    break;
            }
        }
    }
}
