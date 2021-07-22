using MenthaAssembly.Win32.Primitives;
using System;
using static MenthaAssembly.Win32.Desktop;

namespace MenthaAssembly.Win32
{
    public class NotifyIconInfo
    {
        public IntPtr Handle { get; }

        public int ProcessId { get; }

        public uint Uid { get; }

        public Bound<int> Bound { get; }

        public IntPtr HIcon { get; }

        public int CallbackMessageID { get; }

        public bool IsVisible { get; }

        public string ToolTip { get; }

        public string ExePath { get; }

        internal unsafe NotifyIconInfo(ToolbarButtonWindow32<NotifyTrayData> ToolBarButton)
        {
            this.Handle = ToolBarButton.DataContext.Hwnd;
            this.Uid = ToolBarButton.DataContext.Uid;
            this.Bound = ToolBarButton.Bound;
            this.HIcon = ToolBarButton.DataContext.hIcon;
            this.CallbackMessageID = ToolBarButton.DataContext.CallbackMessageId;
            this.IsVisible = ToolBarButton.DataContext.State == NotifyIconState.Visible;
            this.ToolTip = ToolBarButton.Text;
            this.ExePath = ToolBarButton.DataContext.szExePath;

            // ProcessId
            GetWindowThreadProcessId(this.Handle, out int ProcessId);
            this.ProcessId = ProcessId;

            if (this.Bound.IsEmpty)
            {
                NotifyIconIdentifier Indentifier = new NotifyIconIdentifier
                {
                    cbSize = sizeof(NotifyIconIdentifier),
                    Hwnd = this.Handle,
                    Uid = this.Uid
                };

                Shell_NotifyIconGetRect(ref Indentifier, out Bound<int> Bound);
                this.Bound = Bound;
            }
        }

    }
}
