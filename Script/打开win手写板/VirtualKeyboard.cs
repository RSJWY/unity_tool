using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Script.AOT.LoadTool
{
    public static class VirtualKeyboard
    {
        [DllImport("user32",SetLastError = true)]
        static extern IntPtr FindWindow(String sClassName, String sAppName);

        [DllImport("user32",SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private static Process _onScreenKeyboardProcess = null;

        /// <summary>
        /// Show the touch keyboard (tabtip.exe).
        /// </summary>
        public static void ShowTouchKeyboard(string file)
        {
            ExternalCall(file, null, true);
            //ExternalCall("TABTIP", null, false);
        }

        /// <summary>
        /// Hide the touch keyboard (tabtip.exe).
        /// </summary>
        public static void HideTouchKeyboard()
        {
            uint WM_SYSCOMMAND = 274;
            int SC_CLOSE = 61536;
            IntPtr ptr = FindWindow("IPTip_Main_Window", null);
            PostMessage(ptr, WM_SYSCOMMAND, SC_CLOSE, 0);
        }

        /// <summary>
        /// Show the on screen keyboard (osk.exe).
        /// </summary>
        public static void ShowOnScreenKeyboard()
        {
            //ExternalCall("C:\\Windows\\system32\\osk.exe", null, false);

            if (_onScreenKeyboardProcess == null || _onScreenKeyboardProcess.HasExited)
                _onScreenKeyboardProcess = ExternalCall("OSK", null, false);
        }

        /// <summary>
        /// Hide the on screen keyboard (osk.exe).
        /// </summary>
        public static void HideOnScreenKeyboard()
        {
            if (_onScreenKeyboardProcess != null && !_onScreenKeyboardProcess.HasExited)
                _onScreenKeyboardProcess.Kill();
        }

        /// <summary>
        /// Set size and location of the OSK.exe keyboard, via registry changes.  Messy, but only known method.
        /// </summary>
        /// <param name='rect'>
        /// Rect.
        /// </param>
        public static void RepositionOnScreenKeyboard(Rect rect)
        {
            ExternalCall("REG", @"ADD HKCU\Software\Microsoft\Osk /v WindowLeft /t REG_DWORD /d " + (int)rect.x + " /f", true);
            ExternalCall("REG", @"ADD HKCU\Software\Microsoft\Osk /v WindowTop /t REG_DWORD /d " + (int)rect.y + " /f", true);
            ExternalCall("REG", @"ADD HKCU\Software\Microsoft\Osk /v WindowWidth /t REG_DWORD /d " + (int)rect.width + " /f", true);
            ExternalCall("REG", @"ADD HKCU\Software\Microsoft\Osk /v WindowHeight /t REG_DWORD /d " + (int)rect.height + " /f", true);
        }

        private static Process ExternalCall(string filename, string arguments, bool hideWindow)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = filename;
                startInfo.Arguments = arguments;

                // if just command, we don't want to see the console displayed
                if (hideWindow)
                {
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                }

                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                return process;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}