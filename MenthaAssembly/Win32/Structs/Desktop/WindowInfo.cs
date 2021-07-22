namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowinfo">WindowInfo</a>
    /// </summary>
    internal struct WindowInfo
    {
        public int cbSize;
        public Bound<int> rcWindow;
        public Bound<int> rcClient;
        public WindowStyles dwStyle;
        public WindowExStyles dwExStyle;
        public uint dwWindowStatus;
        public int cxWindowBorders;
        public int cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;
    }
}
