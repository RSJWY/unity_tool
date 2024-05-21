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
            displays = Display.displays;
            //设置全屏
            for (int i = 0; i < displays.Length; i++)
            {
                displays[i].Activate(displays[i].renderingWidth,displays[i].renderingHeight,60);
            }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            Resolution[] resolutions = Screen.resolutions;//获取设置当前屏幕分辩率
            //Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);//设置当前分辨率
        }

    }
}

