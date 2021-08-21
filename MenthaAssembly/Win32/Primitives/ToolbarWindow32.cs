using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static MenthaAssembly.Win32.Desktop;
using static MenthaAssembly.Win32.System;

namespace MenthaAssembly.Win32.Primitives
{
    public unsafe class ToolbarWindow32
    {
        public IntPtr Handle { get; }

        private int _ProcessId;
        public int ProcessId
        {
            get
            {
                if (_ProcessId == 0)
                    GetWindowThreadProcessId(Handle, out _ProcessId);

                return _ProcessId;
            }
        }

        private Bound<int> _Bound;
        public Bound<int> Bound
        {
            get
            {
                if (_Bound.IsEmpty)
                {
                    Bound<int> TBound;
                    GetWindowRect(Handle, &TBound);
                    _Bound = TBound;
                }

                return _Bound;
            }
        }

        public IEnumerable<ToolbarButtonWindow32> Buttons
            => GetButtons();

        public ToolbarWindow32(IntPtr Hwnd)
        {
            this.Handle = Hwnd;
        }

        public ToolbarButtonWindow32 this[int Index]
        {
            get
            {
                int Count = SendMessage(Handle, Win32Messages.TB_ButtonCount, IntPtr.Zero, IntPtr.Zero);
                if (-1 < Index && Index < Count)
                    return GetButtonByIndex(Index);

                return null;
            }
        }

        private IEnumerable<ToolbarButtonWindow32> GetButtons()
        {
            int Count = SendMessage(Handle, Win32Messages.TB_ButtonCount, IntPtr.Zero, IntPtr.Zero);
            for (int i = 0; i < Count; i++)
                if (GetButtonByIndex(i) is ToolbarButtonWindow32 Button)
                    yield return Button;
        }

        protected virtual unsafe ToolbarButtonWindow32 GetButtonByIndex(int Index)
        {
            const int BUFFER_SIZE = 0x1000;

            // Process Handle
            IntPtr hProcess = OpenProcess(ProcessRight.AllAccess, false, ProcessId);
            if (hProcess == IntPtr.Zero)
            {
                Debug.Assert(false);
                return null;
            }

            // Process Buffer
            IntPtr ipRemoteBuffer = VirtualAllocEx(hProcess, IntPtr.Zero, BUFFER_SIZE, MemAllocType.Commit, MemProtectType.Page_ReadWrite);
            if (ipRemoteBuffer == IntPtr.Zero)
            {
                CloseHandle(hProcess);
                Debug.Assert(false);
                return null;
            }

            try
            {
                // TBButtonInfo
                int InfoSize = sizeof(TBButtonInfo);
                TBButtonInfo Info = new TBButtonInfo
                {
                    cbSize = InfoSize,
                    dwMask = TBButtonFlag.ByIndex | TBButtonFlag.All,
                    pszText = ipRemoteBuffer + InfoSize,
                    cchText = 256
                };
                IntPtr pInfo = (IntPtr)(&Info);

                if (!WriteProcessMemory(hProcess, ipRemoteBuffer, pInfo, InfoSize, out int WriteLength))
                {
                    Debug.Assert(false);
                    return null;
                }
                if (SendMessage(Handle, Win32Messages.TB_GetButtonInfoW, (IntPtr)Index, ipRemoteBuffer) == -1)
                {
                    Debug.Assert(false);
                    return null;
                }
                if (!ReadProcessMemory(hProcess, ipRemoteBuffer, pInfo, InfoSize, out int TempLen))
                {
                    Debug.Assert(false);
                    return null;
                }

                // TBButton Text
                string Text = string.Empty;
                byte* pTextDatas = stackalloc byte[512];
                IntPtr pText = (IntPtr)pTextDatas;
                if (ReadProcessMemory(hProcess, ipRemoteBuffer + InfoSize, pText, 512, out _))
                {
                    Text = Marshal.PtrToStringUni(pText);
                    if (string.IsNullOrWhiteSpace(Text))
                        Text = string.Empty;
                }
                else
                    Debug.Assert(false);

                // Bound
                Bound<int> Bound = new Bound<int>();
                IntPtr pBound = (IntPtr)(&Bound);
                if (SendMessage(Handle, Win32Messages.TB_GetItemRect, (IntPtr)Index, ipRemoteBuffer) == -1)
                {
                    Debug.Assert(false);
                    return null;
                }
                if (!ReadProcessMemory(hProcess, ipRemoteBuffer, pBound, sizeof(Bound<int>), out _))
                    Debug.Assert(false);

                if (Bound.Left < 0 || Bound.Top < 0)
                    Bound = default;
                else
                    Bound.Offset(this.Bound.Left, this.Bound.Top);

                return new ToolbarButtonWindow32(this, Index, Bound, Text);
            }
            finally
            {
                VirtualFreeEx(hProcess, ipRemoteBuffer, 0, MemFreeType.Release);
                CloseHandle(hProcess);
            }
        }

    }

    public class ToolbarWindow32<T> : ToolbarWindow32
        where T : struct
    {
        public new IEnumerable<ToolbarButtonWindow32<T>> Buttons
            => GetSpecialButtons();

        public ToolbarWindow32(IntPtr Hwnd) : base(Hwnd)
        {
        }

        public new ToolbarButtonWindow32<T> this[int Index]
            => GetSpecialButtonByIndex(Index);

        private IEnumerable<ToolbarButtonWindow32<T>> GetSpecialButtons()
        {
            int Count = SendMessage(Handle, Win32Messages.TB_ButtonCount, IntPtr.Zero, IntPtr.Zero);
            for (int i = 0; i < Count; i++)
                if (GetButtonByIndex(i) is ToolbarButtonWindow32<T> Button)
                    yield return Button;
        }

        protected sealed override ToolbarButtonWindow32 GetButtonByIndex(int Index)
            => GetSpecialButtonByIndex(Index);
        protected virtual unsafe ToolbarButtonWindow32<T> GetSpecialButtonByIndex(int Index)
        {
            const int BUFFER_SIZE = 0x1000;

            // Process Handle
            IntPtr hProcess = OpenProcess(ProcessRight.AllAccess, false, ProcessId);
            if (hProcess == IntPtr.Zero)
            {
                Debug.Assert(false);
                return null;
            }

            // Process Buffer
            IntPtr ipRemoteBuffer = VirtualAllocEx(hProcess, IntPtr.Zero, BUFFER_SIZE, MemAllocType.Commit, MemProtectType.Page_ReadWrite);
            if (ipRemoteBuffer == IntPtr.Zero)
            {
                CloseHandle(hProcess);
                Debug.Assert(false);
                return null;
            }

            try
            {
                // TBButtonInfo
                int InfoSize = sizeof(TBButtonInfo);
                TBButtonInfo Info = new TBButtonInfo
                {
                    cbSize = InfoSize,
                    dwMask = TBButtonFlag.ByIndex | TBButtonFlag.All,
                    pszText = ipRemoteBuffer + InfoSize,
                    cchText = 256
                };
                IntPtr pInfo = (IntPtr)(&Info);

                if (!WriteProcessMemory(hProcess, ipRemoteBuffer, pInfo, InfoSize, out int WriteLength))
                {
                    Debug.Assert(false);
                    return null;
                }
                if (SendMessage(Handle, Win32Messages.TB_GetButtonInfoW, (IntPtr)Index, ipRemoteBuffer) == -1)
                {
                    Debug.Assert(false);
                    return null;
                }
                if (!ReadProcessMemory(hProcess, ipRemoteBuffer, pInfo, InfoSize, out int TempLen))
                {
                    Debug.Assert(false);
                    return null;
                }

                // TBButton Text
                string Text = string.Empty;
                byte* pTextDatas = stackalloc byte[512];
                IntPtr pText = (IntPtr)pTextDatas;
                if (ReadProcessMemory(hProcess, ipRemoteBuffer + InfoSize, pText, 512, out _))
                {
                    Text = Marshal.PtrToStringUni(pText);
                    if (string.IsNullOrWhiteSpace(Text))
                        Text = string.Empty;
                }
                else
                    Debug.Assert(false);

                // Bound
                Bound<int> Bound = new Bound<int>();
                IntPtr pBound = (IntPtr)(&Bound);
                if (SendMessage(Handle, Win32Messages.TB_GetItemRect, (IntPtr)Index, ipRemoteBuffer) == -1)
                {
                    Debug.Assert(false);
                    return null;
                }
                if (!ReadProcessMemory(hProcess, ipRemoteBuffer, pBound, sizeof(Bound<int>), out _))
                    Debug.Assert(false);

                if (Bound.Left < 0 || Bound.Top < 0)
                    Bound = default;
                else
                    Bound.Offset(this.Bound.Left, this.Bound.Top);

                // Content Handle
                int ContentHwnd = 0;
                IntPtr pContentHwnd = (IntPtr)(&ContentHwnd);
                if (!ReadProcessMemory(hProcess, Info.lParam, pContentHwnd, 4, out _))
                {
                    Debug.Assert(false);
                    ContentHwnd = 0;
                }

                // Content
                T Content = default;
                int ContentSize = Marshal.SizeOf<T>();
                IntPtr pContent = Marshal.AllocHGlobal(ContentSize);
                if (ReadProcessMemory(hProcess, Info.lParam, pContent, ContentSize, out _))
                    Content = Marshal.PtrToStructure<T>(pContent);
                else
                    Debug.Assert(false);

                Marshal.FreeHGlobal(pContent);

                return new ToolbarButtonWindow32<T>(this, Index, Bound, Text, Content);
            }
            finally
            {
                VirtualFreeEx(hProcess, ipRemoteBuffer, 0, MemFreeType.Release);
                CloseHandle(hProcess);
            }
        }

    }

}
