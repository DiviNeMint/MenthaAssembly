using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public class ScreenInfo
    {
        #region Windows API
        [DllImport("Shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, DpiType dpiType, out uint DpiX, out uint DpiY);

        private enum DpiType
        {
            Default = 0,
            Effective = 0,
            Angular,
            Raw,
        }

        #endregion

        private readonly IntPtr pScreen;

        public string DeviceID { get; }

        public Int32Bound Bound { get; }

        public Int32Bound WorkArea { get; }

        public uint _DpiX;
        public uint DpiX
        {
            get
            {
                if (_DpiX == 0 &&
                    !TryGetDpi(DpiType.Default, out _DpiX, out _DpiY))
                    _DpiX = 0;
                
                return _DpiX;
            }
        }

        public double DpiFactorX
            => DpiX / 96d;

        public uint _DpiY;
        public uint DpiY
        {
            get
            {
                if (_DpiY == 0 &&
                    !TryGetDpi(DpiType.Default, out _DpiX, out _DpiY))
                    _DpiY = 0;

                return _DpiY;
            }
        }

        public double DpiFactorY
            => DpiY / 96d;

        private bool TryGetDpi(DpiType Type, out uint DpiX, out uint DpiY)
            => GetDpiForMonitor(pScreen, Type, out DpiX, out DpiY) == 0;

        internal ScreenInfo(IntPtr pScreen, string DeviceID, Int32Bound Bound, Int32Bound WorkArea)
        {
            this.pScreen = pScreen;
            this.DeviceID = DeviceID;
            this.Bound = Bound;
            this.WorkArea = WorkArea;
        }

    }
}
