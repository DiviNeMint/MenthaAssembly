using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents an image with the specified pixel type and index struct type.
    /// </summary>
    /// <typeparam name="Pixel">The specified pixel type.</typeparam>
    /// <typeparam name="Struct">The specified index struct type.</typeparam>
    public unsafe class ImageContext<Pixel, Struct> : IImageIndexedContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelIndexed
    {
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();
        private static readonly Type PixelType = typeof(Pixel);
        private static readonly Type StructType = typeof(Struct);

        public int Width { get; }

        public int Height { get; }

        public long Stride { get; }

        public int BitsPerPixel { get; }

        Type IImageContext.PixelType => PixelType;

        Type IImageIndexedContext.StructType => StructType;

        /// <summary>
        /// Gets/Sets the pixel at the specified location for this image.
        /// </summary>
        /// <param name="X">The x-coordinate of the specified location.</param>
        /// <param name="Y">The y-coordinate of the specified location.</param>
        public Pixel this[int X, int Y]
        {
            get
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                Pixel p = default;
                GetAdapter<Pixel>(X, Y).OverrideTo(&p);
                return p;
            }
            set
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                GetAdapter<Pixel>(X, Y).Override(value);
            }
        }
        IReadOnlyPixel IImageContext.this[int X, int Y]
        {
            get => this[X, Y];
            set => this[X, Y] = value.ToPixel<Pixel>();
        }

        private readonly SafeHandle _UnmanagedScan0;
        private readonly IntPtr _Scan0;
        /// <summary>
        /// Gets the data pointer for this image.
        /// </summary>
        public IntPtr Scan0
            => _UnmanagedScan0?.DangerousGetHandle() ?? _Scan0;
        IntPtr[] IImageContext.Scan0
            => new IntPtr[] { Scan0 };

        /// <summary>
        /// Gets the palette for this image.
        /// </summary>
        public ImagePalette<Pixel> Palette { get; }
        IImagePalette IImageIndexedContext.Palette
            => Palette;

        private ImageContext()
        {
            BitsPerPixel = default(Struct).BitsPerPixel;
        }

        /// <summary>
        /// Initializes a new image.
        /// </summary>
        /// <param name="Width">The specified width.</param>
        /// <param name="Height">The specified height.</param>
        public ImageContext(int Width, int Height) : this()
        {
            this.Width = Width;
            this.Height = Height;

            Stride = (Width * BitsPerPixel + 7) >> 3;
            _UnmanagedScan0 = new HGlobalIntPtr(Stride * Height);
            Palette = ImagePalette<Pixel>.GetSystemPalette<Struct>();
        }

        /// <summary>
        /// Initializes a new image.
        /// </summary>
        /// <param name="Width">The specified width.</param>
        /// <param name="Height">The specified height.</param>
        /// <param name="Scan0">The specified pointer to the data.</param>
        /// <param name="Palette">The specified palette.</param>
        public ImageContext(int Width, int Height, IntPtr Scan0, ImagePalette<Pixel> Palette) :
            this(Width, Height, Scan0, (Width * default(Struct).BitsPerPixel + 7) >> 3, Palette)
        {
        }
        /// <summary>
        /// Initializes a new image.
        /// </summary>
        /// <param name="Width">The specified width.</param>
        /// <param name="Height">The specified height.</param>
        /// <param name="Scan0">The specified pointer to the data.</param>
        /// <param name="Stride">The specified stride of <paramref name="Scan0"/>.</param>
        /// <param name="Palette">The specified palette.</param>
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, ImagePalette<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this._Scan0 = Scan0;
            this.Stride = Stride;
            this.Palette = Palette ?? ImagePalette<Pixel>.GetSystemPalette<Struct>();
        }

        /// <summary>
        /// Initializes a new image.
        /// </summary>
        /// <param name="Width">The specified width.</param>
        /// <param name="Height">The specified height.</param>
        /// <param name="Data">The specified image data.</param>
        /// <param name="Palette">The specified palette.</param>
        public ImageContext(int Width, int Height, byte[] Data, ImagePalette<Pixel> Palette) : this()
        {
            this.Width = Width;
            this.Height = Height;

            _UnmanagedScan0 = new PinnedIntPtr(Data);
            Stride = Data.Length / Height;

            this.Palette = Palette ?? ImagePalette<Pixel>.GetSystemPalette<Struct>();
        }

        #region Graphic Processing

        #region Line Rendering

        #region Line
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Pen, BlendMode.Overlay);
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen, BlendMode Blend)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Pen, Blend);
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
            => DrawLine(X0, Y0, X1, Y1, Pen, BlendMode.Overlay);
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen, BlendMode Blend)
        {
            int X = X0 - (Pen.Width >> 1),
                Y = Y0 - (Pen.Height >> 1);

            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, Math.Abs(DeltaX), Math.Abs(DeltaY), (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen, Blend));
        }

        public void DrawLine<T>(Point<int> P0, Point<int> P1, T Color) where T : unmanaged, IPixel
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Color, BlendMode.Overlay);
        public void DrawLine<T>(Point<int> P0, Point<int> P1, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Color, Blend);
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, T Color) where T : unmanaged, IPixel
            => DrawLine(X0, Y0, X1, Y1, Color, BlendMode.Overlay);
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Color),
                BlendMode.Overlay => a => a.Overlay(Color),
                _ => throw new NotSupportedException(),
            };

            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            Dictionary<int, int> LeftBound = new Dictionary<int, int>(),
                                 RightBound = new Dictionary<int, int>();
            #region Line Body Bound
            int MaxX = Width - 1,
                RTx,
                RTy;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, (Dx, Dy) =>
            {
                RTy = Y0 + Dy;
                if (-1 < RTy && RTy < Height)
                {
                    RTx = Math.Min(Math.Max(X0 + Dx, 0), MaxX);

                    // Left
                    if (LeftBound.TryGetValue(RTy, out int LastRx))
                    {
                        if (LastRx > RTx)
                            LeftBound[RTy] = RTx;
                    }
                    else
                    {
                        LeftBound[RTy] = RTx;
                    }

                    // Right
                    if (RightBound.TryGetValue(RTy, out LastRx))
                    {
                        if (LastRx < RTx)
                            RightBound[RTy] = RTx;
                    }
                    else
                    {
                        RightBound[RTy] = RTx;
                    }
                }
            });

            #endregion
            #region Fill
            PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);
            foreach (KeyValuePair<int, int> Data in RightBound)
            {
                int Y = Data.Key,
                    TRx = Data.Value;
                if (LeftBound.TryGetValue(Y, out int TLx))
                {
                    LeftBound.Remove(Y);

                    Adapter.DangerousMove(TLx, Y);
                    for (; TLx <= TRx; TLx++, Adapter.DangerousMoveNextX())
                        Handler(Adapter);
                }
                else
                {
                    Adapter.DangerousMove(TRx, Y);
                    Handler(Adapter);
                }
            }
            RightBound.Clear();

            foreach (KeyValuePair<int, int> Data in LeftBound)
            {
                Adapter.DangerousMove(Data.Value, Data.Key);
                Handler(Adapter);
            }

            LeftBound.Clear();

            #endregion
        }

        public void DrawLine<T>(Point<int> P0, Point<int> P1, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Contour, Fill, BlendMode.Overlay);
        public void DrawLine<T>(Point<int> P0, Point<int> P1, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Contour, Fill, Blend);
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawLine(X0, Y0, X1, Y1, Contour, Fill, BlendMode.Overlay);
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            Bound<int> Bound = Contour.Bound;
            if (Bound.IsEmpty)
                return;

            if (Bound.Width == 1 && Bound.Height == 1)
            {
                DrawLine(X0, Y0, X1, Y1, Fill);
                return;
            }

            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            bool IsHollow = false;
            Dictionary<int, int> LeftBound = new Dictionary<int, int>(),
                                 RightBound = new Dictionary<int, int>();
            #region Pen Bound
            int MaxX = Width - 1,
                PCx = (Bound.Left + Bound.Right) >> 1,
                PCy = (Bound.Top + Bound.Bottom) >> 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            foreach (KeyValuePair<int, ImageContourScanLine> Item in Contour)
            {
                int j = Item.Key;
                ImageContourScanLine Data = Item.Value;
                if (Data.Length > 2)
                {
                    IsHollow = true;
                    LeftBound.Clear();
                    RightBound.Clear();
                    break;
                }

                int Ty = j - PCy;
                // Found Left Bound
                {
                    int Tx = Data[0] - PCx,
                        Predict = DeltaX * Ty - DeltaY * Tx,
                        Distance = Math.Abs(Predict);

                    if (Predict > 0)    // UpperLine
                    {
                        if (UpperDistance < Distance)
                        {
                            UpperDistance = Distance;
                            DUx = Tx;
                            DUy = Ty;
                        }
                    }
                    else                // LowerLine
                    {
                        if (LowerDistance < Distance)
                        {
                            LowerDistance = Distance;
                            DLx = Tx;
                            DLy = Ty;
                        }
                    }

                    // StartPoint
                    int Rx = Math.Min(Math.Max(Tx + X0, 0), MaxX),
                        Ry = Ty + Y0;
                    if (-1 < Ry && Ry < Height &&
                        (!LeftBound.TryGetValue(Ry, out int LastRx) || LastRx > Rx))
                        LeftBound[Ry] = Rx;

                    // EndPoint
                    Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                    Ry = Ty + Y1;
                    if (-1 < Ry && Ry < Height &&
                        (!LeftBound.TryGetValue(Ry, out LastRx) || LastRx > Rx))
                        LeftBound[Ry] = Rx;
                }

                // Found Right Bound
                {
                    int Tx = Data[Data.Length - 1] - PCx,
                        Predict = DeltaX * Ty - DeltaY * Tx,
                        Distance = Math.Abs(Predict);

                    if (Predict > 0)    // UpperLine
                    {
                        if (UpperDistance < Distance)
                        {
                            UpperDistance = Distance;
                            DUx = Tx;
                            DUy = Ty;
                        }
                    }
                    else                // LowerLine
                    {
                        if (LowerDistance < Distance)
                        {
                            LowerDistance = Distance;
                            DLx = Tx;
                            DLy = Ty;
                        }
                    }

                    // StartPoint
                    int Rx = Math.Min(Math.Max(Tx + X0, 0), MaxX),
                        Ry = Ty + Y0;

                    if (-1 < Ry && Ry < Height &&
                        (!RightBound.TryGetValue(Ry, out int LastRx) || LastRx < Rx))
                        RightBound[Ry] = Rx;

                    // EndPoint
                    Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                    Ry = Ty + Y1;

                    if (-1 < Ry && Ry < Height &&
                        (!RightBound.TryGetValue(Ry, out LastRx) || LastRx < Rx))
                        RightBound[Ry] = Rx;
                }
            }

            #endregion

            if (IsHollow)
            {
                #region Line Body Bound
                ImageContour LineContour = new ImageContour(),
                             Stroke = ImageContour.Offset(Contour, X0 - PCx, Y0 - PCy);

                int LastDx = 0,
                    LastDy = 0;

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, (Dx, Dy) =>
                {
                    Stroke.Offset(Dx - LastDx, Dy - LastDy);
                    LineContour.Union(Stroke);

                    LastDx = Dx;
                    LastDy = Dy;
                });
                #endregion

                this.Contour(Contour, 0d, 0d, Handler);
            }
            else
            {
                #region Line Body Bound
                int Ux = X0 + DUx,
                    Uy = Y0 + DUy,
                    Lx = X0 + DLx,
                    Ly = Y0 + DLy,
                    RTx, RTy;

                if (DeltaX == 0 && DeltaY < 0)
                {
                    MathHelper.Swap(ref Ux, ref Lx);
                    MathHelper.Swap(ref Uy, ref Ly);
                }

                GraphicDeltaHandler FoundLineBodyBound = DeltaX * DeltaY < 0 ?
                    new GraphicDeltaHandler(
                        (Dx, Dy) =>
                        {
                            // Right
                            RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                            RTy = Uy + Dy;
                            if (-1 < RTy && RTy < Height &&
                                (!RightBound.TryGetValue(RTy, out int LastRx) || LastRx < RTx))
                                RightBound[RTy] = RTx;

                            // Left
                            RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                            RTy = Ly + Dy;
                            if (-1 < RTy && RTy < Height &&
                                (!LeftBound.TryGetValue(RTy, out LastRx) || LastRx > RTx))
                                LeftBound[RTy] = RTx;
                        }) :
                        (Dx, Dy) =>
                        {
                            // Left
                            RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                            RTy = Uy + Dy;
                            if (-1 < RTy && RTy < Height &&
                                (!LeftBound.TryGetValue(RTy, out int LastRx) || LastRx > RTx))
                                LeftBound[RTy] = RTx;

                            // Right
                            RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                            RTy = Ly + Dy;
                            if (-1 < RTy && RTy < Height &&
                                (!RightBound.TryGetValue(RTy, out LastRx) || LastRx < RTx))
                                RightBound[RTy] = RTx;
                        };

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, FoundLineBodyBound);

                #endregion
                #region Fill
                PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);
                foreach (KeyValuePair<int, int> Data in RightBound)
                {
                    int Y = Data.Key,
                        TRx = Data.Value;
                    if (LeftBound.TryGetValue(Y, out int TLx))
                    {
                        LeftBound.Remove(Y);

                        Adapter.DangerousMove(TLx, Y);
                        for (; TLx <= TRx; TLx++, Adapter.DangerousMoveNextX())
                            Handler(Adapter);
                    }
                    else
                    {
                        Adapter.DangerousMove(TRx, Y);
                        Handler(Adapter);
                    }
                }
                RightBound.Clear();

                foreach (KeyValuePair<int, int> Data in LeftBound)
                {
                    Adapter.DangerousMove(Data.Value, Data.Key);
                    Handler(Adapter);
                }

                LeftBound.Clear();

                #endregion
            }
        }

        #endregion

        #region Arc
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Pen, BlendMode.Overlay);
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen, BlendMode Blend)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Pen, Blend);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Pen, BlendMode.Overlay);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen, BlendMode Blend)
        {
            int X = Cx - (Pen.Width >> 1),
                Y = Cy - (Pen.Height >> 1);

            GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy, Ex - Cx, Ey - Cy, Rx, Ry, Clockwise, false,
                (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen, Blend));
        }

        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, T Color) where T : unmanaged, IPixel
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Color, BlendMode.Overlay);
        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Color, Blend);
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, T Color) where T : unmanaged, IPixel
            => DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Color, BlendMode.Overlay);
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Color),
                BlendMode.Overlay => a => a.Overlay(Color),
                _ => throw new NotSupportedException(),
            };

            int Tx, Ty;
            PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);
            GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy,
                                                   Ex - Cx, Ey - Cy,
                                                   Rx, Ry,
                                                   Clockwise,
                                                   false,
                                                   (Dx, Dy) =>
                                                   {
                                                       Tx = Cx + Dx;
                                                       if (Tx < 0 || Width <= Tx)
                                                           return;

                                                       Ty = Cy + Dy;
                                                       if (Ty < 0 || Height <= Ty)
                                                           return;

                                                       Adapter.DangerousMove(Tx, Ty);
                                                       Handler(Adapter);
                                                   });
        }

        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Contour, Fill, BlendMode.Overlay);
        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Contour, Fill, Blend);
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Contour, Fill, BlendMode.Overlay);
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            ImageContour ArcContour = new();
            Bound<int> Bound = Contour.Bound;

            if (Bound.IsEmpty)
                return;

            if (Bound.Width == 1 && Bound.Height == 1)
            {
                DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Fill);
                return;
            }

            bool IsHollow = Contour.Any(i => i.Value.Length > 2);
            int PCx = (Bound.Left + Bound.Right) >> 1,
                PCy = (Bound.Top + Bound.Bottom) >> 1,
                DSx = Sx - Cx,
                DSy = Sy - Cy,
                DEx = Ex - Cx,
                DEy = Ey - Cy;

            if (IsHollow)
            {
                ImageContour Stroke = ImageContour.Offset(Contour, Cx - PCx, Cy - PCy);

                int LastDx = 0,
                    LastDy = 0;
                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, false,
                    (Dx, Dy) =>
                    {
                        Stroke.Offset(Dx - LastDx, Dy - LastDy);
                        ArcContour.Union(Stroke);

                        LastDx = Dx;
                        LastDy = Dy;
                    });
            }
            else
            {
                Dictionary<int, int> LargeLeftBound = new(),
                                     LargeRightBound = new(),
                                     SmallLeftBound = new(),
                                     SmallRightBound = new();

                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, false,
                    (Dx, Dy) =>
                    {
                        int OffsetX = Dx + Cx - PCx,
                            OffsetY = Dy + Cy - PCy;
                        if (Dx < 0)
                        {
                            foreach (KeyValuePair<int, ImageContourScanLine> item in Contour)
                            {
                                ImageContourScanLine Data = item.Value;
                                int Ty = item.Key + OffsetY;

                                if (Ty < 0)
                                    continue;

                                if (Height <= Ty)
                                    break;

                                int LLTx = Data[0] + OffsetX,
                                    MLTx = Data[1] + OffsetX;

                                if (!LargeLeftBound.TryGetValue(Ty, out int RLLx) || LLTx < RLLx)
                                    LargeLeftBound[Ty] = LLTx;

                                if (!SmallLeftBound.TryGetValue(Ty, out int RMLx) || RMLx < MLTx)
                                    SmallLeftBound[Ty] = MLTx;
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<int, ImageContourScanLine> item in Contour)
                            {
                                ImageContourScanLine Data = item.Value;
                                int Ty = item.Key + OffsetY;

                                if (Ty < 0)
                                    continue;

                                if (Height <= Ty)
                                    break;

                                int LRTx = Data[1] + OffsetX,
                                    MRTx = Data[0] + OffsetX;

                                if (!LargeRightBound.TryGetValue(Ty, out int RLRx) || RLRx < LRTx)
                                    LargeRightBound[Ty] = LRTx;

                                if (!SmallRightBound.TryGetValue(Ty, out int RMRx) || MRTx < RMRx)
                                    SmallRightBound[Ty] = MRTx;
                            }
                        }
                    });

                foreach (KeyValuePair<int, int> item in LargeLeftBound)
                {
                    int X0 = item.Value,
                        Y = item.Key;

                    if (SmallLeftBound.TryGetValue(Y, out int X1))
                    {
                        ArcContour[Y].Union(X0, X1);
                        SmallLeftBound.Remove(Y);
                        continue;
                    }

                    if (LargeRightBound.TryGetValue(Y, out X1))
                    {
                        ArcContour[Y].Union(X0, X1);
                        LargeRightBound.Remove(Y);
                    }
                }
                LargeLeftBound.Clear();

                foreach (KeyValuePair<int, int> item in SmallRightBound)
                {
                    int X0 = item.Value,
                        Y = item.Key;

                    if (LargeRightBound.TryGetValue(Y, out int X1))
                    {
                        ArcContour[Y].Union(X0, X1);
                        LargeRightBound.Remove(Y);
                    }
                }
                SmallRightBound.Clear();
            }

            this.Contour(ArcContour, 0d, 0d, Handler);
        }

        #endregion

        #region Curve
        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen)
            => DrawCurve(Points, Tension, Pen, BlendMode.Overlay);
        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen, BlendMode Blend)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen, Blend);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0], Points[1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < Points.Count - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                       Points[i], Points[i + 1],
                                                       Points[i + 2], Points[i + 3],
                                                       Points[i + 4], Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                   Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[i + 2], Points[i + 3],
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurve(IList<Point<int>> Points, float Tension, IImageContext Pen)
            => DrawCurve(Points, Tension, Pen, BlendMode.Overlay);
        public void DrawCurve(IList<Point<int>> Points, float Tension, IImageContext Pen, BlendMode Blend)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen, Blend);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0].X, Points[0].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < Points.Count - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                   Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurve<T>(IList<int> Points, float Tension, T Color) where T : unmanaged, IPixel
            => DrawCurve(Points, Tension, Color, BlendMode.Overlay);
        public void DrawCurve<T>(IList<int> Points, float Tension, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color, Blend);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0], Points[1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < Points.Count - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                       Points[i], Points[i + 1],
                                                       Points[i + 2], Points[i + 3],
                                                       Points[i + 4], Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                   Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[i + 2], Points[i + 3],
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, T Color) where T : unmanaged, IPixel
            => DrawCurve(Points, Tension, Color, BlendMode.Overlay);
        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color, Blend);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0].X, Points[0].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < Points.Count - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                   Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurve<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawCurve(Points, Tension, Contour, Fill, BlendMode.Overlay);
        public void DrawCurve<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill, Blend);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0], Points[1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < Points.Count - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                       Points[i], Points[i + 1],
                                                       Points[i + 2], Points[i + 3],
                                                       Points[i + 4], Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                   Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[i + 2], Points[i + 3],
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawCurve(Points, Tension, Contour, Fill, BlendMode.Overlay);
        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill, Blend);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0].X, Points[0].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < Points.Count - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                   Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen)
            => DrawCurveClosed(Points, Tension, Pen, BlendMode.Overlay);
        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen, BlendMode Blend)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen, Blend);

            int pn = Points.Count;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 2], Points[pn - 1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < pn - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2],
                                                       Points[i - 1], Points[i],
                                                       Points[i + 1], Points[i + 2],
                                                       Points[i + 3], Points[i + 4],
                                                       Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2],
                                                   Points[i - 1], Points[i],
                                                   Points[i + 1], Points[i + 2],
                                                   Points[i + 3], Points[0],
                                                   Points[1],
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IImageContext Pen)
            => DrawCurveClosed(Points, Tension, Pen, BlendMode.Overlay);
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IImageContext Pen, BlendMode Blend)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen, Blend);

            int pn = Points.Count;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 1].X, Points[pn - 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < pn - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X,
                                                   Points[i - 1].Y, Points[i].X,
                                                   Points[i].Y, Points[i + 1].X,
                                                   Points[i + 1].Y, Points[0].X,
                                                   Points[0].Y,
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurveClosed<T>(IList<int> Points, float Tension, T Color) where T : unmanaged, IPixel
            => DrawCurveClosed(Points, Tension, Color, BlendMode.Overlay);
        public void DrawCurveClosed<T>(IList<int> Points, float Tension, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color, Blend);

            int pn = Points.Count;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 2], Points[pn - 1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < pn - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2],
                                                       Points[i - 1], Points[i],
                                                       Points[i + 1], Points[i + 2],
                                                       Points[i + 3], Points[i + 4],
                                                       Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2],
                                                   Points[i - 1], Points[i],
                                                   Points[i + 1], Points[i + 2],
                                                   Points[i + 3], Points[0],
                                                   Points[1],
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, T Color) where T : unmanaged, IPixel
            => DrawCurveClosed(Points, Tension, Color, BlendMode.Overlay);
        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color, Blend);

            int pn = Points.Count;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 1].X, Points[pn - 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < pn - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X,
                                                   Points[i - 1].Y, Points[i].X,
                                                   Points[i].Y, Points[i + 1].X,
                                                   Points[i + 1].Y, Points[0].X,
                                                   Points[0].Y,
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurveClosed<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawCurveClosed(Points, Tension, Contour, Fill, BlendMode.Overlay);
        public void DrawCurveClosed<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill, Blend);

            int pn = Points.Count;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 2], Points[pn - 1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < pn - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2],
                                                       Points[i - 1], Points[i],
                                                       Points[i + 1], Points[i + 2],
                                                       Points[i + 3], Points[i + 4],
                                                       Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2],
                                                   Points[i - 1], Points[i],
                                                   Points[i + 1], Points[i + 2],
                                                   Points[i + 3], Points[0],
                                                   Points[1],
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Tension,
                                                   DrawHandler);
        }

        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawCurveClosed(Points, Tension, Contour, Fill, BlendMode.Overlay);
        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill, Blend);

            int pn = Points.Count;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 1].X, Points[pn - 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < pn - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X,
                                                   Points[i - 1].Y, Points[i].X,
                                                   Points[i].Y, Points[i + 1].X,
                                                   Points[i + 1].Y, Points[0].X,
                                                   Points[0].Y,
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Tension,
                                                   DrawHandler);
        }


        #endregion

        #region Bezier

        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, T Color) where T : unmanaged, IPixel
            => DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Color, BlendMode.Overlay);
        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                       Cx1, Cy1,
                                                       Cx2, Cy2,
                                                       X2, Y2,
                                                       (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Color, Blend));
        }

        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Contour, Fill, BlendMode.Overlay);
        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                       Cx1, Cy1,
                                                       Cx2, Cy2,
                                                       X2, Y2,
                                                       (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill, Blend));
        }

        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen)
            => DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Pen, BlendMode.Overlay);
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen, BlendMode Blend)
            => GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                          Cx1, Cy1,
                                                          Cx2, Cy2,
                                                          X2, Y2,
                                                          (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Pen, Blend));

        public void DrawBeziers(IList<int> Points, IImageContext Pen)
            => DrawBeziers(Points, Pen, BlendMode.Overlay);
        public void DrawBeziers(IList<int> Points, IImageContext Pen, BlendMode Blend)
        {
            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen, Blend);

            for (int i = 2; i + 5 < Points.Count; i += 6)
            {
                x2 = Points[i + 4];
                y2 = Points[i + 5];
                GraphicAlgorithm.CalculateBezierLinePoints(x1, y1,
                                                           Points[i], Points[i + 1],
                                                           Points[i + 2], Points[i + 3],
                                                           x2, y2,
                                                           DrawHandler);

                x1 = x2;
                y1 = y2;
            }
        }

        public void DrawBeziers<T>(IList<int> Points, T Color) where T : unmanaged, IPixel
            => DrawBeziers(Points, Color, BlendMode.Overlay);
        public void DrawBeziers<T>(IList<int> Points, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color, Blend);

            for (int i = 2; i + 5 < Points.Count; i += 6)
            {
                x2 = Points[i + 4];
                y2 = Points[i + 5];
                GraphicAlgorithm.CalculateBezierLinePoints(x1, y1,
                                                           Points[i], Points[i + 1],
                                                           Points[i + 2], Points[i + 3],
                                                           x2, y2,
                                                           DrawHandler);

                x1 = x2;
                y1 = y2;
            }
        }

        public void DrawBeziers<T>(IList<int> Points, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawBeziers(Points, Contour, Fill, BlendMode.Overlay);
        public void DrawBeziers<T>(IList<int> Points, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill, Blend);

            for (int i = 2; i + 5 < Points.Count; i += 6)
            {
                x2 = Points[i + 4];
                y2 = Points[i + 5];
                GraphicAlgorithm.CalculateBezierLinePoints(x1, y1,
                                                           Points[i], Points[i + 1],
                                                           Points[i + 2], Points[i + 3],
                                                           x2, y2,
                                                           DrawHandler);

                x1 = x2;
                y1 = y2;
            }
        }

        #endregion

        #region Other
        ///// <summary>
        ///// Draws a colored dotted line
        ///// </summary>
        ///// <param name="X0">The x-coordinate of the start point.</param>
        ///// <param name="Y0">The y-coordinate of the start point.</param>
        ///// <param name="X1">The x-coordinate of the end point.</param>
        ///// <param name="Y1">The y-coordinate of the end point.</param>
        ///// <param name="DotSpace">length of space between each line segment</param>
        ///// <param name="DotLength">length of each line segment</param>
        ///// <param name="Color">The color for the line.</param>
        //public void DrawDottedLine(int X0, int Y0, int X1, int Y1, int DotSpace, int DotLength, Pixel Color)
        //{
        //    if (X0 == X1)       // Vertically
        //    {
        //        if (Y1 < Y0)
        //            MathHelper.Swap(ref Y0, ref Y1);

        //        // Draw
        //        {
        //            if (X0 < 0 || X0 > Width)
        //                return;

        //            bool on = true;
        //            int spaceCnt = 0;
        //            for (int i = Y0; i <= Y1; i++)
        //            {
        //                if (i < 1)
        //                    continue;

        //                if (i >= Height)
        //                    break;

        //                if (on)
        //                {
        //                    this.Operator.SetPixel(X0, i - 1, Color);

        //                    on = i % DotLength != 0;
        //                    spaceCnt = 0;
        //                }
        //                else
        //                {
        //                    spaceCnt++;
        //                    on = spaceCnt % DotSpace == 0;
        //                }
        //            }
        //        }
        //    }
        //    else if (Y0 == Y1)  // Horizontally
        //    {
        //        if (X1 < X0)
        //            MathHelper.Swap(ref X0, ref X1);

        //        // Draw
        //        {
        //            if (Y0 < 0 || Y0 > Height)
        //                return;

        //            bool on = true;
        //            int spaceCnt = 0;
        //            for (int i = X0; i <= X1; i++)
        //            {
        //                if (i < 1)
        //                    continue;

        //                if (i >= Width)
        //                    break;

        //                if (Y0 >= Height)
        //                    break;

        //                if (on)
        //                {
        //                    this.Operator.SetPixel(i - 1, Y0, Color);

        //                    on = i % DotLength != 0;
        //                    spaceCnt = 0;
        //                }
        //                else
        //                {
        //                    spaceCnt++;
        //                    on = spaceCnt % DotSpace == 0;
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (X1 < X0)
        //        {
        //            MathHelper.Swap(ref X0, ref X1);
        //            MathHelper.Swap(ref Y0, ref Y1);
        //        }

        //        float m = (Y1 - Y0) / (float)(X1 - X0),
        //              n = Y0 - m * X0;

        //        bool on = true;
        //        int spaceCnt = 0;
        //        for (int i = X0; i <= Width; i++)
        //        {
        //            if (i == 0)
        //                continue;

        //            int y = (int)(m * i + n);
        //            if (y <= 0)
        //                continue;

        //            if (y >= Height || i >= X1)
        //                continue;

        //            if (on)
        //            {
        //                this.Operator.SetPixel(i - 1, y - 1, Color);

        //                spaceCnt = 0;
        //                on = i % DotLength != 0;
        //            }
        //            else
        //            {
        //                spaceCnt++;
        //                on = spaceCnt % DotSpace == 0;
        //            }
        //        }
        //    }
        //}

        #endregion

        #endregion

        #region Shape Rendering

        #region Triangle
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen)
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Pen, BlendMode.Overlay);
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen, BlendMode Blend)
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Pen, Blend);

        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen)
            => DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Pen, BlendMode.Overlay);
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen, BlendMode Blend)
        {
            DrawLine(X1, Y1, X2, Y2, Pen, Blend);
            DrawLine(X2, Y2, X3, Y3, Pen, Blend);
            DrawLine(X3, Y3, X1, Y1, Pen, Blend);
        }

        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, T Color) where T : unmanaged, IPixel
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Color, BlendMode.Overlay);
        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Color, Blend);

        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, T Color) where T : unmanaged, IPixel
            => DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Color, BlendMode.Overlay);
        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            DrawLine(X1, Y1, X2, Y2, Color, Blend);
            DrawLine(X2, Y2, X3, Y3, Color, Blend);
            DrawLine(X3, Y3, X1, Y1, Color, Blend);
        }

        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Contour, Fill, BlendMode.Overlay);
        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Contour, Fill, Blend);

        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Contour, Fill, BlendMode.Overlay);
        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            DrawLine(X1, Y1, X2, Y2, Contour, Fill, Blend);
            DrawLine(X2, Y2, X3, Y3, Contour, Fill, Blend);
            DrawLine(X3, Y3, X1, Y1, Contour, Fill, Blend);
        }

        #endregion

        #region Rectangle
        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen)
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Pen, BlendMode.Overlay);
        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen, BlendMode Blend)
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Pen, Blend);

        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen)
            => DrawRectangle(X1, Y1, X2, Y2, Pen, BlendMode.Overlay);
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen, BlendMode Blend)
        {
            // Check boundaries
            if ((X1 < 0 && X2 < 0) ||
                (Y1 < 0 && Y2 < 0) ||
                (X1 >= Width && X2 >= Width) ||
                (Y1 >= Height && Y2 >= Height))
                return;

            // Clamp boundaries
            MathHelper.MinAndMax(out int L, out int R, X1, X2);
            MathHelper.MinAndMax(out int T, out int B, Y1, Y2);

            DrawLine(L, T, R, T, Pen, Blend);
            DrawLine(L, B, R, B, Pen, Blend);
            DrawLine(L, T, L, B, Pen, Blend);
            DrawLine(R, T, R, B, Pen, Blend);
        }

        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, T Color) where T : unmanaged, IPixel
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Color, BlendMode.Overlay);
        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Color, Blend);

        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, T Color) where T : unmanaged, IPixel
            => DrawRectangle(X1, Y1, X2, Y2, Color, BlendMode.Overlay);
        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            // Check boundaries
            if ((X1 < 0 && X2 < 0) ||
                (Y1 < 0 && Y2 < 0) ||
                (X1 >= Width && X2 >= Width) ||
                (Y1 >= Height && Y2 >= Height))
                return;

            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            // Clamp boundaries
            MathHelper.MinAndMax(out int L, out int R, X1, X2);
            MathHelper.MinAndMax(out int Ty, out int By, Y1, Y2);

            DrawLine(L, Ty, R, Ty, Color, Blend);
            DrawLine(L, By, R, By, Color, Blend);
            DrawLine(L, Ty, L, By, Color, Blend);
            DrawLine(R, Ty, R, By, Color, Blend);
        }

        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Contour, Fill, BlendMode.Overlay);
        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Contour, Fill, Blend);

        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawRectangle(X1, Y1, X2, Y2, Contour, Fill, BlendMode.Overlay);
        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            // Check boundaries
            if ((X1 < 0 && X2 < 0) ||
                (Y1 < 0 && Y2 < 0) ||
                (X1 >= Width && X2 >= Width) ||
                (Y1 >= Height && Y2 >= Height))
                return;

            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            // Clamp boundaries
            MathHelper.MinAndMax(out int L, out int R, X1, X2);
            MathHelper.MinAndMax(out int Ty, out int By, Y1, Y2);

            DrawLine(L, Ty, R, Ty, Contour, Fill, Blend);
            DrawLine(L, By, R, By, Contour, Fill, Blend);
            DrawLine(L, Ty, L, By, Contour, Fill, Blend);
            DrawLine(R, Ty, R, By, Contour, Fill, Blend);
        }

        #endregion

        #region Quad
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen)
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Pen, BlendMode.Overlay);
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen, BlendMode Blend)
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Pen, Blend);

        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen)
            => DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Pen, BlendMode.Overlay);
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen, BlendMode Blend)
        {
            DrawLine(X1, Y1, X2, Y2, Pen, Blend);
            DrawLine(X2, Y2, X3, Y3, Pen, Blend);
            DrawLine(X3, Y3, X4, Y4, Pen, Blend);
            DrawLine(X4, Y4, X1, Y1, Pen, Blend);
        }

        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, T Color) where T : unmanaged, IPixel
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Color, BlendMode.Overlay);
        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Color, Blend);

        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, T Color) where T : unmanaged, IPixel
            => DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Color, BlendMode.Overlay);
        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            DrawLine(X1, Y1, X2, Y2, Color, Blend);
            DrawLine(X2, Y2, X3, Y3, Color, Blend);
            DrawLine(X3, Y3, X4, Y4, Color, Blend);
            DrawLine(X4, Y4, X1, Y1, Color, Blend);
        }

        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Contour, Fill, BlendMode.Overlay);
        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Contour, Fill, Blend);

        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Contour, Fill, BlendMode.Overlay);
        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            DrawLine(X1, Y1, X2, Y2, Contour, Fill, Blend);
            DrawLine(X2, Y2, X3, Y3, Contour, Fill, Blend);
            DrawLine(X3, Y3, X4, Y4, Contour, Fill, Blend);
            DrawLine(X4, Y4, X1, Y1, Contour, Fill, Blend);
        }

        #endregion

        #region Ellipse
        public void DrawEllipse(Bound<int> Bound, IImageContext Pen)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Pen, BlendMode.Overlay);
        }
        public void DrawEllipse(Bound<int> Bound, IImageContext Pen, BlendMode Blend)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Pen, Blend);
        }

        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Pen, BlendMode.Overlay);
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen, BlendMode Blend)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Pen, Blend);

        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen)
            => DrawEllipse(Cx, Cy, Rx, Ry, Pen, BlendMode.Overlay);
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen, BlendMode Blend)
        {
            int X = Cx - (Pen.Width >> 1),
                Y = Cy - (Pen.Height >> 1);

            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry, (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen, Blend));
        }

        public void DrawEllipse<T>(Bound<int> Bound, T Color)
            where T : unmanaged, IPixel
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Color, BlendMode.Overlay);
        }
        public void DrawEllipse<T>(Bound<int> Bound, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Color, Blend);
        }

        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, T Color) where T : unmanaged, IPixel
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Color, BlendMode.Overlay);
        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Color, Blend);

        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Color) where T : unmanaged, IPixel
            => DrawEllipse(Cx, Cy, Rx, Ry, Color, BlendMode.Overlay);
        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Color, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Color),
                BlendMode.Overlay => a => a.Overlay(Color),
                _ => throw new NotSupportedException(),
            };

            int Tx, Ty;
            PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);
            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry,
                (Dx, Dy) =>
                {
                    Tx = Cx + Dx;
                    if (Tx < 0 || Width <= Tx)
                        return;

                    Ty = Cy + Dy;
                    if (Ty < 0 || Height <= Ty)
                        return;

                    Adapter.DangerousMove(Tx, Ty);
                    Handler(Adapter);
                });
        }

        public void DrawEllipse<T>(Bound<int> Bound, ImageContour Contour, T Fill)
            where T : unmanaged, IPixel
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Contour, Fill, BlendMode.Overlay);
        }
        public void DrawEllipse<T>(Bound<int> Bound, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Contour, Fill, Blend);
        }

        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Contour, Fill, BlendMode.Overlay);
        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Contour, Fill, Blend);

        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => DrawEllipse(Cx, Cy, Rx, Ry, Contour, Fill, BlendMode.Overlay);
        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            ImageContour EllipseContour = new();

            Bound<int> Bound = Contour.Bound;
            ImageContour Stroke = ImageContour.Offset(Contour, Cx - (Bound.Width >> 1), Cy - (Bound.Height >> 1));

            int LastDx = 0,
                LastDy = 0;
            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry,
                (Dx, Dy) =>
                {
                    Stroke.Offset(Dx - LastDx, Dy - LastDy);
                    EllipseContour.Union(Stroke);

                    LastDx = Dx;
                    LastDy = Dy;
                });

            this.Contour(EllipseContour, 0d, 0d, Handler);
        }

        public void FillEllipse<T>(Bound<int> Bound, T Fill) where T : unmanaged, IPixel
            => FillEllipse(Bound, Fill, BlendMode.Overlay);
        public void FillEllipse<T>(Bound<int> Bound, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            FillEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Fill, Blend);
        }

        public void FillEllipse<T>(Point<int> Center, int Rx, int Ry, T Fill) where T : unmanaged, IPixel
            => FillEllipse(Center.X, Center.Y, Rx, Ry, Fill, BlendMode.Overlay);
        public void FillEllipse<T>(Point<int> Center, int Rx, int Ry, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => FillEllipse(Center.X, Center.Y, Rx, Ry, Fill, Blend);

        public void FillEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Fill) where T : unmanaged, IPixel
            => FillEllipse(Cx, Cy, Rx, Ry, Fill, BlendMode.Overlay);
        public void FillEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Fill, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            ImageContour.CreateFillEllipse(Cx, Cy, Rx, Ry);
            this.Contour(ImageContour.CreateFillEllipse(Cx, Cy, Rx, Ry), 0, 0, Handler);
        }

        #endregion

        #region Polygon
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, double StartAngle = 0d)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Pen, BlendMode.Overlay, StartAngle);
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, BlendMode Blend, double StartAngle = 0d)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Pen, Blend, StartAngle);

        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, double StartAngle = 0d)
            => DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Pen, BlendMode.Overlay, StartAngle);
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, BlendMode Blend, double StartAngle = 0d)
        {
            if (VertexNum < 3)
                throw new ArgumentException($"VertexNum must more than or equal 3.");

            double DeltaTheta = 360d / VertexNum,
                   LastAngle = StartAngle;

            int P0x = Cx + (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                P0y = Cy + (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta)),
                LastPx = P0x,
                LastPy = P0y;

            for (int i = 1; i < VertexNum; i++)
            {
                LastAngle += DeltaTheta;
                int Px = Cx + (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                    Py = Cy + (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta));

                DrawLine(LastPx, LastPy, Px, Py, Pen, Blend);

                LastPx = Px;
                LastPy = Py;
            }

            DrawLine(LastPx, LastPy, P0x, P0y, Pen, Blend);
        }

        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, T Color, double StartAngle = 0d) where T : unmanaged, IPixel
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Color, BlendMode.Overlay, StartAngle);
        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, T Color, BlendMode Blend, double StartAngle = 0d) where T : unmanaged, IPixel
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Color, Blend, StartAngle);

        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, T Color, double StartAngle = 0d) where T : unmanaged, IPixel
            => DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Color, BlendMode.Overlay, StartAngle);
        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, T Color, BlendMode Blend, double StartAngle = 0d)
            where T : unmanaged, IPixel
        {
            if (VertexNum < 3)
                throw new ArgumentException($"VertexNum must more than or equal 3.");

            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Color.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            double DeltaTheta = 360d / VertexNum,
                   LastAngle = StartAngle;

            int P0x = Cx + (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                P0y = Cy + (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta)),
                LastPx = P0x,
                LastPy = P0y;

            for (int i = 1; i < VertexNum; i++)
            {
                LastAngle += DeltaTheta;
                int Px = Cx + (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                    Py = Cy + (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta));

                DrawLine(LastPx, LastPy, Px, Py, Color, Blend);

                LastPx = Px;
                LastPy = Py;
            }

            DrawLine(LastPx, LastPy, P0x, P0y, Color, Blend);
        }

        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, T Fill, double StartAngle = 0d) where T : unmanaged, IPixel
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Contour, Fill, BlendMode.Overlay, StartAngle);
        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, T Fill, BlendMode Blend, double StartAngle = 0d) where T : unmanaged, IPixel
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Contour, Fill, Blend, StartAngle);

        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, T Fill, double StartAngle = 0d) where T : unmanaged, IPixel
            => DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Contour, Fill, BlendMode.Overlay, StartAngle);
        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, T Fill, BlendMode Blend, double StartAngle = 0d)
            where T : unmanaged, IPixel
        {
            if (VertexNum < 3)
                throw new ArgumentException($"VertexNum must more than or equal 3.");

            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            double DeltaTheta = 360d / VertexNum,
                   LastAngle = StartAngle;

            int P0x = Cx + (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                P0y = Cy + (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta)),
                LastPx = P0x,
                LastPy = P0y;

            for (int i = 1; i < VertexNum; i++)
            {
                LastAngle += DeltaTheta;
                int Px = Cx + (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                    Py = Cy + (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta));

                DrawLine(LastPx, LastPy, Px, Py, Contour, Fill, Blend);

                LastPx = Px;
                LastPy = Py;
            }

            DrawLine(LastPx, LastPy, P0x, P0y, Contour, Fill, Blend);
        }

        public void FillPolygon<T>(IEnumerable<Point<int>> Vertices, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel
            => FillPolygon(Vertices, Fill, OffsetX, OffsetY, BlendMode.Overlay);
        public void FillPolygon<T>(IEnumerable<Point<int>> Vertices, T Fill, int OffsetX, int OffsetY, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            Point<int>[] Datas = GraphicAlgorithm.CropPolygon(Vertices, -OffsetX - 1, -OffsetY - 1, Width - OffsetX, Height - OffsetY);

            int Length = Datas.Length;

            int[] IntersectionsX = new int[Length - 1],
                  HorizontalX = new int[Length << 1];

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = Height,
                yMax = 0;
            for (int i = 1; i < Length; i++)
            {
                int py = Datas[i].Y + OffsetY;
                if (py < yMin)
                    yMin = py;

                if (py > yMax)
                    yMax = py;
            }

            if (yMin < 0)
                yMin = 0;

            if (yMax >= Height)
                yMax = Height - 1;

            // Scan line from min to max
            PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);
            for (int y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                Point<int> P0 = Datas[0];
                float X0 = P0.X + OffsetX,
                      Y0 = P0.Y + OffsetY;

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                int IntersectionCount = 0,
                    HorizontalCount = 0;
                for (int i = 1; i < Length; i++)
                {
                    // Next point x, y
                    Point<int> P1 = Datas[i];
                    float X1 = P1.X + OffsetX,
                          Y1 = P1.Y + OffsetY;

                    // Is the scanline between the two points
                    if ((Y0 < y && y <= Y1) ||
                        (Y1 < y && y <= Y0))
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        IntersectionsX[IntersectionCount++] = (int)(X0 + (y - Y0) * (X1 - X0) / (Y1 - Y0));
                    }
                    else if (Y0 == Y1 && Y0 == y)
                    {
                        HorizontalX[HorizontalCount++] = (int)X0;
                        HorizontalX[HorizontalCount++] = (int)X1;
                    }

                    X0 = X1;
                    Y0 = Y1;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                int t, j;
                for (int i = 1; i < IntersectionCount; i++)
                {
                    t = IntersectionsX[i];
                    j = i;
                    while (j > 0 && IntersectionsX[j - 1] > t)
                    {
                        IntersectionsX[j] = IntersectionsX[j - 1];
                        j -= 1;
                    }
                    IntersectionsX[j] = t;
                }

                // Fill the pixels between the intersections
                for (int i = 0; i < IntersectionCount - 1; i += 2)
                {
                    int x0 = IntersectionsX[i],
                        x1 = IntersectionsX[i + 1];

                    // Check boundary
                    if (x1 > 0 && x0 < Width)
                    {
                        if (x0 < 0)
                            x0 = 0;

                        if (x1 >= Width)
                            x1 = Width - 1;

                        // Fill the pixels
                        Adapter.DangerousMove(x0, y);
                        for (; x0 <= x1; x0++, Adapter.DangerousMoveNextX())
                            Handler(Adapter);
                    }
                }

                // Fill the pixels between the horizontals
                for (int i = 0; i < HorizontalCount - 1; i += 2)
                {
                    int x0 = HorizontalX[i],
                        x1 = HorizontalX[i + 1];

                    // Check boundary
                    if (x1 > 0 && x0 < Width)
                    {
                        if (x0 < 0)
                            x0 = 0;

                        if (x1 >= Width)
                            x1 = Width - 1;

                        // Fill the pixels
                        Adapter.DangerousMove(x0, y);
                        for (; x0 <= x1; x0++, Adapter.DangerousMoveNextX())
                            Handler(Adapter);
                    }
                }
            }
        }

        public void FillPolygon<T>(IEnumerable<int> VerticeDatas, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel
            => FillPolygon(VerticeDatas, Fill, OffsetX, OffsetY, BlendMode.Overlay);
        public void FillPolygon<T>(IEnumerable<int> VerticeDatas, T Fill, int OffsetX, int OffsetY, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            int[] Datas = GraphicAlgorithm.CropPolygon(VerticeDatas, -OffsetX - 1, -OffsetY - 1, Width - OffsetX, Height - OffsetY);

            int pn = Datas.Length,
                pnh = pn >> 1;

            int[] IntersectionsX = new int[pnh - 1],
                  HorizontalX = new int[pn];

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = Height,
                yMax = 0;
            for (int i = 1; i < pn; i += 2)
            {
                int py = Datas[i] + OffsetY;
                if (py < yMin)
                    yMin = py;

                if (py > yMax)
                    yMax = py;
            }

            if (yMin < 0)
                yMin = 0;

            if (yMax >= Height)
                yMax = Height - 1;

            // Scan line from min to max
            PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);
            for (int y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                float X0 = Datas[0] + OffsetX,
                      Y0 = Datas[1] + OffsetY;

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                int IntersectionCount = 0,
                    HorizontalCount = 0;
                for (int i = 2; i < pn; i += 2)
                {
                    // Next point x, y
                    float X1 = Datas[i] + OffsetX,
                          Y1 = Datas[i + 1] + OffsetY;

                    // Is the scanline between the two points
                    if ((Y0 < y && y <= Y1) ||
                        (Y1 < y && y <= Y0))
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        IntersectionsX[IntersectionCount++] = (int)(X0 + (y - Y0) * (X1 - X0) / (Y1 - Y0));
                    }
                    else if (Y0 == Y1 && Y0 == y)
                    {
                        HorizontalX[HorizontalCount++] = (int)X0;
                        HorizontalX[HorizontalCount++] = (int)X1;
                    }

                    X0 = X1;
                    Y0 = Y1;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                int t, j;
                for (int i = 1; i < IntersectionCount; i++)
                {
                    t = IntersectionsX[i];
                    j = i;
                    while (j > 0 && IntersectionsX[j - 1] > t)
                    {
                        IntersectionsX[j] = IntersectionsX[j - 1];
                        j -= 1;
                    }
                    IntersectionsX[j] = t;
                }

                // Fill the pixels between the intersections
                for (int i = 0; i < IntersectionCount - 1; i += 2)
                {
                    int x0 = IntersectionsX[i],
                        x1 = IntersectionsX[i + 1];

                    // Check boundary
                    if (x1 > 0 && x0 < Width)
                    {
                        if (x0 < 0)
                            x0 = 0;

                        if (x1 >= Width)
                            x1 = Width - 1;

                        // Fill the pixels
                        Adapter.DangerousMove(x0, y);
                        for (; x0 <= x1; x0++, Adapter.DangerousMoveNextX())
                            Handler(Adapter);
                    }
                }

                // Fill the pixels between the horizontals
                for (int i = 0; i < HorizontalCount - 1; i += 2)
                {
                    int x0 = HorizontalX[i],
                        x1 = HorizontalX[i + 1];

                    // Check boundary
                    if (x1 > 0 && x0 < Width)
                    {
                        if (x0 < 0)
                            x0 = 0;

                        if (x1 >= Width)
                            x1 = Width - 1;

                        // Fill the pixels
                        Adapter.DangerousMove(x0, y);
                        for (; x0 <= x1; x0++, Adapter.DangerousMoveNextX())
                            Handler(Adapter);
                    }
                }
            }
        }

        #endregion

        #region Other
        public void DrawStamp(Point<int> Position, IImageContext Stamp)
            => DrawStamp(Position.X, Position.Y, Stamp, BlendMode.Overlay);
        public void DrawStamp(int X, int Y, IImageContext Stamp)
            => DrawStamp(X, Y, Stamp, BlendMode.Overlay);
        public void DrawStamp(Point<int> Position, IImageContext Stamp, BlendMode Blend)
            => DrawStamp(Position.X, Position.Y, Stamp, Blend);
        public void DrawStamp(int X, int Y, IImageContext Stamp, BlendMode Blend)
        {
            int Width = Stamp.Width,
                Height = Stamp.Height,
                Sx = X - (Width >> 1),
                Sy = Y - (Height >> 1),
                Ex = Sx + Width,
                Ey = Sy + Height,
                SourceX = 0,
                SourceY = 0;

            if (Sx < 0)
            {
                SourceX -= Sx;
                Sx = 0;
            }

            if (Sy < 0)
            {
                SourceY -= Sy;
                Sy = 0;
            }

            Width = Math.Min(Ex, this.Width - 1) - Sx;
            if (Width < 1)
                return;

            Height = Math.Min(Ey, this.Height - 1) - Sy;
            if (Height < 1)
                return;

            PixelAdapter<Pixel> Sorc = Stamp.GetAdapter<Pixel>(SourceX, SourceY),
                                Dest = GetAdapter<Pixel>(Sx, Sy);
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                {
                    if (Blend == BlendMode.Overlay)
                    {
                        byte Alpha = Sorc.A;
                        if (Alpha == byte.MinValue)
                            continue;

                        if (Alpha == byte.MaxValue)
                            Dest.Override(Sorc);
                        else
                            Dest.Overlay(Sorc);
                    }
                    else
                    {
                        Dest.Override(Sorc);
                    }
                }

                Sorc.OffsetX(-Width);
                Dest.OffsetX(-Width);
            }
        }

        public void FillContour<T>(IImageContour Contour, T Fill, double OffsetX, double OffsetY) where T : unmanaged, IPixel
            => FillContour(Contour, Fill, OffsetX, OffsetY, BlendMode.Overlay);
        public void FillContour<T>(IImageContour Contour, T Fill, double OffsetX, double OffsetY, BlendMode Blend)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            this.Contour(Contour, OffsetX, OffsetY, Handler);
        }

        public void SeedFill<T>(Point<int> SeedPoint, T Fill, ImagePredicate Predicate) where T : unmanaged, IPixel
            => SeedFill(SeedPoint.X, SeedPoint.Y, Fill, BlendMode.Overlay, Predicate);
        public void SeedFill<T>(int SeedX, int SeedY, T Fill, ImagePredicate Predicate) where T : unmanaged, IPixel
            => SeedFill(SeedX, SeedY, Fill, BlendMode.Overlay, Predicate);
        public void SeedFill<T>(Point<int> SeedPoint, T Fill, BlendMode Blend, ImagePredicate Predicate) where T : unmanaged, IPixel
            => SeedFill(SeedPoint.X, SeedPoint.Y, Fill, Blend, Predicate);
        public void SeedFill<T>(int SeedX, int SeedY, T Fill, BlendMode Blend, ImagePredicate Predicate)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            if (this.FindBound(SeedX, SeedY, Predicate) is ImageContour Contour)
            {
                this.Contour(Contour, 0d, 0d, Handler);
                Contour.Clear();
            }
        }

        #endregion

        #endregion

        #region Text Rendering
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, null, CharSize, Fill, BlendMode.Overlay, 0d, FontWeightType.Normal, false);
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, null, CharSize, Fill, BlendMode.Overlay, Angle, Weight, Italic);
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, null, CharSize, Fill, Blend, 0d, FontWeightType.Normal, false);
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, BlendMode Blend, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, null, CharSize, Fill, Blend, Angle, Weight, Italic);
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, FontName, CharSize, Fill, BlendMode.Overlay, 0d, FontWeightType.Normal, false);
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, FontName, CharSize, Fill, BlendMode.Overlay, Angle, Weight, Italic);
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => DrawText(X, Y, Text, FontName, CharSize, Fill, Blend, 0d, FontWeightType.Normal, false);
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, BlendMode Blend, double Angle, FontWeightType Weight, bool Italic)
            where T : unmanaged, IPixel
        {
            if (Blend == BlendMode.Overlay)
            {
                byte Alpha = Fill.A;
                if (Alpha == byte.MinValue)
                    return;

                if (Alpha == byte.MaxValue)
                    Blend = BlendMode.Overlay;
            }

            ImageContour Contour = ImageContour.CreateTextContour(X, Y, Text, FontName, CharSize, Angle, Weight, Italic);
            Action<PixelAdapter<T>> Handler = Blend switch
            {
                BlendMode.Override => a => a.Override(Fill),
                BlendMode.Overlay => a => a.Overlay(Fill),
                _ => throw new NotSupportedException(),
            };

            this.Contour<T>(Contour, 0d, 0d, Handler);
        }

        #endregion

        #endregion

        #region Transform Processing
        public ImageContext<T> Rotate<T>(double Angle, InterpolationTypes Interpolation) where T : unmanaged, IPixel
        {
            if (Angle % 360d == 0)
                return Cast<T>();

            PixelAdapter<T> Sorc = Interpolation switch
            {
                InterpolationTypes.Nearest => new NearestRotatePixelAdapter<T>(this, Angle),
                InterpolationTypes.Bilinear => new BilinearRotatePixelAdapter<T>(this, Angle),
                _ => throw new NotSupportedException($"Not support InterpolationTypes.{Interpolation}."),
            };

            int Nw = Sorc.XLength,
                Nh = Sorc.YLength,
                Dx = -Nw;

            ImageContext<T> Result = new(Nw, Nh);
            PixelAdapter<T> Dest = Result.GetAdapter<T>(0, 0);
            for (int j = 0; j < Nh; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Nw; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Result;
        }
        public ImageContext<T> Rotate<T>(double Angle, InterpolationTypes Interpolation, ParallelOptions Options) where T : unmanaged, IPixel
        {
            if (Angle % 360d == 0)
                return Cast<T>(Options ?? DefaultParallelOptions);

            PixelAdapter<T> RotateAdapter = Interpolation switch
            {
                InterpolationTypes.Nearest => new NearestRotatePixelAdapter<T>(this, Angle),
                InterpolationTypes.Bilinear => new BilinearRotatePixelAdapter<T>(this, Angle),
                _ => throw new NotSupportedException($"Not support InterpolationTypes.{Interpolation}."),
            };

            int Nw = RotateAdapter.XLength,
                Nh = RotateAdapter.YLength;

            ImageContext<T> Result = new(Nw, Nh);
            _ = Parallel.For(0, Nh, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = RotateAdapter.Clone(),
                                Dest = Result.GetAdapter<T>(0, j);

                Sorc.DangerousOffsetY(j);
                for (int i = 0; i < Nw; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);
            });

            return Result;
        }

        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new(Width, Height);

            switch (Interpolation)
            {
                case InterpolationTypes.Nearest:
                    {
                        PixelAdapter<T> Sorc = new NearestResizePixelAdapter<T>(this, Width, Height),
                                        Dest = Result.GetAdapter<T>(0, 0);

                        int Dx = -Width;
                        for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
                        {
                            for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);

                            Sorc.DangerousOffsetX(Dx);
                            Dest.DangerousOffsetX(Dx);
                        }

                        break;
                    }
                case InterpolationTypes.Bilinear:
                    {
                        PixelAdapter<T> Sorc = new BilinearResizePixelAdapter<T>(this, Width, Height),
                                        Dest = Result.GetAdapter<T>(0, 0);

                        int Dx = -Width;
                        for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
                        {
                            for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);

                            Sorc.DangerousOffsetX(Dx);
                            Dest.DangerousOffsetX(Dx);
                        }
                        break;
                    }
            }

            return Result;
        }
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new(Width, Height);
            switch (Interpolation)
            {
                case InterpolationTypes.Nearest:
                    {
                        float StepX = (float)this.Width / Width,
                              StepY = (float)this.Height / Height;
                        _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
                        {
                            PixelAdapter<T> Sorc = new NearestResizePixelAdapter<T>(this, 0, j, StepX, StepY),
                                            Dest = Result.GetAdapter<T>(0, j);

                            for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);
                        });
                        break;
                    }
                case InterpolationTypes.Bilinear:
                    {
                        float StepX = (float)this.Width / Width,
                              StepY = (float)this.Height / Height;

                        _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
                        {
                            PixelAdapter<T> Sorc = new BilinearResizePixelAdapter<T>(this, 0, j, StepX, StepY),
                                            Dest = Result.GetAdapter<T>(0, j);

                            for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);
                        });
                        break;
                    }
            }

            return Result;
        }

        public ImageContext<T> Flip<T>(FlipMode Mode)
            where T : unmanaged, IPixel
        {
            switch (Mode)
            {
                case FlipMode.Vertical:
                    {
                        ImageContext<T> Result = new(Width, Height);
                        PixelAdapter<T> Sorc = GetAdapter<T>(0, Height - 1),
                                        Dest = Result.GetAdapter<T>(0, 0);

                        int Dx = -Width;
                        for (int j = 0; j < Height; j++, Sorc.DangerousMovePreviousY(), Dest.DangerousMoveNextY())
                        {
                            for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);

                            Sorc.DangerousOffsetX(Dx);
                            Dest.DangerousOffsetX(Dx);
                        }

                        return Result;
                    }
                case FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new(Width, Height);
                        PixelAdapter<T> Sorc = GetAdapter<T>(Width - 1, 0),
                                        Dest = Result.GetAdapter<T>(0, 0);

                        int Dx = -Width;
                        for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
                        {
                            for (int i = 0; i < Width; i++, Sorc.DangerousMovePreviousX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);

                            Sorc.DangerousOffsetX(Width);
                            Dest.DangerousOffsetX(Dx);
                        }

                        return Result;
                    }
                case FlipMode.Vertical | FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new(Width, Height);
                        PixelAdapter<T> Sorc = GetAdapter<T>(Width - 1, Height - 1),
                                        Dest = Result.GetAdapter<T>(0, 0);

                        int Dx = -Width;
                        for (int j = 0; j < Height; j++, Sorc.DangerousMovePreviousY(), Dest.DangerousMoveNextY())
                        {
                            for (int i = 0; i < Width; i++, Sorc.DangerousMovePreviousX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);

                            Sorc.DangerousOffsetX(Width);
                            Dest.DangerousOffsetX(Dx);
                        }

                        return Result;
                    }
            }

            return Cast<T>();
        }
        public ImageContext<T> Flip<T>(FlipMode Mode, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            switch (Mode)
            {
                case FlipMode.Vertical:
                    {
                        ImageContext<T> Result = new(Width, Height);

                        int MaxY = Height - 1;
                        _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, (y) =>
                        {
                            PixelAdapter<T> Sorc = GetAdapter<T>(0, MaxY - y),
                                            Dest = Result.GetAdapter<T>(0, y);
                            for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);
                        });

                        return Result;
                    }
                case FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new(Width, Height);

                        int MaxX = Width - 1;
                        _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, (y) =>
                        {
                            PixelAdapter<T> Sorc = GetAdapter<T>(MaxX, y),
                                            Dest = Result.GetAdapter<T>(0, y);
                            for (int i = 0; i < Width; i++, Sorc.DangerousMovePreviousX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);
                        });

                        return Result;
                    }
                case FlipMode.Vertical | FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new(Width, Height);

                        int MaxX = Width - 1,
                            MaxY = Height - 1;
                        _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, (y) =>
                        {
                            PixelAdapter<T> Sorc = GetAdapter<T>(MaxX, MaxY - y),
                                            Dest = Result.GetAdapter<T>(0, y);
                            for (int i = 0; i < Width; i++, Sorc.DangerousMovePreviousX(), Dest.DangerousMoveNextX())
                                Dest.Override(Sorc);
                        });

                        return Result;
                    }
            }

            return Cast<T>(Options);
        }

        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height)
            where T : unmanaged, IPixel
        {
            // If the rectangle is completely out of the bitmap
            if (this.Width <= X || this.Height <= Y)
                return new(0, 0);

            // Clamp to boundaries
            if (X < 0)
            {
                Width = Math.Min(Width + X, this.Width);
                X = 0;
            }
            else
            {
                Width = Width.Clamp(0, this.Width - X);
            }

            if (Y < 0)
            {
                Height = Math.Min(Height + Y, this.Height);
                Y = 0;
            }
            else
            {
                Height = Height.Clamp(0, this.Height - Y);
            }

            if (Width <= 0 || Height <= 0)
                return new(0, 0);

            // Create Result
            ImageContext<T> Result = new(Width, Height);
            PixelAdapter<T> Sorc = GetAdapter<T>(X, Y),
                            Dest = Result.GetAdapter<T>(0, 0);

            int Dx = -Width;
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Result;
        }
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            // If the rectangle is completely out of the bitmap
            if (this.Width <= X || this.Height <= Y)
                return new(0, 0);

            // Clamp to boundaries
            if (X < 0)
            {
                Width = Math.Min(Width + X, this.Width);
                X = 0;
            }
            else
            {
                Width = Width.Clamp(0, this.Width - X);
            }

            if (Y < 0)
            {
                Height = Math.Min(Height + Y, this.Height);
                Y = 0;
            }
            else
            {
                Height = Height.Clamp(0, this.Height - Y);
            }

            if (Width <= 0 || Height <= 0)
                return new(0, 0);

            // Create Result
            ImageContext<T> Result = new(Width, Height);
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = GetAdapter<T>(X, Y + j),
                                Dest = Result.GetAdapter<T>(0, j);

                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);
            });

            return Result;
        }

        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel) where T : unmanaged, IPixel
            => Filter<T>(Kernel);
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel, ParallelOptions Options) where T : unmanaged, IPixel
            => Filter<T>(Kernel, Options);

        public ImageContext<T> Filter<T>(ImageFilter Filter)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new(Width, Height);
            PixelAdapter<T> Sorc = new FilterPixelAdapter<T>(this, Filter),
                            Dest = Result.GetAdapter<T>(0, 0);

            int Dx = -Width;
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Result;
        }
        public ImageContext<T> Filter<T>(ImageFilter Filter, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new(Width, Height);
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, y =>
            {
                PixelAdapter<T> Sorc = new FilterPixelAdapter<T>(this, 0, y, Filter),
                                Dest = Result.GetAdapter<T>(0, y);

                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);
            });

            return Result;
        }

        public ImageContext<T> Quantizate<T>(QuantizationTypes Type, int Count)
            where T : unmanaged, IPixel
        {
            if (Count < 2)
                throw new ArgumentOutOfRangeException($"Parameter {nameof(Count)} must greater than 1.");

            ImageContext<T> Result = new(Width, Height);
            PixelAdapter<T> Sorc = GetAdapter<T>(0, 0),
                            Dest = Result.GetAdapter<T>(0, 0);

            T[] Colors;
            Func<PixelAdapter<T>, int> GetColorIndex;
            switch (Type)
            {
                case QuantizationTypes.KMeans:
                    {
                        QuantizationCluster[] Clusters = ImageContextHelper.ClusterQuantize(Sorc, Count,
                                                                                            out Func<QuantizationCluster, PixelAdapter<T>, int> GetDistanceConst,
                                                                                            out Func<QuantizationCluster, T> GetColor)
                                                                           .ToArray();

                        Colors = Clusters.Select(c => GetColor(c)).ToArray();
                        GetColorIndex = Adapter =>
                        {
                            // Finds Minimum Distance
                            int Index = -1,
                                MinDistance = int.MaxValue,
                                Distance;
                            for (int k = 0; k < Clusters.Length; k++)
                            {
                                Distance = GetDistanceConst(Clusters[k], Adapter);
                                if (Distance < MinDistance)
                                {
                                    MinDistance = Distance;
                                    Index = k;
                                }
                            }

                            return Index;
                        };
                    }
                    break;
                case QuantizationTypes.Mean:
                case QuantizationTypes.Median:
                default:
                    {
                        QuantizationBox[] Boxes = ImageContextHelper.BoxQuantize(Sorc, Type, Count,
                                                                                 out Func<QuantizationBox, PixelAdapter<T>, bool> Contain,
                                                                                 out Func<QuantizationBox, T> GetColor).ToArray();
                        Colors = Boxes.Select(b => GetColor(b)).ToArray();
                        GetColorIndex = Adapter => Boxes.IndexOf(b => Contain(b, Adapter));
                    }
                    break;
            }

            int Dx = -Width;
            Sorc.DangerousMove(0, 0);
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Colors[GetColorIndex(Sorc)]);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Result;
        }
        public ImageContext<T> Quantizate<T>(QuantizationTypes Type, int Count, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            if (Count < 2)
                throw new ArgumentOutOfRangeException($"Parameter {nameof(Count)} must greater than 1.");

            ImageContext<T> Result = new(Width, Height);
            PixelAdapter<T> Sorc0 = GetAdapter<T>(0, 0);

            T[] Colors;
            Func<PixelAdapter<T>, int> GetColorIndex;
            switch (Type)
            {
                case QuantizationTypes.KMeans:
                    {
                        QuantizationCluster[] Clusters = ImageContextHelper.ClusterQuantize(Sorc0, Count, Options ?? DefaultParallelOptions,
                                                                                            out Func<QuantizationCluster, PixelAdapter<T>, int> GetDistanceConst,
                                                                                            out Func<QuantizationCluster, T> GetColor)
                                                                           .ToArray();

                        Colors = Clusters.Select(c => GetColor(c)).ToArray();
                        GetColorIndex = Adapter =>
                        {
                            // Finds Minimum Distance
                            int Index = -1,
                                MinDistance = int.MaxValue,
                                Distance;
                            for (int k = 0; k < Clusters.Length; k++)
                            {
                                Distance = GetDistanceConst(Clusters[k], Adapter);
                                if (Distance < MinDistance)
                                {
                                    MinDistance = Distance;
                                    Index = k;
                                }
                            }

                            return Index;
                        };
                    }
                    break;
                case QuantizationTypes.Mean:
                case QuantizationTypes.Median:
                default:
                    {
                        QuantizationBox[] Boxes = ImageContextHelper.BoxQuantize(Sorc0, Type, Count, Options ?? DefaultParallelOptions,
                                                                                 out Func<QuantizationBox, PixelAdapter<T>, bool> Contain,
                                                                                 out Func<QuantizationBox, T> GetColor).ToArray();
                        Colors = Boxes.Select(b => GetColor(b)).ToArray();
                        GetColorIndex = Adapter => Boxes.IndexOf(b => Contain(b, Adapter));
                    }
                    break;
            }

            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = Sorc0.Clone(),
                                Dest = Result.GetAdapter<T>(0, j);

                Sorc.DangerousMove(0, j);
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Colors[GetColorIndex(Sorc)]);
            });

            return Result;
        }

        public ImageContext<T> Binarize<T>(ImageThreshold Threshold)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Image = new(Width, Height);
            PixelAdapter<T> Sorc = Threshold.CreateAdapter(GetAdapter<T>(0, 0)),
                            Dest = Image.GetAdapter<T>(0, 0);

            int Dx = -Width;
            T Color0 = default;
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Sorc.A != Color0.A || Sorc.R != Color0.R || Sorc.G != Color0.G || Sorc.B != Color0.B)
                        Dest.Override(Sorc);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Image;
        }
        public ImageContext<T> Binarize<T>(ImageThreshold Threshold, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Image = new(Width, Height);
            PixelAdapter<T> Sorc0 = Threshold.CreateAdapter(GetAdapter<T>(0, 0));

            T Color0 = default;
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = Sorc0.Clone(),
                                Dest = Image.GetAdapter<T>(0, j);

                Sorc.Move(0, j);
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Sorc.A != Color0.A || Sorc.R != Color0.R || Sorc.G != Color0.G || Sorc.B != Color0.B)
                        Dest.Override(Sorc);
            });

            return Image;
        }
        public ImageContext<T> Binarize<T>(ImagePredicate Predicate)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Image = new(Width, Height);
            PixelAdapter<T> Sorc = GetAdapter<T>(0, 0),
                            Dest = Image.GetAdapter<T>(0, 0);

            int Dx = -Width;
            T Max = PixelHelper.ToPixel<T>(255, 255, 255, 255);
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Predicate(i, j, Sorc))
                        Dest.Override(Max);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Image;
        }
        public ImageContext<T> Binarize<T>(ImagePredicate Predicate, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Image = new(Width, Height);

            T Max = PixelHelper.ToPixel<T>(255, 255, 255, 255);
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = GetAdapter<T>(0, j),
                                Dest = Image.GetAdapter<T>(0, j);

                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Predicate(i, j, Sorc))
                        Dest.Override(Max);
            });

            return Image;
        }
        public ImageContext<T, U> Binarize<T, U>(ImageThreshold Threshold)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Image = new(Width, Height);
            PixelAdapter<T> Sorc = Threshold.CreateAdapter(GetAdapter<T>(0, 0));
            PixelIndexedAdapter<T> Dest = Image.GetAdapter<T>(0, 0);

            int Dx = -Width;
            T Color0 = default;
            Image.Palette.Datas.Add(Color0);
            Image.Palette.Datas.Add(PixelHelper.ToPixel<T>(255, 255, 255, 255));

            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Sorc.A != Color0.A || Sorc.R != Color0.R || Sorc.G != Color0.G || Sorc.B != Color0.B)
                        Dest.OverrideIndex(1);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Image;
        }
        public ImageContext<T, U> Binarize<T, U>(ImageThreshold Threshold, ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Image = new(Width, Height);
            PixelAdapter<T> Sorc0 = Threshold.CreateAdapter(GetAdapter<T>(0, 0));

            T Color0 = default;
            Image.Palette.Datas.Add(Color0);
            Image.Palette.Datas.Add(PixelHelper.ToPixel<T>(255, 255, 255, 255));

            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = Sorc0.Clone();
                PixelIndexedAdapter<T> Dest = Image.GetAdapter<T>(0, j);

                Sorc.Move(0, j);
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Sorc.A != Color0.A || Sorc.R != Color0.R || Sorc.G != Color0.G || Sorc.B != Color0.B)
                        Dest.OverrideIndex(1);
            });

            return Image;
        }
        public ImageContext<T, U> Binarize<T, U>(ImagePredicate Predicate)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Image = new(Width, Height);
            PixelAdapter<T> Sorc = GetAdapter<T>(0, 0);
            PixelIndexedAdapter<T> Dest = Image.GetAdapter<T>(0, 0);

            Image.Palette.Datas.Add(default);
            Image.Palette.Datas.Add(PixelHelper.ToPixel<T>(255, 255, 255, 255));

            int Dx = -Width;
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Predicate(i, j, Sorc))
                        Dest.OverrideIndex(1);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Image;
        }
        public ImageContext<T, U> Binarize<T, U>(ImagePredicate Predicate, ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Image = new(Width, Height);

            Image.Palette.Datas.Add(default);
            Image.Palette.Datas.Add(PixelHelper.ToPixel<T>(255, 255, 255, 255));

            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = GetAdapter<T>(0, j);
                PixelIndexedAdapter<T> Dest = Image.GetAdapter<T>(0, j);
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    if (Predicate(i, j, Sorc))
                        Dest.OverrideIndex(1);
            });

            return Image;
        }

        public ImageContext<T> Cast<T>()
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new(Width, Height);
            PixelAdapter<T> Sorc = GetAdapter<T>(0, 0),
                            Dest = Result.GetAdapter<T>(0, 0);

            int Dx = -Width;
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Result;
        }
        public ImageContext<T> Cast<T>(ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new(Width, Height);

            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = GetAdapter<T>(0, j),
                                Dest = Result.GetAdapter<T>(0, j);

                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.Override(Sorc);
            });

            return Result;
        }
        public ImageContext<T, U> Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Result = new(Width, Height);
            ImagePalette<T> Palette = Result.Palette;

            PixelAdapter<T> Sorc = GetAdapter<T>(0, 0);
            PixelIndexedAdapter<T> Dest = Result.GetAdapter<T>(0, 0);
            QuantizationBox[] Boxes = ImageContextHelper.BoxQuantize(Sorc, QuantizationTypes.Median, Palette.Capacity,
                                                                  out Func<QuantizationBox, PixelAdapter<T>, bool> Contain,
                                                                  out Func<QuantizationBox, T> GetColor).ToArray();
            Palette.Datas.AddRange(Boxes.Select(b => GetColor(b)));
            Sorc.DangerousMove(0, 0);

            int Dx = -Width;
            for (int j = 0; j < Height; j++, Sorc.DangerousMoveNextY(), Dest.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.OverrideIndex(Boxes.IndexOf(b => Contain(b, Sorc)));

                Sorc.DangerousOffsetX(Dx);
                Dest.DangerousOffsetX(Dx);
            }

            return Result;
        }
        public ImageContext<T, U> Cast<T, U>(ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Result = new(Width, Height);
            ImagePalette<T> Palette = Result.Palette;
            PixelAdapter<T> Sorc0 = GetAdapter<T>(0, 0);
            QuantizationBox[] Boxes = ImageContextHelper.BoxQuantize(Sorc0, QuantizationTypes.Median, Palette.Capacity, Options ?? DefaultParallelOptions,
                                                                     out Func<QuantizationBox, PixelAdapter<T>, bool> Contain,
                                                                     out Func<QuantizationBox, T> GetColor).ToArray();
            Palette.Datas.AddRange(Boxes.Select(b => GetColor(b)));

            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Sorc = Sorc0.Clone();
                PixelIndexedAdapter<T> Dest = Result.GetAdapter<T>(0, j);

                Sorc.DangerousMove(0, j);
                for (int i = 0; i < Width; i++, Sorc.DangerousMoveNextX(), Dest.DangerousMoveNextX())
                    Dest.OverrideIndex(Boxes.IndexOf(b => Contain(b, Sorc)));
            });

            return Result;
        }

        public void Clear<T>(T Color)
            where T : unmanaged, IPixel
        {
            PixelAdapter<T> Adapter = GetAdapter<T>(0, 0);

            int Dx = -Width;
            for (int j = 0; j < Height; j++, Adapter.DangerousMoveNextY())
            {
                for (int i = 0; i < Width; i++, Adapter.DangerousMoveNextX())
                    Adapter.Override(Color);

                Adapter.DangerousOffsetX(Dx);
            }
        }
        public void Clear<T>(T Color, ParallelOptions Options) where T : unmanaged, IPixel
            => Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Adapter = GetAdapter<T>(0, j);
                for (int i = 0; i < Width; i++, Adapter.DangerousMoveNextX())
                    Adapter.Override(Color);
            });

        #endregion

        #region Buffer Processing

        #region BlockCopy
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T));
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T), Options);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride, Options);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
            {
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
            {
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride, Options);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T));
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T), Options);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
            {
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride, Options);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
            {
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
            {
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride, Options);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel));
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => BlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel), Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            int Iw = this.Width;
            if (X < 0 || Iw <= X)
                throw new ArgumentOutOfRangeException(nameof(X));

            if (Width < 0 ||
                Iw < X + Width)
                throw new ArgumentOutOfRangeException(nameof(Width));

            int Ih = this.Height;
            if (Y < 0 || Ih <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Height < 0 ||
                Ih < Y + Height)
                throw new ArgumentOutOfRangeException(nameof(Height));

            PixelAdapter<T> Adapter = GetAdapter<T>(X, Y);
            int Dx = -Iw;
            for (int j = 0; j < Height; j++, Adapter.DangerousMoveNextY(), Dest0 += DestStride)
            {
                T* pDest = (T*)Dest0;
                for (int i = 0; i < Width; i++, Adapter.DangerousMoveNextX())
                    Adapter.OverrideTo(pDest++);

                Adapter.DangerousOffsetX(Dx);
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            int Iw = this.Width;
            if (X < 0 || Iw <= X)
                throw new ArgumentOutOfRangeException(nameof(X));

            if (Width < 0 ||
                Iw < X + Width)
                throw new ArgumentOutOfRangeException(nameof(Width));

            int Ih = this.Height;
            if (Y < 0 || Ih <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Height < 0 ||
                Ih < Y + Height)
                throw new ArgumentOutOfRangeException(nameof(Height));

            PixelAdapter<T> Adapter = GetAdapter<T>(X, Y);
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<T> Src = Adapter.Clone();
                Src.DangerousMove(X, Y + j);

                T* pDest = (T*)(Dest0 + DestStride * j);
                for (int i = 0; i < Width; i++, Src.DangerousMoveNextX())
                    Src.OverrideTo(pDest++);
            });
        }

        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width, Options);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride, Options);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
            {
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
            {
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride, Options);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => BlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
            => BlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => BlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
            => BlockCopy3(X, Y, Width, Height, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Width);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
            => BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Width, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            int Iw = this.Width;
            if (X < 0 || Iw <= X)
                throw new ArgumentOutOfRangeException(nameof(X));

            if (Width < 0 ||
                Iw < X + Width)
                throw new ArgumentOutOfRangeException(nameof(Width));

            int Ih = this.Height;
            if (Y < 0 || Ih <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Height < 0 ||
                Ih < Y + Height)
                throw new ArgumentOutOfRangeException(nameof(Height));

            PixelAdapter<Pixel> Adapter = GetAdapter<Pixel>(X, Y);
            int Dx = -Iw;
            for (int j = 0; j < Height; j++, Adapter.DangerousMoveNextY(), DestR += DestStride, DestG += DestStride, DestB += DestStride)
            {
                byte* pDestR = DestR,
                      pDestG = DestG,
                      pDestB = DestB;
                for (int i = 0; i < Width; i++, Adapter.DangerousMoveNextX())
                    Adapter.OverrideTo(pDestR++, pDestG++, pDestB++);

                Adapter.DangerousOffsetX(Dx);
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
        {
            int Iw = this.Width;
            if (X < 0 || Iw <= X)
                throw new ArgumentOutOfRangeException(nameof(X));

            if (Width < 0 ||
                Iw < X + Width)
                throw new ArgumentOutOfRangeException(nameof(Width));

            int Ih = this.Height;
            if (Y < 0 || Ih <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Height < 0 ||
                Ih < Y + Height)
                throw new ArgumentOutOfRangeException(nameof(Height));

            PixelAdapter<Pixel> Adapter = GetAdapter<Pixel>(X, Y);
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<Pixel> Src = Adapter.Clone();
                Src.DangerousMove(X, Y + j);

                long Offset = DestStride * j;
                byte* pDestR = DestR + Offset,
                      pDestG = DestG + Offset,
                      pDestB = DestB + Offset;
                for (int i = 0; i < Width; i++, Src.DangerousMoveNextX())
                    Src.OverrideTo(pDestR++, pDestG++, pDestB++);
            });
        }

        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width, Options);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
            {
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride, Options);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
            {
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
            {
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride, Options);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => BlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
            => BlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, Width, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => BlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
            => BlockCopy4(X, Y, Width, Height, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB, DestStride, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Width);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
            => BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Width, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            int Iw = this.Width;
            if (X < 0 || Iw <= X)
                throw new ArgumentOutOfRangeException(nameof(X));

            if (Width < 0 ||
                Iw < X + Width)
                throw new ArgumentOutOfRangeException(nameof(Width));

            int Ih = this.Height;
            if (Y < 0 || Ih <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Height < 0 ||
                Ih < Y + Height)
                throw new ArgumentOutOfRangeException(nameof(Height));

            PixelAdapter<Pixel> Adapter = GetAdapter<Pixel>(X, Y);
            int Dx = -Iw;
            for (int j = 0; j < Height; j++, Adapter.DangerousMoveNextY(), DestA += DestStride, DestR += DestStride, DestG += DestStride, DestB += DestStride)
            {
                byte* pDestA = DestA,
                      pDestR = DestR,
                      pDestG = DestG,
                      pDestB = DestB;
                for (int i = 0; i < Width; i++, Adapter.DangerousMoveNextX())
                    Adapter.OverrideTo(pDestA++, pDestR++, pDestG++, pDestB++);

                Adapter.DangerousOffsetX(Dx);
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
        {
            int Iw = this.Width;
            if (X < 0 || Iw <= X)
                throw new ArgumentOutOfRangeException(nameof(X));

            if (Width < 0 ||
                Iw < X + Width)
                throw new ArgumentOutOfRangeException(nameof(Width));

            int Ih = this.Height;
            if (Y < 0 || Ih <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Height < 0 ||
                Ih < Y + Height)
                throw new ArgumentOutOfRangeException(nameof(Height));

            PixelAdapter<Pixel> Adapter = GetAdapter<Pixel>(X, Y);
            _ = Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                PixelAdapter<Pixel> Src = Adapter.Clone();
                Src.DangerousMove(X, Y + j);

                long Offset = DestStride * j;
                byte* pDestA = DestA + Offset,
                      pDestR = DestR + Offset,
                      pDestG = DestG + Offset,
                      pDestB = DestB + Offset;
                for (int i = 0; i < Width; i++, Src.DangerousMoveNextX())
                    Src.OverrideTo(pDestA++, pDestR++, pDestG++, pDestB++);
            });
        }

        #endregion

        #region ScanLineCopy
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0) where T : unmanaged, IPixel
            => ScanLineCopy<T>(OffsetX, Y, Length, (byte*)Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
            {
                ScanLineCopy<T>(OffsetX, Y, Length, (byte*)pDest);
            }
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
            {
                ScanLineCopy<T>(OffsetX, Y, Length, (byte*)pDest);
            }
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
            {
                ScanLineCopy<T>(OffsetX, Y, Length, pDest);
            }
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
            {
                ScanLineCopy<T>(OffsetX, Y, Length, pDest);
            }
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, IntPtr Dest0) where T : unmanaged, IPixel
            => ScanLineCopy<T>(OffsetX, Y, Length, (byte*)Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte* Dest0)
            where T : unmanaged, IPixel
        {
            if (OffsetX < 0 || Width <= OffsetX)
                throw new ArgumentOutOfRangeException(nameof(OffsetX));

            if (Y < 0 || Height <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Length < 0 ||
                Width < OffsetX + Length)
                throw new ArgumentOutOfRangeException(nameof(Length));

            T* pDest = (T*)Dest0;
            PixelAdapter<T> Adapter = GetAdapter<T>(OffsetX, Y);
            for (int i = 0; i < Length; i++, Adapter.DangerousMoveNextX())
                Adapter.OverrideTo(pDest++);
        }

        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ScanLineCopy3(OffsetX, Y, Length, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ScanLineCopy3(OffsetX, Y, Length, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy3(int OffsetX, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => ScanLineCopy3(OffsetX, Y, Length, (byte*)DestR, (byte*)DestG, (byte*)DestB);
        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
        {
            if (OffsetX < 0 || Width <= OffsetX)
                throw new ArgumentOutOfRangeException(nameof(OffsetX));

            if (Y < 0 || Height <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Length < 0 ||
                Width < OffsetX + Length)
                throw new ArgumentOutOfRangeException(nameof(Length));

            PixelAdapter<Pixel> Adapter = GetAdapter<Pixel>(OffsetX, Y);
            for (int i = 0; i < Length; i++, Adapter.DangerousMoveNextX())
                Adapter.OverrideTo(DestR++, DestG++, DestB++);
        }

        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                ScanLineCopy4(OffsetX, Y, Length, pDestA, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                ScanLineCopy4(OffsetX, Y, Length, pDestA, pDestR, pDestG, pDestB);
        }
        public void ScanLineCopy4(int OffsetX, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => ScanLineCopy4(OffsetX, Y, Length, (byte*)DestA, (byte*)DestR, (byte*)DestG, (byte*)DestB);
        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
        {
            if (OffsetX < 0 || Width <= OffsetX)
                throw new ArgumentOutOfRangeException(nameof(OffsetX));

            if (Y < 0 || Height <= Y)
                throw new ArgumentOutOfRangeException(nameof(Y));

            if (Length < 0 ||
                Width < OffsetX + Length)
                throw new ArgumentOutOfRangeException(nameof(Length));

            PixelAdapter<Pixel> Adapter = GetAdapter<Pixel>(OffsetX, Y);
            for (int i = 0; i < Length; i++, Adapter.DangerousMoveNextX())
                Adapter.OverrideTo(DestA++, DestR++, DestG++, DestB++);
        }

        #endregion

        #endregion

        /// <summary>
        /// Creates a new <see cref="PixelIndexedAdapter{T}"/> with the specified pixel type and coordinate.
        /// </summary>
        /// <typeparam name="T">The specified pixel type.</typeparam>
        /// <param name="X">The x-coordinate of start point at adapter.</param>
        /// <param name="Y">The y-coordinate of start point at adapter.</param>
        public PixelIndexedAdapter<T> GetAdapter<T>(int X, int Y) where T : unmanaged, IPixel
            => PixelType == typeof(T) ? new PixelIndexedAdapter<T, Struct>(this, X, Y) :
                                        new PixelIndexedAdapter<Pixel, T, Struct>(this, X, Y);
        PixelAdapter<T> IImageContext.GetAdapter<T>(int X, int Y)
            => GetAdapter<T>(X, Y);
        public IPixelAdapter GetAdapter(int X, int Y)
            => GetAdapter<Pixel>(X, Y);

        /// <summary>
        /// Creates a new <see cref="ImageContext{Pixel, Struct}"/> that is a copy of the current instance.
        /// </summary>
        public ImageContext<Pixel, Struct> Clone()
            => Cast<Pixel, Struct>();
        IImageContext IImageContext.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

    }
}