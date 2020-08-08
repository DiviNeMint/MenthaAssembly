using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    public class System
    {
        #region Windows API (Send/Post)
        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr Hwnd, Win32Messages msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hwnd, Win32Messages msg, int wParam, bool lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hwnd, Win32Messages msg, int wParam, string lParam);

        #endregion

        #region Windows API (Memory)
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(ProcessRights dwDesiredAccess, bool bInheritHandle, int ProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int Size, MemAllocType AllocationType, MemProtectType Protect);

        [DllImport("kernel32.dll")]
        internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int Size, MemFreeType FreeType);

        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpAddress, IntPtr lpBuffer, int Size, out int ReadLength);

        [DllImport("kernel32.dll")]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpAddress, IntPtr lpBuffer, int Size, out int WriteLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region Windows API (System parameter)
        /// <summary>
        /// Gets the maximum number of milliseconds that can elapse between a
        /// first click and a second click for the OS to consider the
        /// mouse action a double-click.
        /// </summary>
        /// <returns>The maximum amount of time, in milliseconds, that can
        /// elapse between a first click and a second click for the OS to
        /// consider the mouse action a double-click.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int GetDoubleClickTime();

        #endregion

    }
}
