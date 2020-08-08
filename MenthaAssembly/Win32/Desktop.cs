using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    public class Desktop
    {
        #region Windows API (Window)
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindowEx(IntPtr pParent, IntPtr pChild, string ClassName, string WindowName);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowInfo(IntPtr Hwnd, ref WindowInfo Info);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr Hwnd, out Int32Bound Bound);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW")]
        internal static extern int RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr Hwnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowThreadProcessId(IntPtr Hwnd, out int ProcessId);

        #endregion

        #region Windows API (AppBar)
        [DllImport("shell32.dll")]
        internal static extern uint SHAppBarMessage(AppBarMessages dwMessage, ref AppBarData data);

        #endregion

        #region Windows API (NotifyIcon)
        [DllImport("shell32.dll")]
        internal static extern bool Shell_NotifyIcon(NotifyCommand Command, ref NotifyIconData Data);

        [DllImport("shell32.dll", SetLastError = true)]
        internal static extern int Shell_NotifyIconGetRect(ref NotifyIconIdentifier identifier, out Int32Bound Bound);

        #endregion

        public static AppBarInfo PrimaryAppBar
        {
            get
            {
                IntPtr Hwnd = FindWindow("Shell_TrayWnd", null);
                if (Hwnd == IntPtr.Zero)
                    return null;

                AppBarData Data = new AppBarData
                {
                    cbSize = Marshal.SizeOf<AppBarData>(),
                    Hwnd = Hwnd
                };

                uint uResult = SHAppBarMessage(AppBarMessages.GetTaskBarPos, ref Data);

                if (uResult != 1)
                    return null;

                return new AppBarInfo(Data);
            }
        }
    }
}
