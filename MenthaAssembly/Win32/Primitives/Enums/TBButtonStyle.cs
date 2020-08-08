using System;

namespace MenthaAssembly.Win32.Primitives
{
    [Flags]
    internal enum TBButtonStyle
    {
        Button = 0x0000,            // obsolete; use BTNS_BUTTON instead
        SEP = 0x0001,               // obsolete; use BTNS_SEP instead
        Check = 0x0002,             // obsolete; use BTNS_CHECK instead
        Group = 0x0004,             // obsolete; use BTNS_GROUP instead
        CheckGroup = Group | Check, // obsolete; use BTNS_CHECKGROUP instead
        DropDown = 0x0008,          // obsolete; use BTNS_DROPDOWN instead
        AutoSize = 0x0010,          // obsolete; use BTNS_AUTOSIZE instead
        NoPrefix = 0x0020,          // obsolete; use BTNS_NOPREFIX instead

        ToolTips = 0x0100,
        Wrapable = 0x0200,
        AltDrag = 0x0400,
        Flat = 0x0800,
        List = 0x1000,
        CustomErase = 0x2000,
        RegisterDrop = 0x4000,
        Transparent = 0x8000,
    }
}
