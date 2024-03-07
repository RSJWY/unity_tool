using ProtoBuf;
using UnityEngine;

/// <summary>
/// 转流，通讯规范，服务器和客户端共同规范
/// </summary>

/// <summary>
/// 本项目自定义的协议
/// </summary>
public enum MyProtocolEnum
{
    None = 0,
    /// <summary>
    /// 心跳包协议
    /// </summary>
    MsgPing = 2,

    /// <summary>
    /// 测试
    /// </summary>
    MsgTest = 9999,
    /// <summary>
    /// 飞屏协议
    /// </summary>
    MsgFeiPing=100,
    /// <summary>
    /// 图片协议
    /// </summary>
    MsgImage = 100
}

[ProtoContract]
public class MsgImage : MsgBase
{
    public MsgImage()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgImage;
    }

    /// <summary>
    /// 记录当前协议
    /// </summary>
    [ProtoMember(1)]
    public override MyProtocolEnum ProtoType { get; set; }

    /// <summary>
    /// 图片数据数组
    /// </summary>
    [ProtoMember(2)]
    public byte[] Image;
    /// <summary>
    /// 图片宽度
    /// </summary>
    [ProtoMember(3)]
    public int width;
    /// <summary>
    /// 图片高度
    /// </summary>
    [ProtoMember(4)]
    public int height;
    #region 图片工具函数
    /// <summary>
    /// 图片转数据
    /// </summary>
    public static byte[] ImageToData(Texture2D _tex)
    {
        if (_tex==null)
        {
            Debug.LogError("传入的图片为空");
            return null;
        }
        byte[] _imageByte= _tex.EncodeToPNG();
        UnityEngine.Object.Destroy(_tex);//用完texture2D，销毁
        return _imageByte;
    }/// <summary>
     /// 图片转数据
     /// </summary>
    public static byte[] ImageToData(Texture _tex)
    {
        if (_tex == null)
        {
            Debug.LogError("传入的图片为空");
            return null;
        }
        Texture2D _t =MsgImage.TextureToTexture2D(_tex);
        byte[] _imageByte = _t.EncodeToPNG();
        UnityEngine.Object.Destroy(_t);//用完texture2D，销毁
        return _imageByte;
    }
    /// <summary>
    /// 数据转图片
    /// </summary>
    /// <param name="_data">图片字节数据</param>
    /// <param name="width">图片宽</param>
    /// <param name="height">图片高</param>
    /// <returns>返回Texture2D，注意！！用完后立即销毁，减少内存占用</returns>
    public static Texture2D ImageDataToImage(byte[] _data,int width,int height)
    {
        if (_data == null|| _data.Length<=0)
        {
            Debug.LogError("传入的数组为空或者长度为0");
            return null;
        }
        if (width<=0||height<=0)
        {
            Debug.LogError("传入的图片尺寸小于0");
            return null;
        }
        Texture2D _clientImage = new Texture2D(width, height, TextureFormat.RGBA32, false); ;
        _clientImage.LoadImage(_data);
        return _clientImage;
    }

    /// <summary>
    /// 运行模式下Texture转换成Texture2D
    /// </summary>
    /// <param name="texture"></param>
    /// <returns>返回Texture2D，注意！！用完后立即销毁，减少内存占用</returns>
    public static Texture2D TextureToTexture2D(Texture texture)
    {
        if (texture==null)
        {
            Debug.LogError("传入的图片为空");
            return null;
        }

        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;

        //Texture2D _t = MsgImage.TextureToTexture2D(livecamera.OutputTexture);//获取纹理
        //Texture2D _img = new Texture2D(_t.width, _t.height, TextureFormat.ARGB32, true);
        //_img.SetPixels(_t.GetPixels());
        //_img.Apply();
    }
    #endregion
}

/// <summary>
/// 飞屏消息
/// </summary>
[ProtoContract]
public class MsgFeiPing : MsgBase
{
    /// <summary>
    /// 飞屏模式枚举
    /// </summary>
    public enum MsgFeiPingModle
    {
        Standby, player, pause, PaiZhao
    }

    public MsgFeiPing()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgFeiPing;
    }

    /// <summary>
    /// 记录当前协议
    /// </summary>
    [ProtoMember(1)]
    public override MyProtocolEnum ProtoType { get; set; }
    /// <summary>
    /// 飞屏模式
    /// </summary>
    [ProtoMember(2)]
    public MsgFeiPingModle msgFeiPingModle;

    /// <summary>
    /// 存储图片
    /// </summary>
    [ProtoMember(3)]
    public MsgImage msgImage;
}

/// <summary>
/// 心跳包
/// </summary>
[ProtoContract]
public class MsgPing: MsgBase 
{
    public MsgPing()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgPing;
    }

    /// <summary>
    /// 记录当前协议
    /// </summary>
    [ProtoMember(1)]
    public override MyProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)]
    public long timeStamp;
}


/// <summary>
/// 测试
/// </summary>
[ProtoContract]
public class MsgTest : MsgBase
{
    public MsgTest()
    {
        //确定具体是哪个协议，无需标记转流
        ProtoType = MyProtocolEnum.MsgTest;
    }

    [ProtoMember(1)]
    public override MyProtocolEnum ProtoType { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [ProtoMember(2)]
    public string Content { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [ProtoMember(3)]
    public string ReContent { get; set; }
}

