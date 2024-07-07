using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Win32
{
    public unsafe static class System
    {
        #region Windows API (Send/Post)
        [DllImport("User32.dll")]
        internal static extern int SendMessage(IntPtr Hwnd, Win32Messages msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        internal static extern int SendMessage(IntPtr Hwnd, Win32Messages msg, IntPtr wParam, int lParam);

        [DllImport("User32.dll")]
        internal static extern int SendMessage(IntPtr hwnd, Win32Messages msg, int wParam, bool lParam);

        [DllImport("User32.dll")]
        internal static extern int SendMessage(IntPtr hwnd, Win32Messages msg, int wParam, string lParam);

        #endregion

        #region Windows API (Library)
        /// <summary>
        /// Loads the specified module into the address space of the calling process.<para/>
        /// The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="FileName">A string that specifies the file name of the module to load.<para/>
        ///                        This name is not related to the name stored in a library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string FileName);

        /// <summary>
        /// Loads the specified module into the address space of the calling process.<para/>
        /// The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="FileName">A string that specifies the file name of the module to load.<para/>
        ///                        This name is not related to the name stored in a library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
        /// <param name="Reserved">This parameter is reserved for future use.<para/> 
        ///                        It must be <see cref="IntPtr.Zero"/></param>
        /// <param name="Flag">The action to be taken when loading the module.<para/>
        ///                    If no flags are specified, the behavior of this function is identical to that of the LoadLibrary function.</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibraryEx(string FileName, IntPtr Reserved, LoadLibraryFlag Flag);

        /// <summary>
        /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.<para/>
        /// When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
        /// </summary>
        /// <param name="hModule">A handle to the loaded library module.<para/>
        /// The LoadLibrary, LoadLibraryEx, GetModuleHandle, or GetModuleHandleEx function returns this handle.</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr module, IntPtr ordinal);

        #endregion

        #region Windows API (Resource)
        internal delegate bool EnumResourceNamesProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, ResourceType lpType);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr FindResource(IntPtr hModule, uint ResourceID, ResourceType lpType);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern bool EnumResourceNames(IntPtr hModule, ResourceType lpszType, EnumResourceNamesProc lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll")]
        internal static extern int SizeofResource(IntPtr hModule, IntPtr hResInfo);

        /// <summary>
        /// Loads a string resource from the executable file associated with a specified module and either copies the string into a buffer with a terminating null character or returns a read-only pointer to the string resource itself.
        /// </summary>
        /// <param name="hInstance">A handle to an instance of the module whose executable file contains the string resource.<para/>
        ///                         To get the handle to the application itself, call the GetModuleHandle function with NULL.</param>
        /// <param name="uID">The identifier of the string to be loaded.</param>
        /// <param name="lpBuffer">The buffer to receive the string.<para/>
        ///                        Must be at least cchBufferMax characters in size.</param>
        /// <param name="nBufferMax">The size of the buffer, in characters.<para/>
        ///                          The string is truncated and null-terminated if it is longer than the number of characters specified.<para/>
        ///                          This parameter may not be zero.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

        #endregion

        #region Windows API (Memory)
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr OpenProcess(ProcessRight dwDesiredAccess, bool bInheritHandle, int ProcessId);

        /// <summary>
        /// Reserves, commits, or changes the state of a region of pages in the virtual address space of the calling process.<para/>
        /// Memory allocated by this function is automatically initialized to zero.
        /// </summary>
        /// <param name="lpAddress">The handle to a process. The function allocates memory within the virtual address space of this process.<para/>
        ///                         The handle must have the PROCESS_VM_OPERATION access right.<para/>
        ///                         For more information, see <see href="https://docs.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights">Process Security and Access Rights</see>.</param>
        /// <param name="Size">The size of the region, in bytes. 
        ///                    If lpAddress is <see cref="IntPtr.Zero"/>, the function rounds dwSize up to the next page boundary.<para/>
        ///                    If lpAddress is not <see cref="IntPtr.Zero"/>, the function allocates all pages that contain one or more bytes in the range from lpAddress to lpAddress+dwSize.<para/>
        ///                    This means, for example, that a 2-byte range that straddles a page boundary causes the function to allocate both pages.</param>
        /// <param name="AllocationType">The type of memory allocation.</param>
        /// <param name="Protect">The memory protection for the region of pages to be allocated.</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint Size, MemAllocType AllocationType, MemProtectType Protect);

        /// <summary>
        /// Reserves, commits, or changes the state of a region of memory within the virtual address space of a specified process.<para/>
        /// The function initializes the memory it allocates to zero.
        /// </summary>
        /// <param name="hProcess">The handle to a process. The function allocates memory within the virtual address space of this process.<para/>
        ///                        The handle must have the PROCESS_VM_OPERATION access right.<para/>
        ///                        For more information, see <see href="https://docs.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights">Process Security and Access Rights</see>.</param>
        /// <param name="lpAddress">The pointer that specifies a desired starting address for the region of pages that you want to allocate.<para/>
        ///                         If you are reserving memory, the function rounds this address down to the nearest multiple of the allocation granularity.<para/>
        ///                         If you are committing memory that is already reserved, the function rounds this address down to the nearest page boundary.<para/>
        ///                         To determine the size of a page and the allocation granularity on the host computer, use the GetSystemInfo function.<para/>
        ///                         If lpAddress is <see cref="IntPtr.Zero"/>, the function determines where to allocate the region.<para/>
        ///                         If this address is within an enclave that you have not initialized by calling InitializeEnclave, VirtualAllocEx allocates a page of zeros for the enclave at that address.<para/>
        ///                         The page must be previously uncommitted, and will not be measured with the EEXTEND instruction of the Intel Software Guard Extensions programming model.<para/>
        ///                         If the address in within an enclave that you initialized, then the allocation operation fails with the ERROR_INVALID_ADDRESS error.</param>
        /// <param name="Size">The size of the region of memory to allocate, in bytes.<para/>
        ///                    If lpAddress is <see cref="IntPtr.Zero"/>, the function rounds dwSize up to the next page boundary.<para/>
        ///                    If lpAddress is not <see cref="IntPtr.Zero"/>, the function allocates all pages that contain one or more bytes in the range from lpAddress to lpAddress+dwSize.<para/>
        ///                    This means, for example, that a 2-byte range that straddles a page boundary causes the function to allocate both pages.</param>
        /// <param name="AllocationType">The type of memory allocation.</param>
        /// <param name="Protect">The memory protection for the region of pages to be allocated.</param>
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int Size, MemAllocType AllocationType, MemProtectType Protect);

        /// <summary>
        /// Changes the protection on a region of committed pages in the virtual address space of the calling process.
        /// </summary>
        /// <param name="lpAddress">The address of the starting page of the region of pages whose access protection attributes are to be changed.<para/>
        ///                         All pages in the specified region must be within the same reserved region allocated when calling the VirtualAlloc or VirtualAllocEx function using <see cref="MemAllocType.Reserve"/>.<para/>
        ///                         The pages cannot span adjacent reserved regions that were allocated by separate calls to VirtualAlloc or VirtualAllocEx using <see cref="MemAllocType.Reserve"/>.</param>
        /// <param name="Size">The size of the region whose access protection attributes are to be changed, in bytes. 
        ///                    The region of affected pages includes all pages containing one or more bytes in the range from the lpAddress parameter to (lpAddress+dwSize). <para/>
        ///                    This means that a 2-byte range straddling a page boundary causes the protection attributes of both pages to be changed.</param>
        /// <param name="flNewProtect">The memory protection option.</param>
        /// <param name="lpflOldProtect">A pointer to a variable that receives the previous access protection value of the first page in the specified region of pages.<para/>
        ///                              If this parameter is <see cref="IntPtr.Zero"/> or does not point to a valid variable, the function fails.</param>
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtect(IntPtr lpAddress, int Size, MemProtectType flNewProtect, out IntPtr lpflOldProtect);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualProtect(IntPtr lpAddress, uint Size, MemProtectType flNewProtect, out IntPtr lpflOldProtect);

        /// <summary>
        /// Releases, decommits, or releases and decommits a region of pages within the virtual address space of the calling process.
        /// </summary>
        /// <param name="lpAddress">A pointer to the base address of the region of pages to be freed.
        ///                         If <paramref name="FreeType"/> is <see cref="MemFreeType.Release"/>, lpAddress must be the base address returned by the VirtualAlloc function when the region of pages is reserved.</param>
        /// <param name="Size">The size of the region of memory to free, in bytes.<para/>
        ///                    If <paramref name="FreeType"/> is <see cref="MemFreeType.Release"/>, Size must be 0 (zero). <para/>
        ///                    The function frees the entire region that is reserved in the initial allocation call to VirtualAlloc.<para/>
        ///                    If <paramref name="FreeType"/> is <see cref="MemFreeType.Decommit"/>, the function decommits all memory pages that contain one or more bytes in the range from the lpAddress parameter to (lpAddress+dwSize). <para/>
        ///                    This means, for example, that a 2-byte region of memory that straddles a page boundary causes both pages to be decommitted.<para/>
        ///                    If lpAddress is the base address returned by VirtualAllocEx and Size is 0 (zero), the function decommits the entire region that is allocated by VirtualAlloc.<para/>
        ///                    After that, the entire region is in the reserved state.</param>
        /// <param name="FreeType">The type of free operation.</param>
        [DllImport("Kernel32.dll")]
        internal static extern bool VirtualFree(IntPtr lpAddress, uint Size, MemFreeType FreeType);

        [DllImport("Kernel32.dll")]
        internal static extern bool VirtualFree(UIntPtr lpAddress, uint Size, MemFreeType FreeType);

        /// <summary>
        /// Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified process.
        /// </summary>
        /// <param name="hProcess">The handle to a process. The function frees memory within the virtual address space of this process.<para/>
        ///                        The handle must have the PROCESS_VM_OPERATION access right.<para/>
        ///                        For more information, see <see href="https://docs.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights">Process Security and Access Rights</see>.</param>
        /// <param name="lpAddress">A pointer to the starting address of the region of memory to be freed.<para/>
        ///                         If <paramref name="FreeType"/> is <see cref="MemFreeType.Release"/>, lpAddress must be the base address returned by the VirtualAllocEx function when the region is reserved.</param>
        /// <param name="Size">The size of the region of memory to free, in bytes.<para/>
        ///                    If <paramref name="FreeType"/> is <see cref="MemFreeType.Release"/>, Size must be 0 (zero). <para/>
        ///                    The function frees the entire region that is reserved in the initial allocation call to VirtualAllocEx.<para/>
        ///                    If <paramref name="FreeType"/> is <see cref="MemFreeType.Decommit"/>, the function decommits all memory pages that contain one or more bytes in the range from the lpAddress parameter to (lpAddress+dwSize). <para/>
        ///                    This means, for example, that a 2-byte region of memory that straddles a page boundary causes both pages to be decommitted.<para/>
        ///                    If lpAddress is the base address returned by VirtualAllocEx and Size is 0 (zero), the function decommits the entire region that is allocated by VirtualAllocEx.<para/>
        ///                    After that, the entire region is in the reserved state.</param>
        /// <param name="FreeType">The type of free operation.</param>
        [DllImport("Kernel32.dll")]
        internal static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int Size, MemFreeType FreeType);

        [DllImport("Kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpAddress, IntPtr lpBuffer, int Size, out int ReadLength);

        [DllImport("Kernel32.dll")]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpAddress, IntPtr lpBuffer, int Size, out int WriteLength);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemoryCopy(IntPtr pDest, IntPtr pSrc, long Length);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemoryCopy(void* pDest, void* pSrc, long Length);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr pDest, IntPtr Size);

        #endregion

        #region Windows API (System parameter)
        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa">SystemParametersInfo</see>
        /// </summary>
        /// <param name="uiParam">
        /// A parameter whose usage and format depends on the system parameter being queried or set. 
        /// <para/>
        /// For more information about system-wide parameters, see <see cref="SystemParameterActionType"/> summary. 
        /// <para/>
        /// If not otherwise indicated, you must specify zero for this parameter.
        /// </param>
        /// <param name="pvParam">
        /// A parameter whose usage and format depends on the system parameter being queried or set. 
        /// <para/>
        /// For more information about system-wide parameters, see <see cref="SystemParameterActionType"/> summary. 
        /// <para/>
        /// If not otherwise indicated, you must specify NULL for this parameter. 
        /// <para/>
        /// For information on the PVOID datatype, see <see href="https://docs.microsoft.com/en-us/windows/win32/winprog/windows-data-types">Windows Data Types</see>.
        /// </param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        internal static extern bool SystemParametersInfo(SystemParameterActionType Action, uint uiParam, IntPtr pvParam, SystemParameterInfoFlag fWinIni);

        /// <summary>
        /// Gets the maximum number of milliseconds that can elapse between a
        /// first click and a second click for the OS to consider the
        /// mouse action a double-click.
        /// </summary>
        /// <returns>The maximum amount of time, in milliseconds, that can
        /// elapse between a first click and a second click for the OS to
        /// consider the mouse action a double-click.</returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern int GetDoubleClickTime();

        [DllImport("User32.dll")]
        internal static extern bool SetSystemCursor(IntPtr hCursor, CursorID type);

        //[DllImport("Kernel32.dll")]
        //internal static extern void GetLocalTime(out SystemTime lpSystemTime);

        [DllImport("Kernel32.dll")]
        internal static extern bool SetLocalTime(ref SystemTime lpSystemTime);

        #endregion

        #region Windows API (Font)
        [DllImport("Gdi32.dll")]
        internal static extern IntPtr CreateFontIndirect([In] FontData lplf);

        [DllImport("Gdi32.dll")]
        internal static extern bool GetTextMetrics(IntPtr hdc, out TextMetric Metric);

        [DllImport("Gdi32.dll")]
        internal static extern int EnumFontFamiliesEx(IntPtr hdc, ref FontData lpLogfont, EnumFontExDelegate Callback, IntPtr lParam, uint dwFlags);
        internal delegate int EnumFontExDelegate(ref FontData Font, ref TextMetric lpntme, int FontType, int lParam);

        #endregion

        public static IEnumerable<string> FontFamilyNames
        {
            get
            {
                List<string> Result = new List<string>();

                int Callback(ref FontData Font, ref TextMetric lpntme, int FontType, int lParam)
                {
                    if (!'@'.Equals(Font.FaceName.FirstOrDefault()))
                        Result.Add(Font.FaceName);

                    return 1;
                }

                FontData Data = new FontData();
                IntPtr hdc = Graphic.GetDC(IntPtr.Zero);

                try
                {
                    EnumFontFamiliesEx(hdc, ref Data, Callback, IntPtr.Zero, 0);
                }
                finally
                {
                    Graphic.DeleteDC(hdc);
                }

                return Result;
            }
        }

        public static bool EnableMinMaxAnimation
        {
            get
            {
                // AnimationInfo
                // https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-animationinfo
                int* Data = stackalloc int[2];
                Data[0] = 8;
                SystemParametersInfo(SystemParameterActionType.GetAnimation, 8, (IntPtr)Data, SystemParameterInfoFlag.None);
                return Data[1] != 0;
            }
            set
            {
                // AnimationInfo
                // https://docs.microsoft.com/zh-tw/windows/win32/api/winuser/ns-winuser-animationinfo
                int* Data = stackalloc int[2];
                Data[0] = 8;
                Data[1] = value ? 1 : 0;
                SystemParametersInfo(SystemParameterActionType.SetAnimation, 8, (IntPtr)Data, SystemParameterInfoFlag.SendChange);
            }
        }

        public static DateTime LocalTime
        {
            //GetLocalTime(out SystemTime SystemTime);
            //return new DateTime(SystemTime.Year, 
            //                    SystemTime.Month, 
            //                    SystemTime.Day, 
            //                    SystemTime.Hour, 
            //                    SystemTime.Minute, 
            //                    SystemTime.Second, 
            //                    SystemTime.Miliseconds, 
            //                    DateTimeKind.Local);
            get => DateTime.Now;
            set
            {
                SystemTime SysTime = new SystemTime
                {
                    Year = (ushort)value.Year,
                    Month = (ushort)value.Month,
                    Day = (ushort)value.Day,
                    Hour = (ushort)value.Hour,
                    Minute = (ushort)value.Minute,
                    Second = (ushort)value.Second,
                    Miliseconds = (ushort)value.Millisecond,
                };
                SetLocalTime(ref SysTime);
            }
        }

    }
}