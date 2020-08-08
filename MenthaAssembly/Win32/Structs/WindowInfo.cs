namespace MenthaAssembly.Win32
{
    internal struct WindowInfo
    {
        public int cbSize;
        public Int32Bound rcWindow;
        public Int32Bound rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;
    }

}
