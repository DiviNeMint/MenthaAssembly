using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class Screen
    {
        #region Windows API
        [DllImport("user32")]
        private static extern IntPtr MonitorFromPoint(Int32Point Position, MonitorOptions dwFlags);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

        private delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Int32Bound pRect, int dwData);

        private enum MonitorOptions : uint
        {
            Monitor_DefaultToNull = 0,
            Monitor_DefaultToPrimary,
            Monitor_DefaultToNearest
        }

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
                        Result.Add(new ScreenInfo(Info.DeviceId, Info.rcMonitor, Info.rcWork));

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
                        return new ScreenInfo(Info.DeviceId, Info.rcMonitor, Info.rcWork);
                }

                return null;
            }
        }

    }
}
