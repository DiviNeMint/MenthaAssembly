using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenthaAssembly
{
    public class ImageContext<Pixel> : ImageContextBase<Pixel, Pixel>
        where Pixel : unmanaged, IPixel
    {
        public new IntPtr ScanA => base.ScanA;
        public new IntPtr ScanR => base.ScanR;
        public new IntPtr ScanG => base.ScanG;
        public new IntPtr ScanB => base.ScanB;

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

        /// <summary>
        /// Create a new flipped ImageContext&lt;<typeparamref name="Pixel"/>&gt;.
        /// </summary>
        /// <param name="Mode">The flip mode.</param>
        public ImageContext<Pixel> Flip(FlipMode Mode)
            => this.Flip<Pixel>(Mode) ?? this;
        /// <summary>
        /// Create a new flipped ImageContext&lt;<typeparamref name="T"/>&gt;.
        /// </summary>
        /// <param name="Mode">The flip mode.</param>
        public unsafe ImageContext<T> Flip<T>(FlipMode Mode)
            where T : unmanaged, IPixel
        {
            if (Mode == FlipMode.Vertical)
            {
                // Create Result
                ImageContext<T> Result = new ImageContext<T>(Width, Height);

                CopyPixelAction Handler = CreateCopyPixelHandler<T>();
                int DestStride = Result.Stride;
                byte* Dest0 = (byte*)Result.Scan0;
                Parallel.For(0, Height, (y) =>
                {
                    byte* Dest = Dest0 + DestStride * (Height - 1 - y);
                    ScanLineCopy(0, y, Width, Dest, Handler);
                });

                return Result;
            }
            else if (Mode == FlipMode.Horizontal)
            {
                // Create Result
                ImageContext<T> Result = new ImageContext<T>(Width, Height);

                CopyPixelAction Handler = CreateCopyPixelHandler<T>();

                int DestStride = Result.Stride;
                byte* Dest0 = (byte*)Result.Scan0;
                Parallel.For(0, Height, (y) =>
                {
                    byte* Dest = Dest0 + DestStride * y;

                    int SourceX = Width - 1;
                    for (int x = 0; x < Width; x++)
                    {
                        Pixel Pixel = GetPixel(SourceX--, y);
                        Handler(ref Dest, Pixel.A, Pixel.R, Pixel.G, Pixel.B);
                        Dest++;
                    }
                });

                return Result;
            }

            return null;
        }
        protected override IImageContext FlipHandler(FlipMode Mode)
            => this.Flip(Mode);

        /// <summary>
        /// Creates a new cropped ImageContext&lt;<typeparamref name="Pixel"/>&gt;.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public ImageContext<Pixel> Crop(int X, int Y, int Width, int Height)
            => this.Crop<Pixel>(X, Y, Width, Height);
        /// <summary>
        /// Creates a new cropped ImageContext&lt;<typeparamref name="T"/>&gt;.
        /// </summary>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public unsafe ImageContext<T> Crop<T>(int X, int Y, int Width, int Height)
            where T : unmanaged, IPixel
        {
            // If the rectangle is completely out of the bitmap
            if (X > this.Width || Y > this.Height)
                return new ImageContext<T>(0, 0);

            // Clamp to boundaries
            X = Math.Max(X, 0);
            Y = Math.Max(Y, 0);
            Width = Math.Max(Math.Min(Width, this.Width - X), 0);
            Height = Math.Max(Math.Min(Height, this.Height - Y), 0);

            // Create Result
            ImageContext<T> Result = new ImageContext<T>(Width, Height);

            this.BlockCopy(X, Y, Width, Height, (byte*)Result.Scan0, Result.Stride, CreateCopyPixelHandler<T>());

            return Result;
        }
        protected override IImageContext CropHandler(int X, int Y, int Width, int Height)
            => this.Crop<Pixel>(X, Y, Width, Height);

        /// <summary>
        /// Creates a new filtered ImageContext&lt;<typeparamref name="Pixel"/>&gt;.
        /// </summary>
        /// <param name="Kernel">The kernel used for convolution.</param>
        /// <param name="KernelFactorSum">The factor used for the kernel summing.</param>
        /// <param name="KernelOffsetSum">The offset used for the kernel summing.</param>
        public ImageContext<Pixel> Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
        {
            int KH = Kernel.GetUpperBound(0) + 1,
                KW = Kernel.GetUpperBound(1) + 1;

            if ((KW & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((KH & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            KernelFactorSum = Math.Max(KernelFactorSum, 1);

            ImageContext<Pixel> Result = new ImageContext<Pixel>(this.Width, this.Height);

            int kwh = KW >> 1;
            int khh = KH >> 1;

            // Init Common Function
            Pixel[] GetVerticalPixels(int X, int Y)
            {
                Pixel[] Pixels = new Pixel[KH];
                for (int j = 0; j < KH; j++)
                    Pixels[j] = this.GetPixel(X, Math.Min(Math.Max(Y + j - khh, 0), this.Height - 1));

                return Pixels;
            };

            Parallel.For(0, this.Height, (y) =>
            {
                Queue<Pixel[]> PixelBlock = new Queue<Pixel[]>();

                // Init Block
                Pixel[] LeftBoundPixels = GetVerticalPixels(0, y);

                // Repeat pixels at borders
                for (int i = 0; i <= kwh; i++)
                    PixelBlock.Enqueue(LeftBoundPixels);

                // Fill pixels at center
                for (int i = 1; i < kwh; i++)
                    PixelBlock.Enqueue(GetVerticalPixels(Math.Min(i, this.Width - 1), y));

                for (int x = 0; x < this.Width; x++)
                {
                    int A = 0,
                        R = 0,
                        G = 0,
                        B = 0;

                    // Left Bound and not enqueue.
                    int KXIndex = 0;
                    Pixel[] Pixels = PixelBlock.Dequeue();
                    for (int j = 0; j < KH; j++)
                    {
                        int k = Kernel[j, KXIndex];
                        if (k == 0)
                            continue;

                        Pixel Pixel = Pixels[j];
                        A += Pixel.A * k;
                        R += Pixel.R * k;
                        G += Pixel.G * k;
                        B += Pixel.B * k;
                    }

                    for (KXIndex = 1; KXIndex < KW - 1; KXIndex++)
                    {
                        Pixels = PixelBlock.Dequeue();
                        for (int j = 0; j < KH; j++)
                        {
                            int k = Kernel[j, KXIndex];
                            if (k == 0)
                                continue;

                            Pixel Pixel = Pixels[j];
                            A += Pixel.A * k;
                            R += Pixel.R * k;
                            G += Pixel.G * k;
                            B += Pixel.B * k;
                        }

                        PixelBlock.Enqueue(Pixels);
                    }

                    // Right Bound and enqueue
                    Pixels = new Pixel[KH];
                    for (int j = 0; j < KH; j++)
                    {
                        Pixel Pixel = this.GetPixel(Math.Min(x + kwh, this.Width - 1), Math.Min(Math.Max(y + j - khh, 0), this.Height - 1));
                        Pixels[j] = Pixel;

                        int k = Kernel[j, KXIndex];
                        if (k == 0)
                            continue;

                        A += Pixel.A * k;
                        R += Pixel.R * k;
                        G += Pixel.G * k;
                        B += Pixel.B * k;
                    }

                    PixelBlock.Enqueue(Pixels);

                    Result.SetPixel(x, y, ToPixel((byte)Math.Min(Math.Max((A / KernelFactorSum) + KernelOffsetSum, 0), 255),
                                                  (byte)Math.Min(Math.Max((R / KernelFactorSum) + KernelOffsetSum, 0), 255),
                                                  (byte)Math.Min(Math.Max((G / KernelFactorSum) + KernelOffsetSum, 0), 255),
                                                  (byte)Math.Min(Math.Max((B / KernelFactorSum) + KernelOffsetSum, 0), 255)));
                }
            });

            return Result;
        }
        /// <summary>
        /// Creates a new filtered ImageContext&lt;<typeparamref name="Pixel"/>&gt;.
        /// </summary>
        /// <param name="Kernel">The kernel used for convolution.</param>
        public ImageContext<Pixel> Convolute(ConvoluteKernel Kernel)
            => Convolute(Kernel.Datas, Kernel.FactorSum, Kernel.Offset);
        protected override IImageContext ConvoluteHandler(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
            => this.Convolute(Kernel, KernelFactorSum, KernelOffsetSum);

        protected override IImageContext CastHandler<T>()
            => this.Cast<T>();
        protected override IImageContext CastHandler<T, U>()
            => this.Cast<T, U>();
        public unsafe ImageContext<T> Cast<T>()
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(this.Width, this.Height);

            this.BlockCopy(0, 0, this.Width, this.Height, (byte*)Result.Scan0, Result.Stride, CreateCopyPixelHandler<T>());

            return Result;
        }
        public ImageContext<T, U> Cast<T, U>()
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
        /// Rotates the bitmap in any degree returns a new rotated ImageContext&lt;<typeparamref name="Pixel"/>&gt;.
        /// </summary>
        /// <param name="Angle">Arbitrary angle in 360 Degrees (positive = clockwise).</param>
        /// <param name="Crop">if true: keep the size, false: adjust canvas to new size</param>
        /// <returns>A new WriteableBitmap that is a rotated version of the input.</returns>
        public ImageContext<Pixel> Rotate(double Angle, bool Crop = true)
        {
            // rotating clockwise, so it's negative relative to Cartesian quadrants
            double cnAngle = -Angle * MathHelper.UnitTheta;

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

            if (Crop)
            {
                newWidth = iWidth;
                newHeight = iHeight;
            }
            else
            {
                double Rad = Angle * MathHelper.UnitTheta;
                newWidth = (int)Math.Ceiling(Math.Abs(Math.Sin(Rad) * iHeight) + Math.Abs(Math.Cos(Rad) * iWidth));
                newHeight = (int)Math.Ceiling(Math.Abs(Math.Sin(Rad) * iWidth) + Math.Abs(Math.Cos(Rad) * iHeight));
            }


            iCentreX = iWidth >> 1;
            iCentreY = iHeight >> 1;

            iDestCentreX = newWidth >> 1;
            iDestCentreY = newHeight >> 1;

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
                    fTrueX += iCentreX;
                    fTrueY = iCentreY - fTrueY;

                    iFloorX = (int)Math.Floor(fTrueX);
                    iFloorY = (int)Math.Floor(fTrueY);
                    iCeilingX = (int)Math.Ceiling(fTrueX);
                    iCeilingY = (int)Math.Ceiling(fTrueY);

                    // check bounds
                    if (iFloorX < 0 || iCeilingX < 0 || iFloorX >= iWidth || iCeilingX >= iWidth || iFloorY < 0 ||
                        iCeilingY < 0 || iFloorY >= iHeight || iCeilingY >= iHeight)
                        continue;

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
                    if (iRed < 0)
                        iRed = 0;
                    if (iRed > 255)
                        iRed = 255;
                    if (iGreen < 0)
                        iGreen = 0;
                    if (iGreen > 255)
                        iGreen = 255;
                    if (iBlue < 0)
                        iBlue = 0;
                    if (iBlue > 255)
                        iBlue = 255;
                    if (iAlpha < 0)
                        iAlpha = 0;
                    if (iAlpha > 255)
                        iAlpha = 255;

                    Result.SetPixel(i, j, ToPixel((byte)iAlpha,
                                                  (byte)iRed,
                                                  (byte)iGreen,
                                                  (byte)iBlue));
                }
            }
            return Result;
        }

        protected override IImageContext CloneHandler()
            => this.Cast<Pixel>();
        public unsafe ImageContext<Pixel> Clone()
            => this.Cast<Pixel>();

    }
    public class ImageContext<Pixel, PixelIndexed> : ImageContextBase<Pixel, PixelIndexed>
        where Pixel : unmanaged, IPixel
        where PixelIndexed : unmanaged, IPixelIndexed
    {

        protected override IntPtr ScanA => throw new NotImplementedException();
        protected override IntPtr ScanR => throw new NotImplementedException();
        protected override IntPtr ScanG => throw new NotImplementedException();
        protected override IntPtr ScanB => throw new NotImplementedException();

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

        /// <summary>
        /// Create a new flipped ImageContext&lt;<typeparamref name="Pixel"/>, <typeparamref name="PixelIndexed"/>&gt;.
        /// </summary>
        /// <param name="Mode">The flip mode.</param>
        public unsafe ImageContext<Pixel, PixelIndexed> Flip(FlipMode Mode)
        {
            if (Mode == FlipMode.Vertical)
            {
                ImageContext<Pixel, PixelIndexed> Result = new ImageContext<Pixel, PixelIndexed>(this.Width, this.Height);

                foreach (Pixel color in this.Palette)
                    Result.Palette.Add(color);

                int dStride = Result.Stride;
                byte* sScan0 = (byte*)this.Scan0,
                      dScan0 = (byte*)Result.Scan0;
                Parallel.For(0, this.Height, (y) =>
                {
                    PixelIndexed* sIndexed = (PixelIndexed*)(sScan0 + this.Stride * (this.Height - y - 1)),
                                  dIndexed = (PixelIndexed*)(dScan0 + dStride * y);

                    for (int i = 0; i < Result.Stride; i++)
                        *dIndexed++ = *sIndexed++;
                });

                return Result;
            }
            else if (Mode == FlipMode.Horizontal)
            {
                ImageContext<Pixel, PixelIndexed> Result = new ImageContext<Pixel, PixelIndexed>(this.Width, this.Height);

                foreach (Pixel Color in this.Palette)
                    Result.Palette.Add(Color);

                int dStride = Result.Stride,
                    dXBit = Result.Width * Result.BitsPerPixel;
                byte* sScan0 = (byte*)this.Scan0,
                      dScan0 = (byte*)Result.Scan0 + (dXBit >> 3);
                Parallel.For(0, this.Height, (y) =>
                {
                    PixelIndexed* sIndexed = (PixelIndexed*)(sScan0 + this.Stride * y),
                                  dIndexed = (PixelIndexed*)(dScan0 + dStride * y);

                    PixelIndexed sPixel = *sIndexed,
                                 dPixel = *dIndexed;

                    int dWidth = Result.Width,
                        dIndexLength = dPixel.Length - 1,
                        sIndexLength = sPixel.Length - 1,
                        dIndex = dXBit % dPixel.Length,
                        sIndex = 0;

                    for (int i = 0; i < dWidth;)
                    {
                        dPixel[dIndex--] = sPixel[sIndex++];
                        i++;
                        if (dIndex < 0)
                        {
                            *dIndexed-- = dPixel;
                            dPixel = *dIndexed;
                            dIndex = dIndexLength;
                        }
                        if (sIndex >= sIndexLength)
                        {
                            *sIndexed++ = sPixel;
                            sPixel = *sIndexed;
                            sIndex = 0;
                        }
                    }
                });

                return Result;
            }

            return this;
        }
        protected override IImageContext FlipHandler(FlipMode Mode)
            => this.Flip(Mode);

        /// <summary>
        /// Creates a new cropped ImageContext&lt;<typeparamref name="Pixel"/>, <typeparamref name="PixelIndexed"/>&gt;.
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
        protected override IImageContext CropHandler(int X, int Y, int Width, int Height)
            => this.Crop(X, Y, Width, Height);

        /// <summary>
        /// Creates a new filtered ImageContext&lt;<typeparamref name="Pixel"/>, <typeparamref name="PixelIndexed"/>&gt;.
        /// </summary>
        /// <param name="Kernel">The kernel used for convolution.</param>
        /// <param name="KernelFactorSum">The factor used for the kernel summing.</param>
        /// <param name="KernelOffsetSum">The offset used for the kernel summing.</param>
        public ImageContext<Pixel, PixelIndexed> Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
        {
            int KH = Kernel.GetUpperBound(0) + 1,
                KW = Kernel.GetUpperBound(1) + 1;

            if ((KW & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((KH & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            ImageContext<Pixel, PixelIndexed> Result = new ImageContext<Pixel, PixelIndexed>(this.Width, this.Height);

            int kwh = KW >> 1;
            int khh = KH >> 1;

            // Init Common Function
            Pixel[] GetVerticalPixels(int X, int Y)
            {
                Pixel[] Pixels = new Pixel[KH];
                for (int j = 0; j < KH; j++)
                    Pixels[j] = this.GetPixel(X, Math.Min(Math.Max(Y + j - khh, 0), this.Height - 1));

                return Pixels;
            };

            Parallel.For(0, this.Height, (y) =>
            {
                Queue<Pixel[]> PixelBlock = new Queue<Pixel[]>();

                // Init Block
                Pixel[] LeftBoundPixels = GetVerticalPixels(0, y);

                // Repeat pixels at borders
                for (int i = 0; i <= kwh; i++)
                    PixelBlock.Enqueue(LeftBoundPixels);

                // Fill pixels at center
                for (int i = 1; i < kwh; i++)
                    PixelBlock.Enqueue(GetVerticalPixels(Math.Min(i, this.Width - 1), y));

                for (int x = 0; x < this.Width; x++)
                {
                    int A = 0,
                        R = 0,
                        G = 0,
                        B = 0;

                    // Left Bound and not enqueue.
                    int KXIndex = 0;
                    Pixel[] Pixels = PixelBlock.Dequeue();
                    for (int j = 0; j < KH; j++)
                    {
                        int k = Kernel[j, KXIndex];
                        if (k == 0)
                            continue;

                        Pixel Pixel = Pixels[j];
                        A += Pixel.A * k;
                        R += Pixel.R * k;
                        G += Pixel.G * k;
                        B += Pixel.B * k;
                    }

                    for (KXIndex = 1; KXIndex < KW - 1; KXIndex++)
                    {
                        Pixels = PixelBlock.Dequeue();
                        for (int j = 0; j < KH; j++)
                        {
                            int k = Kernel[j, KXIndex];
                            if (k == 0)
                                continue;

                            Pixel Pixel = Pixels[j];
                            A += Pixel.A * k;
                            R += Pixel.R * k;
                            G += Pixel.G * k;
                            B += Pixel.B * k;
                        }

                        PixelBlock.Enqueue(Pixels);
                    }

                    // Right Bound and enqueue
                    Pixels = new Pixel[KH];
                    for (int j = 0; j < KH; j++)
                    {
                        Pixel Pixel = this.GetPixel(Math.Min(x + kwh, this.Width - 1), Math.Min(Math.Max(y + j - khh, 0), this.Height - 1));
                        Pixels[j] = Pixel;

                        int k = Kernel[j, KXIndex];
                        if (k == 0)
                            continue;

                        A += Pixel.A * k;
                        R += Pixel.R * k;
                        G += Pixel.G * k;
                        B += Pixel.B * k;
                    }

                    PixelBlock.Enqueue(Pixels);

                    Result.SetPixel(x, y, ToPixel((byte)Math.Min(Math.Max((A / KernelFactorSum) + KernelOffsetSum, 0), 255),
                                                  (byte)Math.Min(Math.Max((R / KernelFactorSum) + KernelOffsetSum, 0), 255),
                                                  (byte)Math.Min(Math.Max((G / KernelFactorSum) + KernelOffsetSum, 0), 255),
                                                  (byte)Math.Min(Math.Max((B / KernelFactorSum) + KernelOffsetSum, 0), 255)));
                }
            });

            return Result;
        }
        /// <summary>
        /// Creates a new filtered ImageContext&lt;<typeparamref name="Pixel"/>, <typeparamref name="PixelIndexed"/>&gt;.
        /// </summary>
        /// <param name="Kernel">The kernel used for convolution.</param>
        public ImageContext<Pixel, PixelIndexed> Convolute(ConvoluteKernel Kernel)
            => Convolute(Kernel.Datas, Kernel.FactorSum, Kernel.Offset);
        protected override IImageContext ConvoluteHandler(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
            => this.Convolute(Kernel, KernelFactorSum, KernelOffsetSum);

        protected override IImageContext CastHandler<T>()
            => this.Cast<T>();
        protected override IImageContext CastHandler<T, U>()
            => this.Cast<T, U>();
        public unsafe ImageContext<T> Cast<T>()
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(this.Width, this.Height);

            this.BlockCopy(0, 0, this.Width, this.Height, (byte*)Result.Scan0, Result.Stride, CreateCopyPixelHandler<T>());

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

        protected override IImageContext CloneHandler()
            => this.Clone();
        public unsafe ImageContext<Pixel, PixelIndexed> Clone()
        {
            ImageContext<Pixel, PixelIndexed> Result = new ImageContext<Pixel, PixelIndexed>(this.Width, this.Height);

            foreach (Pixel color in this.Palette)
                Result.Palette.Add(color);

            PixelIndexed* pIndexed = (PixelIndexed*)this.Scan0,
                          dIndexed = (PixelIndexed*)Result.Scan0;
            Parallel.For(0, this.Height, (y) =>
            {
                for (int i = 0; i < Result.Stride; i++)
                    *dIndexed++ = *pIndexed++;
            });

            return Result;
        }

    }

}