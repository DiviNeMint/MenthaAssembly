using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public unsafe static class Screen
    {
        #region Windows API (Monitor)
        internal delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, Bound<int>* pRect, int dwData);

        [DllImport("user32")]
        private static extern IntPtr MonitorFromPoint(Point<int>* Position, MonitorOptions dwFlags);

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
            public Bound<int> rcMonitor;
            public Bound<int> rcWork;
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
                bool Callback(IntPtr pScreen, IntPtr hdc, Bound<int>* prect, int d)
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
                Point<int> Position = GlobalMouse.Position;
                IntPtr pScreen = MonitorFromPoint(&Position, MonitorOptions.Monitor_DefaultToNearest);

                MonitorInfo Info = new MonitorInfo();
                Info.cbSize = Marshal.SizeOf(Info);

                if (GetMonitorInfo(pScreen, ref Info))
                    return new ScreenInfo(pScreen, Info);

                return null;
            }
        }

    }
}
