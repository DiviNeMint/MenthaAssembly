using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Win32
{
    public static class Graphic
    {
        #region Windows API
        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdc">GetDC</see>
        /// <para/>
        /// The GetDC function retrieves a handle to a device context (DC) for the client area of a specified window or for the entire screen. 
        /// You can use the returned handle in subsequent GDI functions to draw in the DC. 
        /// The device context is an opaque data structure, whose values are used internally by GDI.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr Hwnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDCEx(IntPtr Hwnd, IntPtr hrgnClip, DeviceContextFlags flags);

        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowdc">GetWindowDC</see>
        /// <para/>
        /// The GetWindowDC function retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars. 
        /// A window device context permits painting anywhere in a window, because the origin of the device context is the upper-left corner of the window instead of the client area.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr Hwnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr ReleaseDC(IntPtr Hwnd, IntPtr hDC);

        /// <summary>Deletes the specified device context (DC).</summary>
        /// <param name="hDC">A handle to the device context.</param>
        /// <returns><para>If the function succeeds, the return value is nonzero.</para><para>If the function fails, the return value is zero.</para></returns>
        /// <remarks>An application must not delete a DC whose handle was obtained by calling the <c>GetDC</c> function. Instead, it must call the <c>ReleaseDC</c> function to free the DC.</remarks>
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Creates a memory device context (DC) compatible with the specified device.
        /// </summary>
        /// <param name="hDC">
        /// A handle to an existing DC. If this handle is NULL,
        /// the function creates a memory DC compatible with the application's current screen.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the handle to a memory DC.
        /// <para/>
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        /// </returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int Width, int Height);

        /// <summary>
        /// Performs a bit-block transfer of the color data corresponding to a
        /// rectangle of pixels from the specified source device context into
        /// a destination device context.
        /// </summary>
        /// <param name="hDCDest">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hDCSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        [DllImport("gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hDCDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hDCSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        /// <summary>
        /// Retrieves the bits of the specified compatible bitmap and copies them into a buffer as a DIB using the specified format.
        /// </summary>
        /// <param name="hDC">A handle to the device context.</param>
        /// <param name="hbmp">A handle to the bitmap. This must be a compatible bitmap (DDB).</param>
        /// <param name="uStartScan">The first scan line to retrieve.</param>
        /// <param name="cScanLines">The number of scan lines to retrieve.</param>
        /// <param name="lpvBits">A pointer to a buffer to receive the bitmap data. If this parameter is <see cref="IntPtr.Zero"/>, the function passes the dimensions and format of the bitmap to the <see cref="BITMAPINFO"/> structure pointed to by the <paramref name="lpbi"/> parameter.</param>
        /// <param name="lpbi">A pointer to a <see cref="BITMAPINFO"/> structure that specifies the desired format for the DIB data.</param>
        /// <param name="uUsage">The format of the bmiColors member of the <see cref="BITMAPINFO"/> structure. It must be one of the following values.</param>
        /// <returns>If the lpvBits parameter is non-NULL and the function succeeds, the return value is the number of scan lines copied from the bitmap.
        /// If the lpvBits parameter is NULL and GetDIBits successfully fills the <see cref="BITMAPINFO"/> structure, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// This function can return the following value: ERROR_INVALID_PARAMETER (87 (0×57))</returns>
        [DllImport("gdi32.dll", EntryPoint = "GetDIBits")]
        internal unsafe static extern int GetDIBits(IntPtr hDC, IntPtr hbmp, int uStartScan, int cScanLines, byte* lpvBits, BitmapInfoHeader* lpbi, DIBColorMode uUsage);


        #endregion
    }
}
