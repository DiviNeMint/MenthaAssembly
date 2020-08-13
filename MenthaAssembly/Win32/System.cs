using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    public static class System
    {
        #region Windows API (Send/Post)
        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr Hwnd, Win32Messages msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr Hwnd, Win32Messages msg, IntPtr wParam, int lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hwnd, Win32Messages msg, int wParam, bool lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hwnd, Win32Messages msg, int wParam, string lParam);

        #endregion

        #region Windows API (Memory)
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(ProcessRights dwDesiredAccess, bool bInheritHandle, int ProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int Size, MemAllocType AllocationType, MemProtectType Protect);

        [DllImport("kernel32.dll")]
        internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int Size, MemFreeType FreeType);

        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpAddress, IntPtr lpBuffer, int Size, out int ReadLength);

        [DllImport("kernel32.dll")]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpAddress, IntPtr lpBuffer, int Size, out int WriteLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region Windows API (System parameter)
        /// <summary>
        /// Gets the maximum number of milliseconds that can elapse between a
        /// first click and a second click for the OS to consider the
        /// mouse action a double-click.
        /// </summary>
        /// <returns>The maximum amount of time, in milliseconds, that can
        /// elapse between a first click and a second click for the OS to
        /// consider the mouse action a double-click.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int GetDoubleClickTime();

        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa">SystemParametersInfo</see>
        /// </summary>
        /// <param name="uiParam">
        /// A parameter whose usage and format depends on the system parameter being queried or set. 
        /// <para/>
        /// For more information about system-wide parameters, see <see cref="SystemParameterActionType"/> summary. 
        /// <para/>
        /// If not otherwise indicated, you must specify zero for this parameter.
        /// </param>
        /// <param name="pvParam">
        /// A parameter whose usage and format depends on the system parameter being queried or set. 
        /// <para/>
        /// For more information about system-wide parameters, see <see cref="SystemParameterActionType"/> summary. 
        /// <para/>
        /// If not otherwise indicated, you must specify NULL for this parameter. 
        /// <para/>
        /// For information on the PVOID datatype, see <see href="https://docs.microsoft.com/en-us/windows/win32/winprog/windows-data-types">Windows Data Types</see>.
        /// </param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool SystemParametersInfo(SystemParameterActionType Action, uint uiParam, IntPtr pvParam, SystemParameterInfoFlags fWinIni);

        [DllImport("user32.dll")]
        internal static extern bool SetSystemCursor(IntPtr hCursor, CursorID type);

        #endregion


        public static unsafe bool EnableMinMaxAnimation
        {
            get
            {
                // AnimationInfo
                // https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-animationinfo
                int* Data = stackalloc int[2];
                Data[0] = 8;
                SystemParametersInfo(SystemParameterActionType.GetAnimation, 8, (IntPtr)Data, SystemParameterInfoFlags.None);
                return Data[1] != 0;
            }
            set
            {
                // AnimationInfo
                // https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-animationinfo
                int* Data = stackalloc int[2];
                Data[0] = 8;
                Data[1] = value ? 1 : 0;
                SystemParametersInfo(SystemParameterActionType.SetAnimation, 8, (IntPtr)Data, SystemParameterInfoFlags.SendChange);
            }
        }

    }
}
