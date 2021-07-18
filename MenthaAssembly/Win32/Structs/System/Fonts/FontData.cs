using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-logfonta">LogFont</see>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FontData
    {
        public int Height { set; get; }

        public int Width { set; get; }

        public int Escapement { set; get; }

        public int Orientation { set; get; }

        public FontWeightType Weight { set; get; }

        private byte _Italic;
        public bool Italic
        {
            get => _Italic == 1;
            set => _Italic = value ? (byte)1 : (byte)0;
        }

        private byte _Underline;
        public bool Underline
        {
            get => _Underline == 1;
            set => _Underline = value ? (byte)1 : (byte)0;
        }

        private byte _StrikeOut;
        public bool StrikeOut
        {
            get => _StrikeOut == 1;
            set => _StrikeOut = value ? (byte)1 : (byte)0;
        }

        public FontCharSet CharSet { set; get; }

        public FontOutPrecision OutPrecision { set; get; }

        public FontClipPrecision ClipPrecision { set; get; }

        public FontQuality Quality { set; get; }

        public FontPitchAndFamily PitchAndFamily { set; get; }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        private string _FaceName;
        public string FaceName
        {
            get => _FaceName;
            set => _FaceName = value;
        }

    }
}
