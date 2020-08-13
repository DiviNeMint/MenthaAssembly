namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-minmaxinfo">MinMaxInfo</see>
    /// </summary>
    internal struct WindowMinMaxInfo
    {
        public Int32Point ptReserved;
        public Int32Size ptMaxSize;
        public Int32Point ptMaxPosition;
        public Int32Size ptMinTrackSize;
        public Int32Size ptMaxTrackSize;
    };
}
