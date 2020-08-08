using MenthaAssembly.Win32.Primitives;
using System;
using System.Collections.Generic;
using static MenthaAssembly.Win32.Desktop;

namespace MenthaAssembly.Win32
{
    public class NotifyTrayInfo
    {
        public IntPtr Handle { get; }

        public Int32Bound Bound
        {
            get
            {
                if (GetWindowRect(Handle, out Int32Bound Bound))
                    return Bound;

                return Int32Bound.Empty;
            }
        }

        public IEnumerable<NotifyIconInfo> NotifyIcons
            => GetNotifyIcons();

        public NotifyTrayInfo(IntPtr pNotifyTray)
        {
            this.Handle = pNotifyTray;
        }

        private IEnumerable<NotifyIconInfo> GetNotifyIcons()
        {
            // NotifyIconOverflowWindow
            IntPtr hWndTray = FindWindow("NotifyIconOverflowWindow", null);
            if (hWndTray != IntPtr.Zero)
            {
                hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);

                if (hWndTray != IntPtr.Zero)
                {
                    ToolbarWindow32<NotifyTrayData> ToolBar = new ToolbarWindow32<NotifyTrayData>(hWndTray);
                    foreach (ToolbarButtonWindow32<NotifyTrayData> item in ToolBar.Buttons)
                        yield return new NotifyIconInfo(item);
                }
            }

            // SysPager
            hWndTray = FindWindowEx(Handle, IntPtr.Zero, "SysPager", null);
            if (hWndTray != IntPtr.Zero)
            {
                hWndTray = FindWindowEx(hWndTray, IntPtr.Zero, "ToolbarWindow32", null);
                if (hWndTray != IntPtr.Zero)
                {
                    ToolbarWindow32<NotifyTrayData> ToolBar = new ToolbarWindow32<NotifyTrayData>(hWndTray);
                    foreach (ToolbarButtonWindow32<NotifyTrayData> item in ToolBar.Buttons)
                        yield return new NotifyIconInfo(item);
                }
            }
        }

    }
}
