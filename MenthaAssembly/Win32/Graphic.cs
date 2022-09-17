using MenthaAssembly.Media.Imaging;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    public unsafe static class Graphic
    {
        #region Windows API
        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdc">GetDC</see>
        /// <para/>
        /// The GetDC function retrieves a handle to a device context (DC) for the client area of a specified window or for the entire screen.<para/>
        /// You can use the returned handle in subsequent GDI functions to draw in the DC. <para/>
        /// The device context is an opaque data structure, whose values are used internally by GDI.
        /// </summary>
        [DllImport("User32.dll")]
        internal static extern IntPtr GetDC(IntPtr Hwnd);

        [DllImport("User32.dll")]
        internal static extern IntPtr GetDCEx(IntPtr Hwnd, IntPtr hrgnClip, DeviceContextFlags flags);

        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowdc">GetWindowDC</see>
        /// <para/>
        /// The GetWindowDC function retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars. 
        /// A window device context permits painting anywhere in a window, because the origin of the device context is the upper-left corner of the window instead of the client area.
        /// </summary>
        [DllImport("User32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr Hwnd);

        [DllImport("User32.dll")]
        internal static extern IntPtr ReleaseDC(IntPtr Hwnd, IntPtr hDC);

        /// <summary>Deletes the specified device context (DC).</summary>
        /// <param name="hDC">A handle to the device context.</param>
        /// <returns><para>If the function succeeds, the return value is nonzero.</para><para>If the function fails, the return value is zero.</para></returns>
        /// <remarks>An application must not delete a DC whose handle was obtained by calling the <c>GetDC</c> function. Instead, it must call the <c>ReleaseDC</c> function to free the DC.</remarks>
        [DllImport("Gdi32.dll")]
        internal static extern bool DeleteDC(IntPtr hDC);

        /// <summary>
        /// Windows API : <see href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-getobject">GetObject</see><para/>
        /// If the function succeeds, and lpvObject is a valid pointer, the return value is the number of bytes stored into the buffer.<para/>
        /// If the function succeeds, and lpvObject is <see cref="IntPtr.Zero"/>, the return value is the number of bytes required to hold the information the function would store into the buffer.<para/>
        /// If the function fails, the return value is zero.
        /// </summary>
        /// <param name="hObject">A handle to the graphics object of interest.<para/>
        /// This can be a handle to one of the following: a logical bitmap, a brush, a font, a palette, a pen, or a device independent bitmap created by calling the CreateDIBSection function.</param>
        /// <param name="cbBuffer">The number of bytes of information to be written to the buffer.</param>
        /// <param name="lpvObject"></param>
        /// <returns></returns>
        [DllImport("Gdi32.dll")]
        internal static extern int GetObject(IntPtr hObject, int cbBuffer, IntPtr lpvObject);

        [DllImport("Gdi32.dll")]
        internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("Gdi32.dll")]
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
        [DllImport("Gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("Gdi32.dll")]
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
        [DllImport("Gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hDCDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hDCSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        #endregion

        #region Windows API (Bitmap)
        /// <summary>
        /// Retrieves the bits of the specified compatible bitmap and copies them into a buffer as a DIB using the specified format.
        /// </summary>
        /// <param name="hDC">A handle to the device context.</param>
        /// <param name="hbmp">A handle to the bitmap. This must be a compatible bitmap (DDB).</param>
        /// <param name="uStartScan">The first scan line to retrieve.</param>
        /// <param name="cScanLines">The number of scan lines to retrieve.</param>
        /// <param name="lpvBits">A pointer to a buffer to receive the bitmap data. If this parameter is <see cref="IntPtr.Zero"/>, the function passes the dimensions and format of the bitmap to the <see cref="BITMAPINFO"/> structure pointed to by the <paramref name="lpbi"/> parameter.</param>
        /// <param name="lpbi">A pointer to a <see cref="BitmapInfoHeader"/> structure that specifies the desired format for the DIB data.</param>
        /// <param name="uUsage">The format of the bmiColors member of the <see cref="BitmapInfoHeader"/> structure. It must be one of the following values.</param>
        /// <returns>If the lpvBits parameter is non-NULL and the function succeeds, the return value is the number of scan lines copied from the bitmap.<para/>
        /// If the lpvBits parameter is NULL and GetDIBits successfully fills the <see cref="BitmapInfoHeader"/> structure, the return value is nonzero.<para/>
        /// If the function fails, the return value is zero.<para/>
        /// This function can return the following value: ERROR_INVALID_PARAMETER (87 (0×57))</returns>
        [DllImport("Gdi32.dll")]
        internal static extern int GetDIBits(IntPtr hDC, IntPtr hbmp, int uStartScan, int cScanLines, byte* lpvBits, BitmapInfoHeader* lpbi, DIBColorMode uUsage);

        [DllImport("Gdi32.dll")]
        internal static extern int GetDIBits(IntPtr hDC, IntPtr hbmp, int uStartScan, int cScanLines, byte* lpvBits, IntPtr lpbi, DIBColorMode uUsage);

        [DllImport("Gdi32.dll")]
        internal static extern IntPtr CreateBitmap(int Width, int Height, int cPlanes, int cBitsPerPel, IntPtr lpvBits);

        #endregion

        #region Windows API (Icon)
        [DllImport("User32.dll")]
        internal static extern IntPtr CreateIconIndirect(ref IconInfo piconinfo);

        [DllImport("User32.dll")]
        internal static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("User32.dll")]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("User32.dll")]
        internal static extern bool GetIconInfo(IntPtr hIcon, out IconInfo pIconInfo);

        #endregion

        #region Windows API (Font)
        [DllImport("Gdi32.dll", EntryPoint = "GetGlyphOutlineW")]
        internal static extern int GetGlyphOutline(IntPtr hdc, uint uChar, GetGlyphOutlineFormats uFormat, out GlyphMetrics lpgm, int cbBuffer, byte* lpvBuffer, Mat2* lpmat2);

        #endregion

        /// <summary>
        /// Create a bitmap.<para/>
        /// When you no longer need the bitmap, call the <see cref="DeleteObject(IntPtr)"/> function to delete it.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is a handle to a bitmap.<para/>
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.
        /// </returns>
        public static IntPtr CreateHBitmap(this IImageContext This)
            => CreateBitmap(This.Width, This.Height, 1, This.BitsPerPixel, This.Scan0);

        public static bool TryDecodeHBitmap(IntPtr HBitmap, out IImageContext Bitmap)
        {
            Bitmap = null;
            Bitmap Data = new Bitmap();
            Bitmap* pData = &Data;

            if (GetObject(HBitmap, sizeof(Bitmap), (IntPtr)pData) == 0)
                return false;

            if (Data.bmBits != IntPtr.Zero)
            {
                switch (Data.bmBitsPixel)
                {
                    // TODO:
                    // GetPalette
                    // https://stackoverflow.com/questions/46562369/winapi-gdi-how-to-use-getdibits-to-get-color-table-synthesized-for-a-bitmap
                    case 1:
                        {
                            ImagePalette<BGR> Palettes = new ImagePalette<BGR>(Data.bmBitsPixel);
                            int ColorStep = byte.MaxValue / ((1 << Data.bmBitsPixel) - 1);
                            for (int i = 0; i < 256; i += ColorStep)
                            {
                                byte Value = (byte)i;
                                Palettes.Datas.Add(new BGR(Value, Value, Value));
                            }

                            Bitmap = new ImageContext<BGR, Indexed1>(Data.bmWidth, Data.bmHeight, Data.bmBits, Data.bmWidthBytes, Palettes);
                            return true;
                        }
                    case 4:
                        {
                            ImagePalette<BGR> Palettes = new ImagePalette<BGR>(Data.bmBitsPixel);
                            int ColorStep = byte.MaxValue / ((1 << Data.bmBitsPixel) - 1);
                            for (int i = 0; i < 256; i += ColorStep)
                            {
                                byte Value = (byte)i;
                                Palettes.Datas.Add(new BGR(Value, Value, Value));
                            }

                            Bitmap = new ImageContext<BGR, Indexed4>(Data.bmWidth, Data.bmHeight, Data.bmBits, Data.bmWidthBytes, Palettes);
                            return true;
                        }
                    case 8:
                        {
                            Bitmap = new ImageContext<Gray8>(Data.bmWidth, Data.bmHeight, Data.bmBits, Data.bmWidthBytes);
                            return true;
                        }
                    case 24:
                        {
                            Bitmap = new ImageContext<BGR>(Data.bmWidth, Data.bmHeight, Data.bmBits, Data.bmWidthBytes);
                            return true;
                        }
                    case 32:
                        {
                            Bitmap = new ImageContext<BGRA>(Data.bmWidth, Data.bmHeight, Data.bmBits, Data.bmWidthBytes);
                            return true;
                        }
                }
            }

            IntPtr HDC = GetDC(IntPtr.Zero);
            BitmapInfoHeader Header = new BitmapInfoHeader
            {
                biSize = sizeof(BitmapInfoHeader),
                biWidth = Data.bmWidth,
                biHeight = -Data.bmHeight,
                biPlanes = 1,
                biBitCount = Data.bmBitsPixel,
                biCompression = BitmapCompressionMode.RGB,
                biSizeImage = 0,
                biXPelsPerMeter = 0,
                biYPelsPerMeter = 0,
                biClrUsed = 0,
                biClrImportant = 0,
            };

            try
            {
                if (GetDIBits(HDC, HBitmap, 0, 0, null, &Header, DIBColorMode.RGB_Colors) == 0)
                    return false;

                byte[] Datas = new byte[Header.biSizeImage];
                fixed (byte* pDatas = Datas)
                {
                    if (GetDIBits(HDC, HBitmap, 0, Header.biHeight, pDatas, &Header, DIBColorMode.RGB_Colors) == 0)
                        return false;

                    switch (Header.biBitCount)
                    {
                        // TODO:
                        // GetPalette
                        // https://stackoverflow.com/questions/46562369/winapi-gdi-how-to-use-getdibits-to-get-color-table-synthesized-for-a-bitmap
                        case 1:
                            {
                                ImagePalette<BGR> Palettes = new ImagePalette<BGR>(Header.biBitCount);
                                int ColorStep = byte.MaxValue / ((1 << Data.bmBitsPixel) - 1);
                                for (int i = 0; i < 256; i += ColorStep)
                                {
                                    byte Value = (byte)i;
                                    Palettes.Datas.Add(new BGR(Value, Value, Value));
                                }

                                Bitmap = new ImageContext<BGR, Indexed1>(Header.biWidth, Header.biHeight.Abs(), Datas, Palettes);
                                return true;
                            }
                        case 4:
                            {
                                ImagePalette<BGR> Palettes = new ImagePalette<BGR>(Header.biBitCount);
                                int ColorStep = byte.MaxValue / ((1 << Data.bmBitsPixel) - 1);
                                for (int i = 0; i < 256; i += ColorStep)
                                {
                                    byte Value = (byte)i;
                                    Palettes.Datas.Add(new BGR(Value, Value, Value));
                                }
                                Bitmap = new ImageContext<BGR, Indexed4>(Header.biWidth, Header.biHeight.Abs(), Datas, Palettes);
                                return true;
                            }
                        case 8:
                            {
                                Bitmap = new ImageContext<Gray8>(Header.biWidth, Header.biHeight.Abs(), Datas);
                                return true;
                            }
                        case 24:
                            {
                                Bitmap = new ImageContext<BGR>(Header.biWidth, Header.biHeight.Abs(), Datas);
                                return true;
                            }
                        case 32:
                            {
                                Bitmap = new ImageContext<BGRA>(Header.biWidth, Header.biHeight.Abs(), Datas);
                                return true;
                            }
                    }
                }
            }
            finally
            {
                DeleteDC(HDC);
            }

            return false;
        }

        /// <summary>
        /// Creates a bitmap.<para/>
        /// When you no longer need the icon, call the <see cref="DestroyIcon(IntPtr)"/> function to delete it.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the icon or cursor that is created.<para/>
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.<para/>
        /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        public static IntPtr CreateHIcon(IntPtr HBmpColor, IntPtr HBmpMask)
        {
            try
            {
                IconInfo Info = new IconInfo
                {
                    fIcon = true,
                    hbmMask = HBmpMask,
                    hbmColor = HBmpColor
                };

                return CreateIconIndirect(ref Info);
            }
            finally
            {
                DeleteObject(HBmpMask);
                DeleteObject(HBmpColor);
            }
        }
        /// <summary>
        /// Creates a bitmap.<para/>
        /// When you no longer need the icon, call the <see cref="DestroyIcon(IntPtr)"/> function to delete it.
        /// </summary>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the icon or cursor that is created.<para/>
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.<para/>
        /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        public static IntPtr CreateHIcon(IntPtr HBmpColor, IntPtr HBmpMask, int xHotSpot, int yHotSpot)
        {
            try
            {
                IconInfo Info = new IconInfo
                {
                    fIcon = false,
                    xHotspot = xHotSpot,
                    yHotspot = yHotSpot,
                    hbmMask = HBmpMask,
                    hbmColor = HBmpColor
                };

                return CreateIconIndirect(ref Info);
            }
            finally
            {
                DeleteObject(HBmpMask);
                DeleteObject(HBmpColor);
            }
        }

        public static bool TryDecodeHIcon(IntPtr HIcon, out ImageContext<BGRA> Icon)
        {
            Icon = null;
            GetIconInfo(HIcon, out IconInfo Info);

            if (TryDecodeHBitmap(Info.hbmColor, out IImageContext Color))
            {
                if (Color.PixelType.Equals(typeof(BGRA)))
                {
                    Icon = Color.Cast<BGRA>();
                    return true;
                }
            }

            //if (TryDecodeHBitmap(Info.hbmMask, out IImageContext Mask))
            //{
            //    if (TryDecodeHBitmap(Info.hbmColor, out IImageContext Color))
            //    {

            //    }
            //    else
            //    {

            //    }
            //}

            return false;
        }

        internal static ImageContour CreateTextContour(int X, int Y, string Text, FontData Font)
        {
            IntPtr pFont = System.CreateFontIndirect(Font);

            GlyphMetrics GMetric = new GlyphMetrics();
            Mat2 Matrix = new Mat2();
            Matrix.eM11.Value = 1;
            Matrix.eM12.Value = 0;
            Matrix.eM21.Value = 0;
            Matrix.eM22.Value = 1;

            IntPtr hdc = GetDC(IntPtr.Zero);
            IntPtr prev = SelectObject(hdc, pFont);

            if (!System.GetTextMetrics(hdc, out TextMetric TMetric))
                return null;

            try
            {
                ImageContour Contour = new ImageContour();

                foreach (uint c in Text.Select(i => (uint)i))
                {
                    int Length = GetGlyphOutline(hdc, c, GetGlyphOutlineFormats.Gray8_Bitmap, out GMetric, 0, null, &Matrix);
                    if (Length > 0)
                    {
                        IntPtr Buffer = Marshal.AllocHGlobal(Length);
                        try
                        {
                            byte* pBuffer = (byte*)Buffer;
                            if (GetGlyphOutline(hdc, c, GetGlyphOutlineFormats.Gray8_Bitmap, out GMetric, Length, pBuffer, &Matrix) > 0)
                            {
                                int Ox = GMetric.GlyphOriginX,
                                    Oy = TMetric.Ascent - GMetric.GlyphOriginY,
                                    Stride = Length / GMetric.BlackBoxY;

                                for (int j = 0; j < GMetric.BlackBoxY; j++)
                                {
                                    byte* pData = pBuffer + j * Stride;
                                    int Ty = Y + Oy + j,
                                        Tx = X + Ox,
                                        Dx = 0;

                                    int i = 0;
                                    while (i < GMetric.BlackBoxX)
                                    {
                                        for (; i < GMetric.BlackBoxX; i++)
                                        {
                                            if (*pData++ > 0)
                                            {
                                                Dx = i++;
                                                break;
                                            }
                                        }

                                        if (i == GMetric.BlackBoxX)
                                            break;

                                        do
                                        {
                                            if (*pData++ == 0)
                                            {
                                                Contour[Ty].Union(Tx + Dx, Tx + i);
                                                i++;
                                                break;
                                            }

                                            i++;

                                            if (i == GMetric.BlackBoxX)
                                            {
                                                Contour[Ty].Union(Tx + Dx, Tx + i);
                                                break;
                                            }

                                        } while (true);
                                    }
                                }
                            }

                            X += GMetric.CellIncX;
                            Y += GMetric.CellIncY;
                            continue;
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(Buffer);
                        }
                    }

                    double Theta = Font.Escapement * 0.1d * MathHelper.UnitTheta;
                    X += (int)(Font.Width * Math.Cos(Theta));
                    Y += (int)(Font.Width * Math.Sin(Theta));
                }

                return Contour;
            }
            finally
            {
                DeleteDC(hdc);
                DeleteObject(prev);
                DeleteObject(pFont);
            }
        }

    }
}