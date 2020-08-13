using MenthaAssembly.Devices;
using MenthaAssembly.Media.Imaging;
using System;
using System.Runtime.InteropServices;
using static MenthaAssembly.Win32.Graphic;

namespace MenthaAssembly.Win32
{
    public static class Desktop
    {
        #region Windows API (Window)
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindowEx(IntPtr pParent, IntPtr pChild, string ClassName, string WindowName);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        internal static extern bool GetWindowInfo(IntPtr Hwnd, out WindowInfo Info);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr Hwnd, out Int32Bound Bound);

        [DllImport("user32.dll")]
        internal static extern long GetWindowLong(IntPtr Hwnd, WindowLongType Type);
        [DllImport("user32.dll")]
        internal static extern long SetWindowLong(IntPtr Hwnd, WindowLongType Type, long dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPlacement(IntPtr Hwnd, ref WindowPlacementData lpwndpl);
        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr Hwnd, out WindowPlacementData lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr Hwnd, IntPtr HwndInsertAfter, int X, int Y, int Width, int Height, WindowPosFlags uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr Hwnd, WindowPosZOrders ZOrder, int X, int Y, int Width, int Height, WindowPosFlags uFlags);

        [DllImport("user32.dll")]
        internal static extern int SetLayeredWindowAttributes(IntPtr Hwnd, int ColorKey, byte alpha, WindowLayeredAttributeFlags flags);
        [DllImport("user32.dll")]
        internal static extern bool GetLayeredWindowAttributes(IntPtr Hwnd, out int ColorKey, out byte alpha, out WindowLayeredAttributeFlags Flags);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW")]
        internal static extern int RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr Hwnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowThreadProcessId(IntPtr Hwnd, out int ProcessId);

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr Hwnd, WindowShowType flags);

        [DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr Hwnd, IntPtr hdcBlt, WindowPrintFlags Flags);

        #endregion

        #region Windows API (AppBar)
        [DllImport("shell32.dll")]
        internal static extern uint SHAppBarMessage(AppBarMessages dwMessage, ref AppBarData data);

        #endregion

        #region Windows API (NotifyIcon)
        [DllImport("shell32.dll")]
        internal static extern bool Shell_NotifyIcon(NotifyCommand Command, ref NotifyIconData Data);

        [DllImport("shell32.dll", SetLastError = true)]
        internal static extern int Shell_NotifyIconGetRect(ref NotifyIconIdentifier identifier, out Int32Bound Bound);

        #endregion

        public static AppBarInfo PrimaryAppBar
        {
            get
            {
                IntPtr Hwnd = FindWindow("Shell_TrayWnd", null);
                if (Hwnd == IntPtr.Zero)
                    return null;

                AppBarData Data = new AppBarData
                {
                    cbSize = Marshal.SizeOf<AppBarData>(),
                    Hwnd = Hwnd
                };

                uint uResult = SHAppBarMessage(AppBarMessages.GetTaskBarPos, ref Data);

                if (uResult != 1)
                    return null;

                return new AppBarInfo(Data);
            }
        }

        /// <summary>
        /// Screenshot Current Screen.
        /// </summary>
        public static ImageContext<BGR> Screenshot()
        {
            Int32Bound ScreenArea = Screen.Current.Bound;
            return Screenshot(ScreenArea.Left, 
                              ScreenArea.Top, 
                              ScreenArea.Width,
                              ScreenArea.Height);
        }
        public unsafe static ImageContext<BGR> Screenshot(int X, int Y, int Width, int Height)
        {
            IntPtr Hwnd = GetDesktopWindow(),
                   hdcSrc = GetWindowDC(Hwnd),
                   hdcDest = CreateCompatibleDC(hdcSrc),
                   hBitmap = CreateCompatibleBitmap(hdcSrc, Width, Height),
                   hObject = SelectObject(hdcDest, hBitmap);
            try
            {
                if (BitBlt(hdcDest, 0, 0, Width, Height, hdcSrc, X, Y, TernaryRasterOperations.SourceCopy))
                {
                    SelectObject(hdcDest, hObject);

                    BitmapInfoHeader BmpHeader = new BitmapInfoHeader
                    {
                        biSize = sizeof(BitmapInfoHeader),
                        biWidth = Width,
                        biHeight = -Height,
                        biPlanes = 1,
                        biBitCount = 24,
                        biCompression = BitmapCompressionMode.RGB,
                        biSizeImage = 0,
                        biXPelsPerMeter = 0,
                        biYPelsPerMeter = 0,
                        biClrUsed = 0,
                        biClrImportant = 0,
                    };
                    if (GetDIBits(hdcSrc, hBitmap, 0, 0, null, &BmpHeader, DIBColorMode.RGB_Colors) != 0)
                    {
                        byte[] Datas = new byte[BmpHeader.biSizeImage];
                        fixed (byte* pDatas = Datas)
                        {
                            if (GetDIBits(hdcSrc, hBitmap, 0, BmpHeader.biHeight, pDatas, &BmpHeader, DIBColorMode.RGB_Colors) != 0)
                                return new ImageContext<BGR>(Width, Height, Datas);
                        }
                    }
                }

                return null;
            }
            finally
            {
                // Release
                ReleaseDC(Hwnd, hdcSrc);
                ReleaseDC(Hwnd, hdcDest);
                DeleteDC(hdcDest);
                DeleteDC(hdcSrc);
                DeleteObject(hBitmap);
                DeleteObject(hObject);
            }
        }

        public static ImageContext<BGR> Snapshot(IntPtr Hwnd)
        {
            if (Snapshot(Hwnd, out WindowInfo Info, out int Width, out int Height, out bool IsAeroStyle) is byte[] Datas)
            {
                ImageContext<BGR> Image = new ImageContext<BGR>(Width, Height, Datas);
                int PaddingX = Info.cxWindowBorders - 1,
                    PaddingY = Info.cyWindowBorders - 1;

                return Info.rcWindow.Top < 0 ? Image.Crop(PaddingX, PaddingY, Image.Width - PaddingX * 2, Image.Height - PaddingY * 2) :
                                               IsAeroStyle ? Image.Crop(PaddingX, 0, Image.Width - PaddingX * 2, Image.Height - PaddingY) :
                                                             Image;
            }

            return null;
        }
        private unsafe static byte[] Snapshot(IntPtr Hwnd, out WindowInfo WInfo, out int IWidth, out int IHeight, out bool IsAeroStyle)
        {
            if (GetWindowPlacement(Hwnd, out WindowPlacementData PlacementData) &&
                GetWindowInfo(Hwnd, out WInfo))
            {
                IsAeroStyle = Environment.OSVersion.Version > new Version(6, 1);
                IWidth = WInfo.rcWindow.Width;
                IHeight = WInfo.rcWindow.Height;

                bool IsMoreThanWin7 = IsAeroStyle;
                int Width = IWidth,
                    Height = IHeight;

                byte[] DoSnapshot()
                {
                    IntPtr hdcSrc = GetWindowDC(Hwnd),
                           hdcDest = CreateCompatibleDC(hdcSrc),
                           hBitmap = CreateCompatibleBitmap(hdcSrc, Width, Height),
                           hObject = SelectObject(hdcDest, hBitmap);
                    try
                    {
                        if (PrintWindow(Hwnd, hdcDest, IsMoreThanWin7 ? WindowPrintFlags.RenderFullContent :
                                                                        WindowPrintFlags.EntireWindow))
                        {
                            SelectObject(hdcDest, hObject);

                            BitmapInfoHeader BmpHeader = new BitmapInfoHeader
                            {
                                biSize = sizeof(BitmapInfoHeader),
                                biWidth = Width,
                                biHeight = -Height,
                                biPlanes = 1,
                                biBitCount = 24,
                                biCompression = BitmapCompressionMode.RGB,
                                biSizeImage = 0,
                                biXPelsPerMeter = 0,
                                biYPelsPerMeter = 0,
                                biClrUsed = 0,
                                biClrImportant = 0,
                            };
                            if (GetDIBits(hdcSrc, hBitmap, 0, 0, null, &BmpHeader, DIBColorMode.RGB_Colors) != 0)
                            {
                                byte[] Datas = new byte[BmpHeader.biSizeImage];
                                fixed (byte* pDatas = Datas)
                                {
                                    if (GetDIBits(hdcSrc, hBitmap, 0, BmpHeader.biHeight, pDatas, &BmpHeader, DIBColorMode.RGB_Colors) != 0)
                                        return Datas;
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Release
                        ReleaseDC(Hwnd, hdcSrc);
                        ReleaseDC(Hwnd, hdcDest);
                        DeleteDC(hdcDest);
                        DeleteDC(hdcSrc);
                        DeleteObject(hBitmap);
                        DeleteObject(hObject);
                    }

                    return null;
                }

                if (PlacementData.showCmd == WindowShowType.ShowMinimized)
                {
                    bool EnableMinMaxAnimation = default;
                    WindowExStyles ExStyles = default;
                    WindowLayeredAttributeFlags LayeredAttributeFlag = default;
                    int ColorKey = default;
                    byte Alpha = default;
                    try
                    {
                        // Backup Datas
                        EnableMinMaxAnimation = System.EnableMinMaxAnimation;
                        ExStyles = (WindowExStyles)GetWindowLong(Hwnd, WindowLongType.ExStyle);
                        GetLayeredWindowAttributes(Hwnd, out ColorKey, out Alpha, out LayeredAttributeFlag);

                        // Disable Show Animation
                        System.EnableMinMaxAnimation = false;

                        // Set Window Transparent
                        SetWindowLong(Hwnd, WindowLongType.ExStyle, (long)(ExStyles | WindowExStyles.Layered));
                        SetLayeredWindowAttributes(Hwnd, 0, 1, WindowLayeredAttributeFlags.Alpha);

                        // Show Window
                        ShowWindow(Hwnd, WindowShowType.Restore);

                        return DoSnapshot();
                    }
                    finally
                    {
                        ShowWindow(Hwnd, WindowShowType.ShowMinimized);
                        SetLayeredWindowAttributes(Hwnd, ColorKey, Alpha, LayeredAttributeFlag);
                        SetWindowLong(Hwnd, WindowLongType.ExStyle, (long)ExStyles);
                        System.EnableMinMaxAnimation = EnableMinMaxAnimation;
                    }
                }

                return DoSnapshot();
            }

            WInfo = default;
            IsAeroStyle = false;
            IWidth = default;
            IHeight = default;
            return null;
        }

    }
}
