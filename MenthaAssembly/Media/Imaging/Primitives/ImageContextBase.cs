using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public unsafe abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public int Stride { get; protected set; }

        public int BitsPerPixel { get; }

        public int Channels { get; protected set; }

        public Type PixelType { get; } = typeof(Pixel);

        public Type StructType { get; } = typeof(Struct);

        protected bool IsStructIndexed { get; }

        internal readonly Func<byte, byte, byte, byte, Pixel> ToPixel;
        internal Func<int, int, Pixel> GetPixel;
        internal unsafe Action<int, int, Pixel> SetPixel;
        public unsafe Pixel this[int X, int Y]
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
            get => this[X, Y];
            set => this[X, Y] = ToPixel(value.A, value.R, value.G, value.B);
        }

        internal protected byte[] Data0;
        internal protected IntPtr? _Scan0;
        public unsafe IntPtr Scan0
        {
            get
            {
                if (_Scan0.HasValue)
                    return _Scan0.Value;

                fixed (byte* S0 = &this.Data0[0])
                    return (IntPtr)S0;
            }
        }

        internal protected byte[] DataA;
        internal protected IntPtr? _ScanA;
        protected unsafe virtual IntPtr ScanA
        {
            get
            {
                if (_ScanA.HasValue)
                    return _ScanA.Value;

                fixed (byte* A = &DataA[0])
                    return (IntPtr)A;
            }
        }
        IntPtr IImageContext.ScanA => this.ScanA;

        internal protected byte[] DataR;
        internal protected IntPtr? _ScanR;
        protected unsafe virtual IntPtr ScanR
        {
            get
            {
                if (_ScanR.HasValue)
                    return _ScanR.Value;

                fixed (byte* R = &this.DataR[0])
                    return (IntPtr)R;
            }
        }
        IntPtr IImageContext.ScanR => this.ScanR;

        internal protected byte[] DataG;
        internal protected IntPtr? _ScanG;
        protected unsafe virtual IntPtr ScanG
        {
            get
            {
                if (_ScanG.HasValue)
                    return _ScanG.Value;

                fixed (byte* G = &DataG[0])
                    return (IntPtr)G;
            }
        }
        IntPtr IImageContext.ScanG => this.ScanG;

        internal protected byte[] DataB;
        internal protected IntPtr? _ScanB;
        protected unsafe virtual IntPtr ScanB
        {
            get
            {
                if (_ScanB.HasValue)
                    return _ScanB.Value;

                fixed (byte* B = &this.DataB[0])
                    return (IntPtr)B;
            }
        }
        IntPtr IImageContext.ScanB => this.ScanB;

        public IList<Pixel> Palette { get; }

        IList<IPixel> IImageContext.Palette
            => this.Palette.Cast<IPixel>().ToList();

        internal unsafe ImageContextBase()
        {
            Struct StructFormat = default;
            this.IsStructIndexed = StructFormat is IPixelIndexed;
            this.BitsPerPixel = StructFormat.BitsPerPixel;

            this.CopyPixelHandler = CreateCopyPixelHandler<Pixel>();

            ToPixel = (A, R, G, B) =>
            {
                Pixel Pixel = default;
                byte* pPixel = (byte*)&Pixel;
                this.CopyPixelHandler(ref pPixel, A, R, G, B);
                return Pixel;
            };

            //if (PixelType == typeof(BGRA))
            //{
            //    ToPixel = (A, R, G, B) =>
            //    {
            //        Pixel Pixel = default;
            //        byte* PixelPointer = (byte*)&Pixel;
            //        *PixelPointer++ = B;
            //        *PixelPointer++ = G;
            //        *PixelPointer++ = R;
            //        *PixelPointer = A;
            //        return Pixel;
            //    };
            //}
            //else if (PixelType == typeof(ARGB))
            //{
            //    ToPixel = (A, R, G, B) =>
            //    {
            //        Pixel Pixel = default;
            //        byte* PixelPointer = (byte*)&Pixel;
            //        *PixelPointer++ = A;
            //        *PixelPointer++ = R;
            //        *PixelPointer++ = G;
            //        *PixelPointer = B;
            //        return Pixel;
            //    };
            //}
            //else if (PixelType == typeof(BGR))
            //{
            //    ToPixel = (A, R, G, B) =>
            //    {
            //        Pixel Pixel = default;
            //        byte* PixelPointer = (byte*)&Pixel;
            //        *PixelPointer++ = B;
            //        *PixelPointer++ = G;
            //        *PixelPointer = R;
            //        return Pixel;
            //    };
            //}
            //else if (PixelType == typeof(RGB))
            //{
            //    ToPixel = (A, R, G, B) =>
            //    {
            //        Pixel Pixel = default;
            //        byte* PixelPointer = (byte*)&Pixel;
            //        *PixelPointer++ = R;
            //        *PixelPointer++ = G;
            //        *PixelPointer = B;
            //        return Pixel;
            //    };
            //}
            //else if (PixelType == typeof(Gray8))
            //{
            //    ToPixel = (A, R, G, B) =>
            //    {
            //        Pixel Pixel = default;
            //        byte* PixelPointer = (byte*)&Pixel;
            //        *PixelPointer = (byte)((R * 30 +
            //                                G * 59 +
            //                                B * 11 + 50) / 100);
            //        return Pixel;
            //    };
            //}
            //else
            //{
            //    ToPixel = (A, R, G, B) =>
            //    {
            //        dynamic Result = new BGRA(B, G, R, A);
            //        return (Pixel)Result;
            //    };
            //}
        }

        internal unsafe ImageContextBase(int Width, int Height) : this(Width, Height, new byte[Width * sizeof(Struct) * Height], null)
        {
        }

        internal unsafe ImageContextBase(int Width, int Height, IntPtr Scan0, int Stride, IList<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 1;

            this.Palette = Palette ?? new List<Pixel>();

            this._Scan0 = Scan0;

            #region Define Functions
            if (IsStructIndexed)
            {
                GetPixel = (X, Y) =>
                {
                    int XBits = X * BitsPerPixel;
                    long Offset = Stride * (long)Y + (XBits >> 3);

                    IPixelIndexed Indexed = *(Struct*)((byte*)Scan0 + Offset) as IPixelIndexed;
                    return this.Palette[Indexed[XBits % Indexed.Length]];
                };
                SetPixel = (X, Y, Pixel) =>
                {
                    int XBits = X * BitsPerPixel;
                    long Offset = Stride * (long)Y + (XBits >> 3);

                    Struct* sScan = (Struct*)((byte*)Scan0 + Offset);
                    IPixelIndexed Indexed = *sScan as IPixelIndexed;

                    int Index = this.Palette.IndexOf(Pixel);
                    if (Index == -1)
                    {
                        if ((1 << Indexed.BitsPerPixel) <= this.Palette.Count)
                            throw new IndexOutOfRangeException("Palette is full.");

                        Index = this.Palette.Count;
                        this.Palette.Add(Pixel);
                    }

                    Indexed[XBits % Indexed.Length] = Index;
                    *sScan = (Struct)Indexed;
                };

                ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
                {
                    long XBits = (long)OffsetX * BitsPerPixel,
                         Offset = Stride * Y + (XBits >> 3);

                    Struct* pStructs = (Struct*)((byte*)Scan0 + Offset);
                    for (int i = 0; i < Length;)
                    {
                        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                        XBits %= Indexed.Length;

                        for (; i < Length && XBits < Indexed.Length; i++)
                        {
                            Pixel Pixel = this.Palette[Indexed[(int)XBits++]];
                            Handler(ref Dest0, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                            Dest0++;
                        }

                        pStructs++;
                    }
                };
                ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
                {
                    long XBits = (long)OffsetX * BitsPerPixel,
                         Offset = Stride * Y + (XBits >> 3);

                    Struct* pStructs = (Struct*)((byte*)Scan0 + Offset);
                    for (int i = 0; i < Length;)
                    {
                        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                        XBits %= Indexed.Length;

                        for (; i < Length && XBits < Indexed.Length; i++)
                        {
                            Pixel Pixel = this.Palette[Indexed[(int)XBits++]];
                            *DestR++ = Pixel.R;
                            *DestG++ = Pixel.G;
                            *DestB++ = Pixel.B;
                        }

                        pStructs++;
                    }
                };
                ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
                {
                    long XBits = (long)OffsetX * BitsPerPixel,
                         Offset = Stride * Y + (XBits >> 3);

                    Struct* pStructs = (Struct*)((byte*)Scan0 + Offset);
                    for (int i = 0; i < Length;)
                    {
                        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                        XBits %= Indexed.Length;

                        for (; i < Length && XBits < Indexed.Length; i++)
                        {
                            Pixel Pixel = this.Palette[Indexed[(int)XBits++]];
                            *DestA++ = Pixel.A;
                            *DestR++ = Pixel.R;
                            *DestG++ = Pixel.G;
                            *DestB++ = Pixel.B;
                        }

                        pStructs++;
                    }
                };

            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    long Offset = Stride * (long)Y + ((X * BitsPerPixel) >> 3);
                    return *(Pixel*)((byte*)Scan0 + Offset);
                };
                SetPixel = (X, Y, Pixel) =>
                {
                    long Offset = Stride * (long)Y + ((X * BitsPerPixel) >> 3);
                    *(Pixel*)((byte*)Scan0 + Offset) = Pixel;
                };

                ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
                {
                    long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                    Pixel* pPixels = (Pixel*)((byte*)Scan0 + Offset);
                    for (int i = 0; i < Length; i++)
                    {
                        Handler(ref Dest0, pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                        pPixels++;
                        Dest0++;
                    }
                };
                ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                    Pixel* pPixels = (Pixel*)((byte*)Scan0 + Offset);
                    for (int i = 0; i < Length; i++)
                    {
                        *DestR++ = pPixels->R;
                        *DestG++ = pPixels->G;
                        *DestB++ = pPixels->B;
                        pPixels++;
                        Offset++;
                    }
                };
                ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                    Pixel* pPixels = (Pixel*)((byte*)Scan0 + Offset);
                    for (int i = 0; i < Length; i++)
                    {
                        *DestA++ = pPixels->A;
                        *DestR++ = pPixels->R;
                        *DestG++ = pPixels->G;
                        *DestB++ = pPixels->B;
                        pPixels++;
                        Offset++;
                    }
                };

                ScanLinePaste3 = (OffsetX, Y, Length, SourceR, SourceG, SourceB) =>
                {
                    long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                    byte* pScan0 = (byte*)Scan0 + Offset;

                    for (int i = 0; i < Length; i++)
                    {
                        this.CopyPixelHandler(ref pScan0, byte.MaxValue, *SourceR++, *SourceG++, *SourceB++);
                        pScan0++;
                    }
                };
                ScanLinePaste4 = (OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB) =>
                {
                    long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                    byte* pScan0 = (byte*)Scan0 + Offset;

                    for (int i = 0; i < Length; i++)
                    {
                        this.CopyPixelHandler(ref pScan0, *SourceA++, *SourceR++, *SourceG++, *SourceB++);
                        pScan0++;
                    }
                };

            }

            #endregion
        }
        internal unsafe ImageContextBase(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 3;

            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;

            #region Define Functions
            SetPixel = (X, Y, Pixel) =>
            {
                long Offset = Stride * (long)Y + X;
                *((byte*)ScanR + Offset) = Pixel.R;
                *((byte*)ScanG + Offset) = Pixel.G;
                *((byte*)ScanB + Offset) = Pixel.B;
            };

            if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanB + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer = *((byte*)ScanR + Offset);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanR + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer = *((byte*)ScanB + Offset);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer = (byte)((*((byte*)ScanR + Offset) * 30 +
                                            *((byte*)ScanG + Offset) * 59 +
                                            *((byte*)ScanB + Offset) * 11 + 50) / 100);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanB + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer++ = *((byte*)ScanR + Offset);
                    *PixelPointer = byte.MaxValue;
                    return Pixel;
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = byte.MaxValue;
                    *PixelPointer++ = *((byte*)ScanR + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer = *((byte*)ScanB + Offset);
                    return Pixel;
                };
            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    long Offset = Stride * (long)Y + X;
                    dynamic Result = new BGR(*((byte*)ScanB + Offset),
                                             *((byte*)ScanG + Offset),
                                             *((byte*)ScanR + Offset));
                    return (Pixel)Result;
                };
            }

            ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    Handler(ref Dest0, byte.MaxValue, *PixelR++, *PixelG++, *PixelB++);
                    Dest0++;
                }
            };
            ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *DestR++ = *PixelR++;
                    *DestG++ = *PixelG++;
                    *DestB++ = *PixelB++;
                }
            };
            ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *DestA++ = byte.MaxValue;
                    *DestR++ = *PixelR++;
                    *DestG++ = *PixelG++;
                    *DestB++ = *PixelB++;
                }
            };

            ScanLinePaste3 = (OffsetX, Y, Length, SourceR, SourceG, SourceB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *PixelR++ = *SourceR++;
                    *PixelG++ = *SourceG++;
                    *PixelB++ = *SourceB++;
                }
            };
            ScanLinePaste4 = (OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *PixelR++ = *SourceR++;
                    *PixelG++ = *SourceG++;
                    *PixelB++ = *SourceB++;
                }
            };

            #endregion

        }
        internal unsafe ImageContextBase(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 4;

            this._ScanA = ScanA;
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;

            #region Define Functions
            SetPixel = (X, Y, Pixel) =>
            {
                long Offset = Stride * (long)Y + X;
                *((byte*)ScanR + Offset) = Pixel.R;
                *((byte*)ScanG + Offset) = Pixel.G;
                *((byte*)ScanB + Offset) = Pixel.B;
                *((byte*)ScanA + Offset) = Pixel.A;
            };

            if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanB + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer++ = *((byte*)ScanR + Offset);
                    *PixelPointer = *((byte*)ScanA + Offset);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanA + Offset);
                    *PixelPointer++ = *((byte*)ScanR + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer = *((byte*)ScanB + Offset);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanB + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer = *((byte*)ScanR + Offset);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer++ = *((byte*)ScanR + Offset);
                    *PixelPointer++ = *((byte*)ScanG + Offset);
                    *PixelPointer = *((byte*)ScanB + Offset);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    long Offset = Stride * (long)Y + X;
                    *PixelPointer = (byte)((*((byte*)ScanR + Offset) * 30 +
                                            *((byte*)ScanG + Offset) * 59 +
                                            *((byte*)ScanB + Offset) * 11 + 50) / 100);
                    return Pixel;
                };
            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    long Offset = Stride * (long)Y + X;
                    dynamic Result = new BGRA(*((byte*)ScanB + Offset),
                                              *((byte*)ScanG + Offset),
                                              *((byte*)ScanR + Offset),
                                              *((byte*)ScanA + Offset));
                    return (Pixel)Result;
                };
            }

            ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelA = (byte*)ScanA + Offset,
                      PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    Handler(ref Dest0, *PixelA++, *PixelR++, *PixelG++, *PixelB++);
                    Dest0++;
                }
            };
            ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *DestR++ = *PixelR++;
                    *DestG++ = *PixelG++;
                    *DestB++ = *PixelB++;
                }
            };
            ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelA = (byte*)ScanA + Offset,
                      PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *DestA++ = *PixelA++;
                    *DestR++ = *PixelR++;
                    *DestG++ = *PixelG++;
                    *DestB++ = *PixelB++;
                }
            };

            ScanLinePaste3 = (OffsetX, Y, Length, SourceR, SourceG, SourceB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelA = (byte*)ScanA + Offset,
                      PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *PixelA++ = byte.MaxValue;
                    *PixelR++ = *SourceR++;
                    *PixelG++ = *SourceG++;
                    *PixelB++ = *SourceB++;
                }
            };
            ScanLinePaste4 = (OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB) =>
            {
                long Offset = Stride * Y + (((long)OffsetX * BitsPerPixel) >> 3);
                byte* PixelA = (byte*)ScanA + Offset,
                      PixelR = (byte*)ScanR + Offset,
                      PixelG = (byte*)ScanG + Offset,
                      PixelB = (byte*)ScanB + Offset;

                for (int i = 0; i < Length; i++)
                {
                    *PixelA++ = *SourceA++;
                    *PixelR++ = *SourceR++;
                    *PixelG++ = *SourceG++;
                    *PixelB++ = *SourceB++;
                }
            };

            #endregion

        }

        internal unsafe ImageContextBase(int Width, int Height, byte[] Data, IList<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;

            this.Stride = Data.Length / Height;
            this.Channels = 1;

            this.Palette = Palette ?? new List<Pixel>();

            this.Data0 = Data;

            #region Define Functions
            if (IsStructIndexed)
            {
                GetPixel = (X, Y) =>
                {
                    int XBits = X * BitsPerPixel;

                    IPixelIndexed Indexed;
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + (XBits >> 3)])
                        Indexed = *(Struct*)PixelPointer as IPixelIndexed;

                    return this.Palette[Indexed[XBits % Indexed.Length]];
                };
                SetPixel = (X, Y, Pixel) =>
                {
                    int XBits = X * BitsPerPixel;

                    Struct* sScan;
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + (XBits >> 3)])
                        sScan = (Struct*)PixelPointer;

                    IPixelIndexed Indexed = *sScan as IPixelIndexed;

                    int Index = this.Palette.IndexOf(Pixel);
                    if (Index == -1)
                    {
                        if ((1 << Indexed.BitsPerPixel) <= this.Palette.Count)
                            throw new IndexOutOfRangeException("Palette is full.");

                        Index = this.Palette.Count;
                        this.Palette.Add(Pixel);
                    }

                    Indexed[XBits % Indexed.Length] = Index;
                    *sScan = (Struct)Indexed;
                };

                ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
                {
                    int XBits = OffsetX * BitsPerPixel,
                        Offset = Stride * Y + (XBits >> 3);

                    Struct* pStructs;
                    fixed (byte* pScan0 = &this.Data0[Offset])
                        pStructs = (Struct*)pScan0;

                    for (int i = 0; i < Length;)
                    {
                        IPixelIndexed Indexed = *pStructs++ as IPixelIndexed;
                        XBits %= Indexed.Length;

                        for (; i < Length && XBits < Indexed.Length; i++)
                        {
                            Pixel Pixel = this.Palette[Indexed[XBits++]];
                            Handler(ref Dest0, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                            Dest0++;
                        }
                    }
                };
                ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
                {
                    int XBits = OffsetX * BitsPerPixel,
                        Offset = Stride * Y + (XBits >> 3);

                    Struct* pStructs;
                    fixed (byte* pScan0 = &this.Data0[Offset])
                        pStructs = (Struct*)pScan0;

                    for (int i = 0; i < Length;)
                    {
                        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                        XBits %= Indexed.Length;

                        for (; i < Length && XBits < Indexed.Length; i++)
                        {
                            Pixel Pixel = this.Palette[Indexed[XBits++]];
                            *DestR++ = Pixel.R;
                            *DestG++ = Pixel.G;
                            *DestB++ = Pixel.B;
                        }

                        pStructs++;
                    }
                };
                ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
                {
                    int XBits = OffsetX * BitsPerPixel,
                        Offset = Stride * Y + (XBits >> 3);

                    Struct* pStructs;
                    fixed (byte* pScan0 = &this.Data0[Offset])
                        pStructs = (Struct*)pScan0;

                    for (int i = 0; i < Length;)
                    {
                        IPixelIndexed Indexed = *pStructs as IPixelIndexed;
                        XBits %= Indexed.Length;

                        for (; i < Length && XBits < Indexed.Length; i++)
                        {
                            Pixel Pixel = this.Palette[Indexed[XBits++]];
                            *DestA++ = Pixel.A;
                            *DestR++ = Pixel.R;
                            *DestG++ = Pixel.G;
                            *DestB++ = Pixel.B;
                        }

                        pStructs++;
                    }
                };

            }
            else
            {
                GetPixel = (X, Y) =>
                {
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + ((X * BitsPerPixel) >> 3)])
                        return *(Pixel*)PixelPointer;
                };
                SetPixel = (X, Y, Value) =>
                {
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + ((X * BitsPerPixel) >> 3)])
                        *(Pixel*)PixelPointer = Value;
                };

                ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);

                    Pixel* pPixels;
                    fixed (byte* pScan0 = &this.Data0[Offset])
                        pPixels = (Pixel*)pScan0;

                    for (int i = 0; i < Length; i++)
                    {
                        Handler(ref Dest0, pPixels->A, pPixels->R, pPixels->G, pPixels->B);
                        pPixels++;
                        Dest0++;
                    }
                };
                ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);

                    Pixel* pPixels;
                    fixed (byte* pScan0 = &this.Data0[Offset])
                        pPixels = (Pixel*)pScan0;

                    for (int i = 0; i < Length; i++)
                    {
                        *DestR++ = pPixels->R;
                        *DestG++ = pPixels->G;
                        *DestB++ = pPixels->B;
                        pPixels++;
                    }
                };
                ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);

                    Pixel* pPixels;
                    fixed (byte* pScan0 = &this.Data0[Offset])
                        pPixels = (Pixel*)pScan0;

                    for (int i = 0; i < Length; i++)
                    {
                        *DestA++ = pPixels->A;
                        *DestR++ = pPixels->R;
                        *DestG++ = pPixels->G;
                        *DestB++ = pPixels->B;
                        pPixels++;
                    }
                };

                ScanLinePaste3 = (OffsetX, Y, Length, SourceR, SourceG, SourceB) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);

                    byte* pScan0;
                    fixed (byte* pTemp = &this.Data0[Offset])
                        pScan0 = pTemp;

                    for (int i = 0; i < Length; i++)
                    {
                        this.CopyPixelHandler(ref pScan0, byte.MaxValue, *SourceR++, *SourceG++, *SourceB++);
                        pScan0++;
                    }
                };
                ScanLinePaste4 = (OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB) =>
                {
                    int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);

                    byte* pScan0;
                    fixed (byte* pTemp = &this.Data0[Offset])
                        pScan0 = pTemp;

                    for (int i = 0; i < Length; i++)
                    {
                        this.CopyPixelHandler(ref pScan0, *SourceA++, *SourceR++, *SourceG++, *SourceB++);
                        pScan0++;
                    }
                };

            }

            #endregion

        }
        internal unsafe ImageContextBase(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Height;
            this.Channels = 3;

            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;

            #region Define Functions
            SetPixel = (X, Y, Pixel) =>
            {
                int Offset = Stride * Y + X;
                DataR[Offset] = Pixel.R;
                DataG[Offset] = Pixel.G;
                DataB[Offset] = Pixel.B;
            };

            if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataB[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer = this.DataR[Offset];
                    return Pixel;
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataR[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer = this.DataB[Offset];
                    return Pixel;
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer = (byte)((this.DataR[Offset] * 30 + this.DataG[Offset] * 59 + this.DataB[Offset] * 11 + 50) / 100);
                    return Pixel;
                };
            }
            else if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataB[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer++ = this.DataR[Offset];
                    *PixelPointer = byte.MaxValue;
                    return Pixel;
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = byte.MaxValue;
                    *PixelPointer++ = this.DataR[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer = this.DataB[Offset];
                    return Pixel;
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
                    return (Pixel)Result;
                };
            }

            ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    Handler(ref Dest0, byte.MaxValue, DataR[Offset], DataG[Offset], DataB[Offset]);
                    Offset++;
                    Dest0++;
                }
            };
            ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    *DestR++ = DataR[Offset];
                    *DestG++ = DataG[Offset];
                    *DestB++ = DataB[Offset];
                    Offset++;
                }
            };
            ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    *DestA++ = byte.MaxValue;
                    *DestR++ = DataR[Offset];
                    *DestG++ = DataG[Offset];
                    *DestB++ = DataB[Offset];
                    Offset++;
                }
            };

            ScanLinePaste3 = (OffsetX, Y, Length, SourceR, SourceG, SourceB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    DataR[Offset] = *SourceR++;
                    DataG[Offset] = *SourceG++;
                    DataB[Offset] = *SourceB++;
                    Offset++;
                }
            };
            ScanLinePaste4 = (OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    DataR[Offset] = *SourceR++;
                    DataG[Offset] = *SourceG++;
                    DataB[Offset] = *SourceB++;
                    Offset++;
                }
            };

            #endregion
        }
        internal unsafe ImageContextBase(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Height;
            this.Channels = 4;

            this.DataA = DataA;
            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;

            #region Define Functions
            SetPixel = (X, Y, Pixel) =>
            {
                int Offset = Stride * Y + X;
                DataR[Offset] = Pixel.R;
                DataG[Offset] = Pixel.G;
                DataB[Offset] = Pixel.B;
                DataA[Offset] = Pixel.A;
            };

            if (PixelType == typeof(BGRA))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataB[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer++ = this.DataR[Offset];
                    *PixelPointer = this.DataA[Offset];
                    return Pixel;
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataA[Offset];
                    *PixelPointer++ = this.DataR[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer = this.DataB[Offset];
                    return Pixel;
                };
            }
            else if (PixelType == typeof(BGR))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataB[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer = this.DataR[Offset];
                    return Pixel;
                };
            }
            else if (PixelType == typeof(RGB))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer++ = this.DataR[Offset];
                    *PixelPointer++ = this.DataG[Offset];
                    *PixelPointer = this.DataB[Offset];
                    return Pixel;
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                GetPixel = (X, Y) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    int Offset = Stride * Y + X;
                    *PixelPointer = (byte)((this.DataR[Offset] * 30 + this.DataG[Offset] * 59 + this.DataB[Offset] * 11 + 50) / 100);
                    return Pixel;
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
                    return (Pixel)Result;
                };
            }

            ScanLineCopy1 = (OffsetX, Y, Length, Dest0, Handler) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    Handler(ref Dest0, DataA[Offset], DataR[Offset], DataG[Offset], DataB[Offset]);
                    Offset++;
                    Dest0++;
                }
            };
            ScanLineCopy3 = (OffsetX, Y, Length, DestR, DestG, DestB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    *DestR++ = DataR[Offset];
                    *DestG++ = DataG[Offset];
                    *DestB++ = DataB[Offset];
                    Offset++;
                }
            };
            ScanLineCopy4 = (OffsetX, Y, Length, DestA, DestR, DestG, DestB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    *DestA++ = DataA[Offset];
                    *DestR++ = DataR[Offset];
                    *DestG++ = DataG[Offset];
                    *DestB++ = DataB[Offset];
                    Offset++;
                }
            };

            ScanLinePaste3 = (OffsetX, Y, Length, SourceR, SourceG, SourceB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    DataA[Offset] = byte.MaxValue;
                    DataR[Offset] = *SourceR++;
                    DataG[Offset] = *SourceG++;
                    DataB[Offset] = *SourceB++;
                    Offset++;
                }
            };
            ScanLinePaste4 = (OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB) =>
            {
                int Offset = Stride * Y + ((OffsetX * BitsPerPixel) >> 3);
                for (int i = 0; i < Length; i++)
                {
                    DataA[Offset] = *SourceA++;
                    DataR[Offset] = *SourceR++;
                    DataG[Offset] = *SourceG++;
                    DataB[Offset] = *SourceB++;
                    Offset++;
                }
            };

            #endregion
        }

        protected abstract IImageContext FlipHandler(FlipMode Mode);
        IImageContext IImageContext.Flip(FlipMode Mode)
            => FlipHandler(Mode);

        protected abstract IImageContext CropHandler(int X, int Y, int Width, int Height);
        IImageContext IImageContext.Crop(int X, int Y, int Width, int Height)
            => CropHandler(X, Y, Width, Height);

        protected abstract IImageContext ConvoluteHandler(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum);
        IImageContext IImageContext.Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
            => ConvoluteHandler(Kernel, KernelFactorSum, KernelOffsetSum);
        IImageContext IImageContext.Convolute(ConvoluteKernel Kernel)
            => ConvoluteHandler(Kernel.Datas, Kernel.FactorSum, Kernel.Offset);

        protected abstract IImageContext CastHandler<T>()
            where T : unmanaged, IPixel;
        protected abstract IImageContext CastHandler<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed;
        IImageContext IImageContext.Cast<T>()
            => CastHandler<T>();
        IImageContext IImageContext.Cast<T, U>()
            => CastHandler<T, U>();

        protected abstract IImageContext CloneHandler();
        object ICloneable.Clone()
            => CloneHandler();

    }

}
