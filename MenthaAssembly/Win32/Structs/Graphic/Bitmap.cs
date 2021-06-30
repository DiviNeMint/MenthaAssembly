using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmap">Bitmap</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct Bitmap
    {
       public int bmType;
       public int bmWidth;
       public int bmHeight;
       public int bmWidthBytes;
       public ushort bmPlanes;
       public ushort bmBitsPixel;
       public IntPtr bmBits;
    }
}
