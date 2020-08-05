using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class Screen
    {
        #region Windows API
        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Int32Bound pRect, int dwData);

        [DllImport("user32")]
        private static extern IntPtr MonitorFromPoint(Int32Point Position, MonitorOptions dwFlags);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
        //private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //[DllImport("shell32.dll")]
        //private static extern uint SHAppBarMessage(uint dwMessage, ref AppBarData data);

        private enum MonitorOptions : uint
        {
            Monitor_DefaultToNull = 0,
            Monitor_DefaultToPrimary,
            Monitor_DefaultToNearest
        }

        //public enum AppBarSide : uint
        //{
        //    Left = 0,
        //    Top,
        //    Right,
        //    Bottom
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MonitorInfo
        {
            public int cbSize;
            public Int32Bound rcMonitor;
            public Int32Bound rcWork;
            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceId;
        }

        //[StructLayout(LayoutKind.Sequential)]
        //private struct AppBarData
        //{
        //    public int cbSize;
        //    public IntPtr Hwnd;
        //    public uint uCallbackMessage;
        //    public AppBarSide uEdge;
        //    public Int32Bound Bound;
        //    public int lParam;
        //}

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
                        Result.Add(new ScreenInfo(pScreen, Info.DeviceId, Info.rcMonitor, Info.rcWork));

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
                if (Position.X > 0 && Position.Y > 0)
                {
                    IntPtr pScreen = MonitorFromPoint(Position, MonitorOptions.Monitor_DefaultToNearest);

                    MonitorInfo Info = new MonitorInfo();
                    Info.cbSize = Marshal.SizeOf(Info);

                    if (GetMonitorInfo(pScreen, ref Info))
                        return new ScreenInfo(pScreen, Info.DeviceId, Info.rcMonitor, Info.rcWork);
                }

                return null;
            }
        }

        //public static AppBarInfo AppBar
        //{
        //    get
        //    {
        //        AppBarData Data = new AppBarData
        //        {
        //            cbSize = Marshal.SizeOf<AppBarData>()
        //        };

        //        IntPtr Hwnd = FindWindow("Shell_TrayWnd", null);
        //        if (Hwnd != IntPtr.Zero)
        //        {
        //            uint uResult = SHAppBarMessage(ABM_GETTASKBARPOS, ref Data);

        //            if (uResult != 1)
        //                throw new Exception("Failed to communicate with the given AppBar");
        //        }

        //        return null;
        //    }
        //}

    }
}
