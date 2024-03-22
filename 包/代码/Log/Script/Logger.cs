using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;
using UnityEngine;

namespace Log
{
    /// <summary>
    /// 日志配置，不要直接引用，已做调用范围限制处理，仅 RSJWY.Log命名空间可调用
    /// 引用：https://passion.blog.csdn.net/article/details/127886423?spm=1001.2101.3001.6650.5&utm_medium=distribute.pc_relevant.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-5-127886423-blog-119851412.235%5Ev36%5Epc_relevant_anti_vip_base&depth_1-utm_source=distribute.pc_relevant.none-task-blog-2%7Edefault%7ECTRLIST%7ERate-5-127886423-blog-119851412.235%5Ev36%5Epc_relevant_anti_vip_base&utm_relevant_index=10
    /// 注意！！本类内不得使用debug方法，否则会导致死循环，请使用log4net方法输出
    /// </summary>
    public static class Logger
    {
        private static readonly ILog log = LogManager.GetLogger("FileLogger"); //获取记录器
        /// <summary>
        /// 日志写入线程
        /// </summary>
        static Thread loggerThread;
        /// <summary>
        /// 消息处理队列
        /// </summary>
        static ConcurrentQueue<LoggerInfo> loggerInfoQueue=new ConcurrentQueue<LoggerInfo>();
        static Logger()
        {
            string str_filePath = Application.streamingAssetsPath + "/Log/LogOutPut/";//输出路径设置
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
            if (_isOpen == true)
            {
                Application.logMessageReceivedThreaded += onLogMessageReceived; //添加unity日志监听
                Log("开始UnityDebug事件监听，时间：{0}", GetNowTime());
                //启动日志处理线程
                loggerThread = new Thread(LoggerInfoThread);
                loggerThread.IsBackground = true;
                loggerThread.Start();
            }
            if (_isOpen == false)
            {
                Application.logMessageReceivedThreaded -= onLogMessageReceived; //添加unity日志监听
                Log("关闭UnityDebug事件监听，时间：{0}", GetNowTime());
                if (loggerThread!=null&&loggerThread.IsAlive)
                {
                    loggerThread.Abort();
                }
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
            /*
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
                    log.Info(
                        string.Format("事件类型：{0} 状态：{1} 堆栈跟踪：{2}",
                        type.ToString(), condition.ToString(), stackTrace.Replace("\n", "\r\n")));
                    break;
                default:
                    log.Info("LogType类型无法匹配！" + condition);//这个情况基本不会发生
                    break;
            }
            */
            LoggerInfo loggerInfo = new LoggerInfo();//创建
            //存数据
            loggerInfo.type = type;
            loggerInfo.condition = condition;
            loggerInfo.stackTrace = stackTrace;
            loggerInfo.time = GetNowTime();//记录回调发生的时间
            loggerInfo.threadID = Thread.CurrentThread.ManagedThreadId.ToString();
            //放入队列
            loggerInfoQueue.Enqueue(loggerInfo);
        }

        static string GetNowTime()
        {
            string time = string.Format(
               "{3:D4}_{4:D2}_{5:D2}_{0:D2}_{1:D2}_{2:D2}",
               DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second,
               DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);//获取当前时间
            return time;
        }
        /// <summary>
        /// 日志处理线程
        /// </summary>
        static void LoggerInfoThread()
        {
            //创建需要的变量
            LoggerInfo _logger;

            while (true)
            {
                if (loggerInfoQueue.Count<=0)
                {
                    continue;
                }
                loggerInfoQueue.TryDequeue(out _logger);
                switch (_logger.type)
                {
                    case LogType.Error:
                        log.ErrorFormat(
                            string.Format("事件类型：{0} 内容：\n{1} \n消息创建时间：{3} 消息线程ID：{4} 堆栈跟踪：{2} ",
                            _logger.type.ToString(), _logger.condition.ToString(), _logger.stackTrace.Replace("\n", "\r\n"),
                            _logger.time.ToString(), _logger.threadID.ToString()));
                        break;
                    case LogType.Assert:
                        log.DebugFormat(
                             string.Format("事件类型：{0} 内容：\n{1} \n消息创建时间：{3} 消息线程ID：{4} 堆栈跟踪：{2} ",
                            _logger.type.ToString(), _logger.condition.ToString(), _logger.stackTrace.Replace("\n", "\r\n"),
                            _logger.time.ToString(), _logger.threadID.ToString()));
                        break;
                    case LogType.Exception:
                        log.FatalFormat(
                            string.Format("事件类型：{0} 内容：\n{1} \n消息创建时间：{3} 消息线程ID：{4} 堆栈跟踪：{2} ",
                            _logger.type.ToString(), _logger.condition.ToString(), _logger.stackTrace.Replace("\n", "\r\n"),
                            _logger.time.ToString(), _logger.threadID.ToString()));
                        break;
                    case LogType.Warning:
                        log.WarnFormat(
                             string.Format("事件类型：{0} 内容：\n{1} \n消息创建时间：{3} 消息线程ID：{4} 堆栈跟踪：{2} ",
                            _logger.type.ToString(), _logger.condition.ToString(), _logger.stackTrace.Replace("\n", "\r\n"),
                            _logger.time.ToString(), _logger.threadID.ToString()));
                        break;
                    case LogType.Log:
                        log.Info(
                             string.Format("事件类型：{0} 内容：\n{1} \n消息创建时间：{3} 消息线程ID：{4} 堆栈跟踪：{2} ",
                            _logger.type.ToString(), _logger.condition.ToString(), _logger.stackTrace.Replace("\n", "\r\n"),
                            _logger.time.ToString(), _logger.threadID.ToString()));
                        break;
                    default:
                        log.Info(
                             string.Format("事件类型：{0} 内容：\n{1} \n消息创建时间：{3} 消息线程ID：{4} 堆栈跟踪：{2} ",
                            _logger.type.ToString(), _logger.condition.ToString(), _logger.stackTrace.Replace("\n", "\r\n"),
                            _logger.time.ToString(), _logger.threadID.ToString()));//这个情况基本不会发生
                        break;
                }
            }
        }

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

        /// <summary>
        /// 存储消息的结构题
        /// </summary>
        struct LoggerInfo 
        { 
            /// <summary>
            /// 事件内容
            /// </summary>
            public string condition;
            /// <summary>
            /// 堆栈跟踪
            /// </summary>
            public string stackTrace;
            /// <summary>
            /// 事件类型
            /// </summary>
            public LogType type;
            /// <summary>
            /// 当前结构题创建时间
            /// </summary>
            public string time;
            /// <summary>
            /// 线程id
            /// </summary>
            public string threadID;

        }


    }
}
