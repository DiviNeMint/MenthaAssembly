﻿using MenthaAssembly.Devices;
using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static MenthaAssembly.Win32.Graphic;

namespace MenthaAssembly.Win32
{
    public static unsafe class Desktop
    {
        private const string Shell32 = "Shell32.dll";
        private const string User32 = "User32.dll";

        #region Windows API (Window)
        [DllImport(User32, CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImport(User32)]
        internal static extern IntPtr FindWindowEx(IntPtr pParent, IntPtr pChild, string ClassName, string WindowName);

        [DllImport(User32)]
        internal static extern IntPtr GetDesktopWindow();

        [DllImport(User32)]
        internal static extern bool GetWindowInfo(IntPtr Hwnd, out WindowInfo Info);

        [DllImport(User32)]
        internal static extern bool GetWindowRect(IntPtr Hwnd, Bound<int>* Bound);

        [DllImport(User32)]
        internal static extern long GetWindowLong(IntPtr Hwnd, WindowLongType Type);
        [DllImport(User32)]
        internal static extern long SetWindowLong(IntPtr Hwnd, WindowLongType Type, long dwNewLong);

        [DllImport(User32, SetLastError = true)]
        internal static extern bool SetWindowPlacement(IntPtr Hwnd, ref WindowPlacementData lpwndpl);
        [DllImport(User32)]
        internal static extern bool GetWindowPlacement(IntPtr Hwnd, out WindowPlacementData lpwndpl);

        [DllImport(User32, SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr Hwnd, IntPtr HwndInsertAfter, int X, int Y, int Width, int Height, WindowPosFlags uFlags);

        [DllImport(User32, SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr Hwnd, WindowPosZOrders ZOrder, int X, int Y, int Width, int Height, WindowPosFlags uFlags);

        [DllImport(User32)]
        internal static extern int SetLayeredWindowAttributes(IntPtr Hwnd, int ColorKey, byte alpha, WindowLayeredAttributeFlags flags);
        [DllImport(User32)]
        internal static extern bool GetLayeredWindowAttributes(IntPtr Hwnd, out int ColorKey, out byte alpha, out WindowLayeredAttributeFlags Flags);

        [DllImport(User32, EntryPoint = "RegisterWindowMessageW")]
        internal static extern int RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        [DllImport(User32)]
        internal static extern bool SetForegroundWindow(IntPtr Hwnd);

        [DllImport(User32, CharSet = CharSet.Auto)]
        internal static extern int GetWindowThreadProcessId(IntPtr Hwnd, out int ProcessId);

        [DllImport(User32)]
        internal static extern bool ShowWindow(IntPtr Hwnd, WindowShowType flags);

        [DllImport(User32)]
        internal static extern bool PrintWindow(IntPtr Hwnd, IntPtr hdcBlt, WindowPrintFlags Flags);

        [DllImport(User32)]
        internal static extern int EnumChildWindows(IntPtr hwnd, EnumChildCallbackProc lpfn, IntPtr lParam);
        internal delegate bool EnumChildCallbackProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("Comctl32.dll")]
        internal static extern bool SetWindowSubclass(IntPtr hwnd, SubClassProc pfnSubclass, int uIdSubclass, IntPtr dwRefData);
        internal delegate int SubClassProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam, int uIdSubclass, IntPtr dwRefData);

        [DllImport("Comctl32.dll")]
        internal static extern int DefSubclassProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Windows API (AppBar)
        [DllImport(Shell32)]
        internal static extern uint SHAppBarMessage(AppBarMessages dwMessage, ref AppBarData data);

        #endregion

        #region Windows API (NotifyIcon)
        [DllImport(Shell32)]
        internal static extern bool Shell_NotifyIcon(NotifyCommand Command, ref NotifyIconData Data);

        [DllImport(Shell32, SetLastError = true)]
        internal static extern int Shell_NotifyIconGetRect(ref NotifyIconIdentifier identifier, out Bound<int> Bound);

        #endregion

        #region Windows API (File System)
        [DllImport(Shell32, CharSet = CharSet.Auto)]
        internal static extern int SHGetFileInfo(string pszPath, SHFileAttributes dwFileAttributes, out SHFileInfo psfi, int cbfileInfo, SHGetFileInfoFlags uFlags);

        #endregion

        #region  Windows API (COM Interface)
        //internal enum OBJID : uint
        //{
        //    Window = 0x00000000,
        //    SysMenu = 0xFFFFFFFF,
        //    TitleBar = 0xFFFFFFFE,
        //    Menu = 0xFFFFFFFD,
        //    Client = 0xFFFFFFFC,
        //    VScroll = 0xFFFFFFFB,
        //    HScroll = 0xFFFFFFFA,
        //    SizeGrip = 0xFFFFFFF9,
        //    Caret = 0xFFFFFFF8,
        //    Cursor = 0xFFFFFFF7,
        //    Alert = 0xFFFFFFF6,
        //    Sound = 0xFFFFFFF5,
        //}
        //[DllImport("oleacc.dll")]
        //internal static extern int AccessibleObjectFromWindow(IntPtr hwnd, OBJID id, ref Guid iid, [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppvObject);

        //#if NETCOREAPP3_1
        //            private enum Role
        //            {
        //                TITLEBAR = 0x1,
        //                MENUBAR = 0x2,
        //                SCROLLBAR = 0x3,
        //                GRIP = 0x4,
        //                SOUND = 0x5,
        //                CURSOR = 0x6,
        //                CARET = 0x7,
        //                ALERT = 0x8,
        //                WINDOW = 0x9,
        //                CLIENT = 0xa,
        //                MENUPOPUP = 0xb,
        //                MENUITEM = 0xc,
        //                TOOLTIP = 0xd,
        //                APPLICATION = 0xe,
        //                DOCUMENT = 0xf,
        //                PANE = 0x10,
        //                CHART = 0x11,
        //                DIALOG = 0x12,
        //                BORDER = 0x13,
        //                GROUPING = 0x14,
        //                SEPARATOR = 0x15,
        //                TOOLBAR = 0x16,
        //                STATUSBAR = 0x17,
        //                TABLE = 0x18,
        //                COLUMNHEADER = 0x19,
        //                ROWHEADER = 0x1a,
        //                COLUMN = 0x1b,
        //                ROW = 0x1c,
        //                CELL = 0x1d,
        //                LINK = 0x1e,
        //                HELPBALLOON = 0x1f,
        //                CHARACTER = 0x20,
        //                LIST = 0x21,
        //                LISTITEM = 0x22,
        //                OUTLINE = 0x23,
        //                OUTLINEITEM = 0x24,
        //                PAGETAB = 0x25,
        //                PROPERTYPAGE = 0x26,
        //                INDICATOR = 0x27,
        //                GRAPHIC = 0x28,
        //                STATICTEXT = 0x29,
        //                TEXT = 0x2a,
        //                PUSHBUTTON = 0x2b,
        //                CHECKBUTTON = 0x2c,
        //                RADIOBUTTON = 0x2d,
        //                COMBOBOX = 0x2e,
        //                DROPLIST = 0x2f,
        //                PROGRESSBAR = 0x30,
        //                DIAL = 0x31,
        //                HOTKEYFIELD = 0x32,
        //                SLIDER = 0x33,
        //                SPINBUTTON = 0x34,
        //                DIAGRAM = 0x35,
        //                ANIMATION = 0x36,
        //                EQUATION = 0x37,
        //                BUTTONDROPDOWN = 0x38,
        //                BUTTONMENU = 0x39,
        //                BUTTONDROPDOWNGRID = 0x3a,
        //                WHITESPACE = 0x3b,
        //                PAGETABLIST = 0x3c,
        //                CLOCK = 0x3d,
        //                SPLITBUTTON = 0x3e,
        //                IPADDRESS = 0x3f,
        //                OUTLINEBUTTON = 0x40,
        //            }

        //            private enum State
        //            {
        //                NORMAL = 0,
        //                UNAVAILABLE = 0x1,
        //                SELECTED = 0x2,
        //                FOCUSED = 0x4,
        //                PRESSED = 0x8,
        //                CHECKED = 0x10,
        //                MIXED = 0x20,
        //                //#define STATE_SYSTEM_INDETERMINATE (STATE_SYSTEM_MIXED)
        //                READONLY = 0x40,
        //                HOTTRACKED = 0x80,
        //                DEFAULT = 0x100,
        //                EXPANDED = 0x200,
        //                COLLAPSED = 0x400,
        //                BUSY = 0x800,
        //                FLOATING = 0x1000,
        //                MARQUEED = 0x2000,
        //                ANIMATED = 0x4000,
        //                INVISIBLE = 0x8000,
        //                OFFSCREEN = 0x10000,
        //                SIZEABLE = 0x20000,
        //                MOVEABLE = 0x40000,
        //                SELFVOICING = 0x80000,
        //                FOCUSABLE = 0x100000,
        //                SELECTABLE = 0x200000,
        //                LINKED = 0x400000,
        //                TRAVERSED = 0x800000,
        //                MULTISELECTABLE = 0x1000000,
        //                EXTSELECTABLE = 0x2000000,
        //                ALERT_LOW = 0x4000000,
        //                ALERT_MEDIUM = 0x8000000,
        //                ALERT_HIGH = 0x10000000,
        //                PROTECTED = 0x20000000,
        //                VALID = 0x7fffffff,
        //            }

        //            [DllImport("oleacc.dll")]
        //            public static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [Out] object[] rgvarChildren, out int pcObtained);

        //            [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-C000-000000000046")]
        //            private interface IDispatch
        //            {
        //                [PreserveSig]
        //                int GetTypeInfoCount(out int Count);

        //                [PreserveSig]
        //                int GetTypeInfo([MarshalAs(UnmanagedType.U4)] int iTInfo,
        //                                [MarshalAs(UnmanagedType.U4)] int lcid,
        //                                out ITypeInfo typeInfo);

        //                [PreserveSig]
        //                int GetIDsOfNames(ref Guid riid,
        //                                  [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgsNames,
        //                                  int cNames,
        //                                  int lcid,
        //                                  [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);

        //                [PreserveSig]
        //                int Invoke(int dispIdMember,
        //                           ref Guid riid,
        //                           uint lcid,
        //                           ushort wFlags,
        //                           ref DISPPARAMS pDispParams,
        //                           out object pVarResult,
        //                           ref EXCEPINFO pExcepInfo,
        //                           out uint pArgErr);
        //            }

        //#endif


        //#if NETCOREAPP3_1
        //                IntPtr pDirectUIHwnd = Desktop.FindWindowEx(Hwnd, IntPtr.Zero, "DUIViewWndClassName", null);
        //                pDirectUIHwnd = Desktop.FindWindowEx(pDirectUIHwnd, IntPtr.Zero, "DirectUIHwnd", null);

        //                Guid IID = Guid.Parse("618736E0-3C3D-11CF-810C-00AA00389B71");
        //                Desktop.AccessibleObjectFromWindow(pDirectUIHwnd, Desktop.OBJID.Client, ref IID, out object iaobject);
        //                if (iaobject is IAccessible ia)
        //                {
        //                    object[] children = new object[1];
        //                    AccessibleChildren(ia, 0, 1, children, out int obtained);

        //                    foreach (IAccessible item in children.OfType<IAccessible>())
        //                    {
        //                        string Name = item.accName[0];
        //                        State acc_State = (State)item.accState[0];
        //                        Role acc_Role = (Role)item.accRole[0];

        //                        AccessibleChildren(item, 3, 1, children, out obtained);



        //                        foreach (IAccessible item2 in children.OfType<IAccessible>())
        //                        {
        //                            Name = item2.accName[0];
        //                            acc_State = (State)item2.accState[0];
        //                            acc_Role = (Role)item2.accRole[0];
        //                            string Value = item.accValue[0];
        //                            string Help = item.accHelp[0];
        //                            string Action = item.accDefaultAction[0];

        //                            IDispatch dispatch = item2 as IDispatch;
        //                            dispatch.GetTypeInfoCount(out int TypesCount);
        //                            dispatch.GetTypeInfo(0, 0, out ITypeInfo Info);

        //                            Info.GetDocumentation(-1, out string t1, out string t2, out int i1, out string t3);

        //                        }
        //                    }
        //                }
        //#endif
        #endregion

        #region  Windows API (Process)

        [DllImport("Ntdll.dll")]
        internal static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, out ProcessBasicInfo ProcessInformation, int ProcessInformationLength, out int ReturnLength);

        [DllImport(User32)]
        internal static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        internal delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        #endregion

        /// <summary>
        /// The handle pointer of desktop.
        /// </summary>
        public static IntPtr Handle
            => GetDesktopWindow();

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

                return uResult != 1 ? null : new AppBarInfo(Data);
            }
        }

        /// <summary>
        /// Screenshots the current screen.
        /// </summary>
        public static ImageContext<BGR> Screenshot()
        {
            Bound<int> ScreenArea = Screen.Current.Bound;
            return Screenshot(ScreenArea.Left,
                              ScreenArea.Top,
                              ScreenArea.Width,
                              ScreenArea.Height);
        }
        /// <summary>
        /// Screenshots the specified rectangle in the screen.
        /// </summary>
        /// <param name="X">The x-coordinate of screen.</param>
        /// <param name="Y">The y-coordinate of screen.</param>
        /// <param name="Width">The width of the specified rectangle.</param>
        /// <param name="Height">The height of the specified rectangle.</param>
        public static ImageContext<BGR> Screenshot(int X, int Y, int Width, int Height)
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

        /// <summary>
        /// Snapshots the specified window.
        /// </summary>
        /// <param name="Hwnd">A handle to the window that will be snapshotted.</param>
        public static ImageContext<BGR> Snapshot(IntPtr Hwnd)
        {
            if (GetWindowPlacement(Hwnd, out WindowPlacementData PlacementData))
            {
                ImageContext<BGR> DoSnapshot()
                {
                    if (!GetWindowInfo(Hwnd, out WindowInfo Info))
                        return null;

                    int Width = Info.rcWindow.Width,
                        Height = Info.rcWindow.Height;

                    IntPtr hdcSrc = GetWindowDC(Hwnd),
                           hdcDest = CreateCompatibleDC(hdcSrc),
                           hBitmap = CreateCompatibleBitmap(hdcSrc, Width, Height),
                           hObject = SelectObject(hdcDest, hBitmap);
                    try
                    {
                        if (PrintWindow(Hwnd, hdcDest, WindowPrintFlags.EntireWindow))
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
                                    {
                                        ImageContext<BGR> Snap = new ImageContext<BGR>(Width, Height, Datas);

                                        // Maximized
                                        if (Info.rcWindow.Top < 0)
                                        {
                                            int ThicknessX = Info.cxWindowBorders,
                                                ThicknessY = Info.cyWindowBorders;
                                            return Snap.Crop<BGR>(ThicknessX, ThicknessY, Snap.Width - ThicknessX * 2, Snap.Height - ThicknessY * 2);
                                        }

                                        return Snap;
                                    }
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

            return null;
        }

        /// <summary>
        /// Gets the process id of the parent of the associated process.
        /// </summary>
        public static int GetParentId(this Process Process)
        {
            NtQueryInformationProcess(Process.Handle, 0, out ProcessBasicInfo pbi, Marshal.SizeOf(typeof(ProcessBasicInfo)), out _);
            return pbi.InheritedFromUniqueProcessId.ToInt32();
        }

        /// <summary>
        /// Gets the window handle of the windows of the associated process.
        /// </summary>
        public static IEnumerable<IntPtr> GetWindowHandles(this Process Process)
        {
            List<IntPtr> Handles = [];
            bool Handler(IntPtr hWnd, IntPtr lParam)
            {
                Handles.Add(hWnd);
                return true;
            }

            foreach (ProcessThread Thread in Process.Threads)
                EnumThreadWindows(Thread.Id, Handler, IntPtr.Zero);

            return Handles;
        }

    }
}