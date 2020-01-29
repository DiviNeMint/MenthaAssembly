using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MenthaAssembly
{
    public class ImageContext
    {
        public int Width { get; }

        public int Height { get; }

        public int Stride { get; }

        public int Channels { get; }

        internal protected IntPtr? _Scan0;
        public IntPtr Scan0
        {
            get
            {
                if (_Scan0 is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* S0 = &this.Datas[0][0])
                        return (IntPtr)S0;
                }
            }
        }

        internal protected IntPtr? _ScanA;
        public IntPtr ScanA
        {
            get
            {
                if (_ScanA is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* A = &this.Datas[1][0])
                        return (IntPtr)A;
                }
            }
        }

        internal protected IntPtr? _ScanR;
        public IntPtr ScanR
        {
            get
            {
                if (_ScanR is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* R = &this.Datas[2][0])
                        return (IntPtr)R;
                }
            }
        }

        internal protected IntPtr? _ScanG;
        public IntPtr ScanG
        {
            get
            {
                if (_ScanG is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* G = &this.Datas[3][0])
                        return (IntPtr)G;
                }
            }
        }

        internal protected IntPtr? _ScanB;
        public IntPtr ScanB
        {
            get
            {
                if (_ScanB is IntPtr Result)
                    return Result;

                unsafe
                {
                    fixed (byte* B = &this.Datas[4][0])
                        return (IntPtr)B;
                }
            }
        }

        public int BitsPerPixel { get; }

        public IList<int> Palette { get; }

        public ImageContext(int Width, int Height, IntPtr Scan0, IList<int> Palette = null) : this(Width, Height, Scan0, Width, Palette)
        {
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = (Stride << 3) / Width;
            this.Palette = Palette;
            this._Scan0 = Scan0;
            this.Channels = 1;
        }
        public ImageContext(int Width, int Height, IntPtr Scan0, int Stride, int BitsPerPixel, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this.BitsPerPixel = BitsPerPixel;
            this.Palette = Palette;
            this._Scan0 = Scan0;
            this.Channels = 1;
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) : this(Width, Height, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;
            this.BitsPerPixel = 24;
            this.Channels = 3;
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB) : this(Width, Height, ScanA, ScanR, ScanG, ScanB, Width)
        {
        }
        public ImageContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Stride;
            this._ScanA = ScanA;
            this._ScanR = ScanR;
            this._ScanG = ScanG;
            this._ScanB = ScanB;
            this.BitsPerPixel = 32;
            this.Channels = 4;
        }

        /// <summary>
        /// Index :
        /// Scan0 = 0, 
        /// ScanA = 1, 
        /// ScanR = 2, 
        /// ScanG = 3, 
        /// ScanB = 4
        /// </summary>
        internal protected byte[][] Datas { set; get; } = new byte[5][];
        public ImageContext(int Width, int Height, byte[] Data, IList<int> Palette = null)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = Data.Length / Height;
            this.BitsPerPixel = (this.Stride << 3) / Width;
            this.Palette = Palette;
            this.Channels = 1;

            this.Datas[0] = Data;
        }
        public ImageContext(int Width, int Height, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataR.Length / Height;
            this.BitsPerPixel = 24;
            this.Channels = 3;

            this.Datas[2] = DataR;
            this.Datas[3] = DataG;
            this.Datas[4] = DataB;
        }
        public ImageContext(int Width, int Height, byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            this.Width = Width;
            this.Height = Height;
            this.Stride = DataA.Length / Height;
            this.Channels = 4;
            this.BitsPerPixel = 32;

            this.Datas[1] = DataA;
            this.Datas[2] = DataR;
            this.Datas[3] = DataG;
            this.Datas[4] = DataB;
        }


        public void DrawLine(int X0, int Y0, int X1, int Y1, byte Value, int PenWidth)
            => DrawLine(X0, Y0, X1, Y1, byte.MaxValue, Value, Value, Value, PenWidth);
        public void DrawLine(int X0, int Y0, int X1, int Y1, byte A, byte R, byte G, byte B, int PenWidth)
        {
            switch (BitsPerPixel)
            {
                case 1:
                case 4:
                    break;
                case 8:
                    unsafe
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

    }
}
