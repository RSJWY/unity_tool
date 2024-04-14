using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ping = System.Net.NetworkInformation.Ping;//声明当前的ping是C#的ping，不是unity的ping

namespace MyTool
{
    /// <summary>
    /// IP工具
    /// </summary>
    public static partial class MyTool
    {
        /// <summary>
        /// 获取唯一GUID
        /// </summary>
        public static string GenerateRandomString()
        {
            string guid = Guid.NewGuid().ToString("N");
            return guid.Substring(0, 15);
        }

        /// <summary>
        /// 返回时间戳
        /// </summary>
        public static string UpdateTime()
        {
            //获取当前时间
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            int second = DateTime.Now.Second;
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;

            ////格式化显示当前时间
            return string.Format
                ("{0:D2}_{1:D2}_{2:D2}_" + "{3:D4}_{4:D2}_{5:D2}",
                year, month, day, hour, minute, second);
        }
    }
}

