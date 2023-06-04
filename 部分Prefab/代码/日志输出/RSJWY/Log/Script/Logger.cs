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
    public static class Logger
    {
        private static ILog log = LogManager.GetLogger("FileLogger"); //获取记录器

        static bool isOpen;
        static Logger()
        {
            isOpen = false;
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

            Log("构造初始化完成，时间：{0}", GetNowTime());
        }

        /// <summary>
        /// 设置UnityDebug事件监听
        /// </summary>
        internal static void SetUnityDebugOutPut(bool _isOpen=true)
        {
            if (_isOpen == true && isOpen == false)
            {
                Application.logMessageReceivedThreaded += onLogMessageReceived; //添加unity日志监听
                Log("开始UnityDebug事件监听，时间：{0}", GetNowTime());
                isOpen = true;
            }
            if (_isOpen == false && isOpen == true)
            {
                Application.logMessageReceivedThreaded -= onLogMessageReceived; //添加unity日志监听
                Log("关闭UnityDebug事件监听，时间：{0}", GetNowTime());
                isOpen = false;
            }
        }

        /// <summary>
        /// 事件监听
        /// </summary>
        /// <param name="condition">状态</param>
        /// <param name="stackTrace">线程</param>
        /// <param name="type">事件类型</param>
        static void onLogMessageReceived(string condition, string stackTrace, LogType type)
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
}
