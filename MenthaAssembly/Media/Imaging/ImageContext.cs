using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Primitives;
using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MenthaAssembly
{
    public class ImageContext<PixelStruct> : IImageContext
        where PixelStruct : unmanaged, IPixelBase
    {
        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public int BitsPerPixel { get; }

        public int Channels { get; }

        public Type PixelType { get; } = typeof(PixelStruct);

        public IList<BGRA> Palette { get; }

        protected bool IsPixelIndexed { get; }

        internal protected byte[] Data0;
        internal protected IntPtr? _Scan0;
        public unsafe IntPtr Scan0
        {
            get
            {
                if (_Scan0 is IntPtr Result)
                    return Result;

                fixed (byte* S0 = &this.Data0[0])
                    return (IntPtr)S0;
            }
        }

        internal protected byte[] DataA;
        internal protected IntPtr? _ScanA;
        public unsafe IntPtr ScanA
        {
            get
            {
                if (_ScanA is IntPtr Result)
                    return Result;

                fixed (byte* A = &DataA[0])
                    return (IntPtr)A;
            }
        }

        internal protected byte[] DataR;
        internal protected IntPtr? _ScanR;
        public unsafe IntPtr ScanR
        {
            get
            {
                if (_ScanR is IntPtr Result)
                    return Result;

                fixed (byte* R = &this.DataR[0])
                    return (IntPtr)R;
            }
        }

        internal protected byte[] DataG;
        internal protected IntPtr? _ScanG;
        public unsafe IntPtr ScanG
        {
            get
            {
                if (_ScanG is IntPtr Result)
                    return Result;

                fixed (byte* G = &DataG[0])
                    return (IntPtr)G;
            }
        }

        internal protected byte[] DataB;
        internal protected IntPtr? _ScanB;
        public unsafe IntPtr ScanB
        {
            get
            {
                if (_ScanB is IntPtr Result)
                    return Result;

                fixed (byte* B = &this.DataB[0])
                    return (IntPtr)B;
            }
        }

        private readonly Func<IPixel, PixelStruct> ToPixel;
        private readonly Func<int, int, PixelStruct> GetPixel;
        private readonly Action<int, int, PixelStruct> SetPixel;
        public unsafe PixelStruct this[int X, int Y]
        {
            get
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                return GetPixel(X, Y);
            }
            set
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                SetPixel(X, Y, value);
            }
        }
        unsafe IPixel IImageContext.this[int X, int Y]
        {
            get
            {
                PixelStruct Result = this[X, Y];
                if (Result is IPixelIndexed Indexed)
                    return Palette[Indexed[X * BitsPerPixel % Indexed.Length]];

                return (IPixel)Result;
            }
            set
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                if (IsPixelIndexed &&
                    GetPixel(X, Y) is IPixelIndexed Indexed)
                {
                    ARGB Color = new ARGB(value.A, value.R, value.G, value.B);
                    int Index = Palette.IndexOf(Color);
                    if (Index == -1)
                    {
                        if ((1 << Indexed.BitsPerPixel) <= Palette.Count)
                            throw new IndexOutOfRangeException("Palette is full.");

                        Index = Palette.Count;
                        Palette.Add(Color);
                    }

                    Indexed[X * BitsPerPixel % Indexed.Length] = (byte)Index;

                    PixelStruct Result = default;
                    byte* ResultPointer = (byte*)&Result;
                    *ResultPointer = Indexed.Data;
                    SetPixel(X, Y, Result);
                    return;
                }
                SetPixel(X, Y, ToPixel(value));
            }
        }

        private ImageContext()
        {
            PixelStruct PixelFormat = default;
            this.IsPixelIndexed = PixelFormat is IPixelIndexed;
            this.BitsPerPixel = PixelFormat.BitsPerPixel;

            if (PixelType == typeof(BGRA))
            {
                ToPixel = (IPixel) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        *PixelPointer++ = IPixel.B;
                        *PixelPointer++ = IPixel.G;
                        *PixelPointer++ = IPixel.R;
                        *PixelPointer = IPixel.A;
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                ToPixel = (IPixel) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        *PixelPointer++ = IPixel.A;
                        *PixelPointer++ = IPixel.R;
                        *PixelPointer++ = IPixel.G;
                        *PixelPointer = IPixel.B;
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(BGR))
            {
                ToPixel = (IPixel) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        *PixelPointer++ = IPixel.B;
                        *PixelPointer++ = IPixel.G;
                        *PixelPointer = IPixel.R;
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(RGB))
            {
                ToPixel = (IPixel) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        *PixelPointer++ = IPixel.R;
                        *PixelPointer++ = IPixel.G;
                        *PixelPointer = IPixel.B;
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                ToPixel = (IPixel) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        *PixelPointer = (byte)((IPixel.R * 30 +
                                                IPixel.G * 59 +
                                                IPixel.B * 11 + 50) / 100);
                        return Pixel;
                    }
                };
            }
            else
            {
                ToPixel = (IPixel) =>
                {
                    unsafe
                    {
                        dynamic Result = new BGRA(IPixel.B, IPixel.G, IPixel.R, IPixel.A);
                        return (PixelStruct)Result;
                    }
                };
            }
        }

        public ImageContext(int Width, int Height) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = ((Width * BitsPerPixel) + 7) >> 3;
            this.Channels = 1;

            if (IsPixelIndexed)
                Palette = new List<BGRA>();

            this.Data0 = new byte[this.Stride * Height];
            Array.Fill(Data0, byte.MaxValue);
        }

        public ImageContext(int Width, int Height, IntPtr Scan0, IList<BGRA> Palette = null) : this(Width, Height, Scan0, Width, Palette)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, IList<BGRA> Palette = null) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 1;

            this.Palette = Palette;

            this._Scan0 = Scan0;

            GetPixel = (X, Y) =>
            {
                unsafe
                {
                    long Offset = Stride * (long)Y + ((X * BitsPerPixel) >> 3);
                    return *(PixelStruct*)((byte*)Scan0 + Offset);
                }
            };
            SetPixel = (X, Y, Value) =>
            {
                unsafe
                {
                    long Offset = Stride * (long)Y + ((X * BitsPerPixel) >> 3);
                    *(PixelStruct*)((byte*)Scan0 + Offset) = Value;
                }
            };
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) :
            this(Width, Height, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 3;

            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;

            if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanB + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer = *((byte*)ScanR + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanB + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanR + Offset) = *PixelPointer;
                    }
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanR + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer = *((byte*)ScanB + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanR + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanB + Offset) = *PixelPointer;
                    }
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer = (byte)((*((byte*)ScanR + Offset) * 30 +
                                                *((byte*)ScanG + Offset) * 59 +
                                                *((byte*)ScanB + Offset) * 11 + 50) / 100);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte Gray = *(byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanR + Offset) = Gray;
                        *((byte*)ScanG + Offset) = Gray;
                        *((byte*)ScanB + Offset) = Gray;
                    }
                };
            }
            else if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanB + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer++ = *((byte*)ScanR + Offset);
                        *PixelPointer = byte.MaxValue;
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanB + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanR + Offset) = *PixelPointer;
                    }
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = byte.MaxValue;
                        *PixelPointer++ = *((byte*)ScanR + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer = *((byte*)ScanB + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        PixelPointer++;
                        *((byte*)ScanR + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanB + Offset) = *PixelPointer;
                    }
                };
            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        long Offset = Stride * (long)Y + X;
                        dynamic Result = new BGRA(*((byte*)ScanB + Offset),
                                                  *((byte*)ScanG + Offset),
                                                  *((byte*)ScanR + Offset),
                                                  byte.MaxValue);
                        return (PixelStruct)Result;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        IPixel Pixel = (IPixel)Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanR + Offset) = Pixel.R;
                        *((byte*)ScanG + Offset) = Pixel.G;
                        *((byte*)ScanB + Offset) = Pixel.B;
                    }
                };
            }

        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) :
            this(Width, Height, ScanA, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 4;

            this._ScanA = ScanA;
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;

            if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanB + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer++ = *((byte*)ScanR + Offset);
                        *PixelPointer = *((byte*)ScanA + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanB + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanR + Offset) = *PixelPointer++;
                        *((byte*)ScanA + Offset) = *PixelPointer;
                    }
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanA + Offset);
                        *PixelPointer++ = *((byte*)ScanR + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer = *((byte*)ScanB + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanA + Offset) = *PixelPointer++;
                        *((byte*)ScanR + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanB + Offset) = *PixelPointer;
                    }
                };
            }
            else if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanB + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer = *((byte*)ScanR + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanB + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanR + Offset) = *PixelPointer;
                        *((byte*)ScanA + Offset) = byte.MaxValue;
                    }
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer++ = *((byte*)ScanR + Offset);
                        *PixelPointer++ = *((byte*)ScanG + Offset);
                        *PixelPointer = *((byte*)ScanB + Offset);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte* PixelPointer = (byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanA + Offset) = byte.MaxValue;
                        *((byte*)ScanR + Offset) = *PixelPointer++;
                        *((byte*)ScanG + Offset) = *PixelPointer++;
                        *((byte*)ScanB + Offset) = *PixelPointer;
                    }
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        long Offset = Stride * (long)Y + X;
                        *PixelPointer = (byte)((*((byte*)ScanR + Offset) * 30 +
                                                *((byte*)ScanG + Offset) * 59 +
                                                *((byte*)ScanB + Offset) * 11 + 50) / 100);
                        return Pixel;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        byte Gray = *(byte*)&Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanR + Offset) = Gray;
                        *((byte*)ScanG + Offset) = Gray;
                        *((byte*)ScanB + Offset) = Gray;
                        *((byte*)ScanA + Offset) = byte.MaxValue;
                    }
                };
            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        long Offset = Stride * (long)Y + X;
                        dynamic Result = new BGRA(*((byte*)ScanB + Offset),
                                                  *((byte*)ScanG + Offset),
                                                  *((byte*)ScanR + Offset),
                                                  *((byte*)ScanA + Offset));
                        return (PixelStruct)Result;
                    }
                };
                SetPixel = (X, Y, Value) =>
                {
                    unsafe
                    {
                        IPixel Pixel = (IPixel)Value;
                        long Offset = Stride * (long)Y + X;
                        *((byte*)ScanR + Offset) = Pixel.R;
                        *((byte*)ScanG + Offset) = Pixel.G;
                        *((byte*)ScanB + Offset) = Pixel.B;
                        *((byte*)ScanA + Offset) = Pixel.A;
                    }
                };
            }
        }

        public ImageContext(int Width, int Height, byte[] Data, IList<BGRA> Palette = null) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Data.Length / Width;
            this.Channels = 1;

            this.Palette = Palette;

            this.Data0 = Data;

            GetPixel = (X, Y) =>
            {
                unsafe
                {
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + ((X * BitsPerPixel) >> 3)])
                        return *(PixelStruct*)PixelPointer;
                }
            };
            SetPixel = (X, Y, Value) =>
            {
                unsafe
                {
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + ((X * BitsPerPixel) >> 3)])
                        *(PixelStruct*)PixelPointer = Value;
                }
            };
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Width;
            this.Channels = 3;

            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;

            if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataB[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer = this.DataR[Offset];
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataR[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer = this.DataB[Offset];
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer = (byte)((this.DataR[Offset] * 30 + this.DataG[Offset] * 59 + this.DataB[Offset] * 11 + 50) / 100);
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataB[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer++ = this.DataR[Offset];
                        *PixelPointer = byte.MaxValue;
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = byte.MaxValue;
                        *PixelPointer++ = this.DataR[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer = this.DataB[Offset];
                        return Pixel;
                    }
                };
            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    int Offset = Stride * Y + X;
                    dynamic Result = new BGRA(this.DataB[Offset],
                                              this.DataG[Offset],
                                              this.DataR[Offset],
                                              this.DataA?[Offset] ?? byte.MaxValue);
                    return (PixelStruct)Result;
                };
            }
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Width;
            this.Channels = 4;

            this.DataA = DataA;
            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;

            if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataB[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer++ = this.DataR[Offset];
                        *PixelPointer = this.DataA[Offset];
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataA[Offset];
                        *PixelPointer++ = this.DataR[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer = this.DataB[Offset];
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataB[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer = this.DataR[Offset];
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer++ = this.DataR[Offset];
                        *PixelPointer++ = this.DataG[Offset];
                        *PixelPointer = this.DataB[Offset];
                        return Pixel;
                    }
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    unsafe
                    {
                        PixelStruct Pixel = default;
                        byte* PixelPointer = (byte*)&Pixel;
                        int Offset = Stride * Y + X;
                        *PixelPointer = (byte)((this.DataR[Offset] * 30 + this.DataG[Offset] * 59 + this.DataB[Offset] * 11 + 50) / 100);
                        return Pixel;
                    }
                };
            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    int Offset = Stride * Y + X;
                    dynamic Result = new BGRA(this.DataB[Offset],
                                              this.DataG[Offset],
                                              this.DataR[Offset],
                                              this.DataA[Offset]);
                    return (PixelStruct)Result;
                };
            }
        }

        public unsafe void DrawLine(int X0, int Y0, int X1, int Y1, byte A, byte R, byte G, byte B, int PenWidth)
        {
            switch (BitsPerPixel)
            {
                case 1:
                case 4:
                    break;
                case 8:
                    {
                        bool IsOdd = (PenWidth & 1).Equals(1);
                        int HalfWidth = (PenWidth + 1) >> 1,
                            DeltaX = X1 - X0;
                        //Vertical Line
                        if (DeltaX is 0)
                        {
                            if (X0 < 0 || Width < X0)
                                return;

                            if (Y0 > Y1)
                                MathHelper.Swap(ref Y0, ref Y1);

                            Y0 = Math.Min(Math.Max(Y0, 0), Height - 1);
                            Y1 = Math.Min(Math.Max(Y1, 0), Height - 1);

                            int PenX0 = Math.Min(Math.Max(X0 - HalfWidth, 0), Width),
                                PenX1 = Math.Min(Math.Max(X0 + HalfWidth + (IsOdd ? 1 : 0), 0), Width),
                                PenDeltaX = PenX1 - PenX0;

                            if (PenDeltaX <= 0)
                                return;

                            byte* RectDatas = (byte*)(Scan0 + Y0 * Stride + PenX0);
                            for (int j = Y0; j <= Y1; j++)
                            {
                                for (int i = PenX0; i < PenX1; i++)
                                    *RectDatas++ = R;

                                RectDatas += Stride - PenDeltaX;
                            }
                            return;
                        }

                        int DeltaY = Y1 - Y0;
                        // Horizontal Line
                        if (DeltaY is 0)
                        {
                            if (Y0 < 0 || Width < Y0)
                                return;

                            if (X0 > X1)
                                MathHelper.Swap(ref X0, ref X1);

                            X0 = Math.Min(Math.Max(X0, 0), Width - 1);
                            X1 = Math.Min(Math.Max(X1, 0), Width - 1);


                            int PenY0 = Math.Min(Math.Max(Y0 - HalfWidth, 0), Height),
                                PenY1 = Math.Min(Math.Max(Y0 + HalfWidth + (IsOdd ? 1 : 0), 0), Height),
                                PenDeltaX = X1 - X0;

                            if (PenY1 - PenY0 <= 0)
                                return;

                            byte* RectDatas = (byte*)(Scan0 + PenY0 * Stride + X0);

                            for (int j = PenY0; j < PenY1; j++)
                            {
                                for (int i = X0; i <= X1; i++)
                                    *RectDatas++ = R;

                                RectDatas += Stride - PenDeltaX - 1;
                            }
                            return;
                        }

                        // Slash Line
                        int PenHalfWidthSquare = HalfWidth * HalfWidth,
                            AbsDeltaX = DeltaX.Abs(),
                            AbsDeltaY = DeltaY.Abs();

                        bool IsPositiveM = (DeltaX > 0 && DeltaY > 0) || (DeltaX < 0 && DeltaY < 0);
                        if (AbsDeltaX >= AbsDeltaY)
                        {
                            if (X0 > X1)
                            {
                                MathHelper.Swap(ref X0, ref X1);
                                MathHelper.Swap(ref Y0, ref Y1);
                            }

                            List<Int32Vector> WidthDeltas = new List<Int32Vector>();
                            int Temp = AbsDeltaX >> 1,
                                TempX = 0;

                            // Calculate WidthDeltas
                            for (int TempY = 0; TempY < HalfWidth; TempY++)
                            {
                                Int32Vector Delta = new Int32Vector(TempX, TempY);
                                if (Delta.LengthSquare > PenHalfWidthSquare)
                                    break;

                                WidthDeltas.Add(Delta);
                                if (!Delta.IsZero)
                                    WidthDeltas.Add(-Delta);

                                Temp -= AbsDeltaY;
                                if (Temp < 0)
                                {
                                    Temp += AbsDeltaX;
                                    TempX += IsPositiveM ? -1 : 1;
                                }
                            }

                            byte* RectDatas = (byte*)(Scan0 + Y0 * Stride + X0);
                            int Error = AbsDeltaX >> 1,
                                DeltaY2 = 0;

                            bool IsSearchFillGaps = false,
                                 DrawingSucceed = false;
                            bool[] IsFillGaps = new bool[WidthDeltas.Count];

                            // Draw
                            for (; X0 <= X1; X0++)
                            {
                                for (int i = 0; i < WidthDeltas.Count; i++)
                                {
                                    Int32Vector Delta = WidthDeltas[i];
                                    // Search ShiftFillDeltas
                                    if (!IsSearchFillGaps && DeltaY2 != 0)
                                        IsFillGaps[i] = DeltaY2 != 0 &&
                                                        WidthDeltas.FirstOrNull(j => j.Y == Delta.Y + DeltaY2 && j.X - Delta.X != 0) != null;

                                    bool FillGap = DeltaY2 != 0 && IsFillGaps[i];
                                    int X2 = X0 + Delta.X,
                                        Y2 = Y0 + Delta.Y,
                                        Offset = Delta.Y * Stride + Delta.X;
                                    if (0 <= Y2 && Y2 < Height)
                                    {
                                        if (0 <= X2 && X2 < Width)
                                        {
                                            *(RectDatas + Offset) = R;
                                            DrawingSucceed = true;
                                        }

                                        if (FillGap && 0 < X2 && X2 <= Width)
                                        {
                                            *(RectDatas + Offset - 1) = R;
                                            DrawingSucceed = true;
                                        }
                                    }
                                }

                                if (!DrawingSucceed)
                                    return;

                                // Reset
                                if (DeltaY2 != 0)
                                    IsSearchFillGaps = true;
                                DrawingSucceed = false;

                                // Calculate NextPoint
                                Error -= AbsDeltaY;
                                if (Error < 0)
                                {
                                    Error += AbsDeltaX;
                                    if (Y1 > Y0)
                                    {
                                        Y0++;
                                        DeltaY2 = 1;
                                        RectDatas += Stride + 1;
                                    }
                                    else
                                    {
                                        Y0--;
                                        DeltaY2 = -1;
                                        RectDatas += ~Stride + 2;
                                    }
                                    continue;
                                }
                                DeltaY2 = 0;
                                RectDatas++;
                            }
                        }
                        else
                        {
                            if (Y0 > Y1)
                            {
                                MathHelper.Swap(ref X0, ref X1);
                                MathHelper.Swap(ref Y0, ref Y1);
                            }

                            List<Int32Vector> WidthDeltas = new List<Int32Vector>();
                            int Temp = AbsDeltaY >> 1,
                                TempY = 0;

                            // Calculate WidthDeltas
                            for (int TempX = 0; TempX < HalfWidth; TempX++)
                            {
                                Int32Vector Delta = new Int32Vector(TempX, TempY);
                                if (Delta.LengthSquare > PenHalfWidthSquare)
                                    break;

                                WidthDeltas.Add(Delta);
                                if (!Delta.IsZero)
                                    WidthDeltas.Add(-Delta);

                                Temp -= AbsDeltaX;
                                if (Temp < 0)
                                {
                                    Temp += AbsDeltaY;
                                    TempY += IsPositiveM ? -1 : 1;
                                }
                            }

                            byte* RectDatas = (byte*)(Scan0 + Y0 * Stride + X0);
                            int Error = AbsDeltaY >> 1,
                                DeltaX2 = 0;

                            bool IsSearchFillGaps = false,
                                 DrawingSucceed = false;
                            bool[] IsFillGaps = new bool[WidthDeltas.Count];

                            // Draw
                            for (; Y0 <= Y1; Y0++)
                            {
                                for (int i = 0; i < WidthDeltas.Count; i++)
                                {
                                    Int32Vector Delta = WidthDeltas[i];
                                    // Search ShiftFillDeltas
                                    if (!IsSearchFillGaps && DeltaX2 != 0)
                                        IsFillGaps[i] = DeltaX2 != 0 &&
                                                        WidthDeltas.FirstOrNull(j => j.X == Delta.X + DeltaX2 && j.Y - Delta.Y != 0) != null;

                                    bool FillGap = DeltaX2 != 0 && IsFillGaps[i];
                                    int X2 = X0 + Delta.X,
                                        Y2 = Y0 + Delta.Y,
                                        Offset = Delta.Y * Stride + Delta.X;
                                    if (0 <= X2 && X2 < Width)
                                    {
                                        if (0 <= Y2 && Y2 < Height)
                                        {
                                            *(RectDatas + Offset) = R;
                                            DrawingSucceed = true;
                                        }

                                        if (FillGap && 0 < Y2 && Y2 <= Height)
                                        {
                                            *(RectDatas + Offset - Stride) = R;
                                            DrawingSucceed = true;
                                        }
                                    }
                                }

                                if (!DrawingSucceed)
                                    return;

                                // Reset
                                if (DeltaX2 != 0)
                                    IsSearchFillGaps = true;
                                DrawingSucceed = false;

                                // Calculate NextPoint
                                Error -= AbsDeltaX;
                                if (Error < 0)
                                {
                                    Error += AbsDeltaY;
                                    if (X1 > X0)
                                    {
                                        X0++;
                                        DeltaX2 = 1;
                                        RectDatas += Stride + 1;
                                    }
                                    else
                                    {
                                        X0--;
                                        DeltaX2 = -1;
                                        RectDatas += Stride - 1;
                                    }
                                    continue;
                                }
                                DeltaX2 = 0;
                                RectDatas += Stride;
                            }
                        }
                        break;
                    }
                case 24:
                case 32:
                default:
                    break;
            }
        }

        public unsafe void DrawLine(double X0, double Y0, double X1, double Y1, IPixel Color, double PenWidth)
        {
            int IntX0 = (int)(X0 * 128),
                IntY0 = (int)(Y0 * 128),
                IntX1 = (int)(X1 * 128),
                IntY1 = (int)(Y1 * 128),
                IntHalfPen = (int)(PenWidth * 64);

            if (IntHalfPen == 0 ||
                (IntX0 == IntX1 && IntY0 == IntY1))
                return;

            if (IntY0 > IntY1)
            {
                MathHelper.Swap(ref IntX0, ref IntX1);
                MathHelper.Swap(ref IntY0, ref IntY1);
            }

            int DeltaX = IntX1 - IntX0,
                AbsDeltaX = DeltaX.Abs(),
                AbsDeltaY = IntY1 - IntY0;

            Dictionary<int, int> Bounds = new Dictionary<int, int>();

            #region Define FillScan
            Action<int, int, int> FillScan;
            switch (Channels)
            {
                case 1:
#pragma warning disable IDE0059 // 指派了不必要的值
                    PixelStruct Pixel = ToPixel(Color);
#pragma warning restore IDE0059 // 指派了不必要的值
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.Scan0, X0, X1, Y, Pixel);
                    break;
                case 3:
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.ScanR, (byte*)this.ScanG, (byte*)this.ScanB, X0, X1, Y, Color);
                    break;
                case 4:
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.ScanA, (byte*)this.ScanR, (byte*)this.ScanG, (byte*)this.ScanB, X0, X1, Y, Color);
                    break;
                default:
                    throw new NotImplementedException();
            }

            #endregion

            #region Vertical Line
            if (DeltaX == 0)
            {
                IntY0 = Math.Max(IntY0 >> 7, 0);
                IntY1 = Math.Min(IntY1 >> 7, this.Height);

                for (; IntY0 <= IntY1; IntY0++)
                    FillScan((IntX0 - IntHalfPen) >> 7, (IntX0 + IntHalfPen) >> 7, IntY0);
                return;
            }
            #endregion
            #region Horizontal Line
            if (DeltaX == 0)
            {
                IntY0 = Math.Max((IntY0 - IntHalfPen) >> 7, 0);
                IntY1 = Math.Min((IntY0 + IntHalfPen) >> 7, this.Height);

                for (; IntY0 <= IntY1; IntY0++)
                    FillScan(IntX0 >> 7, IntX1 >> 7, IntY0);
                return;
            }
            #endregion

            // Define Push
            void Push(int X, int Y)
            {
                if (Bounds.TryGetValue(Y, out int X1))
                {
                    Bounds.Remove(Y);

                    // Convert to real value
                    X >>= 7;
                    Y >>= 7;
                    X1 >>= 7;

                    // Fill
                    FillScan(X, X1, Y);
                }
                else
                    Bounds.Add(Y, X);
            }

            #region Define WidthEdgeHandler
            Action<Int32Vector, Int32Vector> WidthEdgeHandler;
            Action<Int32Point, Int32Point, Int32Vector> LineEdgeHandler;

            if (AbsDeltaX < AbsDeltaY)
            {
                if (DeltaX > 0)
                {
                    WidthEdgeHandler = (LastDelta, Delta) =>
                    {
                        Push(IntX0 + Delta.X, IntY0 + Delta.Y);
                        Push(IntX1 - Delta.X, IntY1 - Delta.Y);
                        Push(IntX0 - LastDelta.X, IntY0 - LastDelta.Y);
                        Push(IntX1 + LastDelta.X, IntY1 + LastDelta.Y);
                    };
                }
                else
                {
                    WidthEdgeHandler = (LastDelta, Delta) =>
                    {
                        Push(IntX0 + LastDelta.X, IntY0 + LastDelta.Y);
                        Push(IntX1 - LastDelta.X, IntY1 - LastDelta.Y);
                        Push(IntX0 - Delta.X, IntY0 - Delta.Y);
                        Push(IntX1 + Delta.X, IntY1 + Delta.Y);
                    };
                }

                LineEdgeHandler = (LastPoint, Point, Delta) =>
                {
                    Push(Point.X + Delta.X, Point.Y + Delta.Y);
                    Push(Point.X - Delta.X, Point.Y - Delta.Y);
                };
            }
            else
            {
                WidthEdgeHandler = (LastDelta, Delta) =>
                {
                    Push(IntX0 + Delta.X, IntY0 + Delta.Y);
                    Push(IntX0 - Delta.X, IntY0 - Delta.Y);
                    Push(IntX1 + Delta.X, IntY1 + Delta.Y);
                    Push(IntX1 - Delta.X, IntY1 - Delta.Y);
                };

                Push(IntX0, IntY0);
                Push(IntX1, IntY1);

                if (DeltaX > 0)
                {
                    LineEdgeHandler = (LastPoint, Point, Delta) =>
                    {
                        Push(LastPoint.X + Delta.X, LastPoint.Y + Delta.Y);
                        Push(Point.X - Delta.X, Point.Y - Delta.Y);
                    };
                }
                else
                {
                    LineEdgeHandler = (LastPoint, Point, Delta) =>
                    {
                        Push(Point.X + Delta.X, Point.Y + Delta.Y);
                        Push(LastPoint.X - Delta.X, LastPoint.Y - Delta.Y);
                    };
                }
            }

            #endregion

            Int32Vector WidthDelta = default;
            int IntPenSquare = IntHalfPen * IntHalfPen;
            // Calculate PenWidth Delta
            foreach (Int32Vector d in LineDrawer.LoopNextWidthDelta(AbsDeltaY, -DeltaX, AbsDeltaX, 128))
            {
                if (d.LengthSquare > IntPenSquare)
                    break;

                if (d.Y != WidthDelta.Y)
                    WidthEdgeHandler(WidthDelta, d);

                WidthDelta = d;
            }

            // Draw
            Int32Point LastPoint = new Int32Point(IntX0, IntY0);
            foreach (Int32Point p in MathHelper.LinePoints(IntX0, IntY0, IntX1, IntY1, 128, DeltaX, AbsDeltaX, AbsDeltaY))
            {
                if (LastPoint.Y != p.Y)
                    LineEdgeHandler(LastPoint, p, WidthDelta);

                LastPoint = p;
            }
        }

        public unsafe void DrawLine2(double X0, double Y0, double X1, double Y1, IPixel Color, double PenWidth)
        {
            int IntX0 = (int)(X0 * 128),
                IntY0 = (int)(Y0 * 128),
                IntX1 = (int)(X1 * 128),
                IntY1 = (int)(Y1 * 128),
                IntHalfPen = (int)(PenWidth * 64);

            if (IntHalfPen == 0 ||
                (IntX0 == IntX1 && IntY0 == IntY1))
                return;

            if (IntY0 > IntY1)
            {
                MathHelper.Swap(ref IntX0, ref IntX1);
                MathHelper.Swap(ref IntY0, ref IntY1);
            }

            int DeltaX = IntX1 - IntX0,
                AbsDeltaX = DeltaX.Abs(),
                AbsDeltaY = IntY1 - IntY0;

            Dictionary<int, int> Bounds = new Dictionary<int, int>();

            #region Define FillScan
            Action<int, int, int> FillScan;
            switch (Channels)
            {
                case 1:
#pragma warning disable IDE0059 // 指派了不必要的值
                    PixelStruct Pixel = ToPixel(Color);
#pragma warning restore IDE0059 // 指派了不必要的值
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.Scan0, X0, X1, Y, Pixel);
                    break;
                case 3:
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.ScanR, (byte*)this.ScanG, (byte*)this.ScanB, X0, X1, Y, Color);
                    break;
                case 4:
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.ScanA, (byte*)this.ScanR, (byte*)this.ScanG, (byte*)this.ScanB, X0, X1, Y, Color);
                    break;
                default:
                    throw new NotImplementedException();
            }

            #endregion

            #region Vertical Line
            if (DeltaX == 0)
            {
                IntY0 = Math.Max(IntY0 >> 7, 0);
                IntY1 = Math.Min(IntY1 >> 7, this.Height);

                for (; IntY0 <= IntY1; IntY0++)
                    FillScan((IntX0 - IntHalfPen) >> 7, (IntX0 + IntHalfPen) >> 7, IntY0);
                return;
            }
            #endregion
            #region Horizontal Line
            if (DeltaX == 0)
            {
                IntY0 = Math.Max((IntY0 - IntHalfPen) >> 7, 0);
                IntY1 = Math.Min((IntY0 + IntHalfPen) >> 7, this.Height);

                for (; IntY0 <= IntY1; IntY0++)
                    FillScan(IntX0 >> 7, IntX1 >> 7, IntY0);
                return;
            }
            #endregion

            // Define Push
            void Push(int X, int Y)
            {
                if (Bounds.TryGetValue(Y, out int X1))
                {
                    Bounds.Remove(Y);

                    // Convert to real value
                    X >>= 7;
                    Y >>= 7;
                    X1 >>= 7;

                    // Fill
                    FillScan(X, X1, Y);
                }
                else
                    Bounds.Add(Y, X);
            }

            #region Define WidthEdgeHandler
            Action<Int32Vector, Int32Vector> WidthEdgeHandler;
            Action<Int32Point, Int32Point, Int32Vector> LineEdgeHandler;

            if (AbsDeltaX < AbsDeltaY)
            {
                if (DeltaX > 0)
                {
                    WidthEdgeHandler = (LastDelta, Delta) =>
                    {
                        Push(IntX0 + Delta.X, IntY0 + Delta.Y);
                        Push(IntX1 - Delta.X, IntY1 - Delta.Y);
                        Push(IntX0 - LastDelta.X, IntY0 - LastDelta.Y);
                        Push(IntX1 + LastDelta.X, IntY1 + LastDelta.Y);
                    };
                }
                else
                {
                    WidthEdgeHandler = (LastDelta, Delta) =>
                    {
                        Push(IntX0 + LastDelta.X, IntY0 + LastDelta.Y);
                        Push(IntX1 - LastDelta.X, IntY1 - LastDelta.Y);
                        Push(IntX0 - Delta.X, IntY0 - Delta.Y);
                        Push(IntX1 + Delta.X, IntY1 + Delta.Y);
                    };
                }

                LineEdgeHandler = (LastPoint, Point, Delta) =>
                {
                    Push(Point.X + Delta.X, Point.Y + Delta.Y);
                    Push(Point.X - Delta.X, Point.Y - Delta.Y);
                };
            }
            else
            {
                WidthEdgeHandler = (LastDelta, Delta) =>
                {
                    Push(IntX0 + Delta.X, IntY0 + Delta.Y);
                    Push(IntX0 - Delta.X, IntY0 - Delta.Y);
                    Push(IntX1 + Delta.X, IntY1 + Delta.Y);
                    Push(IntX1 - Delta.X, IntY1 - Delta.Y);
                };

                Push(IntX0, IntY0);
                Push(IntX1, IntY1);

                if (DeltaX > 0)
                {
                    LineEdgeHandler = (LastPoint, Point, Delta) =>
                    {
                        Push(LastPoint.X + Delta.X, LastPoint.Y + Delta.Y);
                        Push(Point.X - Delta.X, Point.Y - Delta.Y);
                    };
                }
                else
                {
                    LineEdgeHandler = (LastPoint, Point, Delta) =>
                    {
                        Push(Point.X + Delta.X, Point.Y + Delta.Y);
                        Push(LastPoint.X - Delta.X, LastPoint.Y - Delta.Y);
                    };
                }
            }

            #endregion

            Int32Vector WidthDelta = default;
            int IntPenSquare = IntHalfPen * IntHalfPen;
            // Calculate PenWidth Delta
            foreach (Int32Vector d in LineDrawer.LoopNextWidthDelta(AbsDeltaY, -DeltaX, AbsDeltaX, 128))
            {
                if (d.LengthSquare > IntPenSquare)
                    break;

                if (d.Y != WidthDelta.Y)
                    WidthEdgeHandler(WidthDelta, d);

                WidthDelta = d;
            }

            // Draw
            Int32Point LastPoint = new Int32Point(IntX0, IntY0);
            foreach (Int32Point p in MathHelper.LinePoints(IntX0, IntY0, IntX1, IntY1, 128, DeltaX, AbsDeltaX, AbsDeltaY))
            {
                if (LastPoint.Y != p.Y)
                    LineEdgeHandler(LastPoint, p, WidthDelta);

                LastPoint = p;
            }
        }

        private unsafe void FillScan(byte* Scan0, int X0, int X1, int Y, PixelStruct Pixel)
        {
            if (X0 > X1)
                MathHelper.Swap(ref X0, ref X1);

            X0 = Math.Max(0, X0);
            X1 = Math.Min(this.Width, X1);

            long Offset = (long)Y * Stride + (X0 * BitsPerPixel >> 3);
            PixelStruct* Scan = (PixelStruct*)(Scan0 + Offset);

            // Draw
            for (; X0 <= X1; X0++)
                *Scan++ = Pixel;
        }
        private unsafe void FillScan(byte* ScanR0, byte* ScanG0, byte* ScanB0, int X0, int X1, int Y, IPixel Pixel)
        {
            if (X0 > X1)
                MathHelper.Swap(ref X0, ref X1);

            X0 = Math.Max(0, X0);
            X1 = Math.Min(this.Width, X1);

            long Offset = (long)Y * Stride + (X0 * BitsPerPixel >> 3);
            byte* ScanR = ScanR0 + Offset,
                  ScanG = ScanG0 + Offset,
                  ScanB = ScanB0 + Offset;

            // Draw
            for (; X0 <= X1; X0++)
            {
                *ScanR++ = Pixel.R;
                *ScanG++ = Pixel.G;
                *ScanB++ = Pixel.B;
            }
        }
        private unsafe void FillScan(byte* ScanA0, byte* ScanR0, byte* ScanG0, byte* ScanB0, int X0, int X1, int Y, IPixel Pixel)
        {
            if (X0 > X1)
                MathHelper.Swap(ref X0, ref X1);


            X0 = Math.Max(0, X0);
            X1 = Math.Min(this.Width, X1);

            long Offset = (long)Y * Stride + (X0 * BitsPerPixel >> 3);
            byte* ScanA = ScanA0 + Offset,
                  ScanR = ScanR0 + Offset,
                  ScanG = ScanG0 + Offset,
                  ScanB = ScanB0 + Offset;

            // Draw
            for (; X0 <= X1; X0++)
            {
                *ScanA++ = Pixel.A;
                *ScanR++ = Pixel.R;
                *ScanG++ = Pixel.G;
                *ScanB++ = Pixel.B;
            }
        }

    }


}
