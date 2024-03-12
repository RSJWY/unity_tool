using UnityEngine;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
public static partial class MySocketTool
{
    #region 序列化
      /// <summary>
    /// 编码协议名
    /// </summary>
    /// <param name="msgBase">要编码的消息</param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.ProtoType.ToString());//编码协议名
        byte[] AESNameBytes =AESEncrypt(nameBytes, MySocketTool.MsgKey); //加密协议名
        uint nameLen = (uint)AESNameBytes.Length;//记录协议名长度
        byte[] nameLenBytes =MySocketTool.GetCrc32Bytes(nameLen);//获取记录长度的数组
        byte[] bytes = new byte[4 + nameLen];//开新的存储空间用于返回编码好的协议长度及协议名（协议长度通过前面4位数组存储
        //拷贝并存储
        Array.Copy(nameLenBytes, 0, bytes, 0, 4);//将协议名长度拷贝到数组中
        Array.Copy(AESNameBytes, 0, bytes, 4, nameLen);//将编码好的协议名拷贝到存储空间去，前两位存储长度，所以从第三位存储协议名
        return bytes;
    }
    /// <summary>
    /// 解码协议名
    /// </summary>
    /// <param name="bytes">数据数组</param>
    /// <param name="offset">开始读索引</param>
    /// <param name="count">返回存储协议名信息的数组长度（包含协议名长度和协议名体长度）</param>
    /// <returns>返回解析出来的协议</returns>
    public static MyProtocolEnum DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;//初始0
        //判断存储协议名长度的数组是否存在
        if (offset+4 > bytes.Length)
        {
            //存储协议名长度的数组不存在
            Debug.LogError("解码协议长度出错！！存储协议名长度的数组位不存在，无法获取本条协议名" );
            return MyProtocolEnum.None;//协议不存在
        }
        byte[] lenByte = bytes.Skip(offset).Take(4).ToArray();//取长度数组
        int len = (int)MySocketTool.ReverseBytesToCRC32(lenByte);
        if (offset+4 + len > bytes.Length)
        {
            //只包含了协议名长度信息，但与之对应存储协议名称的数组不存在
            Debug.LogError("解码协议名出错！！只包含了协议长度信息，没有包协议名数组!!");
            return MyProtocolEnum.None;//协议不存在
        }
        //协议长度和协议名都存在
        //记录存储协议名信息的数组长度，并返回，规避存储协议名长度的位置，以方便解析协议体
        count = 4 + len;
        //开始解析
        try
        { 
            //string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);//规避长度信息数组
            //取出被加密后的协议名
            byte[] AESBytes = bytes.Skip(offset+4).Take(len).ToArray();
            //解密协议名
            byte[] nameBytes = AESDecrypt(AESBytes, MySocketTool.MsgKey);
            //确认协议类型
            string name = System.Text.Encoding.UTF8.GetString(nameBytes);
            MyProtocolEnum _mpe = (MyProtocolEnum)System.Enum.Parse(typeof(MyProtocolEnum), name);//查找对应的枚举
            return _mpe;
        }
        catch (Exception ex)
        {
            Debug.LogError($"出现错误，传入的协议不存在,无法从本地枚举信息中找到传入的枚举，错误信息：{ex.ToString()} ");
            return MyProtocolEnum.None;
        }
    }
    /// <summary>
    /// 协议体序列化
    /// </summary>
    /// <param name="msgBase">协议体（消息）</param>
    /// <returns>返回序列化后的消息数组</returns>
    public static byte[] Encond(MsgBase msgBase)
    {
        using (var mermory = new MemoryStream())
        {
            //将协议类进行序列化，转换成数组
            Serializer.Serialize(mermory, msgBase);
            byte[] bytes = mermory.ToArray();

            //对协议体加密
            byte[] AESBodyBytes = AESEncrypt(bytes, MySocketTool.MsgKey);

            return AESBodyBytes;
        }
    }
    /// <summary>
    /// 协议体反序列化
    /// </summary>
    /// <param name="protocol">协议名称枚举</param>
    /// <param name="bytes">消息数组</param>
    /// <param name="offset">开始读索引</param>
    /// <param name="count">整条消息的长度</param>
    /// <returns>解析后的协议体（即消息）</returns>
    public static MsgBase Decode(MyProtocolEnum protocol,byte[] bytes ,int offset,int count)
    {
        if (count<=0)
        {
            Debug.LogError("协议解密出错，数据长度为0");
            return null;
        }
        //协议反序列化
        try
        {
            byte[] AESBytes = new byte[count];
            Array.Copy(bytes,offset, AESBytes, 0,count);//拷贝到AESBytes
            byte[] bodyBytes= AESDecrypt(AESBytes, MySocketTool.MsgKey);//解密协议体
            //反序列化
            using (var memory=new MemoryStream(bodyBytes, 0, bodyBytes.Length))
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
    
    #endregion
}