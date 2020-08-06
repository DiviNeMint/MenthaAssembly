using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class Screen
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

        internal struct WindowInfo
        {
            public int cbSize;
            public Int32Bound rcWindow;
            public Int32Bound rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        #endregion

        #region Windows API (Monitor)
        internal delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Int32Bound pRect, int dwData);

        [DllImport("user32")]
        private static extern IntPtr MonitorFromPoint(Int32Point Position, MonitorOptions dwFlags);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32")]
        internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        private enum MonitorOptions : uint
        {
            Monitor_DefaultToNull = 0,
            Monitor_DefaultToPrimary,
            Monitor_DefaultToNearest
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct MonitorInfo
        {
            public int cbSize;
            public Int32Bound rcMonitor;
            public Int32Bound rcWork;
            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceId;
        }

        #endregion

        public static IEnumerable<ScreenInfo> Screens
        {
            get
            {
                List<ScreenInfo> Result = new List<ScreenInfo>();
                bool Callback(IntPtr pScreen, IntPtr hdc, ref Int32Bound prect, int d)
                {
                    MonitorInfo Info = new MonitorInfo();
                    Info.cbSize = Marshal.SizeOf(Info);

                    if (GetMonitorInfo(pScreen, ref Info))
                        Result.Add(new ScreenInfo(pScreen, Info));

                    return true;
                }

                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, Callback, 0);
                return Result;
            }
        }

        public static ScreenInfo Current
        {
            get
            {
                Int32Point Position = GlobalMouse.Position;
                IntPtr pScreen = MonitorFromPoint(Position, MonitorOptions.Monitor_DefaultToNearest);

                MonitorInfo Info = new MonitorInfo();
                Info.cbSize = Marshal.SizeOf(Info);

                if (GetMonitorInfo(pScreen, ref Info))
                    return new ScreenInfo(pScreen, Info);

                return null;
            }
        }

        #region Windows API (AppBar)
        [DllImport("shell32.dll")]
        internal static extern uint SHAppBarMessage(AppBarMessages dwMessage, ref AppBarData data);

        internal enum AppBarMessages : uint
        {
            // Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.
            New = 0x00,

            // Unregisters an appbar, removing the bar from the system's internal list.
            Remove = 0x01,

            // Requests a size and screen position for an appbar.
            QueryPos = 0x02,

            // Sets the size and screen position of an appbar.
            SetPos = 0x03,

            // Retrieves the autohide and always-on-top states of the Windows taskbar.
            GetState = 0x04,

            // Retrieves the bounding rectangle of the Windows taskbar.
            // Note that this applies only to the system taskbar. 
            // Other objects, particularly toolbars supplied with third-party software, also can be present. 
            // As a result, some of the screen area not covered by the Windows taskbar might not be visible to the user. 
            // To retrieve the area of the screen not covered by both the taskbar and other app bars—the working area available to your application—, use the GetMonitorInfo function.
            GetTaskBarPos = 0x05,

            // Notifies the system to activate or deactivate an appbar.The lParam member of the APPBARDATA pointed to by pData is set to TRUE to activate or FALSE to deactivate.
            Activate = 0x06,

            // Retrieves the handle to the autohide appbar associated with a particular edge of the screen.
            GetAutoHideBar = 0x07,

            // Registers or unregisters an autohide appbar for an edge of the screen.
            SetAutoHideBar = 0x08,

            // Notifies the system when an appbar's position has changed.
            WindowPosChanged = 0x09,

            // Windows XP and later: Sets the state of the appbar's autohide and always-on-top attributes.
            SetState = 0x0A,

            // Windows XP and later: Retrieves the handle to the autohide appbar associated with a particular edge of a particular monitor.
            GetAutoHideBarEx = 0x0B,

            // Windows XP and later: Registers or unregisters an autohide appbar for an edge of a particular monitor.
            SetAutoHideBarEx = 0x0C
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AppBarData
        {
            public int cbSize;
            public IntPtr Hwnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public Int32Bound Bound;
            public int lParam;
        }

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
