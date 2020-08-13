using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowplacement">WindowPlacement</a>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowPlacementData
    {
        public int cbSize;
        public WindowPlacementFlags flags;
        public WindowShowType showCmd;
        public Int32Point ptMinPosition;
        public Int32Point ptMaxPosition;
        public Int32Bound rcNormalPosition;
        public Int32Bound rcDevice;
    }
}
