using UnityEngine;

public static partial class SendMsgMethod
{
    /// <summary>
    /// 图片提交和获取
    /// </summary>
    /// <param name="_sendImageType">消息类型</param>
    /// <param name="_firstImage">图片字节数据</param>
    /// <param name="_width">图片宽</param>
    /// <param name="_height">图片高</param>
    /// <param name="_name">图片名字</param>
    /// <returns></returns>
    public static SendImage SendSendImage(SendImage.SendImageType _sendImageType, byte[] _firstImage, int _width, int _height, string _name)
    {
        SendImage _sendImage = new()
        {
            sendImageType = _sendImageType,
            ImageByte = _firstImage,
            width = _width,
            height = _height,
            name = _name
        };
        return _sendImage;
    }
    /// <summary>
    /// 心跳包
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public static MsgPing SendMsgPing(long timeStamp)
    {
        MsgPing _MsgPing = new()
        {
            timeStamp =MySocketTool.GetTimeStamp()
        };
        return _MsgPing;
    }
    /// <summary>
    /// 发送错误信息
    /// </summary>
    /// <param name="_msg"></param>
    /// <returns></returns>
    public static TipsMsg SendTipsMsg(string _msg)
    {
        TipsMsg _TipsMsg = new()
        {
            Msg =_msg
        };
        return _TipsMsg;
    }
}