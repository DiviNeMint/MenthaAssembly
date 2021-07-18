using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    public unsafe static class System
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

        [DllImport("user32.dll")]
        internal static extern bool SetSystemCursor(IntPtr hCursor, CursorID type);

        //[DllImport("kernel32.dll")]
        //internal static extern void GetLocalTime(out SystemTime lpSystemTime);

        [DllImport("kernel32.dll")]
        internal static extern bool SetLocalTime(ref SystemTime lpSystemTime);

        #endregion

        #region Windows API (Font)
        [DllImport("Gdi32.dll")]
        internal static extern IntPtr CreateFontIndirect([In] FontData lplf);

        [DllImport("Gdi32.dll")]
        internal static extern bool GetTextMetrics(IntPtr hdc, out TextMetric Metric);

        [DllImport("Gdi32.dll")]
        static extern int EnumFontFamiliesEx(IntPtr hdc, ref FontData lpLogfont, EnumFontExDelegate Callback, IntPtr lParam, uint dwFlags);
        internal delegate int EnumFontExDelegate(ref FontData Font, ref TextMetric lpntme, int FontType, int lParam);

        #endregion

        public static IEnumerable<string> FontFamilyNames
        {
            get
            {
                List<string> Result = new List<string>();

                int Callback(ref FontData Font, ref TextMetric lpntme, int FontType, int lParam)
                {
                    if (!'@'.Equals(Font.FaceName.FirstOrDefault()))
                        Result.Add(Font.FaceName);

                    return 1;
                }

                FontData Data = new FontData();
                IntPtr hdc = Graphic.GetDC(IntPtr.Zero);

                try
                {
                    EnumFontFamiliesEx(hdc, ref Data, Callback, IntPtr.Zero, 0);
                }
                finally
                {
                    Graphic.DeleteDC(hdc);
                }

                return Result;
            }
        }

        public static bool EnableMinMaxAnimation
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

        public static DateTime LocalTime
        {
            //GetLocalTime(out SystemTime SystemTime);
            //return new DateTime(SystemTime.Year, 
            //                    SystemTime.Month, 
            //                    SystemTime.Day, 
            //                    SystemTime.Hour, 
            //                    SystemTime.Minute, 
            //                    SystemTime.Second, 
            //                    SystemTime.Miliseconds, 
            //                    DateTimeKind.Local);
            get => DateTime.Now;
            set
            {
                SystemTime SysTime = new SystemTime
                {
                    Year = (ushort)value.Year,
                    Month = (ushort)value.Month,
                    Day = (ushort)value.Day,
                    Hour = (ushort)value.Hour,
                    Minute = (ushort)value.Minute,
                    Second = (ushort)value.Second,
                    Miliseconds = (ushort)value.Millisecond,
                };
                SetLocalTime(ref SysTime);
            }
        }

    }
}
