using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class GlobalMouse
    {
        #region Windows API
        private const int WH_MOUSE_LL = 14;

        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        //private const int WM_LBUTTONDBLCLK = 0x203;

        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_RBUTTONUP = 0x205;
        //private const int WM_RBUTTONDBLCLK = 0x206;

        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_MBUTTONUP = 0x208;
        //private const int WM_MBUTTONDBLCLK = 0x209;

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public Int32Point Position;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr dwExtraInfo;
        }

        [Flags]
        private enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            WHEEL = 0x00000800,
            XDOWN = 0x00000080,
            XUP = 0x00000100
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
        private static extern void mouse_event(MouseEventFlags dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

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
                    _MouseMove is null)
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
                    _MouseMove is null)
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
                    _MouseUp is null)
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
        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT MouseHookInfo = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(MouseHookInfo.Position, MouseButton.Left);
                            _MouseDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_RBUTTONDOWN:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(MouseHookInfo.Position, MouseButton.Right);
                            _MouseDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_MBUTTONDOWN:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(MouseHookInfo.Position, MouseButton.Middle);
                            _MouseDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_LBUTTONUP:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(MouseHookInfo.Position, MouseButton.Left);
                            _MouseUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_RBUTTONUP:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(MouseHookInfo.Position, MouseButton.Right);
                            _MouseUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_MBUTTONUP:
                        {
                            GlobalMouseEventArgs e = new GlobalMouseEventArgs(MouseHookInfo.Position, MouseButton.Middle);
                            _MouseUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_MOUSEMOVE:
                        _MouseMove?.Invoke(MouseHookInfo.Position);
                        break;
                }
            }
            return CallNextHookEx(HookId, nCode, wParam, lParam);
        }

        public static void DoMouseDown(MouseButton Button)
        {
            switch (Button)
            {
                case MouseButton.Left:
                    mouse_event(MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MouseEventFlags.MIDDLEDOWN, 0, 0, 0, 0);
                    break;
                case MouseButton.XButton1:
                    mouse_event(MouseEventFlags.XDOWN, 0, 0, 1, 0);
                    break;
                case MouseButton.XButton2:
                    mouse_event(MouseEventFlags.XDOWN, 0, 0, 2, 0);
                    break;
            }
        }
        public static void DoMouseDown(MouseButton Button, int X, int Y)
        {
            switch (Button)
            {
                case MouseButton.Left:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.LEFTDOWN, X, Y, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.RIGHTDOWN, X, Y, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.MIDDLEDOWN, X, Y, 0, 0);
                    break;
                case MouseButton.XButton1:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.XDOWN, X, Y, 1, 0);
                    break;
                case MouseButton.XButton2:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.XDOWN, X, Y, 2, 0);
                    break;
            }
        }

        public static void DoMouseUp(MouseButton Button)
        {
            switch (Button)
            {
                case MouseButton.Left:
                    mouse_event(MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MouseEventFlags.MIDDLEUP, 0, 0, 0, 0);
                    break;
                case MouseButton.XButton1:
                    mouse_event(MouseEventFlags.XUP, 0, 0, 1, 0);
                    break;
                case MouseButton.XButton2:
                    mouse_event(MouseEventFlags.XUP, 0, 0, 2, 0);
                    break;
            }
        }
        public static void DoMouseUp(MouseButton Button, int X, int Y)
        {
            switch (Button)
            {
                case MouseButton.Left:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.LEFTUP, X, Y, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.RIGHTUP, X, Y, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.MIDDLEUP, X, Y, 0, 0);
                    break;
                case MouseButton.XButton1:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.XUP, X, Y, 1, 0);
                    break;
                case MouseButton.XButton2:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.XUP, X, Y, 2, 0);
                    break;
            }
        }

        public static void DoClick(MouseButton Button)
        {
            switch (Button)
            {
                case MouseButton.Left:
                    mouse_event(MouseEventFlags.LEFTDOWN | MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MouseEventFlags.RIGHTDOWN | MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MouseEventFlags.MIDDLEDOWN | MouseEventFlags.MIDDLEUP, 0, 0, 0, 0);
                    break;
                case MouseButton.XButton1:
                    mouse_event(MouseEventFlags.XDOWN | MouseEventFlags.XUP, 0, 0, 1, 0);
                    break;
                case MouseButton.XButton2:
                    mouse_event(MouseEventFlags.XDOWN | MouseEventFlags.XUP, 0, 0, 2, 0);
                    break;
            }
        }
        public static void DoClick(MouseButton Button, int X, int Y)
        {
            switch (Button)
            {
                case MouseButton.Left:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.LEFTDOWN | MouseEventFlags.LEFTUP, X, Y, 0, 0);
                    break;
                case MouseButton.Right:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.RIGHTDOWN | MouseEventFlags.RIGHTUP, X, Y, 0, 0);
                    break;
                case MouseButton.Middle:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.MIDDLEDOWN | MouseEventFlags.MIDDLEUP, X, Y, 0, 0);
                    break;
                case MouseButton.XButton1:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.XDOWN | MouseEventFlags.XUP, X, Y, 1, 0);
                    break;
                case MouseButton.XButton2:
                    mouse_event(MouseEventFlags.ABSOLUTE | MouseEventFlags.XDOWN | MouseEventFlags.XUP, X, Y, 2, 0);
                    break;
            }
        }

    }
}
