namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-minmaxinfo">MinMaxInfo</see>
    /// </summary>
    internal struct WindowMinMaxInfo
    {
        public Point<int> ptReserved;
        public Size<int> ptMaxSize;
        public Point<int> ptMaxPosition;
        public Size<int> ptMinTrackSize;
        public Size<int> ptMaxTrackSize;
    };
}
