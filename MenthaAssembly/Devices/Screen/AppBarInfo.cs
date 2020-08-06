using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static MenthaAssembly.Devices.Screen;

namespace MenthaAssembly.Devices
{
    public class AppBarInfo
    {
        private readonly IntPtr pAppBar;

        public Sides Side { get; }

        public Int32Bound Bound { get; }

        public bool AutoHide
        {
            get
            {
                AppBarData Data = new AppBarData
                {
                    cbSize = Marshal.SizeOf<AppBarData>(),
                    Hwnd = pAppBar
                };

                return SHAppBarMessage(AppBarMessages.GetState, ref Data) == 1;
            }
            set
            {
                AppBarData Data = new AppBarData
                {
                    cbSize = Marshal.SizeOf<AppBarData>(),
                    Hwnd = pAppBar,
                    lParam = value ? 1 : 0
                };

                SHAppBarMessage(AppBarMessages.SetState, ref Data);
            }
        }

        private NotifyTrayInfo _NotifyTray;
        public NotifyTrayInfo NotifyTray
        {
            get
            {
                if (_NotifyTray is null)
                {
                    IntPtr pNotifyTray = FindWindowEx(pAppBar, IntPtr.Zero, "TrayNotifyWnd", "");
                    if (pNotifyTray == IntPtr.Zero)
                        return null;

                    _NotifyTray = new NotifyTrayInfo(pNotifyTray);
                }
                return _NotifyTray;
            }
        }

        internal AppBarInfo(AppBarData Data)
        {
            this.pAppBar = Data.Hwnd;
            this.Side = (Sides)Data.uEdge;
            this.Bound = Data.Bound;
        }

    }
}
