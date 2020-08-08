using System;

namespace MenthaAssembly.Win32.Primitives
{
    [Flags]
    internal enum TBButtonState : byte
    {
        Checked = 0x01,
        Pressed = 0x02,
        Enabled = 0x04,
        Hidden = 0x08,
        Indeterminate = 0x10,
        Wrap = 0x20,
        Ellipses = 0x40,
        Marked = 0x80,
    }
}
