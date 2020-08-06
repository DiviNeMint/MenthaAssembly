using System;
using System.Collections.Generic;
using System.Text;
using static MenthaAssembly.Devices.Screen;

namespace MenthaAssembly.Devices
{
    public class NotifyTrayInfo
    {
        private readonly IntPtr pNotifyTray;

        public Int32Bound Bound
        {
            get
            {
                if (GetWindowRect(pNotifyTray, out Int32Bound Bound))
                    return Bound;

                return Int32Bound.Empty;
            }
        }

        public NotifyTrayInfo(IntPtr pNotifyTray)
        {
            this.pNotifyTray = pNotifyTray;
        }
    }
}
