using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// 基础协议类 序列化反序列化，服务器和客户端共用
/// </summary>
public class MsgBase
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public virtual MyProtocolEnum ProtoType{get; set;}
    /// <summary>
    /// 目标socket
    /// </summary>
    public Socket targetSocket;

    /// <summary>
    /// 编码协议名
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());
        Int16 len = (Int16)nameBytes.Length;//记录协议名长度
        byte[] bytes = new byte[2 + len];//开新的存储空间用于返回编码好的协议及协议长度（协议长度通过两位数组存储
        //添加值
        bytes[0] = (byte)(len % 256);//这个教程；里说是一个自定义的编码方案
        bytes[1] = (byte)(len / 256);//没问题别动就行了
        Array.Copy(nameBytes, 0, bytes, 2, len);//将编码好的协议名拷贝到存储空间去，前两位存储长度，所以从第三位存储协议名
        return bytes;
    }
    /// <summary>
    /// 解码协议名
    /// </summary>
    /// <param name="bytes">数据数组</param>
    /// <param name="offset">开始读索引</param>
    /// <param name="count">返回协议名长度</param>
    /// <returns></returns>
    public static MyProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;//初始0
        if (offset + 2 > bytes.Length)
        {
            //存储协议名长度信息的数组位不存在，无法获取长度且协议名协议体都不存在
            Debug.LogError("解码协议名出错！！存储协议名长度信息的数组位不存在，无法获取长度且协议名协议体都不存在!!返回：" 
                + MyProtocolEnum.None.ToString());
            return MyProtocolEnum.None;//协议不存在
        }
        //协议存在时，获取协议长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (offset + 2 + len > bytes.Length)
        {
            //只包含了长度信息，没有包含名字且协议体都不存在
            Debug.LogError("解码协议名出错！！只包含了长度信息，没有包含名字且协议体都不存在!!返回：" 
                + MyProtocolEnum.None.ToString());
            return MyProtocolEnum.None;//协议不存在
        }
        count = 2 + len;//移动位置，规避存储协议名长度的位置
        //开始解析
        try
        {
            //确认协议类型
            string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
            MyProtocolEnum _tmp = (MyProtocolEnum)System.Enum.Parse(typeof(MyProtocolEnum), name);
            return _tmp;
        }
        catch (Exception ex)
        {
            Debug.LogError("不存在的协议: " + ex.ToString());
            return MyProtocolEnum.None;
        }
    }
    /// <summary>
    /// 协议体序列化
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encond(MsgBase msgBase)
    {
        using (var mermory = new MemoryStream())
        {
            //将我们的协议类进行序列化，转换成数组
            Serializer.Serialize(mermory, msgBase);
            byte[] bytes = mermory.ToArray();
            return bytes;
        }
    }
    /// <summary>
    /// 协议体反序列化
    /// </summary>
    /// <param name="protocol"></param>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static MsgBase Decode(MyProtocolEnum protocol,byte[] bytes ,int offset,int count)
    {
        if (count<=0)
        {
            Debug.LogError("协议解密出错，数据长度为0");
            return null;
        }
        //解密阶段
        try
        {
            byte[] newBytes = new byte[count];
            Array.Copy(bytes,offset,newBytes,0,count);//拷贝到newBytes
            //反序列化
            using (var memory=new MemoryStream(newBytes, 0, newBytes.Length))
            {
                Type t = System.Type.GetType(protocol.ToString());//转化类
                MsgBase _tmp= (MsgBase)Serializer.NonGeneric.Deserialize(t, memory);
                return _tmp;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("协议解密出错: "+ex.ToString());
            return null;
        }
    }
    

}
