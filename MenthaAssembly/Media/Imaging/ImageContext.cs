using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    public unsafe class ImageContext<Pixel> : IImageContext<Pixel>, ICloneable
        where Pixel : unmanaged, IPixel
    {
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

        internal IImageOperator<Pixel> Operator { get; }
        IImageOperator IImageContext.Operator => Operator;

        public int Width { get; }

        public int Height { get; }

        public long Stride { get; }

        public int BitsPerPixel { get; }

        public int Channels { get; }

        private static readonly Type PixelType = typeof(Pixel);
        Type IImageContext.PixelType => PixelType;

        Type IImageContext.StructType => PixelType;

        public Pixel this[int X, int Y]
        {
            get
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                return Operator.GetPixel(X, Y);
            }
            set
            {
                if (X < 0 || X >= Width ||
                    Y < 0 || Y >= Height)
                    throw new IndexOutOfRangeException();

                Operator.SetPixel(X, Y, value);
            }
        }
        IPixel IImageContext.this[int X, int Y]
        {
            get => this[X, Y];
            set => this[X, Y] = value.ToPixel<Pixel>();
        }

        internal readonly byte[] Data0;
        private readonly IntPtr _Scan0;
        private readonly Func<IntPtr> GetScan0;
        public IntPtr Scan0 => GetScan0();

        internal readonly byte[] DataA;
        private readonly IntPtr _ScanA;
        private readonly Func<IntPtr> GetScanA;
        public IntPtr ScanA => GetScanA();

        internal readonly byte[] DataR;
        private readonly IntPtr _ScanR;
        private readonly Func<IntPtr> GetScanR;
        public IntPtr ScanR => GetScanR();

        internal readonly byte[] DataG;
        private readonly IntPtr _ScanG;
        private readonly Func<IntPtr> GetScanG;
        public IntPtr ScanG => GetScanG();

        internal readonly byte[] DataB;
        internal readonly IntPtr _ScanB;
        private readonly Func<IntPtr> GetScanB;
        public IntPtr ScanB => GetScanB();

        IImagePalette IImageContext.Palette => null;

        private ImageContext()
        {
            BitsPerPixel = default(Pixel).BitsPerPixel;
        }

        private readonly HGlobalIntPtr UnmanagedScan0;
        public ImageContext(int Width, int Height) : this()
        {
            this.Width = Width;
            this.Height = Height;

            Stride = (Width * BitsPerPixel + 7) >> 3;
            Channels = 1;

            long Size = Stride * Height;
            if (Size <= int.MaxValue || Environment.Is64BitProcess)
            {
                UnmanagedScan0 = new HGlobalIntPtr(Size);
                _Scan0 = UnmanagedScan0.DangerousGetHandle();
                GetScan0 = () => _Scan0;
            }
            else
            {
                GetScan0 = () => throw new OutOfMemoryException("Can't allocate buffer more than 2GB in 32bit process.");
            }

            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => throw new NotImplementedException();
            GetScanG = () => throw new NotImplementedException();
            GetScanB = () => throw new NotImplementedException();

            Operator = new ImageOperator<Pixel>(this);
        }

        public ImageContext(int Width, int Height, IntPtr Scan0) :
            this(Width, Height, Scan0, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, long Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            Channels = 1;

            _Scan0 = Scan0;
            GetScan0 = () => _Scan0;
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => throw new NotImplementedException();
            GetScanG = () => throw new NotImplementedException();
            GetScanB = () => throw new NotImplementedException();

            Operator = new ImageOperator<Pixel>(this);
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) :
            this(Width, Height, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, long Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            Channels = 3;

            _ScanR = ScanR;
            _ScanG = ScanG;
            _ScanB = ScanB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => _ScanR;
            GetScanG = () => _ScanG;
            GetScanB = () => _ScanB;

            Operator = new ImageOperator3<Pixel>(this);
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) :
            this(Width, Height, ScanA, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, long Stride) : this()
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            Channels = 4;

            _ScanA = ScanA;
            _ScanR = ScanR;
            _ScanG = ScanG;
            _ScanB = ScanB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () => _ScanA;
            GetScanR = () => _ScanR;
            GetScanG = () => _ScanG;
            GetScanB = () => _ScanB;

            Operator = new ImageOperator4<Pixel>(this);
        }

        public ImageContext(int Width, int Height, byte[] Data) : this()
        {
            this.Width = Width;
            this.Height = Height;

            Stride = Data.Length / Height;
            Channels = 1;

            Data0 = Data;
            GetScan0 = () =>
            {
                fixed (byte* pScan0 = &Data0[0])
                    return (IntPtr)pScan0;
            };
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () => throw new NotImplementedException();
            GetScanG = () => throw new NotImplementedException();
            GetScanB = () => throw new NotImplementedException();

            Operator = new ImageOperator<Pixel>(this);
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            Stride = DataR.Length / Height;
            Channels = 3;

            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () => throw new NotImplementedException();
            GetScanR = () =>
            {
                fixed (byte* pScanR = &this.DataR[0])
                    return (IntPtr)pScanR;
            };
            GetScanG = () =>
            {
                fixed (byte* pScanG = &this.DataG[0])
                    return (IntPtr)pScanG;
            };
            GetScanB = () =>
            {
                fixed (byte* pScanB = &this.DataB[0])
                    return (IntPtr)pScanB;
            };

            Operator = new ImageOperator3<Pixel>(this);
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB) : this()
        {
            this.Width = Width;
            this.Height = Height;
            Stride = DataA.Length / Height;
            Channels = 4;

            this.DataA = DataA;
            this.DataR = DataR;
            this.DataG = DataG;
            this.DataB = DataB;
            GetScan0 = () => throw new NotImplementedException();
            GetScanA = () =>
            {
                fixed (byte* pScanA = &this.DataA[0])
                    return (IntPtr)pScanA;
            };
            GetScanR = () =>
            {
                fixed (byte* pScanR = &this.DataR[0])
                    return (IntPtr)pScanR;
            };
            GetScanG = () =>
            {
                fixed (byte* pScanG = &this.DataG[0])
                    return (IntPtr)pScanG;
            };
            GetScanB = () =>
            {
                fixed (byte* pScanB = &this.DataB[0])
                    return (IntPtr)pScanB;
            };

            Operator = new ImageOperator4<Pixel>(this);
        }

        #region Graphic Processing

        #region Line Rendering

        #region Line
        public void DrawLine(Point<int> P0, Point<int> P1, Pixel Color)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Color);
        public void DrawLine(int X0, int Y0, int X1, int Y1, Pixel Color)
        {
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
            foreach (KeyValuePair<int, int> Data in RightBound)
            {
                int Y = Data.Key,
                    TRx = Data.Value;
                if (LeftBound.TryGetValue(Y, out int TLx))
                {
                    LeftBound.Remove(Y);
                    Operator.ScanLine<Pixel>(TLx, Y, TRx - TLx + 1, a => a.Overlay(Color));
                }
                else
                {
                    Operator.SetPixel(TRx, Y, Color);
                }
            }
            RightBound.Clear();

            foreach (KeyValuePair<int, int> Data in LeftBound)
                Operator.SetPixel(Data.Value, Data.Key, Color);

            LeftBound.Clear();

            #endregion
        }
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Pen);
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
        {
            int X = X0 - (Pen.Width >> 1),
                Y = Y0 - (Pen.Height >> 1);

            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, Math.Abs(DeltaX), Math.Abs(DeltaY), (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen));
        }
        public void DrawLine(Point<int> P0, Point<int> P1, ImageContour Contour, Pixel Fill)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Contour, Fill);
        public void DrawLine(int X0, int Y0, int X1, int Y1, ImageContour Contour, Pixel Fill)
        {
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

            foreach (KeyValuePair<int, ContourData> Item in Contour)
            {
                int j = Item.Key;
                ContourData Data = Item.Value;
                if (Data.Count > 2)
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
                    int Tx = Data[Data.Count - 1] - PCx,
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

                Operator.ContourOverlay(LineContour, Fill, 0, 0);
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
                foreach (KeyValuePair<int, int> Data in RightBound)
                {
                    int Y = Data.Key,
                        TRx = Data.Value;
                    if (LeftBound.TryGetValue(Y, out int TLx))
                    {
                        LeftBound.Remove(Y);
                        Operator.ScanLine<Pixel>(TLx, Y, TRx - TLx + 1, a => a.Overlay(Fill));
                    }
                    else
                    {
                        Operator.SetPixel(TRx, Y, Fill);
                    }
                }
                RightBound.Clear();

                foreach (KeyValuePair<int, int> Data in LeftBound)
                    Operator.SetPixel(Data.Value, Data.Key, Fill);

                LeftBound.Clear();

                #endregion
            }
        }

        void IImageContext.DrawLine(Point<int> P0, Point<int> P1, IPixel Color)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Color.ToPixel<Pixel>());
        void IImageContext.DrawLine(int X0, int Y0, int X1, int Y1, IPixel Color)
            => DrawLine(X0, Y0, X1, Y1, Color.ToPixel<Pixel>());
        void IImageContext.DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Pen);
        void IImageContext.DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
            => DrawLine(X0, Y0, X1, Y1, Pen);
        void IImageContext.DrawLine(Point<int> P0, Point<int> P1, ImageContour Contour, IPixel Fill)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawLine(int X0, int Y0, int X1, int Y1, ImageContour Contour, IPixel Fill)
            => DrawLine(X0, Y0, X1, Y1, Contour, Fill.ToPixel<Pixel>());
        #endregion

        #region Arc
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, Pixel Color)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Color);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, Pixel Color)
            => GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy,
                                                      Ex - Cx, Ey - Cy,
                                                      Rx, Ry,
                                                      Clockwise,
                                                      (Dx, Dy) => Operator.SetPixel(Cx + Dx, Cy + Dy, Color));
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Pen);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            int X = Cx - (Pen.Width >> 1),
                Y = Cy - (Pen.Height >> 1);

            GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy, Ex - Cx, Ey - Cy, Rx, Ry, Clockwise, false,
                (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen));
        }
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, Pixel Fill)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Contour, Fill);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, Pixel Fill)
        {
            ImageContour ArcContour = new ImageContour();
            Bound<int> Bound = Contour.Bound;

            if (Bound.IsEmpty)
                return;

            if (Bound.Width == 1 && Bound.Height == 1)
            {
                DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Fill);
                return;
            }

            bool IsHollow = Contour.Any(i => i.Value.Count > 2);
            int MaxX = Width - 1,
                PCx = (Bound.Left + Bound.Right) >> 1,
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
                Dictionary<int, int> LargeLeftBound = new Dictionary<int, int>(),
                                     LargeRightBound = new Dictionary<int, int>(),
                                     SmallLeftBound = new Dictionary<int, int>(),
                                     SmallRightBound = new Dictionary<int, int>();

                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, false,
                    (Dx, Dy) =>
                    {
                        int OffsetX = Dx + Cx - PCx,
                            OffsetY = Dy + Cy - PCy;
                        if (Dx < 0)
                        {
                            foreach (KeyValuePair<int, ContourData> item in Contour)
                            {
                                ContourData Data = item.Value;
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
                            foreach (KeyValuePair<int, ContourData> item in Contour)
                            {
                                ContourData Data = item.Value;
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

            Operator.ContourOverlay(ArcContour, Fill, 0, 0);
        }

        void IImageContext.DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IPixel Color)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Color.ToPixel<Pixel>());
        void IImageContext.DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IPixel Color)
            => DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Color.ToPixel<Pixel>());
        void IImageContext.DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Pen);
        void IImageContext.DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Pen);
        void IImageContext.DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill)
            => DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Contour, Fill.ToPixel<Pixel>());

        #endregion

        #region Curve
        public void DrawCurve(IList<int> Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

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
        public void DrawCurve(IList<Point<int>> Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

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
        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen);

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
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen);

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
        public void DrawCurve(IList<int> Points, float Tension, ImageContour Contour, Pixel Fill)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill);

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
        public void DrawCurve(IList<Point<int>> Points, float Tension, ImageContour Contour, Pixel Fill)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill);

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

        void IImageContext.DrawCurve(IList<int> Points, float Tension, IPixel Color)
            => DrawCurve(Points, Tension, Color.ToPixel<Pixel>());
        void IImageContext.DrawCurve(IList<Point<int>> Points, float Tension, IPixel Color)
            => DrawCurve(Points, Tension, Color.ToPixel<Pixel>());
        void IImageContext.DrawCurve(IList<int> Points, float Tension, ImageContour Contour, IPixel Fill)
            => DrawCurve(Points, Tension, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawCurve(IList<Point<int>> Points, float Tension, ImageContour Contour, IPixel Fill)
            => DrawCurve(Points, Tension, Contour, Fill.ToPixel<Pixel>());

        public void DrawCurveClosed(IList<int> Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

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
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

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
        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen);

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
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen);

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
        public void DrawCurveClosed(IList<int> Points, float Tension, ImageContour Contour, Pixel Fill)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill);

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
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, ImageContour Contour, Pixel Fill)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill);

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

        void IImageContext.DrawCurveClosed(IList<int> Points, float Tension, IPixel Color)
            => DrawCurveClosed(Points, Tension, Color.ToPixel<Pixel>());
        void IImageContext.DrawCurveClosed(IList<Point<int>> Points, float Tension, IPixel Color)
            => DrawCurveClosed(Points, Tension, Color.ToPixel<Pixel>());
        void IImageContext.DrawCurveClosed(IList<int> Points, float Tension, ImageContour Contour, IPixel Fill)
            => DrawCurveClosed(Points, Tension, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawCurveClosed(IList<Point<int>> Points, float Tension, ImageContour Contour, IPixel Fill)
            => DrawCurveClosed(Points, Tension, Contour, Fill.ToPixel<Pixel>());

        #endregion

        #region Bezier
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, Pixel Color)
            => GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                          Cx1, Cy1,
                                                          Cx2, Cy2,
                                                          X2, Y2,
                                                          (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Color));
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen)
            => GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                          Cx1, Cy1,
                                                          Cx2, Cy2,
                                                          X2, Y2,
                                                          (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Pen));
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, Pixel Fill)
            => GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                          Cx1, Cy1,
                                                          Cx2, Cy2,
                                                          X2, Y2,
                                                          (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill));

        void IImageContext.DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IPixel Color)
            => DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Color.ToPixel<Pixel>());
        void IImageContext.DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, IPixel Fill)
            => DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Contour, Fill.ToPixel<Pixel>());

        public void DrawBeziers(IList<int> Points, Pixel Color)
        {
            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

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
        public void DrawBeziers(IList<int> Points, IImageContext Pen)
        {
            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Pen);

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
        public void DrawBeziers(IList<int> Points, ImageContour Contour, Pixel Fill)
        {
            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Contour, Fill);

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

        void IImageContext.DrawBeziers(IList<int> Points, IPixel Color)
            => DrawBeziers(Points, Color.ToPixel<Pixel>());
        void IImageContext.DrawBeziers(IList<int> Points, ImageContour Contour, IPixel Fill)
            => DrawBeziers(Points, Contour, Fill.ToPixel<Pixel>());

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
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, Pixel Color)
        {
            DrawLine(X1, Y1, X2, Y2, Color);
            DrawLine(X2, Y2, X3, Y3, Color);
            DrawLine(X3, Y3, X1, Y1, Color);
        }
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, Pixel Color)
        {
            DrawLine(P1.X, P1.Y, P2.X, P2.Y, Color);
            DrawLine(P2.X, P2.Y, P3.X, P3.Y, Color);
            DrawLine(P3.X, P3.Y, P1.X, P1.Y, Color);
        }
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen)
        {
            DrawLine(X1, Y1, X2, Y2, Pen);
            DrawLine(X2, Y2, X3, Y3, Pen);
            DrawLine(X3, Y3, X1, Y1, Pen);
        }
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen)
        {
            DrawLine(P1.X, P1.Y, P2.X, P2.Y, Pen);
            DrawLine(P2.X, P2.Y, P3.X, P3.Y, Pen);
            DrawLine(P3.X, P3.Y, P1.X, P1.Y, Pen);
        }
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, Pixel Fill)
        {
            DrawLine(X1, Y1, X2, Y2, Contour, Fill);
            DrawLine(X2, Y2, X3, Y3, Contour, Fill);
            DrawLine(X3, Y3, X1, Y1, Contour, Fill);
        }
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, Pixel Fill)
        {
            DrawLine(P1.X, P1.Y, P2.X, P2.Y, Contour, Fill);
            DrawLine(P2.X, P2.Y, P3.X, P3.Y, Contour, Fill);
            DrawLine(P3.X, P3.Y, P1.X, P1.Y, Contour, Fill);
        }

        void IImageContext.DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IPixel Color)
            => DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Color.ToPixel<Pixel>());
        void IImageContext.DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IPixel Color)
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Color.ToPixel<Pixel>());
        void IImageContext.DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, IPixel Fill)
            => DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, IPixel Fill)
            => DrawTriangle(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, Contour, Fill.ToPixel<Pixel>());

        #endregion

        #region Rectangle
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, Pixel Color)
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

            DrawLine(L, T, R, T, Color);
            DrawLine(L, B, R, B, Color);
            DrawLine(L, T, L, B, Color);
            DrawLine(R, T, R, B, Color);
        }
        public void DrawRectangle(Point<int> P1, Point<int> P2, Pixel Color)
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Color);
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen)
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

            DrawLine(L, T, R, T, Pen);
            DrawLine(L, B, R, B, Pen);
            DrawLine(L, T, L, B, Pen);
            DrawLine(R, T, R, B, Pen);
        }
        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen)
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Pen);
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, ImageContour Contour, Pixel Fill)
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

            DrawLine(L, T, R, T, Contour, Fill);
            DrawLine(L, B, R, B, Contour, Fill);
            DrawLine(L, T, L, B, Contour, Fill);
            DrawLine(R, T, R, B, Contour, Fill);
        }
        public void DrawRectangle(Point<int> P1, Point<int> P2, ImageContour Contour, Pixel Fill)
            => DrawRectangle(P1.X, P1.Y, P2.X, P2.Y, Contour, Fill);

        void IImageContext.DrawRectangle(int X1, int Y1, int X2, int Y2, IPixel Color)
            => DrawRectangle(X1, Y1, X2, Y2, Color.ToPixel<Pixel>());
        void IImageContext.DrawRectangle(Point<int> P1, Point<int> P2, IPixel Color)
            => DrawRectangle(P1, P2, Color.ToPixel<Pixel>());
        void IImageContext.DrawRectangle(int X1, int Y1, int X2, int Y2, ImageContour Contour, IPixel Fill)
            => DrawRectangle(X1, Y1, X2, Y2, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawRectangle(Point<int> P1, Point<int> P2, ImageContour Contour, IPixel Fill)
            => DrawRectangle(P1, P2, Contour, Fill.ToPixel<Pixel>());

        #endregion

        #region Quad
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, Pixel Color)
        {
            DrawLine(X1, Y1, X2, Y2, Color);
            DrawLine(X2, Y2, X3, Y3, Color);
            DrawLine(X3, Y3, X4, Y4, Color);
            DrawLine(X4, Y4, X1, Y1, Color);
        }
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, Pixel Color)
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Color);
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen)
        {
            DrawLine(X1, Y1, X2, Y2, Pen);
            DrawLine(X2, Y2, X3, Y3, Pen);
            DrawLine(X3, Y3, X4, Y4, Pen);
            DrawLine(X4, Y4, X1, Y1, Pen);
        }
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen)
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Pen);
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, Pixel Fill)
        {
            DrawLine(X1, Y1, X2, Y2, Contour, Fill);
            DrawLine(X2, Y2, X3, Y3, Contour, Fill);
            DrawLine(X3, Y3, X4, Y4, Contour, Fill);
            DrawLine(X4, Y4, X1, Y1, Contour, Fill);
        }
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, Pixel Fill)
            => DrawQuad(P1.X, P1.Y, P2.X, P2.Y, P3.X, P3.Y, P4.X, P4.Y, Contour, Fill);

        void IImageContext.DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IPixel Color)
            => DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Color.ToPixel<Pixel>());
        void IImageContext.DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IPixel Color)
            => DrawQuad(P1, P2, P3, P4, Color.ToPixel<Pixel>());
        void IImageContext.DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, IPixel Fill)
            => DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, IPixel Fill)
            => DrawQuad(P1, P2, P3, P4, Contour, Fill.ToPixel<Pixel>());

        #endregion

        #region Ellipse
        public void DrawEllipse(Bound<int> Bound, Pixel Color)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Color);
        }
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, Pixel Color)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Color);
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, Pixel Color)
            => GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry, (Dx, Dy) => Operator.SetPixel(Cx + Dx, Cy + Dy, Color));
        public void DrawEllipse(Bound<int> Bound, IImageContext Pen)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Pen);
        }
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Pen);
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen)
        {
            int X = Cx - (Pen.Width >> 1),
                Y = Cy - (Pen.Height >> 1);

            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry, (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen));
        }
        public void DrawEllipse(Bound<int> Bound, ImageContour Contour, Pixel Fill)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Contour, Fill);
        }
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, ImageContour Contour, Pixel Fill)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Contour, Fill);
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, Pixel Fill)
        {
            ImageContour EllipseContour = new ImageContour();

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

            Operator.ContourOverlay(EllipseContour, Fill, 0, 0);
        }

        void IImageContext.DrawEllipse(Bound<int> Bound, IPixel Color)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Color.ToPixel<Pixel>());
        }
        void IImageContext.DrawEllipse(Point<int> Center, int Rx, int Ry, IPixel Color)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Color.ToPixel<Pixel>());
        void IImageContext.DrawEllipse(int Cx, int Cy, int Rx, int Ry, IPixel Color)
            => DrawEllipse(Cx, Cy, Rx, Ry, Color.ToPixel<Pixel>());
        void IImageContext.DrawEllipse(Bound<int> Bound, ImageContour Contour, IPixel Fill)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Contour, Fill.ToPixel<Pixel>());
        }
        void IImageContext.DrawEllipse(Point<int> Center, int Rx, int Ry, ImageContour Contour, IPixel Fill)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Contour, Fill.ToPixel<Pixel>());
        void IImageContext.DrawEllipse(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, IPixel Fill)
            => DrawEllipse(Cx, Cy, Rx, Ry, Contour, Fill.ToPixel<Pixel>());

        public void FillEllipse(Bound<int> Bound, Pixel Fill)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            FillEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Fill);
        }
        public void FillEllipse(Point<int> Center, int Rx, int Ry, Pixel Fill)
            => FillEllipse(Center.X, Center.Y, Rx, Ry, Fill);
        public void FillEllipse(int Cx, int Cy, int Rx, int Ry, Pixel Fill)
        {
            // Avoid endless loop
            if (Rx < 1 || Ry < 1)
                return;

            // Skip completly outside objects
            if (Cx - Rx >= Width ||
                Cx + Rx < 0 ||
                Cy - Ry >= Height ||
                Cy + Ry < 0)
                return;

            checked
            {
                // Init vars
                int uy, ly, lx, rx,
                    x = Rx,
                    y = 0;
                long LRx = Rx,
                     LRy = Ry,
                     xrSqTwo = (LRx * LRx) << 1,
                     yrSqTwo = (LRy * LRy) << 1,
                     xChg = LRy * LRy * (1 - (LRx << 1)),
                     yChg = LRx * LRx,
                     err = 0,
                     xStopping = yrSqTwo * LRx,
                     yStopping = 0;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xStopping >= yStopping)
                {
                    // Draw 4 quadrant points at once
                    // Upper half
                    uy = Cy + y;
                    // Lower half
                    ly = Cy - y - 1;

                    // Clip
                    if (uy < 0)
                        uy = 0;

                    if (uy >= Height)
                        uy = Height - 1;

                    if (ly < 0)
                        ly = 0;

                    if (ly >= Height)
                        ly = Height - 1;

                    rx = Cx + x;
                    lx = Cx - x;

                    // Clip
                    if (rx < 0)
                        rx = 0;
                    if (rx >= Width)
                        rx = Width - 1;
                    if (lx < 0)
                        lx = 0;
                    if (lx >= Width)
                        lx = Width - 1;

                    int Length = rx - lx + 1;
                    Operator.ScanLine<Pixel>(lx, uy, Length, a => a.Overlay(Fill));
                    Operator.ScanLine<Pixel>(lx, ly, Length, a => a.Overlay(Fill));

                    y++;
                    yStopping += xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                    if ((xChg + (err << 1)) > 0)
                    {
                        x--;
                        xStopping -= yrSqTwo;
                        err += xChg;
                        xChg += yrSqTwo;
                    }
                }

                // ReInit vars
                x = 0;
                y = Ry;

                // Upper half
                uy = Cy + y;
                // Lower half
                ly = Cy - y;

                // Clip
                if (uy < 0)
                    uy = 0;

                if (uy >= Height)
                    uy = Height - 1;

                if (ly < 0)
                    ly = 0;

                if (ly >= Height)
                    ly = Height - 1;

                xChg = LRy * LRy;
                yChg = LRx * LRx * (1 - (LRy << 1));
                err = 0;
                xStopping = 0;
                yStopping = xrSqTwo * LRy;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
                {
                    // Draw 4 quadrant points at once
                    rx = Cx + x;
                    lx = Cx - x;

                    // Clip
                    if (rx < 0)
                        rx = 0;
                    if (rx >= Width)
                        rx = Width - 1;
                    if (lx < 0)
                        lx = 0;
                    if (lx >= Width)
                        lx = Width - 1;

                    // Draw line
                    int Length = rx - lx + 1;
                    Operator.ScanLine<Pixel>(lx, uy, Length, a => a.Overlay(Fill));
                    Operator.ScanLine<Pixel>(lx, ly, Length, a => a.Overlay(Fill));

                    x++;
                    xStopping += yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                    if ((yChg + (err << 1)) > 0)
                    {
                        y--;
                        uy = Cy + y; // Upper half
                        ly = Cy - y; // Lower half
                        if (uy < 0)
                            uy = 0; // Clip
                        if (uy >= Height)
                            uy = Height - 1; // ...
                        if (ly < 0)
                            ly = 0;
                        if (ly >= Height)
                            ly = Height - 1;
                        yStopping -= xrSqTwo;
                        err += yChg;
                        yChg += xrSqTwo;
                    }
                }
            }
        }

        void IImageContext.FillEllipse(Bound<int> Bound, IPixel Fill)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            FillEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Fill.ToPixel<Pixel>());
        }
        void IImageContext.FillEllipse(Point<int> Center, int Rx, int Ry, IPixel Fill)
            => FillEllipse(Center.X, Center.Y, Rx, Ry, Fill.ToPixel<Pixel>());
        void IImageContext.FillEllipse(int Cx, int Cy, int Rx, int Ry, IPixel Fill)
            => FillEllipse(Cx, Cy, Rx, Ry, Fill.ToPixel<Pixel>());

        #endregion

        #region Polygon
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, Pixel Color, double StartAngle = 0d)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Color, StartAngle);
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, Pixel Color, double StartAngle = 0d)
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

                DrawLine(LastPx, LastPy, Px, Py, Color);

                LastPx = Px;
                LastPy = Py;
            }

            DrawLine(LastPx, LastPy, P0x, P0y, Color);
        }
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, double StartAngle = 0d)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Pen, StartAngle);
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, double StartAngle = 0d)
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

                DrawLine(LastPx, LastPy, Px, Py, Pen);

                LastPx = Px;
                LastPy = Py;
            }

            DrawLine(LastPx, LastPy, P0x, P0y, Pen);
        }
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, Pixel Fill, double StartAngle = 0d)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Contour, Fill, StartAngle);
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, Pixel Fill, double StartAngle = 0d)
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

                DrawLine(LastPx, LastPy, Px, Py, Contour, Fill);

                LastPx = Px;
                LastPy = Py;
            }

            DrawLine(LastPx, LastPy, P0x, P0y, Contour, Fill);
        }

        void IImageContext.DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IPixel Color, double StartAngle)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Color.ToPixel<Pixel>(), StartAngle);
        void IImageContext.DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IPixel Color, double StartAngle)
            => DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Color.ToPixel<Pixel>(), StartAngle);
        void IImageContext.DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, IPixel Fill, double StartAngle)
            => DrawRegularPolygon(Center.X, Center.Y, Radius, VertexNum, Contour, Fill.ToPixel<Pixel>(), StartAngle);
        void IImageContext.DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, IPixel Fill, double StartAngle)
            => DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Contour, Fill.ToPixel<Pixel>(), StartAngle);

        public void FillPolygon(IEnumerable<Point<int>> Vertices, Pixel Fill, int OffsetX, int OffsetY)
        {
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
                    if (Y0 < y && y <= Y1 ||
                        Y1 < y && y <= Y0)
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
                        Operator.ScanLineOverlay(x0, y, x1 - x0 + 1, Fill);
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
                        Operator.ScanLine<Pixel>(x0, y, x1 - x0 + 1, a => a.Overlay(Fill));
                    }
                }
            }
        }
        public void FillPolygon(IEnumerable<int> VerticeDatas, Pixel Fill, int OffsetX, int OffsetY)
        {
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
                    if (Y0 < y && y <= Y1 ||
                        Y1 < y && y <= Y0)
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
                        Operator.ScanLineOverlay(x0, y, x1 - x0 + 1, Fill);
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
                        Operator.ScanLine<Pixel>(x0, y, x1 - x0 + 1, a => a.Overlay(Fill));
                    }
                }
            }
        }

        void IImageContext.FillPolygon(IEnumerable<Point<int>> Vertices, IPixel Fill, int OffsetX, int OffsetY)
            => FillPolygon(Vertices, Fill.ToPixel<Pixel>(), OffsetX, OffsetY);
        void IImageContext.FillPolygon(IEnumerable<int> VerticeDatas, IPixel Fill, int OffsetX, int OffsetY)
            => FillPolygon(VerticeDatas, Fill.ToPixel<Pixel>(), OffsetX, OffsetY);

        #endregion

        #region Other
        public void DrawStamp(Point<int> Position, IImageContext Stamp)
            => DrawStamp(Position.X, Position.Y, Stamp);
        public void DrawStamp(int X, int Y, IImageContext Stamp)
        {
            int Bx = X + Stamp.Width,
                By = Y + Stamp.Height,
                OffsetX = 0,
                OffsetY = 0;

            if (X < 0)
            {
                OffsetX -= X;
                X = 0;
            }

            if (Y < 0)
            {
                OffsetY -= Y;
                Y = 0;
            }

            int Width = Math.Min(Bx, this.Width) - X;
            if (Width < 1)
                return;

            int Height = Math.Min(By, this.Width) - Y;
            if (Height < 1)
                return;

            Operator.BlockOverlay(X, Y, Stamp, OffsetX, OffsetY, Width, Height);
        }

        public void FillContour(ImageContour Contour, Pixel Fill, int OffsetX, int OffsetY)
            => Operator.ContourOverlay(Contour, Fill, OffsetX, OffsetY);

        void IImageContext.FillContour(ImageContour Contour, IPixel Fill, int OffsetX, int OffsetY)
            => FillContour(Contour, Fill.ToPixel<Pixel>(), OffsetX, OffsetY);

        public void SeedFill(Point<int> SeedPoint, Pixel Fill, ImagePredicate Predicate)
            => SeedFill(SeedPoint.X, SeedPoint.Y, Fill, Predicate);
        public void SeedFill(int SeedX, int SeedY, Pixel Fill, ImagePredicate Predicate)
        {
            if (Operator.FindBound(SeedX, SeedY, Predicate) is ImageContour Contour)
            {
                FillContour(Contour, Fill, 0, 0);
                Contour.Clear();
            }
        }

        void IImageContext.SeedFill(Point<int> SeedPoint, IPixel Fill, ImagePredicate Predicate)
            => SeedFill(SeedPoint.X, SeedPoint.Y, Fill.ToPixel<Pixel>(), Predicate);
        void IImageContext.SeedFill(int SeedX, int SeedY, IPixel Fill, ImagePredicate Predicate)
            => SeedFill(SeedX, SeedY, Fill.ToPixel<Pixel>(), Predicate);

        #endregion

        #endregion

        #region Text Rendering
        public void DrawText(int X, int Y, string Text, int CharSize, Pixel Fill)
            => DrawText(X, Y, Text, null, CharSize, Fill, 0d, FontWeightType.Normal, false);
        public void DrawText(int X, int Y, string Text, int CharSize, Pixel Fill, double Angle, FontWeightType Weight, bool Italic)
            => DrawText(X, Y, Text, null, CharSize, Fill, Angle, Weight, Italic);
        public void DrawText(int X, int Y, string Text, string FontName, int CharSize, Pixel Fill)
            => DrawText(X, Y, Text, FontName, CharSize, Fill, 0d, FontWeightType.Normal, false);
        public void DrawText(int X, int Y, string Text, string FontName, int CharSize, Pixel Fill, double Angle, FontWeightType Weight, bool Italic)
        {
            ImageContour Contour = ImageContour.CreateTextContour(X, Y, Text, FontName, CharSize, Angle, Weight, Italic);
            Operator.ContourOverlay(Contour, Fill, 0, 0);
        }

        void IImageContext.DrawText(int X, int Y, string Text, int CharSize, IPixel Fill)
            => DrawText(X, Y, Text, CharSize, Fill.ToPixel<Pixel>());
        void IImageContext.DrawText(int X, int Y, string Text, int CharSize, IPixel Fill, double Angle, FontWeightType Weight, bool Italic)
            => DrawText(X, Y, Text, CharSize, Fill.ToPixel<Pixel>(), Angle, Weight, Italic);
        void IImageContext.DrawText(int X, int Y, string Text, string FontName, int CharSize, IPixel Fill)
            => DrawText(X, Y, Text, FontName, CharSize, Fill.ToPixel<Pixel>());
        void IImageContext.DrawText(int X, int Y, string Text, string FontName, int CharSize, IPixel Fill, double Angle, FontWeightType Weight, bool Italic)
            => DrawText(X, Y, Text, FontName, CharSize, Fill.ToPixel<Pixel>(), Angle, Weight, Italic);

        #endregion

        #endregion

        #region Transform Processing

        #region Rotate
        public ImageContext<T> Rotate<T>(double Angle, bool Crop) where T : unmanaged, IPixel
        {
            if (Angle % 360d == 0)
                return Cast<T>();

            double Theta = Angle * MathHelper.UnitTheta,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            int Wo = Width,
                Lo = Height,
                Wt, Lt;

            if (Crop)
            {
                Wt = Wo;
                Lt = Lo;
            }
            else
            {
                Wt = (int)(Math.Abs(Wo * Cos) + Math.Abs(Lo * Sin));
                Lt = (int)(Math.Abs(Wo * Sin) + Math.Abs(Lo * Cos));
            }

            ImageContext<T> Result = new ImageContext<T>(Wt, Lt);
            double MaxWt = Wt - 1,
                   MaxLt = Lt - 1,
                   MaxWo = Wo - 1,
                   MaxLo = Lo - 1;

            for (int j = 0; j < Lt; j++)
            {
                IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, j);
                double FracX = j * Sin - (MaxWt * Cos + MaxLt * Sin - MaxWo) * 0.5d,
                       FracY = j * Cos + (MaxWt * Sin - MaxLt * Cos + MaxLo) * 0.5d;

                Operator.ScanLineRotateTo(0, 0, Wt, FracX, FracY, Sin, Cos, Adapter);
            }
            return Result;
        }
        public ImageContext<T> Rotate<T>(double Angle, bool Crop, ParallelOptions Options) where T : unmanaged, IPixel
        {
            if (Angle % 360d == 0)
                return Cast<T>(Options ?? DefaultParallelOptions);

            double Theta = Angle * MathHelper.UnitTheta,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            int Wo = Width,
                Lo = Height,
                Wt, Lt;

            if (Crop)
            {
                Wt = Wo;
                Lt = Lo;
            }
            else
            {
                Wt = (int)(Math.Abs(Wo * Cos) + Math.Abs(Lo * Sin));
                Lt = (int)(Math.Abs(Wo * Sin) + Math.Abs(Lo * Cos));
            }

            ImageContext<T> Result = new ImageContext<T>(Wt, Lt);
            double MaxWt = Wt - 1,
                   MaxLt = Lt - 1,
                   MaxWo = Wo - 1,
                   MaxLo = Lo - 1;

            Parallel.For(0, Lt, Options ?? DefaultParallelOptions, j =>
            {
                IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, j);
                double FracX = j * Sin - (MaxWt * Cos + MaxLt * Sin - MaxWo) * 0.5d,
                       FracY = j * Cos + (MaxWt * Sin - MaxLt * Cos + MaxLo) * 0.5d;

                Operator.ScanLineRotateTo(0, 0, Wt, FracX, FracY, Sin, Cos, Adapter);
            });
            return Result;
        }

        #endregion

        #region Resize
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(Width, Height);

            float StepX = (float)this.Width / Width,
                  StepY = (float)this.Height / Height,
                  SumStepY = 0f;
            switch (Interpolation)
            {
                case InterpolationTypes.Nearest:
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, j);
                            Operator.ScanLineNearestResizeTo(0, (int)SumStepY, Width, 0f, StepX, Adapter);

                            SumStepY += StepY;
                        }
                        break;
                    }
                case InterpolationTypes.Bilinear:
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            int Y = (int)SumStepY;
                            IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, j);
                            this.Operator.ScanLineBilinearResizeTo(0, Y, Width, 0f, StepX, SumStepY - Y, Adapter);

                            SumStepY += StepY;
                        }
                        break;
                    }
            }

            return Result;
        }
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(Width, Height);

            float StepX = (float)this.Width / Width,
                  StepY = (float)this.Height / Height;
            switch (Interpolation)
            {
                case InterpolationTypes.Nearest:
                    {
                        Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
                        {
                            IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, j);
                            Operator.ScanLineNearestResizeTo(0, (int)(StepY * j), Width, 0f, StepX, Adapter);
                        });
                        break;
                    }
                case InterpolationTypes.Bilinear:
                    {
                        Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
                        {
                            float SumStepY = StepY * j;
                            int Y = (int)SumStepY;
                            IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, j);
                            this.Operator.ScanLineBilinearResizeTo(0, Y, Width, 0f, StepX, SumStepY - Y, Adapter);
                        });
                        break;
                    }
            }

            return Result;
        }

        #endregion

        #region Flip
        public ImageContext<T> Flip<T>(FlipMode Mode)
            where T : unmanaged, IPixel
        {
            switch (Mode)
            {
                case FlipMode.Vertical:
                    {
                        ImageContext<T> Result = new ImageContext<T>(Width, Height);
                        long DestStride = Result.Stride;
                        byte* Dest0 = (byte*)Result.Scan0;

                        for (int y = 0; y < Height; y++)
                        {
                            T* pDest = (T*)(Dest0 + DestStride * (Height - 1 - y));
                            Operator.ScanLine<T>(0, y, Width, a => a.OverrideTo(pDest++));
                        }

                        return Result;
                    }
                case FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new ImageContext<T>(Width, Height);
                        long DestStride = Result.Stride;
                        byte* Dest0 = (byte*)Result.Scan0 + DestStride - sizeof(T);

                        for (int y = 0; y < Height; y++)
                        {
                            T* pDest = (T*)(Dest0 + DestStride * y);
                            Operator.ScanLine<T>(0, y, Width, a => a.OverrideTo(pDest--));
                        }

                        return Result;
                    }
                case FlipMode.Vertical | FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new ImageContext<T>(Width, Height);
                        long DestStride = Result.Stride;
                        byte* Dest0 = (byte*)Result.Scan0 + DestStride - sizeof(T);

                        for (int y = 0; y < Height; y++)
                        {
                            T* pDest = (T*)(Dest0 + DestStride * (Height - 1 - y));
                            Operator.ScanLine<T>(0, y, Width, a => a.OverrideTo(pDest--));
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
                        ImageContext<T> Result = new ImageContext<T>(Width, Height);
                        long DestStride = Result.Stride;
                        byte* Dest0 = (byte*)Result.Scan0;

                        Parallel.For(0, Height, Options, (y) =>
                        {
                            T* pDest = (T*)(Dest0 + DestStride * (Height - 1 - y));
                            Operator.ScanLine<T>(0, y, Width, a => a.OverrideTo(pDest++));
                        });

                        return Result;
                    }
                case FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new ImageContext<T>(Width, Height);
                        long DestStride = Result.Stride;
                        byte* Dest0 = (byte*)Result.Scan0 + DestStride - sizeof(T);

                        Parallel.For(0, Height, Options, (y) =>
                        {
                            T* pDest = (T*)(Dest0 + DestStride * y);
                            Operator.ScanLine<T>(0, y, Width, a => a.OverrideTo(pDest--));
                        });

                        return Result;
                    }
                case FlipMode.Vertical | FlipMode.Horizontal:
                    {
                        ImageContext<T> Result = new ImageContext<T>(Width, Height);
                        long DestStride = Result.Stride;
                        byte* Dest0 = (byte*)Result.Scan0 + DestStride - sizeof(T);

                        Parallel.For(0, Height, Options, (y) =>
                        {
                            T* pDest = (T*)(Dest0 + DestStride * (Height - 1 - y));
                            Operator.ScanLine<T>(0, y, Width, a => a.OverrideTo(pDest--));
                        });

                        return Result;
                    }
            }

            return Cast<T>(Options);
        }

        public ImageContext<T, U> Flip<T, U>(FlipMode Mode)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            switch (Mode)
            {
                case FlipMode.Vertical:
                    {
                        ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);
                        for (int Y = 0; Y < Height; Y++)
                        {
                            IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(0, Y);
                            Result.Operator.ScanLine<T>(0, Height - 1 - Y, Width, Adapter =>
                            {
                                T Pixel;
                                SAdapter.OverlayTo(&Pixel);
                                SAdapter.MoveNext();
                                Adapter.Override(Pixel);
                            });
                        }

                        return Result;
                    }
                case FlipMode.Horizontal:
                    {
                        ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

                        int Lx = Width - 1;
                        for (int Y = 0; Y < Height; Y++)
                        {
                            IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(Lx, Y);
                            Result.Operator.ScanLine<T>(0, Y, Width, Adapter =>
                            {
                                T Pixel;
                                SAdapter.OverlayTo(&Pixel);
                                SAdapter.MovePrevious();
                                Adapter.Override(Pixel);
                            });
                        }

                        return Result;
                    }
                case FlipMode.Vertical | FlipMode.Horizontal:
                    {
                        ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

                        int Lx = Width - 1;
                        for (int Y = 0; Y < Height; Y++)
                        {
                            IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(Lx, Y);
                            Result.Operator.ScanLine<T>(0, Height - 1 - Y, Width, Adapter =>
                            {
                                T Pixel;
                                SAdapter.OverlayTo(&Pixel);
                                SAdapter.MovePrevious();
                                Adapter.Override(Pixel);
                            });
                        }

                        return Result;
                    }
            }

            return Cast<T, U>();
        }
        public ImageContext<T, U> Flip<T, U>(FlipMode Mode, ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            switch (Mode)
            {
                case FlipMode.Vertical:
                    {
                        ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);
                        Parallel.For(0, Height, Options ?? DefaultParallelOptions, Y =>
                        {
                            IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(0, Y);
                            Result.Operator.ScanLine<T>(0, Height - 1 - Y, Width, Adapter =>
                            {
                                T Pixel;
                                SAdapter.OverlayTo(&Pixel);
                                SAdapter.MoveNext();
                                Adapter.Override(Pixel);
                            });
                        });

                        return Result;
                    }
                case FlipMode.Horizontal:
                    {
                        ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

                        int Lx = Width - 1;
                        Parallel.For(0, Height, Options ?? DefaultParallelOptions, Y =>
                        {
                            IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(Lx, Y);
                            Result.Operator.ScanLine<T>(0, Y, Width, Adapter =>
                            {
                                T Pixel;
                                SAdapter.OverlayTo(&Pixel);
                                SAdapter.MovePrevious();
                                Adapter.Override(Pixel);
                            });
                        });

                        return Result;
                    }
                case FlipMode.Vertical | FlipMode.Horizontal:
                    {
                        ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

                        int Lx = Width - 1;
                        Parallel.For(0, Height, Options ?? DefaultParallelOptions, Y =>
                        {
                            IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(Lx, Y);
                            Result.Operator.ScanLine<T>(0, Height - 1 - Y, Width, Adapter =>
                            {
                                T Pixel;
                                SAdapter.OverlayTo(&Pixel);
                                SAdapter.MovePrevious();
                                Adapter.Override(Pixel);
                            });
                        });

                        return Result;
                    }
            }

            return Cast<T, U>(Options);
        }

        #endregion

        #region Crop
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height)
            where T : unmanaged, IPixel
        {
            // If the rectangle is completely out of the bitmap
            if (X > this.Width || Y > this.Height)
                return new ImageContext<T>(0, 0);

            // Clamp to boundaries
            X = Math.Max(X, 0);
            Y = Math.Max(Y, 0);
            Width = Width.Clamp(0, this.Width - X);// Math.Max(Math.Min(Width, this.Width - X), 0);
            Height = Height.Clamp(0, this.Height - Y);// Math.Max(Math.Min(Height, this.Height - Y), 0);

            // Create Result
            ImageContext<T> Result = new ImageContext<T>(Width, Height);

            BlockCopy<T>(X, Y, Width, Height, (byte*)Result.Scan0, Result.Stride);

            return Result;
        }
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height, ParallelOptions Options)
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

            BlockCopy<T>(X, Y, Width, Height, (byte*)Result.Scan0, Result.Stride, Options);

            return Result;
        }

        public ImageContext<T, U> Crop<T, U>(int X, int Y, int Width, int Height)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            // If the rectangle is completely out of the bitmap
            if (X > this.Width || Y > this.Height)
                return new ImageContext<T, U>(0, 0);

            // Clamp to boundaries
            X = Math.Max(X, 0);
            Y = Math.Max(Y, 0);
            Width = Math.Max(Math.Min(Width, this.Width - X), 0);
            Height = Math.Max(Math.Min(Height, this.Height - Y), 0);

            // Create Result
            ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

            for (int j = 0; j < Height; j++)
            {
                IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(X, Y + j);
                Result.Operator.ScanLine<T>(0, j, Width, Adapter =>
                {
                    T Pixel;
                    SAdapter.OverrideTo(&Pixel);
                    SAdapter.MoveNext();
                    Adapter.Override(Pixel);
                });
            }

            return Result;
        }
        public ImageContext<T, U> Crop<T, U>(int X, int Y, int Width, int Height, ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            // If the rectangle is completely out of the bitmap
            if (X > this.Width || Y > this.Height)
                return new ImageContext<T, U>(0, 0);

            // Clamp to boundaries
            X = Math.Max(X, 0);
            Y = Math.Max(Y, 0);
            Width = Math.Max(Math.Min(Width, this.Width - X), 0);
            Height = Math.Max(Math.Min(Height, this.Height - Y), 0);

            // Create Result
            ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

            Parallel.For(0, Height, Options ?? DefaultParallelOptions, j =>
            {
                IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(X, Y + j);
                Result.Operator.ScanLine<T>(0, j, Width, Adapter =>
                {
                    T Pixel;
                    SAdapter.OverrideTo(&Pixel);
                    SAdapter.MoveNext();
                    Adapter.Override(Pixel);
                });
            });

            return Result;
        }

        #endregion

        #region Convolute
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel)
            where T : unmanaged, IPixel
            => Filter<T>(Kernel);
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel, ParallelOptions Options)
            where T : unmanaged, IPixel
            => Filter<T>(Kernel, Options);

        #endregion

        #region Filter
        public ImageContext<T> Filter<T>(ImageFilter Filter)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(Width, Height);

            for (int y = 0; y < Height; y++)
            {
                IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, y);
                Operator.ScanLineFilterTo(0, y, Width, Filter, Adapter);
            };

            return Result;
        }
        public ImageContext<T> Filter<T>(ImageFilter Filter, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(Width, Height);

            Parallel.For(0, Height, Options ?? DefaultParallelOptions, y =>
            {
                IPixelAdapter<Pixel> Adapter = Result.Operator.GetAdapter<Pixel>(0, y);
                Operator.ScanLineFilterTo(0, y, Width, Filter, Adapter);
            });

            return Result;
        }

        #endregion

        #region Cast
        public ImageContext<T> Cast<T>()
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(Width, Height);
            BlockCopy<T>(0, 0, Width, Height, (byte*)Result.Scan0, Result.Stride);
            return Result;
        }
        public ImageContext<T> Cast<T>(ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            ImageContext<T> Result = new ImageContext<T>(Width, Height);
            BlockCopy<T>(0, 0, Width, Height, (byte*)Result.Scan0, Result.Stride, Options);
            return Result;
        }

        public ImageContext<T, U> Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);

            for (int Y = 0; Y < Height; Y++)
            {
                IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(0, Y);
                Result.Operator.ScanLine<T>(0, Y, Width, Adapter =>
                {
                    T Pixel;
                    SAdapter.OverlayTo(&Pixel);
                    SAdapter.MoveNext();
                    Adapter.Override(Pixel);
                });
            }

            return Result;
        }
        public ImageContext<T, U> Cast<T, U>(ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            ImageContext<T, U> Result = new ImageContext<T, U>(Width, Height);
            Parallel.For(0, Height, Options ?? DefaultParallelOptions, Y =>
            {
                IPixelAdapter<T> SAdapter = Operator.GetAdapter<T>(0, Y);
                Result.Operator.ScanLine<T>(0, Y, Width, Adapter =>
                {
                    T Pixel;
                    SAdapter.OverlayTo(&Pixel);
                    SAdapter.MoveNext();
                    Adapter.Override(Pixel);
                });
            });

            return Result;
        }

        #endregion

        #region Clear
        public void Clear(Pixel Color)
        {
            for (int j = 0; j < Height; j++)
                Operator.ScanLine<Pixel>(0, j, Width, a => a.Override(Color));
        }
        public void Clear(Pixel Color, ParallelOptions Options)
            => Parallel.For(0, Height, Options ?? DefaultParallelOptions, j => Operator.ScanLine<Pixel>(0, j, Width, a => a.Override(Color)));

        void IImageContext.Clear(IPixel Color)
            => Clear(Color.ToPixel<Pixel>());
        void IImageContext.Clear(IPixel Color, ParallelOptions Options)
            => Clear(Color.ToPixel<Pixel>(), Options);

        #endregion

        #endregion

        #region Buffer Processing

        #region BlockCopy
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, Width * sizeof(T), Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                BlockCopy<T>(X, Y, Width, Height, (byte*)pDest, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T));
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, pDest, Width * sizeof(T), Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                BlockCopy<T>(X, Y, Width, Height, pDest, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T));
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, Width * sizeof(T), Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, (byte*)Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel));
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options)
            where T : unmanaged, IPixel => BlockCopy<T>(X, Y, Width, Height, Dest0, Width * sizeof(Pixel), Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            where T : unmanaged, IPixel
        {
            for (int j = 0; j < Height; j++)
            {
                T* pDest = (T*)(Dest0 + DestStride * j);
                Operator.ScanLine<T>(X, Y + j, Width, a => a.OverrideTo(pDest++));
            }
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options)
            where T : unmanaged, IPixel
        {
            Parallel.For(0, Height, Options ?? DefaultParallelOptions, (j) =>
            {
                T* pDest = (T*)(Dest0 + DestStride * j);
                Operator.ScanLine<T>(X, Y + j, Width, a => a.OverrideTo(pDest++));
            });
        }

        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, Width, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                BlockCopy3(X, Y, Width, Height, pDestR, pDestG, pDestB, DestStride, Options);
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
            for (int j = 0; j < Height; j++)
            {
                long Offset = DestStride * j;
                byte* pDestR = DestR + Offset,
                      pDestG = DestG + Offset,
                      pDestB = DestB + Offset;

                Operator.ScanLine<Pixel>(X, Y + j, Width, a => a.OverrideTo(pDestR++, pDestG++, pDestB++));
            }
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
            => Parallel.For(0, Height, Options ?? DefaultParallelOptions, (j) =>
            {
                long Offset = DestStride * j;
                byte* pDestR = DestR + Offset,
                      pDestG = DestG + Offset,
                      pDestB = DestB + Offset;

                Operator.ScanLine<Pixel>(X, Y + j, Width, a => a.OverrideTo(pDestR++, pDestG++, pDestB++));
            });

        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, Width, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[0],
                         pDestR = &DestR[0],
                         pDestG = &DestG[0],
                         pDestB = &DestB[0])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            fixed (byte* pDestA = &DestA[DestOffset],
                         pDestR = &DestR[DestOffset],
                         pDestG = &DestG[DestOffset],
                         pDestB = &DestB[DestOffset])
                BlockCopy4(X, Y, Width, Height, pDestA, pDestR, pDestG, pDestB, DestStride, Options);
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
            for (int j = 0; j < Height; j++)
            {
                long Offset = DestStride * j;
                byte* pDestA = DestA + Offset,
                      pDestR = DestR + Offset,
                      pDestG = DestG + Offset,
                      pDestB = DestB + Offset;

                Operator.ScanLine<Pixel>(X, Y + j, Width, a => a.OverrideTo(pDestA++, pDestR++, pDestG++, pDestB++));
            }
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
            => Parallel.For(0, Height, Options ?? DefaultParallelOptions, (j) =>
            {
                long Offset = DestStride * j;
                byte* pDestA = DestA + Offset,
                      pDestR = DestR + Offset,
                      pDestG = DestG + Offset,
                      pDestB = DestB + Offset;

                Operator.ScanLine<Pixel>(X, Y + j, Width, a => a.OverrideTo(pDestA++, pDestR++, pDestG++, pDestB++));
            });

        #endregion

        #region ScanLineCopy
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0)
            where T : unmanaged, IPixel => ScanLineCopy<T>(OffsetX, Y, Length, (byte*)Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[0])
                ScanLineCopy<T>(OffsetX, Y, Length, (byte*)pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (T* pDest = &Dest0[DestOffset])
                ScanLineCopy<T>(OffsetX, Y, Length, (byte*)pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[0])
                ScanLineCopy<T>(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
            where T : unmanaged, IPixel
        {
            fixed (byte* pDest = &Dest0[DestOffset])
                ScanLineCopy<T>(OffsetX, Y, Length, pDest);
        }
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, IntPtr Dest0)
            where T : unmanaged, IPixel => ScanLineCopy<T>(OffsetX, Y, Length, (byte*)Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte* Dest0)
            where T : unmanaged, IPixel
        {
            T* pDest = (T*)Dest0;
            Operator.ScanLine<T>(OffsetX, Y, Length, a => a.OverrideTo(pDest++));
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
            => Operator.ScanLine<Pixel>(OffsetX, Y, Length, a => a.OverrideTo(DestR++, DestG++, DestB++));

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
            => Operator.ScanLine<Pixel>(OffsetX, Y, Length, a => a.OverrideTo(DestA++, DestR++, DestG++, DestB++));

        #endregion

        #endregion

        public ImageContext<Pixel> Clone()
            => Cast<Pixel>();

        object ICloneable.Clone()
            => Clone();

    }
}