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

        public static void SetView()
        {
            //需要引入R3
            var Switchfullscreen = Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Tab));
            var obserSwitchfullscreen = Switchfullscreen.Chunk(Switchfullscreen.Debounce(TimeSpan.FromMilliseconds(250)));   //不多说，原理我没研究，但效果跟我上面的实现一样
            obserSwitchfullscreen.Where(xs => xs.Length >= 3)
                .Subscribe((units =>
                {
                    Resolution[] resolutions = Screen.resolutions;//获取设置当前屏幕分辩率
                    if (Screen.fullScreen)
                    {
                        Screen.SetResolution(resolutions[0].width, resolutions[0].height, false);//设置当前分辨率
                        Screen.fullScreen = false;
                        Cursor.visible = true;
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);//设置当前分辨率
                        Screen.fullScreen = true;  //设置成全屏
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.None;
                    }
                }));
            //全屏
            Resolution[] resolutions = Screen.resolutions;//获取设置当前屏幕分辩率
            Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);//设置当前分辨率
            Screen.fullScreen = true;  //设置成全屏
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;

        }

    }
}

