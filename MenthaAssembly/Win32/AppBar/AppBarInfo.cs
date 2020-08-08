using System;
using System.Runtime.InteropServices;
using static MenthaAssembly.Win32.Desktop;

namespace MenthaAssembly.Win32
{
    public class AppBarInfo
    {
        public IntPtr Handle { get; }

        public Sides Side { get; }

        public Int32Bound Bound { get; }

        public bool AutoHide
        {
            get
            {
                AppBarData Data = new AppBarData
                {
                    cbSize = Marshal.SizeOf<AppBarData>(),
                    Hwnd = Handle
                };

                return SHAppBarMessage(AppBarMessages.GetState, ref Data) == 1;
            }
            set
            {
                AppBarData Data = new AppBarData
                {
                    cbSize = Marshal.SizeOf<AppBarData>(),
                    Hwnd = Handle,
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
                    IntPtr pNotifyTray = FindWindowEx(Handle, IntPtr.Zero, "TrayNotifyWnd", null);
                    if (pNotifyTray == IntPtr.Zero)
                        return null;

                    _NotifyTray = new NotifyTrayInfo(pNotifyTray);
                }
                return _NotifyTray;
            }
        }

        internal AppBarInfo(AppBarData Data)
        {
            this.Handle = Data.Hwnd;
            this.Side = (Sides)Data.uEdge;
            this.Bound = Data.Bound;


            //IntPtr pTaskTray = FindWindowEx(Handle, IntPtr.Zero, "ReBarWindow32", null);
            //if (pTaskTray != IntPtr.Zero)
            //{
            //    pTaskTray = FindWindowEx(pTaskTray, IntPtr.Zero, "MSTaskSwWClass", null);
            //    if (pTaskTray != IntPtr.Zero)
            //    {
            //        pTaskTray = FindWindowEx(pTaskTray, IntPtr.Zero, "MSTaskListWClass", null);
            //        if (pTaskTray != IntPtr.Zero)
            //        {
            //            ToolbarWindow32 ToolBar = new ToolbarWindow32(pTaskTray);

            //            var a = ToolBar.Buttons.ToArray();

            //        }
            //    }
            //}


        }

    }
}
