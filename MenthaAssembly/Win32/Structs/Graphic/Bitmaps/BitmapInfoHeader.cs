using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/previous-versions/dd183376(v=vs.85)">BitmapInfoHeader</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BitmapInfoHeader
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public BitmapCompressionMode biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BitmapV5Header
    {
        public uint bV5Size;
        public int bV5Width;
        public int bV5Height;
        public ushort bV5Planes;
        public ushort bV5BitCount;
        public BitmapCompressionMode bV5Compression;
        public uint bV5SizeImage;
        public int bV5XPelsPerMeter;
        public int bV5YPelsPerMeter;
        public uint bV5ClrUsed;
        public uint bV5ClrImportant;
        public uint bV5RedMask;
        public uint bV5GreenMask;
        public uint bV5BlueMask;
        public uint bV5AlphaMask;
        public uint bV5CSType;
        public CIEXYZTriple bV5Endpoints;
        public uint bV5GammaRed;
        public uint bV5GammaGreen;
        public uint bV5GammaBlue;
        public uint bV5Intent;
        public uint bV5ProfileData;
        public uint bV5ProfileSize;
        public uint bV5Reserved;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct CIEXYZTriple
        {
            public CIEXYZ ciexyzRed;
            public CIEXYZ ciexyzGreen;
            public CIEXYZ ciexyzBlue;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal struct CIEXYZ
            {
                public int ciexyzX;
                public int ciexyzY;
                public int ciexyzZ;
            }
        }
    }

}