﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 数据数组
/// </summary>
public class ByteArray
{
    /// <summary>
    /// 默认大小
    /// </summary>
    public const int default_Size = 1024;
    /// <summary>
    /// 初始大小
    /// </summary>
    private int m_InitSize = 0;
    /// <summary>
    /// 缓冲区，存储数据的位置
    /// </summary>
    public byte[] Bytes;
    /// <summary>
    /// 开始读索引
    /// </summary>
    public int ReadIndex = 0;//开始读索引
    /// <summary>
    /// 已经写入的索引
    /// </summary>
    public int WriteIndex = 0;//已经写入的索引
    /// <summary>
    /// 容量
    /// </summary>
    private int Capacity = 0;

    /// <summary>
    /// 剩余空间
    /// </summary>
    public int Remain { get { return Capacity - WriteIndex; } }

    /// <summary>
    /// 允许读取的数据长度
    /// </summary>
    public int length { get { return WriteIndex - ReadIndex; } }

    public ByteArray()
    {
        //数组初始化
        Bytes = new byte[default_Size];
        Capacity = default_Size;
        m_InitSize = default_Size;
        ReadIndex = 0;
        WriteIndex = 0;
    }
    /// <summary>
    /// 发送信息构造函数
    /// </summary>
    public ByteArray(byte[] defalutBytes)
    {
        Bytes = defalutBytes;
        Capacity = defalutBytes.Length;
        m_InitSize = defalutBytes.Length;
        ReadIndex = 0;
        WriteIndex = defalutBytes.Length;
    }
    /// <summary>
    /// 移动数组
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (length < 8)
        {
            //可读空间不足
            MoveBytes();
        }
    }
    /// <summary>
    /// 移动数据
    /// </summary>
    public void MoveBytes()
    {
        if (ReadIndex < 0)
        {
            return;
        }
        //拷贝数据
        Array.Copy(Bytes, ReadIndex, Bytes, 0, length);
        //写入长度等于总长度
        WriteIndex = length;
        ReadIndex = 0;

    }
    /// <summary>
    /// 重设尺寸
    /// </summary>
    /// <param name="size">新的数据长度</param>
    public void ReSize(int size)
    {
        if (ReadIndex < 0 || size < length || size < m_InitSize)
        {
            return;//开始读取位置小于0(超范围），可读数据长度大于要设置的新数据长度，初始空间大于要设置的新长度
        }
        int n = 1024;
        while (n < size)
        {
            //一直翻倍，直到可以容纳下要重设的数据长度
            n *= 2;
        }
        //重新指定容量大小
        Capacity = n;
        byte[] newBytes = new byte[Capacity];//创建新存储空间，使用新的长度
                                             //拷贝
        Array.Copy(Bytes, ReadIndex, newBytes, 0, length);
        Bytes = newBytes;
        //重新读
        WriteIndex = length;
        ReadIndex = 0;
    }
}

/*一次接收数据，ReadIndex在0位，WriteIndex移动到整个数据长度的位，确认是不是一个完整消息
 * 是，进行读取，不是WriteIndex不变，跳过，等待下一次数据。
 * 
 * 解析是否完整，每处里一条消息，ReadIndex进一步
 * WriteIndex-ReadIndex得结果就是这次读的长度length
 * 
 * 
 */