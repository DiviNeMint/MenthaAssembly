using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {

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

        public unsafe void DrawLine(double X0, double Y0, double X1, double Y1, Pixel Color, double PenWidth)
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
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.Scan0, X0, X1, Y, Color);
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

        public unsafe void DrawLine2(double X0, double Y0, double X1, double Y1, Pixel Color, double PenWidth)
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
                    FillScan = (X0, X1, Y) => this.FillScan((byte*)this.Scan0, X0, X1, Y, Color);
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

        private unsafe void FillScan(byte* Scan0, int X0, int X1, int Y, Pixel Pixel)
        {
            if (X0 > X1)
                MathHelper.Swap(ref X0, ref X1);

            X0 = Math.Max(0, X0);
            X1 = Math.Min(this.Width, X1);

            long Offset = (long)Y * Stride + (X0 * BitsPerPixel >> 3);
            Pixel* Scan = (Pixel*)(Scan0 + Offset);

            // Draw
            for (; X0 <= X1; X0++)
                *Scan++ = Pixel;
        }
        private unsafe void FillScan(byte* ScanR0, byte* ScanG0, byte* ScanB0, int X0, int X1, int Y, Pixel Pixel)
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
        private unsafe void FillScan(byte* ScanA0, byte* ScanR0, byte* ScanG0, byte* ScanB0, int X0, int X1, int Y, Pixel Pixel)
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


        /// <summary>
        /// Draws a colored line by connecting two points using the Bresenham algorithm.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start point.</param>
        /// <param name="Y0">The y-coordinate of the start point.</param>
        /// <param name="X1">The x-coordinate of the end point.</param>
        /// <param name="Y1">The y-coordinate of the end point.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLineBresenham(int X0, int Y0, int X1, int Y1, Pixel Color)
        {
            // Distance start and end point
            int dx = X1 - X0;
            int dy = Y1 - Y0;

            // Determine sign for direction x
            int incx = 0;
            if (dx < 0)
            {
                dx = -dx;
                incx = -1;
            }
            else if (dx > 0)
            {
                incx = 1;
            }

            // Determine sign for direction y
            int incy = 0;
            if (dy < 0)
            {
                dy = -dy;
                incy = -1;
            }
            else if (dy > 0)
            {
                incy = 1;
            }

            // Which gradient is larger
            int pdx, pdy, odx, ody, es, el;
            if (dx > dy)
            {
                pdx = incx;
                pdy = 0;
                odx = incx;
                ody = incy;
                es = dy;
                el = dx;
            }
            else
            {
                pdx = 0;
                pdy = incy;
                odx = incx;
                ody = incy;
                es = dx;
                el = dy;
            }

            // Init start
            int x = X0,
                y = Y0,
                error = el >> 1;
            if (0 <= x && x < this.Width &&
                0 <= y && y < this.Height)
                SetPixel(x, y, Color);

            // Walk the line!
            for (int i = 0; i < el; i++)
            {
                // Update error term
                error -= es;

                // Decide which coord to use
                if (error < 0)
                {
                    error += el;
                    x += odx;
                    y += ody;
                }
                else
                {
                    x += pdx;
                    y += pdy;
                }

                // Set pixel
                if (0 <= x && x < this.Width &&
                    0 <= y && y < this.Height)
                    SetPixel(x, y, Color);
            }

        }

        /// <summary>
        /// Draws a colored line by connecting two points using a DDA algorithm (Digital Differential Analyzer).
        /// </summary>
        /// <param name="X0">The x-coordinate of the start point.</param>
        /// <param name="Y0">The y-coordinate of the start point.</param>
        /// <param name="X1">The x-coordinate of the end point.</param>
        /// <param name="Y1">The y-coordinate of the end point.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLineDDA(int X0, int Y0, int X1, int Y1, Pixel Color)
        {
            // Distance start and end point
            int dx = X1 - X0,
                dy = Y1 - Y0;

            // Determine slope (absolute value)
            int len = dy >= 0 ? dy : -dy,
                lenx = dx >= 0 ? dx : -dx;

            if (lenx > len)
                len = lenx;

            // Prevent division by zero
            if (len != 0)
            {
                // Init steps and start
                float incx = dx / (float)len,
                      incy = dy / (float)len,
                      x = X0,
                      y = Y0;

                // Walk the line!
                for (int i = 0; i < len; i++)
                {
                    if (0 <= x && x < this.Width &&
                        0 <= y && y < this.Height)
                        SetPixel((int)x, (int)y, Color);

                    x += incx;
                    y += incy;
                }
            }
        }

        /// <summary>
        /// Draws a colored line by connecting two points using an optimized DDA. 
        /// Uses the pixels array and the width directly for best performance.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start point.</param>
        /// <param name="Y0">The y-coordinate of the start point.</param>
        /// <param name="X1">The x-coordinate of the end point.</param>
        /// <param name="Y1">The y-coordinate of the end point.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, Pixel Color)
        {
            // Get clip coordinates
            int clipX1 = 0;
            int clipX2 = Width;
            int clipY1 = 0;
            int clipY2 = Height;

            //// Perform cohen-sutherland clipping if either point is out of the viewport
            //if (!CohenSutherlandLineClip(new Rect(clipX1, clipY1, clipX2 - clipX1, clipY2 - clipY1), ref X0, ref Y0, ref X1, ref Y1)) return;

            // Distance start and end point
            int dx = X1 - X0,
                dy = Y1 - Y0;

            const int PRECISION_SHIFT = 8;

            // Determine slope (absolute value)
            int lenX = dx < 0 ? -dx : dx,
                lenY = dy < 0 ? -dy : dy;

            if (lenX > lenY)
            { // x increases by +/- 1
                if (dx < 0)
                {
                    int t = X0;
                    X0 = X1;
                    X1 = t;
                    t = Y0;
                    Y0 = Y1;
                    Y1 = t;
                }

                // Init steps and start
                int incy = (dy << PRECISION_SHIFT) / dx;

                int y1s = Y0 << PRECISION_SHIFT;
                int y2s = Y1 << PRECISION_SHIFT;
                int hs = Height << PRECISION_SHIFT;

                if (Y0 < Y1)
                {
                    if (Y0 >= clipY2 || Y1 < clipY1)
                        return;

                    if (y1s < 0)
                    {
                        if (incy == 0)
                            return;

                        int oldy1s = y1s;
                        // Find lowest y1s that is greater or equal than 0.
                        y1s = incy - 1 + ((y1s + 1) % incy);
                        X0 += (y1s - oldy1s) / incy;
                    }
                    if (y2s >= hs)
                    {
                        if (incy != 0)
                        {
                            // Find highest y2s that is less or equal than ws - 1.
                            // y2s = y1s + n * incy. Find n.
                            y2s = hs - 1 - (hs - 1 - y1s) % incy;
                            X1 = X0 + (y2s - y1s) / incy;
                        }
                    }
                }
                else
                {
                    if (Y1 >= clipY2 || Y0 < clipY1)
                        return;

                    if (y1s >= hs)
                    {
                        if (incy == 0)
                            return;

                        int oldy1s = y1s;
                        // Find highest y1s that is less or equal than ws - 1.
                        // y1s = oldy1s + n * incy. Find n.
                        y1s = hs - 1 + (incy - (hs - 1 - oldy1s) % incy);
                        X0 += (y1s - oldy1s) / incy;
                    }
                    if (y2s < 0)
                    {
                        if (incy != 0)
                        {
                            // Find lowest y2s that is greater or equal than 0.
                            // y2s = y1s + n * incy. Find n.
                            y2s = y1s % incy;
                            X1 = X0 + (y2s - y1s) / incy;
                        }
                    }
                }

                if (X0 < 0)
                {
                    y1s -= incy * X0;
                    X0 = 0;
                }
                if (X1 >= Width)
                {
                    X1 = Width - 1;
                }

                int ys = y1s;

                // Walk the line!
                int y = ys >> PRECISION_SHIFT;
                int previousY = y;
                //int index = X0 + y * Width;
                //int k = incy < 0 ? 1 - Width : 1 + Width;
                for (int x = X0; x <= X1; ++x)
                {
                    SetPixel(x, y, Color);

                    //pixels[index] = Color;
                    ys += incy;
                    y = ys >> PRECISION_SHIFT;
                    //if (y != previousY)
                    //{
                    //    previousY = y;
                    //    index += k;
                    //}
                    //else
                    //{
                    //    ++index;
                    //}

                    if (y != previousY)
                        previousY = y;
                }
            }
            else
            {
                // Prevent division by zero
                if (lenY == 0)
                    return;

                if (dy < 0)
                {
                    int t = X0;
                    X0 = X1;
                    X1 = t;
                    t = Y0;
                    Y0 = Y1;
                    Y1 = t;
                }

                // Init steps and start
                int x1s = X0 << PRECISION_SHIFT;
                int x2s = X1 << PRECISION_SHIFT;
                int ws = Width << PRECISION_SHIFT;

                int incx = (dx << PRECISION_SHIFT) / dy;

                if (X0 < X1)
                {
                    if (X0 >= clipX2 || X1 < clipX1)
                        return;

                    if (x1s < 0)
                    {
                        if (incx == 0)
                            return;

                        int oldx1s = x1s;
                        // Find lowest x1s that is greater or equal than 0.
                        x1s = incx - 1 + ((x1s + 1) % incx);
                        Y0 += (x1s - oldx1s) / incx;
                    }
                    if (x2s >= ws)
                    {
                        if (incx != 0)
                        {
                            // Find highest x2s that is less or equal than ws - 1.
                            // x2s = x1s + n * incx. Find n.
                            x2s = ws - 1 - (ws - 1 - x1s) % incx;
                            Y1 = Y0 + (x2s - x1s) / incx;
                        }
                    }
                }
                else
                {
                    if (X1 >= clipX2 || X0 < clipX1)
                        return;

                    if (x1s >= ws)
                    {
                        if (incx == 0)
                            return;

                        int oldx1s = x1s;
                        // Find highest x1s that is less or equal than ws - 1.
                        // x1s = oldx1s + n * incx. Find n.
                        x1s = ws - 1 + (incx - (ws - 1 - oldx1s) % incx);
                        Y0 += (x1s - oldx1s) / incx;
                    }
                    if (x2s < 0)
                    {
                        if (incx != 0)
                        {
                            // Find lowest x2s that is greater or equal than 0.
                            // x2s = x1s + n * incx. Find n.
                            x2s = x1s % incx;
                            Y1 = Y0 + (x2s - x1s) / incx;
                        }
                    }
                }

                if (Y0 < 0)
                {
                    x1s -= incx * Y0;
                    Y0 = 0;
                }
                if (Y1 >= Height)
                {
                    Y1 = Height - 1;
                }

                //int index = x1s;
                //int indexBaseValue = Y0 * Width;

                //// Walk the line!
                //int inc = (Width << PRECISION_SHIFT) + incx;
                //for (int y = Y0; y <= Y1; ++y)
                //{
                //    pixels[indexBaseValue + (index >> PRECISION_SHIFT)] = Color;
                //    index += inc;
                //}

                int x = x1s;
                int indexBaseValue = Y0 * Width;

                // Walk the line!
                for (int y = Y0; y <= Y1; ++y)
                {
                    SetPixel(x >> PRECISION_SHIFT, y, Color);
                    x += incx;
                }
            }
        }

        /// <summary>
        /// Draws a line using a pen / stamp for the line 
        /// </summary>
        /// <param name="X0">The x-coordinate of the start point.</param>
        /// <param name="Y0">The y-coordinate of the start point.</param>
        /// <param name="X1">The x-coordinate of the end point.</param>
        /// <param name="Y1">The y-coordinate of the end point.</param>
        /// <param name="pen">The pen context.</param>
        public void DrawLinePenned(int x1, int y1, int x2, int y2, ImageContextBase<Pixel, Struct> pen)
        {
            // Edge case where lines that went out of vertical bounds clipped instead of disappearing
            if ((y1 < 0 && y2 < 0) || (y1 > Height && y2 > Height))
                return;

            if (x1 == x2 && y1 == y2)
                return;

            int size = pen.Width;
            Int32Bound srcRect = new Int32Bound(0, 0, size, size);

            // Distance start and end point
            int dx = x2 - x1;
            int dy = y2 - y1;

            // Determine sign for direction x
            int incx = 0;
            if (dx < 0)
            {
                dx = -dx;
                incx = -1;
            }
            else if (dx > 0)
            {
                incx = 1;
            }

            // Determine sign for direction y
            int incy = 0;
            if (dy < 0)
            {
                dy = -dy;
                incy = -1;
            }
            else if (dy > 0)
            {
                incy = 1;
            }

            // Which gradient is larger
            int pdx, pdy, odx, ody, es, el;
            if (dx > dy)
            {
                pdx = incx;
                pdy = 0;
                odx = incx;
                ody = incy;
                es = dy;
                el = dx;
            }
            else
            {
                pdx = 0;
                pdy = incy;
                odx = incx;
                ody = incy;
                es = dx;
                el = dy;
            }

            // Init start
            int x = x1;
            int y = y1;
            int error = el >> 1;

            Int32Bound destRect = new Int32Bound(x, y, x + size, y + size);

            if (y < Height && y >= 0 && x < Width && x >= 0)
            {
                //Blit(context.WriteableBitmap, new Rect(x,y,3,3), pen.WriteableBitmap, new Rect(0,0,3,3));
                Blit(destRect, pen, srcRect);
                //pixels[y * w + x] = color;
            }

            // Walk the line!
            for (int i = 0; i < el; i++)
            {
                // Update error term
                error -= es;

                // Decide which coord to use
                if (error < 0)
                {
                    error += el;
                    x += odx;
                    y += ody;
                }
                else
                {
                    x += pdx;
                    y += pdy;
                }

                // Set pixel
                if (y < Height && y >= 0 && x < Width && x >= 0)
                {
                    //Blit(context, w, h, destRect, pen, srcRect, pw);
                    Blit(new Int32Bound(x, y, x + size, y + size), pen, srcRect);
                    //Blit(context.WriteableBitmap, destRect, pen.WriteableBitmap, srcRect);
                    //pixels[y * w + x] = color;
                }
            }
        }


        /// <summary>
        /// Draws a colored dotted line
        /// </summary>
        /// <param name="X0">The x-coordinate of the start point.</param>
        /// <param name="Y0">The y-coordinate of the start point.</param>
        /// <param name="X1">The x-coordinate of the end point.</param>
        /// <param name="Y1">The y-coordinate of the end point.</param>
        /// <param name="DotSpace">length of space between each line segment</param>
        /// <param name="DotLength">length of each line segment</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawDottedLine(int X0, int Y0, int X1, int Y1, int DotSpace, int DotLength, Pixel Color)
        {
            if (X0 == X1)       // Vertically
            {
                if (Y1 < Y0)
                    MathHelper.Swap(ref Y0, ref Y1);

                // Draw
                {
                    if (X0 < 0 || X0 > Width)
                        return;

                    bool on = true;
                    int spaceCnt = 0;
                    for (int i = Y0; i <= Y1; i++)
                    {
                        if (i < 1)
                            continue;

                        if (i >= Height)
                            break;

                        if (on)
                        {
                            SetPixel(X0, i - 1, Color);

                            on = i % DotLength != 0;
                            spaceCnt = 0;
                        }
                        else
                        {
                            spaceCnt++;
                            on = spaceCnt % DotSpace == 0;
                        }
                    }
                }
            }
            else if (Y0 == Y1)  // Horizontally
            {
                if (X1 < X0)
                    MathHelper.Swap(ref X0, ref X1);

                // Draw
                {
                    if (Y0 < 0 || Y0 > Height)
                        return;

                    bool on = true;
                    int spaceCnt = 0;
                    for (int i = X0; i <= X1; i++)
                    {
                        if (i < 1)
                            continue;

                        if (i >= Width)
                            break;

                        if (Y0 >= Height)
                            break;

                        if (on)
                        {
                            SetPixel(i - 1, Y0, Color);

                            on = i % DotLength != 0;
                            spaceCnt = 0;
                        }
                        else
                        {
                            spaceCnt++;
                            on = spaceCnt % DotSpace == 0;
                        }
                    }
                }
            }
            else
            {
                if (X1 < X0)
                {
                    MathHelper.Swap(ref X0, ref X1);
                    MathHelper.Swap(ref Y0, ref Y1);
                }

                float m = (Y1 - Y0) / (float)(X1 - X0),
                      n = Y0 - m * X0;

                bool on = true;
                int spaceCnt = 0;
                for (int i = X0; i <= Width; i++)
                {
                    if (i == 0)
                        continue;

                    int y = (int)(m * i + n);
                    if (y <= 0)
                        continue;

                    if (y >= Height || i >= X1)
                        continue;

                    if (on)
                    {
                        SetPixel(i - 1, y - 1, Color);

                        spaceCnt = 0;
                        on = i % DotLength != 0;
                    }
                    else
                    {
                        spaceCnt++;
                        on = spaceCnt % DotSpace == 0;
                    }
                }
            }
        }

        /// <summary>
        /// Draws a segment of a Cardinal spline (cubic) defined by four control points.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st control point.</param>
        /// <param name="Y1">The y-coordinate of the 1st control point.</param>
        /// <param name="X2">The x-coordinate of the 2nd control point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd control point.</param>
        /// <param name="X3">The x-coordinate of the 3rd control point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd control point.</param>
        /// <param name="X4">The x-coordinate of the 4th control point.</param>
        /// <param name="Y4">The y-coordinate of the 4th control point.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color.</param>
        private void DrawCurveSegment(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, float Tension, Pixel Color)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            int minX = Math.Min(X1, Math.Min(X2, Math.Min(X3, X4))),
                minY = Math.Min(Y1, Math.Min(Y2, Math.Min(Y3, Y4))),
                maxX = Math.Max(X1, Math.Max(X2, Math.Max(X3, X4))),
                maxY = Math.Max(Y1, Math.Max(Y2, Math.Max(Y3, Y4)));

            // Get slope
            int len = Math.Max(maxY - minY, maxX - minX);

            // Prevent division by zero
            if (len != 0)
            {
                // Init vars
                float step = 2f / len;
                int tx1 = X2,
                    ty1 = Y2,
                    tx2,
                    ty2;

                // Calculate factors
                float sx1 = Tension * (X3 - X1),
                      sy1 = Tension * (Y3 - Y1),
                      sx2 = Tension * (X4 - X2),
                      sy2 = Tension * (Y4 - Y2),
                      ax = sx1 + sx2 + 2 * X2 - 2 * X3,
                      ay = sy1 + sy2 + 2 * Y2 - 2 * Y3,
                      bx = -2 * sx1 - sx2 - 3 * X2 + 3 * X3,
                      by = -2 * sy1 - sy2 - 3 * Y2 + 3 * Y3;

                // Interpolate
                for (float t = step; t <= 1; t += step)
                {
                    float tSq = t * t;

                    tx2 = (int)(ax * tSq * t + bx * tSq + sx1 * t + X2);
                    ty2 = (int)(ay * tSq * t + by * tSq + sy1 * t + Y2);

                    // Draw line
                    DrawLine(tx1, ty1, tx2, ty2, Color);
                    tx1 = tx2;
                    ty1 = ty2;
                }

                // Prevent rounding gap
                DrawLine(tx1, ty1, X3, Y3, Color);
            }
        }

        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurve(int[] Points, float Tension, Pixel Color)
        {
            // First segment
            DrawCurveSegment(Points[0], Points[1], Points[0], Points[1], Points[2], Points[3], Points[4], Points[5], Tension, Color);

            // Middle segments
            int i;
            for (i = 2; i < Points.Length - 4; i += 2)
            {
                DrawCurveSegment(Points[i - 2], Points[i - 1], Points[i], Points[i + 1], Points[i + 2], Points[i + 3], Points[i + 4], Points[i + 5], Tension, Color);
            }

            // Last segment
            DrawCurveSegment(Points[i - 2], Points[i - 1], Points[i], Points[i + 1], Points[i + 2], Points[i + 3], Points[i + 2], Points[i + 3], Tension, Color);
        }

        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurve(Int32Point[] Points, float Tension, Pixel Color)
        {
            // First segment
            DrawCurveSegment(Points[0].X, Points[0].Y, Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, Points[2].X, Points[2].Y, Tension, Color);

            // Middle segments
            int i;
            for (i = 1; i < Points.Length - 2; i++)
            {
                DrawCurveSegment(Points[i - 1].X, Points[i - 1].Y, Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Points[i + 2].X, Points[i + 2].Y, Tension, Color);
            }

            // Last segment
            DrawCurveSegment(Points[i - 1].X, Points[i - 1].Y, Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Points[i + 1].X, Points[i + 1].Y, Tension, Color);
        }

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public void DrawCurveClosed(int[] points, float tension, Pixel color)
        {
            int pn = points.Length;

            // First segment
            DrawCurveSegment(points[pn - 2], points[pn - 1], points[0], points[1], points[2], points[3], points[4], points[5], tension, color);

            // Middle segments
            int i;
            for (i = 2; i < pn - 4; i += 2)
            {
                DrawCurveSegment(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2], points[i + 3], points[i + 4], points[i + 5], tension, color);
            }

            // Last segment
            DrawCurveSegment(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2], points[i + 3], points[0], points[1], tension, color);

            // Last-to-First segment
            DrawCurveSegment(points[i], points[i + 1], points[i + 2], points[i + 3], points[0], points[1], points[2], points[3], tension, color);
        }

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurveClosed(Int32Point[] Points, float Tension, Pixel Color)
        {
            int pn = Points.Length;

            // First segment
            DrawCurveSegment(Points[pn - 1].X, Points[pn - 1].Y, Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, Points[2].X, Points[2].Y, Tension, Color);

            // Middle segments
            int i;
            for (i = 1; i < pn - 2; i++)
            {
                DrawCurveSegment(Points[i - 1].X, Points[i - 1].Y, Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Points[i + 2].X, Points[i + 2].Y, Tension, Color);
            }

            // Last segment
            DrawCurveSegment(Points[i - 1].X, Points[i - 1].Y, Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Points[0].X, Points[0].Y, Tension, Color);

            // Last-to-First segment
            DrawCurveSegment(Points[i].X, Points[i].Y, Points[i + 1].X, Points[i + 1].Y, Points[0].X, Points[0].Y, Points[1].X, Points[1].Y, Tension, Color);
        }

        /// <summary>
        /// Draws a cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="X1">The x-coordinate of the start point.</param>
        /// <param name="Y1">The y-coordinate of the start point.</param>
        /// <param name="CX1">The x-coordinate of the 1st control point.</param>
        /// <param name="CY1">The y-coordinate of the 1st control point.</param>
        /// <param name="CX2">The x-coordinate of the 2nd control point.</param>
        /// <param name="CY2">The y-coordinate of the 2nd control point.</param>
        /// <param name="X2">The x-coordinate of the end point.</param>
        /// <param name="Y2">The y-coordinate of the end point.</param>
        /// <param name="Color">The color.</param>
        public void DrawBezier(int X1, int Y1, int CX1, int CY1, int CX2, int CY2, int X2, int Y2, Pixel Color)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            int minX = Math.Min(X1, Math.Min(CX1, Math.Min(CX2, X2))),
                minY = Math.Min(Y1, Math.Min(CY1, Math.Min(CY2, Y2))),
                maxX = Math.Max(X1, Math.Max(CX1, Math.Max(CX2, X2))),
                maxY = Math.Max(Y1, Math.Max(CY1, Math.Max(CY2, Y2)));

            // Get slope
            int len = Math.Max(maxY - minY, maxX - minX);

            // Prevent division by zero
            if (len != 0)
            {
                // Init vars
                float step = 2f / len;
                int tx1 = X1,
                    ty1 = Y1,
                    tx2, ty2;

                // Interpolate
                for (float t = step; t <= 1; t += step)
                {
                    float tSq = t * t,
                          t1 = 1 - t,
                          t1Sq = t1 * t1;

                    tx2 = (int)(t1 * t1Sq * X1 + 3 * t * t1Sq * CX1 + 3 * t1 * tSq * CX2 + t * tSq * X2);
                    ty2 = (int)(t1 * t1Sq * Y1 + 3 * t * t1Sq * CY1 + 3 * t1 * tSq * CY2 + t * tSq * Y2);

                    // Draw line
                    DrawLine(tx1, ty1, tx2, ty2, Color);

                    tx1 = tx2;
                    ty1 = ty2;
                }

                // Prevent rounding gap
                DrawLine(tx1, ty1, X2, Y2, Color);
            }
        }

        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawBeziers(int[] Points, Pixel Color)
        {
            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            for (int i = 2; i + 5 < Points.Length; i += 6)
            {
                x2 = Points[i + 4];
                y2 = Points[i + 5];
                DrawBezier(x1, y1, Points[i], Points[i + 1], Points[i + 2], Points[i + 3], x2, y2, Color);

                x1 = x2;
                y1 = y2;
            }
        }

    }
}
