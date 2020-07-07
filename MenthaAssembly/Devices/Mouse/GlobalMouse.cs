using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class GlobalMouse
    {
        #region Windows API
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        private const int WH_MOUSE_LL = 14;

        //https://docs.microsoft.com/en-us/windows/win32/inputdev/mouse-input-notifications
        private const int WM_MouseMove = 0x200,
                          WM_LButtonDown = 0x201,
                          WM_LButtonUp = 0x202,

                          WM_RButtonDown = 0x204,
                          WM_RButtonUp = 0x205,

                          WM_MButtonDown = 0x207,
                          WM_MButtonUp = 0x208,
                          WM_MouseWheel = 0x20A;

        // MSLLHOOKSTRUCT
        // https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-msllhookstruct
        [StructLayout(LayoutKind.Sequential)]
        internal struct MouseInputInfo
        {
            public Int32Point Position { set; get; }

            public int MouseData { set; get; }

            public MouseFlag Flags { set; get; }

            public uint Time { set; get; }

            public IntPtr ExtraInfo { get; }
        }

        private enum MouseData : int
        {
            None = 0,
            XButton1,
            XButton2
        }

        [Flags]
        internal enum MouseFlag : uint
        {
            Move = 1,
            LeftDown = 2,
            LeftUp = 4,
            RightDown = 8,
            RightUp = 10,
            MiddleDown = 20,
            MiddleUp = 40,
            XDown = 90,
            XUp = 100,
            Wheel = 800,
            VirtualDesk = 4000,
            Absolute = 8000,
        }

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Int32Point Point);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mouse_event
        [DllImport("user32.dll")]
        private static extern void mouse_event(MouseFlag dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        #endregion

        private static event Action<GlobalMouseEventArgs> _MouseDown;
        public static event Action<GlobalMouseEventArgs> MouseDown
        {
            add
            {
                _MouseDown += value;
                CaptureGlobalMouse();
            }
            remove
            {
                _MouseDown -= value;

                if (_MouseDown is null &&
                    _MouseUp is null &&
                    _MouseMove is null &&
                    _MouseWheel is null)
                    ReleaseGlobalMouse();
            }
        }

        private static event Action<GlobalMouseEventArgs> _MouseUp;
        public static event Action<GlobalMouseEventArgs> MouseUp
        {
            add
            {
                _MouseUp += value;
                CaptureGlobalMouse();
            }
            remove
            {
                _MouseUp -= value;

                if (_MouseUp is null &&
                    _MouseDown is null &&
                    _MouseMove is null &&
                    _MouseWheel is null)
                    ReleaseGlobalMouse();
            }
        }

        private static event Action<Int32Point> _MouseMove;
        public static event Action<Int32Point> MouseMove
        {
            add
            {
                _MouseMove += value;
                CaptureGlobalMouse();
            }
            remove
            {
                _MouseMove -= value;

                if (_MouseMove is null &&
                    _MouseDown is null &&
                    _MouseUp is null &&
                    _MouseWheel is null)
                    ReleaseGlobalMouse();
            }
        }

        private static event Action<GlobalMouseEventArgs, int> _MouseWheel;
        public static event Action<GlobalMouseEventArgs, int> MouseWheel
        {
            add
            {
                _MouseWheel += value;
                CaptureGlobalMouse();
            }
            remove
            {
                _MouseWheel -= value;

                if (_MouseWheel is null &&
                    _MouseUp is null &&
                    _MouseDown is null &&
                    _MouseMove is null)
                    ReleaseGlobalMouse();
            }
        }

        public static Int32Point Position
        {
            get
            {
                if (GetCursorPos(out Int32Point Position))
                    return Position;

                return new Int32Point(-1, -1);
            }
        }

        public static bool IsCapturing { private set; get; } = false;
        private static int HookId;
        private static HookProc Proc;
        private static void CaptureGlobalMouse()
        {
            if (IsCapturing)
                return;

            Proc = MouseHookProc;
            HookId = SetWindowsHookEx(WH_MOUSE_LL, Proc, IntPtr.Zero, 0);

            if (HookId == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            IsCapturing = true;
        }
        private static void ReleaseGlobalMouse()
        {
            if (IsCapturing)
            {
                if (UnhookWindowsHookEx(HookId) == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                Proc = null;
                IsCapturing = false;
            }
        }
        private unsafe static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MouseInputInfo* Info = (MouseInputInfo*)lParam;
                switch (wParam)
                {
                    case WM_LButtonDown:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Left);
                            _MouseDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_RButtonDown:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Right);
                            _MouseDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_MButtonDown:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Middle);
                            _MouseDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_LButtonUp:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Left);
                            _MouseUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_RButtonUp:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Right);
                            _MouseUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_MButtonUp:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Middle);
                            _MouseUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_MouseMove:
                        _MouseMove?.Invoke(Info->Position);
                        break;
                    case WM_MouseWheel:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(Info->Position, MouseKey.Middle);
                            _MouseWheel?.Invoke(e, Info->MouseData >> 16);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                }
            }
            return CallNextHookEx(HookId, nCode, wParam, lParam);
        }

        public static void DoMouseDown(MouseKey Button)
        {
            switch (Button)
            {
                case MouseKey.Left:
                    mouse_event(MouseFlag.LeftDown, 0, 0, 0, 0);
                    break;
                case MouseKey.Right:
                    mouse_event(MouseFlag.RightDown, 0, 0, 0, 0);
                    break;
                case MouseKey.Middle:
                    mouse_event(MouseFlag.MiddleDown, 0, 0, 0, 0);
                    break;
                case MouseKey.XButton1:
                    mouse_event(MouseFlag.XDown, 0, 0, (int)MouseData.XButton1, 0);
                    break;
                case MouseKey.XButton2:
                    mouse_event(MouseFlag.XDown, 0, 0, (int)MouseData.XButton2, 0);
                    break;
            }
        }
        public static void DoMouseDown(MouseKey Button, int X, int Y)
        {
            switch (Button)
            {
                case MouseKey.Left:
                    mouse_event(MouseFlag.Absolute | MouseFlag.LeftDown, X, Y, 0, 0);
                    break;
                case MouseKey.Right:
                    mouse_event(MouseFlag.Absolute | MouseFlag.RightDown, X, Y, 0, 0);
                    break;
                case MouseKey.Middle:
                    mouse_event(MouseFlag.Absolute | MouseFlag.MiddleDown, X, Y, 0, 0);
                    break;
                case MouseKey.XButton1:
                    mouse_event(MouseFlag.Absolute | MouseFlag.XDown, X, Y, (int)MouseData.XButton1, 0);
                    break;
                case MouseKey.XButton2:
                    mouse_event(MouseFlag.Absolute | MouseFlag.XDown, X, Y, (int)MouseData.XButton2, 0);
                    break;
            }
        }

        public static void DoMouseUp(MouseKey Button)
        {
            switch (Button)
            {
                case MouseKey.Left:
                    mouse_event(MouseFlag.LeftUp, 0, 0, 0, 0);
                    break;
                case MouseKey.Right:
                    mouse_event(MouseFlag.RightUp, 0, 0, 0, 0);
                    break;
                case MouseKey.Middle:
                    mouse_event(MouseFlag.MiddleUp, 0, 0, 0, 0);
                    break;
                case MouseKey.XButton1:
                    mouse_event(MouseFlag.XUp, 0, 0, (int)MouseData.XButton1, 0);
                    break;
                case MouseKey.XButton2:
                    mouse_event(MouseFlag.XUp, 0, 0, (int)MouseData.XButton2, 0);
                    break;
                default:
                    throw new NotImplementedException($"Not imaplement MouseKey.{Button}.");
            }
        }
        public static void DoMouseUp(MouseKey Button, int X, int Y)
        {
            switch (Button)
            {
                case MouseKey.Left:
                    mouse_event(MouseFlag.Absolute | MouseFlag.LeftUp, X, Y, 0, 0);
                    break;
                case MouseKey.Right:
                    mouse_event(MouseFlag.Absolute | MouseFlag.RightUp, X, Y, 0, 0);
                    break;
                case MouseKey.Middle:
                    mouse_event(MouseFlag.Absolute | MouseFlag.MiddleUp, X, Y, 0, 0);
                    break;
                case MouseKey.XButton1:
                    mouse_event(MouseFlag.Absolute | MouseFlag.XUp, X, Y, (int)MouseData.XButton1, 0);
                    break;
                case MouseKey.XButton2:
                    mouse_event(MouseFlag.Absolute | MouseFlag.XUp, X, Y, (int)MouseData.XButton2, 0);
                    break;
            }
        }

        public static void DoClick(MouseKey Button)
        {
            switch (Button)
            {
                case MouseKey.Left:
                    mouse_event(MouseFlag.LeftDown | MouseFlag.LeftUp, 0, 0, 0, 0);
                    break;
                case MouseKey.Right:
                    mouse_event(MouseFlag.RightDown | MouseFlag.RightUp, 0, 0, 0, 0);
                    break;
                case MouseKey.Middle:
                    mouse_event(MouseFlag.MiddleDown | MouseFlag.MiddleUp, 0, 0, 0, 0);
                    break;
                case MouseKey.XButton1:
                    mouse_event(MouseFlag.XDown | MouseFlag.XUp, 0, 0, (int)MouseData.XButton1, 0);
                    break;
                case MouseKey.XButton2:
                    mouse_event(MouseFlag.XDown | MouseFlag.XUp, 0, 0, (int)MouseData.XButton2, 0);
                    break;
            }
        }
        public static void DoClick(MouseKey Button, int X, int Y)
        {
            switch (Button)
            {
                case MouseKey.Left:
                    mouse_event(MouseFlag.Absolute | MouseFlag.LeftDown | MouseFlag.LeftUp, X, Y, 0, 0);
                    break;
                case MouseKey.Right:
                    mouse_event(MouseFlag.Absolute | MouseFlag.RightDown | MouseFlag.RightUp, X, Y, 0, 0);
                    break;
                case MouseKey.Middle:
                    mouse_event(MouseFlag.Absolute | MouseFlag.MiddleDown | MouseFlag.MiddleUp, X, Y, 0, 0);
                    break;
                case MouseKey.XButton1:
                    mouse_event(MouseFlag.Absolute | MouseFlag.XDown | MouseFlag.XUp, X, Y, (int)MouseData.XButton1, 0);
                    break;
                case MouseKey.XButton2:
                    mouse_event(MouseFlag.Absolute | MouseFlag.XDown | MouseFlag.XUp, X, Y, (int)MouseData.XButton2, 0);
                    break;
            }
        }

    }
}
