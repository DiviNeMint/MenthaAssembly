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
        public Point<int> ptMinPosition;
        public Point<int> ptMaxPosition;
        public Bound<int> rcNormalPosition;
        public Bound<int> rcDevice;
    }
}
