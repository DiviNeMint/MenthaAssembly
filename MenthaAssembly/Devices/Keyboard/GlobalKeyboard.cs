using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Devices
{
    public static class GlobalKeyboard
    {
        #region Windows API
        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        private const int WH_KEYBOARD_LL = 13;

        //https://docs.microsoft.com/en-us/windows/win32/inputdev/keyboard-input-notifications
        private const int WM_KeyDown = 0x100;
        private const int WM_KeyUp = 0x101;

        private const int WM_SysKeyDown = 0x104;
        private const int WM_SysKeyUp = 0x105;

        [Flags]
        internal enum KeyboardFlag
        {
            KeyDown = 0,
            ExtendedKey = 1,
            KeyUp = 2,
            Unicode = 4,
            ScanCode = 8,
        }

        private enum MapType
        {
            VirtualKey_To_ScanCode = 0x00,
            ScanCode_To_VirtualKey = 0x01,
            VirtualKey_To_Char = 0x02,
            ScanCode_To_VirtualKey_EX = 0x03,
            VirtualKey_To_ScanCode_EX = 0x04
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardInputInfo
        {
            public KeyboardKey Key { get; }

            public uint ScanCode { get; }

            public KeyboardFlag Flags { get; }

            public uint Time { get; }

            public IntPtr ExtraInfo { get; }
        }

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int MapVirtualKey(uint uCode, MapType Type);

        [DllImport("user32.dll")]
        private static extern void keybd_event(KeyboardKey Key, byte bScan, KeyboardFlag Flags, int ExtraInfo);

        //[DllImport("User32.dll")]
        //private static extern short GetAsyncKeyState(KeyboardKey Key);
        [DllImport("user32.dll")]
        private static extern short GetKeyState(KeyboardKey Key);

        #endregion

        private static event Action<GlobalKeyboardEventArgs> _KeyDown;
        public static event Action<GlobalKeyboardEventArgs> KeyDown
        {
            add
            {
                _KeyDown += value;
                CaptureGlobalKeyboard();
            }
            remove
            {
                _KeyDown -= value;

                if (_KeyDown is null &&
                    _KeyUp is null)
                    ReleaseGlobalKeyboard();
            }
        }

        private static event Action<GlobalKeyboardEventArgs> _KeyUp;
        public static event Action<GlobalKeyboardEventArgs> KeyUp
        {
            add
            {
                _KeyUp += value;
                CaptureGlobalKeyboard();
            }
            remove
            {
                _KeyUp -= value;

                if (_KeyUp is null &&
                    _KeyDown is null)
                    ReleaseGlobalKeyboard();
            }
        }

        public static bool NumLock
        {
            get => GetKeyState(KeyboardKey.NumLock) == 0;
            set
            {
                if (value != NumLock)
                {
                    int ScanCode = MapVirtualKey((uint)KeyboardKey.NumLock, MapType.VirtualKey_To_ScanCode);
                    keybd_event(KeyboardKey.NumLock, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyDown, 0);
                    keybd_event(KeyboardKey.NumLock, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyUp, 0);
                }
            }
        }

        public static bool CapsLock
        {
            get => GetKeyState(KeyboardKey.CapsLock) == 0;
            set
            {
                if (value != CapsLock)
                {
                    int ScanCode = MapVirtualKey((uint)KeyboardKey.CapsLock, MapType.VirtualKey_To_ScanCode);
                    keybd_event(KeyboardKey.CapsLock, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyDown, 0);
                    keybd_event(KeyboardKey.CapsLock, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyUp, 0);
                }
            }
        }

        public static bool ScrollLock
        {
            get => GetKeyState(KeyboardKey.ScrollLock) == 0;
            set
            {
                if (value != ScrollLock)
                {
                    int ScanCode = MapVirtualKey((uint)KeyboardKey.ScrollLock, MapType.VirtualKey_To_ScanCode);
                    keybd_event(KeyboardKey.ScrollLock, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyDown, 0);
                    keybd_event(KeyboardKey.ScrollLock, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyUp, 0);
                }
            }
        }

        public static bool IsCapturing { private set; get; } = false;
        private static int HookId;
        private static HookProc Proc;
        private static void CaptureGlobalKeyboard()
        {
            if (IsCapturing)
                return;

            Proc = KeyboardHookProc;
            HookId = SetWindowsHookEx(WH_KEYBOARD_LL, Proc, IntPtr.Zero, 0);

            if (HookId == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            IsCapturing = true;
        }
        private static void ReleaseGlobalKeyboard()
        {
            if (IsCapturing)
            {
                if (UnhookWindowsHookEx(HookId) == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                Proc = null;
                IsCapturing = false;
            }
        }
        private static unsafe int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyboardInputInfo* Info = (KeyboardInputInfo*)lParam;
                switch (wParam)
                {
                    case WM_KeyDown:
                    case WM_SysKeyDown:
                        {
                            GlobalKeyboardEventArgs e = new GlobalKeyboardEventArgs(Info->Key);
                            _KeyDown?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                    case WM_KeyUp:
                    case WM_SysKeyUp:
                        {
                            GlobalKeyboardEventArgs e = new GlobalKeyboardEventArgs(Info->Key);
                            _KeyUp?.Invoke(e);

                            if (e.Handled)
                                return 1;

                            break;
                        }
                }
            }
            return CallNextHookEx(HookId, nCode, wParam, lParam);
        }

        public static void DoKeyDown(KeyboardKey Key)
        {
            int ScanCode = MapVirtualKey((uint)Key, MapType.VirtualKey_To_ScanCode);
            keybd_event(Key, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyDown, 0);
        }
        public static void DoKeyUp(KeyboardKey Key)
        {
            int ScanCode = MapVirtualKey((uint)Key, MapType.VirtualKey_To_ScanCode);
            keybd_event(Key, (byte)ScanCode, KeyboardFlag.ExtendedKey | KeyboardFlag.KeyUp, 0);
        }

    }
}
