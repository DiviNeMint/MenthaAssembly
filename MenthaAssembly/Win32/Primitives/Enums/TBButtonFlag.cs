using System;

namespace MenthaAssembly.Win32.Primitives
{
    [Flags]
    internal enum TBButtonFlag : uint
    {
        All = Image | Text | State | Style | lParam | Command | Size,
        Image = 0x00000001,
        Text = 0x00000002,
        State = 0x00000004,
        Style = 0x00000008,
        lParam = 0x00000010,
        Command = 0x00000020,
        Size = 0x00000040,
        ByIndex = 0x80000000, // this specifies that the wparam in Get/SetButtonInfo is an index, not id
    }
}
