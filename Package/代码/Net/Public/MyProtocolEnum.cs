using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 本项目自定义的协议
/// </summary>
public enum MyProtocolEnum
{
    None=0,
    /// <summary>
    /// 心跳包协议
    /// </summary>
    MsgPing=2,

    /// <summary>
    /// 测试
    /// </summary>
    MsgTest = 9999,
}
