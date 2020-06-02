using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenthaAssembly
{
    public class ImageContext<Pixel> : ImageContextBase<Pixel, Pixel>
        where Pixel : unmanaged, IPixel
    {
        public ImageContext(int Width, int Height) : base(Width, Height)
        {
        }

        public ImageContext(int Width, int Height, IntPtr Scan0, IList<Pixel> Palette = null) :
            this(Width, Height, Scan0, Width, Palette)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, IList<Pixel> Palette = null) :
            base(Width, Height, Scan0, Stride, Palette)
        {
        }

        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) :
            this(Width, Height, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) :
            base(Width, Height, ScanR, ScanG, ScanB, Stride)
        {
        }

        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) :
            this(Width, Height, ScanA, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride) :
            base(Width, Height, ScanA, ScanR, ScanG, ScanB, Stride)
        {
        }

        public ImageContext(int Width, int Height, byte[] Data, IList<Pixel> Palette = null) :
            base(Width, Height, Data, Palette)
        {
        }

        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) :
            base(Width, Height, DataR, DataG, DataB)
        {
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) :
            base(Width, Height, DataA, DataR, DataG, DataB)
        {
        }

        public ImageContext<Pixel> Flip(FlipMode Mode)
        {
            if (Mode == FlipMode.Vertical)
            {
                Parallel.For(0, Height >> 1, (y) =>
                {
                    int DestY = Height - 1 - y;
                    for (int x = 0; x < Width; x++)
                    {
                        Pixel Pixel = GetPixel(x, y);
                        SetPixel(x, y, GetPixel(x, DestY));
                        SetPixel(x, DestY, Pixel);
                    }
                });
            }
            else if (Mode == FlipMode.Horizontal)
            {
                Parallel.For(0, Width >> 1, (x) =>
                {
                    int DestX = Width - 1 - x;
                    for (int y = 0; y < Height; y++)
                    {
                        Pixel Pixel = GetPixel(x, y);
                        SetPixel(x, y, GetPixel(DestX, y));
                        SetPixel(DestX, y, Pixel);
                    }
                });
            }

            return this;
        }

        /// <summary>
        /// Creates a new cropped ImageContext<<typeparamref name="T"/>>.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public unsafe ImageContext<Pixel> Crop(int X, int Y, int Width, int Height)
        {
            // If the rectangle is completely out of the bitmap
            if (X > this.Width || Y > this.Height)
                return new ImageContext<Pixel>(0, 0);

            // Clamp to boundaries
            X = Math.Max(X, 0);
            Y = Math.Max(Y, 0);
            Width = Math.Max(Math.Min(Width, this.Width - X), 0);
            Height = Math.Max(Math.Min(Height, this.Height - Y), 0);

            // Create Result
            ImageContext<Pixel> Result = new ImageContext<Pixel>(Width, Height);

            Parallel.For(0, Height, (j) =>
            {
                Pixel* Pixels = (Pixel*)Result.Scan0;
                Pixels += j * (long)Width;

                int SourceY = Y + j;
                for (int i = 0; i < Width; i++)
                    *Pixels++ = this.GetPixel(X + i, SourceY);
            });

            return Result;
        }
        protected override IImageContext HandleCrop(int X, int Y, int Width, int Height)
            => this.Crop(X, Y, Width, Height);

        public unsafe ImageContext<T> Cast<T>()
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(this.Width, this.Height);

            Parallel.For(0, this.Height, (Y) =>
            {
                T* Pixels = (T*)Result.Scan0;
                Pixels += Y * (long)Width;

                Pixel SourcePixel;
                for (int X = 0; X < Width; X++)
                {
                    SourcePixel = this.GetPixel(X, Y);
                    *Pixels++ = Result.ToPixel(SourcePixel.A, SourcePixel.R, SourcePixel.G, SourcePixel.B);
                }
            });

            return Result;
        }
        public unsafe ImageContext<T, U> Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Result = new ImageContext<T, U>(this.Width, this.Height);

            for (int Y = 0; Y < this.Height; Y++)
            {
                Pixel SourcePixel;
                for (int X = 0; X < Width; X++)
                {
                    SourcePixel = this.GetPixel(X, Y);
                    Result.SetPixel(X, Y, Result.ToPixel(SourcePixel.A, SourcePixel.R, SourcePixel.G, SourcePixel.B));
                }
            }

            return Result;
        }




        /// <summary>
        /// Rotates the bitmap in any degree returns a new rotated WriteableBitmap.
        /// </summary>
        /// <param name="angle">Arbitrary angle in 360 Degrees (positive = clockwise).</param>
        /// <param name="crop">if true: keep the size, false: adjust canvas to new size</param>
        /// <returns>A new WriteableBitmap that is a rotated version of the input.</returns>
        public ImageContext<Pixel> RotateFree(double angle, bool crop = true)
        {
            // rotating clockwise, so it's negative relative to Cartesian quadrants
            double cnAngle = -1.0 * MathHelper.UnitTheta * angle;

            // general iterators
            int i, j;
            // calculated indices in Cartesian coordinates
            int x, y;
            double fDistance, fPolarAngle;
            // for use in neighboring indices in Cartesian coordinates
            int iFloorX, iCeilingX, iFloorY, iCeilingY;
            // calculated indices in Cartesian coordinates with trailing decimals
            double fTrueX, fTrueY;
            // for interpolation
            double fDeltaX, fDeltaY;

            // interpolated "top" pixels
            double fTopRed, fTopGreen, fTopBlue, fTopAlpha;

            // interpolated "bottom" pixels
            double fBottomRed, fBottomGreen, fBottomBlue, fBottomAlpha;

            // final interpolated color components
            int iRed, iGreen, iBlue, iAlpha;

            int iCentreX, iCentreY;
            int iDestCentreX, iDestCentreY;
            int iWidth, iHeight, newWidth, newHeight;

            iWidth = this.Width;
            iHeight = this.Height;

            if (crop)
            {
                newWidth = iWidth;
                newHeight = iHeight;
            }
            else
            {
                double rad = angle * MathHelper.UnitTheta;
                newWidth = (int)Math.Ceiling(Math.Abs(Math.Sin(rad) * iHeight) + Math.Abs(Math.Cos(rad) * iWidth));
                newHeight = (int)Math.Ceiling(Math.Abs(Math.Sin(rad) * iWidth) + Math.Abs(Math.Cos(rad) * iHeight));
            }


            iCentreX = iWidth / 2;
            iCentreY = iHeight / 2;

            iDestCentreX = newWidth / 2;
            iDestCentreY = newHeight / 2;

            ImageContext<Pixel> Result = new ImageContext<Pixel>(newWidth, newHeight);

            // assigning pixels of destination image from source image
            // with bilinear interpolation
            for (i = 0; i < newHeight; ++i)
            {
                for (j = 0; j < newWidth; ++j)
                {
                    // convert raster to Cartesian
                    x = j - iDestCentreX;
                    y = iDestCentreY - i;

                    // convert Cartesian to polar
                    fDistance = Math.Sqrt(x * x + y * y);
                    if (x == 0)
                    {
                        if (y == 0)
                        {
                            // center of image, no rotation needed
                            Result.SetPixel(i, j, this.GetPixel(iCentreX, iCentreY));
                            continue;
                        }
                        fPolarAngle = (y < 0 ? 1.5 : 0.5) * Math.PI;
                    }
                    else
                    {
                        fPolarAngle = Math.Atan2(y, x);
                    }

                    // the crucial rotation part
                    // "reverse" rotate, so minus instead of plus
                    fPolarAngle -= cnAngle;

                    // convert polar to Cartesian
                    fTrueX = fDistance * Math.Cos(fPolarAngle);
                    fTrueY = fDistance * Math.Sin(fPolarAngle);

                    // convert Cartesian to raster
                    fTrueX = fTrueX + iCentreX;
                    fTrueY = iCentreY - fTrueY;

                    iFloorX = (int)Math.Floor(fTrueX);
                    iFloorY = (int)Math.Floor(fTrueY);
                    iCeilingX = (int)Math.Ceiling(fTrueX);
                    iCeilingY = (int)Math.Ceiling(fTrueY);

                    // check bounds
                    if (iFloorX < 0 || iCeilingX < 0 || iFloorX >= iWidth || iCeilingX >= iWidth || iFloorY < 0 ||
                        iCeilingY < 0 || iFloorY >= iHeight || iCeilingY >= iHeight) continue;

                    fDeltaX = fTrueX - iFloorX;
                    fDeltaY = fTrueY - iFloorY;

                    Pixel clrTopLeft = this.GetPixel(iFloorX, iFloorY),
                          clrTopRight = this.GetPixel(iCeilingX, iFloorY),
                          clrBottomLeft = this.GetPixel(iFloorX, iCeilingY),
                          clrBottomRight = this.GetPixel(iCeilingX, iCeilingY);

                    fTopAlpha = (1 - fDeltaX) * clrTopLeft.A + fDeltaX * clrTopRight.A;
                    fTopRed = (1 - fDeltaX) * clrTopLeft.R + fDeltaX * clrTopRight.R;
                    fTopGreen = (1 - fDeltaX) * clrTopLeft.G + fDeltaX * clrTopRight.G;
                    fTopBlue = (1 - fDeltaX) * clrTopLeft.B + fDeltaX * clrTopRight.B;

                    // linearly interpolate horizontally between bottom neighbors
                    fBottomAlpha = (1 - fDeltaX) * clrBottomLeft.A + fDeltaX * clrBottomRight.A;
                    fBottomRed = (1 - fDeltaX) * clrBottomLeft.R + fDeltaX * clrBottomRight.R;
                    fBottomGreen = (1 - fDeltaX) * clrBottomLeft.G + fDeltaX * clrBottomRight.G;
                    fBottomBlue = (1 - fDeltaX) * clrBottomLeft.B + fDeltaX * clrBottomRight.B;

                    // linearly interpolate vertically between top and bottom interpolated results
                    iRed = (int)(Math.Round((1 - fDeltaY) * fTopRed + fDeltaY * fBottomRed));
                    iGreen = (int)(Math.Round((1 - fDeltaY) * fTopGreen + fDeltaY * fBottomGreen));
                    iBlue = (int)(Math.Round((1 - fDeltaY) * fTopBlue + fDeltaY * fBottomBlue));
                    iAlpha = (int)(Math.Round((1 - fDeltaY) * fTopAlpha + fDeltaY * fBottomAlpha));

                    // make sure color values are valid
                    if (iRed < 0) iRed = 0;
                    if (iRed > 255) iRed = 255;
                    if (iGreen < 0) iGreen = 0;
                    if (iGreen > 255) iGreen = 255;
                    if (iBlue < 0) iBlue = 0;
                    if (iBlue > 255) iBlue = 255;
                    if (iAlpha < 0) iAlpha = 0;
                    if (iAlpha > 255) iAlpha = 255;

                    int a = iAlpha + 1;
                    Result.SetPixel(i, j, ToPixel((byte)iAlpha,
                                                 (byte)(((iRed * a) >> 8) << 16),
                                                 (byte)(((iGreen * a) >> 8) << 8),
                                                 (byte)((iBlue * a) >> 8)));

                }
            }
            return Result;
        }


    }
    public class ImageContext<Pixel, PixelIndexed> : ImageContextBase<Pixel, PixelIndexed>
        where Pixel : unmanaged, IPixel
        where PixelIndexed : unmanaged, IPixelIndexed
    {
        public ImageContext(int Width, int Height) : base(Width, Height)
        {
        }

        public ImageContext(int Width, int Height, IntPtr Scan0, IList<Pixel> Palette) :
            this(Width, Height, Scan0, Width, Palette)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, IList<Pixel> Palette) :
            base(Width, Height, Scan0, Stride, Palette)
        {
        }

        public ImageContext(int Width, int Height, byte[] Data, IList<Pixel> Palette) :
            base(Width, Height, Data, Palette)
        {
        }

        public ImageContext<Pixel, PixelIndexed> Flip(FlipMode Mode)
        {
            if (Mode == FlipMode.Vertical)
            {
                Parallel.For(0, Height >> 1, (y) =>
                {
                    int DestY = Height - 1 - y;
                    for (int x = 0; x < Width; x++)
                    {
                        Pixel Pixel = GetPixel(x, y);
                        SetPixel(x, y, GetPixel(x, DestY));
                        SetPixel(x, DestY, Pixel);
                    }
                });
            }
            else if (Mode == FlipMode.Horizontal)
            {
                Parallel.For(0, Width >> 1, (x) =>
                {
                    int DestX = Width - 1 - x;
                    for (int y = 0; y < Height; y++)
                    {
                        Pixel Pixel = GetPixel(x, y);
                        SetPixel(x, y, GetPixel(DestX, y));
                        SetPixel(DestX, y, Pixel);
                    }
                });
            }

            return this;
        }

        /// <summary>
        /// Creates a new cropped ImageContext<<typeparamref name="Pixel"/>>.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public unsafe ImageContext<Pixel, PixelIndexed> Crop(int X, int Y, int Width, int Height)
        {
            // If the rectangle is completely out of the bitmap
            if (X > this.Width || Y > this.Height)
                return new ImageContext<Pixel, PixelIndexed>(0, 0);

            // Clamp to boundaries
            X = Math.Max(X, 0);
            Y = Math.Max(Y, 0);
            Width = Math.Max(Math.Min(Width, this.Width - X), 0);
            Height = Math.Max(Math.Min(Height, this.Height - Y), 0);

            // Create Result
            ImageContext<Pixel, PixelIndexed> Result = new ImageContext<Pixel, PixelIndexed>(Width, Height);

            Parallel.For(0, Height, (j) =>
            {
                int SourceY = Y + j;

                for (int i = 0; i < Width; i++)
                    Result.SetPixel(i, j, this.GetPixel(X + i, SourceY));
            });

            return Result;
        }
        protected override IImageContext HandleCrop(int X, int Y, int Width, int Height)
            => this.Crop(X, Y, Width, Height);

        public unsafe ImageContext<T> Cast<T>()
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(this.Width, this.Height);

            Parallel.For(0, this.Height, (Y) =>
            {
                T* Pixels = (T*)Result.Scan0;
                Pixels += Y * (long)Width;

                Pixel SourcePixel;
                for (int X = 0; X < Width; X++)
                {
                    SourcePixel = this.GetPixel(X, Y);
                    *Pixels++ = Result.ToPixel(SourcePixel.A, SourcePixel.R, SourcePixel.G, SourcePixel.B);
                }
            });

            return Result;
        }
        public unsafe ImageContext<T, U> Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Result = new ImageContext<T, U>(this.Width, this.Height);

            for (int Y = 0; Y < this.Height; Y++)
            {
                Pixel SourcePixel;
                for (int X = 0; X < Width; X++)
                {
                    SourcePixel = this.GetPixel(X, Y);
                    Result.SetPixel(X, Y, Result.ToPixel(SourcePixel.A, SourcePixel.R, SourcePixel.G, SourcePixel.B));
                }
            }

            return Result;
        }

    }

    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
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

        public IList<Pixel> Palette { get; }

        IList<IPixel> IImageContext.Palette
            => this.Palette.Cast<IPixel>().ToList();

        internal unsafe ImageContextBase()
        {
            Struct StructFormat = default;
            this.IsStructIndexed = StructFormat is IPixelIndexed;
            this.BitsPerPixel = StructFormat.BitsPerPixel;

            if (PixelType == typeof(BGRA))
            {
                ToPixel = (A, R, G, B) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    *PixelPointer++ = B;
                    *PixelPointer++ = G;
                    *PixelPointer++ = R;
                    *PixelPointer = A;
                    return Pixel;
                };
            }
            else if (PixelType == typeof(ARGB))
            {
                ToPixel = (A, R, G, B) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    *PixelPointer++ = A;
                    *PixelPointer++ = R;
                    *PixelPointer++ = G;
                    *PixelPointer = B;
                    return Pixel;
                };
            }
            else if (PixelType == typeof(BGR))
            {
                ToPixel = (A, R, G, B) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    *PixelPointer++ = B;
                    *PixelPointer++ = G;
                    *PixelPointer = R;
                    return Pixel;
                };
            }
            else if (PixelType == typeof(RGB))
            {
                ToPixel = (A, R, G, B) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    *PixelPointer++ = R;
                    *PixelPointer++ = G;
                    *PixelPointer = B;
                    return Pixel;
                };
            }
            else if (PixelType == typeof(Gray8))
            {
                ToPixel = (A, R, G, B) =>
                {
                    Pixel Pixel = default;
                    byte* PixelPointer = (byte*)&Pixel;
                    *PixelPointer = (byte)((R * 30 +
                                            G * 59 +
                                            B * 11 + 50) / 100);
                    return Pixel;
                };
            }
            else
            {
                ToPixel = (A, R, G, B) =>
                {
                    dynamic Result = new BGRA(B, G, R, A);
                    return (Pixel)Result;
                };
            }
        }

        internal unsafe ImageContextBase(int Width, int Height) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = ((Width * BitsPerPixel) + 7) >> 3;
            this.Channels = 1;

            this.Palette = new List<Pixel>();

            this.Data0 = new byte[this.Stride * Height];

            if (IsStructIndexed)
            {
                GetPixel = (X, Y) =>
                {
                    int XBits = X * BitsPerPixel;

                    IPixelIndexed Indexed;
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + (XBits >> 3)])
                        Indexed = *(Struct*)PixelPointer as IPixelIndexed;

                    return Palette[Indexed[XBits % Indexed.Length]];
                };
                SetPixel = (X, Y, Pixel) =>
                {
                    int XBits = X * BitsPerPixel;

                    IPixelIndexed Indexed;
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + (XBits >> 3)])
                    {
                        Indexed = *(Struct*)PixelPointer as IPixelIndexed;

                        int Index = Palette.IndexOf(Pixel);
                        if (Index == -1)
                        {
                            if ((1 << Indexed.BitsPerPixel) <= Palette.Count)
                                throw new IndexOutOfRangeException("Palette is full.");

                            Index = Palette.Count;
                            Palette.Add(Pixel);
                        }

                        Indexed[XBits % Indexed.Length] = Index;

                        *PixelPointer = Indexed.Data;
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
            }

        }

        internal unsafe ImageContextBase(int Width, int Height, IntPtr Scan0, int Stride, IList<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.Channels = 1;

            this.Palette = Palette ?? new List<Pixel>();

            this._Scan0 = Scan0;

            if (IsStructIndexed)
            {
                GetPixel = (X, Y) =>
                {
                    int XBits = X * BitsPerPixel;
                    long Offset = Stride * (long)Y + (XBits >> 3);

                    IPixelIndexed Indexed = *(Struct*)((byte*)Scan0 + Offset) as IPixelIndexed;
                    return Palette[Indexed[XBits % Indexed.Length]];
                };
                SetPixel = (X, Y, Pixel) =>
                {
                    int XBits = X * BitsPerPixel;
                    long Offset = Stride * (long)Y + (XBits >> 3);

                    IPixelIndexed Indexed = *(Struct*)((byte*)Scan0 + Offset) as IPixelIndexed;

                    int Index = Palette.IndexOf(Pixel);
                    if (Index == -1)
                    {
                        if ((1 << Indexed.BitsPerPixel) <= Palette.Count)
                            throw new IndexOutOfRangeException("Palette is full.");

                        Index = Palette.Count;
                        Palette.Add(Pixel);
                    }

                    Indexed[XBits % Indexed.Length] = Index;
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
            }
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

        }

        internal unsafe ImageContextBase(int Width, int Height, byte[] Data, IList<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;

            this.Stride = Data.Length / Width;
            this.Channels = 1;

            this.Palette = Palette ?? new List<Pixel>();

            this.Data0 = Data;

            if (IsStructIndexed)
            {
                GetPixel = (X, Y) =>
                {
                    int XBits = X * BitsPerPixel;

                    IPixelIndexed Indexed;
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + (XBits >> 3)])
                        Indexed = *(Struct*)PixelPointer as IPixelIndexed;

                    return Palette[Indexed[XBits % Indexed.Length]];
                };
                SetPixel = (X, Y, Pixel) =>
                {
                    int XBits = X * BitsPerPixel;

                    IPixelIndexed Indexed;
                    fixed (byte* PixelPointer = &this.Data0[Stride * Y + (XBits >> 3)])
                        Indexed = *(Struct*)PixelPointer as IPixelIndexed;

                    int Index = Palette.IndexOf(Pixel);
                    if (Index == -1)
                    {
                        if ((1 << Indexed.BitsPerPixel) <= Palette.Count)
                            throw new IndexOutOfRangeException("Palette is full.");

                        Index = Palette.Count;
                        Palette.Add(Pixel);
                    }

                    Indexed[XBits % Indexed.Length] = Index;
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
            }

        }
        internal unsafe ImageContextBase(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Width;
            this.Channels = 3;

            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;

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
        }
        internal unsafe ImageContextBase(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Width;
            this.Channels = 4;

            this.DataA = DataA;
            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;

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
        }

        IImageContext IImageContext.Flip(FlipMode Mode)
        {
            if (Mode == FlipMode.Vertical)
            {
                Parallel.For(0, Height >> 1, (y) =>
                {
                    int DestY = Height - 1 - y;
                    for (int x = 0; x < Width; x++)
                    {
                        Pixel Pixel = GetPixel(x, y);
                        SetPixel(x, y, GetPixel(x, DestY));
                        SetPixel(x, DestY, Pixel);
                    }
                });
            }
            else if (Mode == FlipMode.Horizontal)
            {
                Parallel.For(0, Width >> 1, (x) =>
                {
                    int DestX = Width - 1 - x;
                    for (int y = 0; y < Height; y++)
                    {
                        Pixel Pixel = GetPixel(x, y);
                        SetPixel(x, y, GetPixel(DestX, y));
                        SetPixel(DestX, y, Pixel);
                    }
                });
            }

            return this;
        }

        IImageContext IImageContext.Crop(int X, int Y, int Width, int Height)
           => HandleCrop(X, Y, Width, Height);
        protected abstract IImageContext HandleCrop(int X, int Y, int Width, int Height);


    }

}
