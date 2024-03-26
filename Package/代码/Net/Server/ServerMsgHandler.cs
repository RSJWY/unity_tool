using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// 服务器端对消息的处理
    /// </summary>
    public partial class ServerMsgHandler
    {
    //    //所有的协议处理函数都是这个标准，函数名=协议枚举名=类名

    //    /// <summary>
    //    /// 密钥协议处理
    //    /// </summary>
    //    /// <param name="c"></param>
    //    /// <param name="msgBase"></param>
    //    public static void MsgSecret(ServerClientSocket c,MsgBase msgBase)
    //    {
    //        MsgSecret msgSecret = (MsgSecret)msgBase;
    //        msgSecret.Srcret = ServerSocketTCP.SecretKey;//密钥获取
    //        Debug.Log("服务器收到密钥请求，返回一个新密钥");
    //        ServerSocketTCP.Send(c, msgSecret);//返回客户端
    //    }

    //    /// <summary>
    //    /// 心跳包处理
    //    /// </summary>
    //    /// <param name="c"></param>
    //    /// <param name="msgBase"></param>
    //    public static void MsgPing(ServerClientSocket c, MsgBase msgBase)
    //    {
    //        c.lastPingTime = ServerSocketTCP.GetTimeStamp();//记录客户端发送心跳包时间
    //        MsgPing msgPong = new MsgPing();
    //        Debug.Log("服务器收到心跳包，并返回一个心跳包");
    //        ServerSocketTCP.Send(c, msgPong);//返回客户端
    //    }
    //    /// <summary>
    //    /// 注册处理
    //    /// </summary>
    //    /// <param name="c"></param>
    //    /// <param name="msgBase"></param>
    //    public static void MsgRegister(ServerClientSocket c, MsgBase msgBase)
    //    {
    //        Debug.Log("接收到注册请求");
    //        string token;
    //        MsgRegister msg = (MsgRegister)msgBase;
    //        var rst= UserManager.Instance.Register(msg.RegisterType, msg.Acocount, msg.Password, out token);
    //        msg.Result = rst;
    //        Debug.Log("返回注册状态："+ rst.ToString());
    //        ServerSocketTCP.Send(c, msg);
    //    }
    //    /// <summary>
    //    /// 登录处理
    //    /// </summary>
    //    /// <param name="c"></param>
    //    /// <param name="msgBase"></param>
    //    public static void MsgLogin(ServerClientSocket c, MsgBase msgBase)
    //    {
    //        Debug.Log("接收到登录请求");
    //        string token;
    //        int userId;
    //        MsgLogin msg = (MsgLogin)msgBase;
    //        var rst = UserManager.Instance.Login(msg.LoginType, msg.Acocount, msg.Password, out token,out userId);
    //        msg.Result = rst;
    //        msg.Token = token;
    //        c.UserId = userId;
    //        Debug.Log("返回登录状态：" + rst.ToString());
    //        ServerSocketTCP.Send(c, msg);
    //    }

    //    /// <summary>
    //    /// 测试
    //    /// </summary>
    //    /// <param name="c"></param>
    //    /// <param name="msgBase"></param>
    //    public static void MsgTest(ServerClientSocket c, MsgBase msgBase)
    //    {
    //        MsgTest msgTest = (MsgTest)msgBase;
    //        Debug.Log("收到客户端信息 "+msgTest.ReContent);
    //        msgTest.ReContent = "服务器发送的数据!!!sfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
    //        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
    //        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
    //        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" + "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsaf" +
    //        "dasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffa" +
    //        "sfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfs" +
    //        "afdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
    //        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
    //        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas" +
    //        "fasfsafdasffasfasfsafdasffasfasfsafdasffasfasfsafdasffas";
    //        ServerSocketTCP.Send(c, msgTest);//返回客户端
    //    }

    }
}
