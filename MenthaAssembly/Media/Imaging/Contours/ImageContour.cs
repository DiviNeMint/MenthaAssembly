using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public class ImageContour : IEnumerable<KeyValuePair<int, ContourData>>, ICloneable
    {
        internal readonly SortedList<int, ContourData> Datas = new SortedList<int, ContourData>();

        public int Count => Datas.Count;

        public Bound<int> Bound
        {
            get
            {
                IEnumerator<KeyValuePair<int, ContourData>> Enumerator = Datas.GetEnumerator();
                try
                {
                    if (Enumerator.MoveNext())
                    {
                        int Key = Enumerator.Current.Key;
                        ContourData Value = Enumerator.Current.Value;
                        while (Value.Count == 0)
                        {
                            if (!Enumerator.MoveNext())
                                return Bound<int>.Empty;

                            Key = Enumerator.Current.Key;
                            Value = Enumerator.Current.Value;
                        }

                        int Left = Value[0],
                            Top = Key,
                            Right = Value[Value.Count - 1],
                            Bottom = Key;

                        while (Enumerator.MoveNext())
                        {
                            Value = Enumerator.Current.Value;
                            Left = Math.Min(Left, Value[0]);
                            Right = Math.Max(Right, Value[Value.Count - 1]);
                            Bottom = Enumerator.Current.Key;
                        }

                        return new Bound<int>(Left, Top, Right + 1, Bottom + 1);
                    }

                    return Bound<int>.Empty;
                }
                finally
                {
                    Enumerator.Dispose();
                }
            }
        }

        public ContourData this[int Y]
        {
            get
            {
                if (Datas.TryGetValue(Y, out ContourData Info))
                    return Info;

                Info = new ContourData();

                Datas[Y] = Info;
                return Info;
            }
            set
            {
                if (value is null)
                {
                    Datas.Remove(Y);
                    return;
                }

                Datas[Y] = value;
            }
        }

        public bool Contain(int X, int Y)
            => Datas.TryGetValue(Y, out ContourData Data) && Data.Contain(X);

        public void Clear()
        {
            foreach (ContourData Data in Datas.Values)
                Data.Clear();

            Datas.Clear();
        }

        public void Union(ImageContour Contour)
        {
            foreach (KeyValuePair<int, ContourData> Pair in Contour.Datas)
                this[Pair.Key].Union(Pair.Value);
        }

        public void Difference(ImageContour Contour)
        {
            foreach (KeyValuePair<int, ContourData> Pair in Contour.Datas)
                this[Pair.Key].Difference(Pair.Value);
        }

        public void Flip(FlipMode Mode)
        {
            // Horizontal
            if ((Mode & FlipMode.Horizontal) > 0)
            {
                Bound<int> Bound = this.Bound;
                int Delta = Bound.Left + Bound.Right;
                foreach (KeyValuePair<int, ContourData> Data in Datas)
                {
                    int Count = Data.Value.Count,
                        Length = Count >> 1;
                    Count--;
                    for (int i = 0; i < Length; i++)
                    {
                        int Temp = Delta - Data.Value[i];
                        Data.Value[i] = Delta - Data.Value[Count - i];
                        Data.Value[Count - i] = Temp;
                    }
                }
            }

            // Vertical
            if ((Mode & FlipMode.Vertical) > 0)
            {
                KeyValuePair<int, ContourData>[] TempDatas = Datas.ToArray();
                Datas.Clear();

                Bound<int> Bound = this.Bound;
                int Delta = Bound.Top + Bound.Bottom;
                foreach (KeyValuePair<int, ContourData> Data in TempDatas)
                    Datas.Add(Delta - Data.Key, Data.Value);

            }
        }
        public void Flip(int Center, FlipMode Mode)
        {
            // Horizontal
            if ((Mode & FlipMode.Horizontal) > 0)
            {
                int Delta = Center << 1;
                foreach (KeyValuePair<int, ContourData> Data in Datas)
                {
                    int Count = Data.Value.Count,
                        Length = Count >> 1;
                    Count--;
                    for (int i = 0; i < Length; i++)
                    {
                        int Temp = Delta - Data.Value[i];
                        Data.Value[i] = Delta - Data.Value[Count - i];
                        Data.Value[Count - i] = Temp;
                    }
                }
            }

            // Vertical
            if ((Mode & FlipMode.Vertical) > 0)
            {
                KeyValuePair<int, ContourData>[] TempDatas = Datas.ToArray();
                Datas.Clear();

                int Delta = Center << 1;
                foreach (KeyValuePair<int, ContourData> Data in TempDatas)
                    Datas.Add(Delta - Data.Key, Data.Value);
            }
        }
        public static ImageContour Flip(ImageContour Source, int Center, FlipMode Mode)
        {
            ImageContour Contour = new ImageContour();

            switch (Mode)
            {
                case FlipMode.Horizontal:
                    {
                        int Delta = Center << 1;
                        foreach (KeyValuePair<int, ContourData> Data in Source.Datas)
                        {
                            ContourData SourData = Data.Value,
                                        DestData = Contour[Data.Key];
                            int Count = SourData.Count;
                            for (int i = Count - 1; i >= 0; i--)
                                DestData.Datas.Add(Delta - SourData.Datas[i]);
                        }

                        return Contour;
                    }
                case FlipMode.Vertical:
                    {
                        int Delta = Center << 1;
                        foreach (KeyValuePair<int, ContourData> Data in Source.Datas)
                            Contour[Delta - Data.Key].Datas.AddRange(Data.Value.Datas);

                        return Contour;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        int Delta = Center << 1;
                        foreach (KeyValuePair<int, ContourData> Data in Source.Datas)
                        {
                            ContourData SourData = Data.Value,
                                        DestData = Contour[Center - Data.Key];
                            int Count = SourData.Count;
                            for (int i = Count - 1; i >= 0; i--)
                                DestData.Datas.Add(Delta - SourData.Datas[i]);
                        }

                        return Contour;
                    }
            }

            return Source.Clone();
        }

        public void Offset(int X, int Y)
        {
            IList<int> Keys = Datas.Keys;
            if (Y < 0)
            {
                for (int i = 0; i < Keys.Count; i++)
                {
                    int OldY = Keys[i];
                    ContourData Data = Datas[OldY];
                    Data.Offset(X);

                    Datas.RemoveAt(i);
                    Datas.Add(OldY + Y, Data);
                }
            }
            else if (Y == 0)
            {
                foreach (int Key in Keys)
                    Datas[Key].Offset(X);
            }
            else
            {
                for (int i = Keys.Count - 1; i >= 0; i--)
                {
                    int OldY = Keys[i];
                    ContourData Data = Datas[OldY];
                    Data.Offset(X);

                    Datas.RemoveAt(i);
                    Datas.Add(OldY + Y, Data);
                }
            }
        }
        public static ImageContour Offset(ImageContour Source, int X, int Y)
        {
            ImageContour Result = new ImageContour();
            foreach (KeyValuePair<int, ContourData> Data in Source.Datas)
                Result[Data.Key + Y] = ContourData.Offset(Data.Value, X);

            return Result;
        }

        public ImageContour Clone()
        {
            ImageContour Result = new ImageContour();

            foreach (KeyValuePair<int, ContourData> Data in Datas)
                Result[Data.Key] = Data.Value.Clone();

            return Result;
        }
        object ICloneable.Clone()
            => this.Clone();

        //public static ImageContour Rotate(ImageContour Source, int Ox, int Oy, int Angle)
        //{
        //    ImageContour r = new ImageContour();

        //    double Theta = Angle * MathHelper.UnitTheta;

        //    double vsin = Math.Sin(Theta),
        //           vcos = Math.Cos(Theta);

        //    //int x, y, nx2, ny2; //平移後之點
        //    //for (int ny = 0; ny < height; ++ny)
        //    //{
        //    //    for (int nx = 0; nx < width; ++nx)
        //    //    {
        //    //        // 平移 ox,oy
        //    //        nx2 = nx - Ox;
        //    //        ny2 = ny - Oy;

        //    //        // 再旋轉, 平移(-ox,-oy)
        //    //        x = (int)(nx2 * vcos + ny2 * vsin + 0.5 + Ox);
        //    //        y = (int)(-nx2 * vsin + ny2 * vcos + 0.5 + Oy);

        //    //        // 寫入
        //    //        if (y >= 0 && y < height && x >= 0 && x < width)
        //    //        {
        //    //            nr[ny][nx] = r[y][x];
        //    //            ng[ny][nx] = g[y][x];
        //    //            nb[ny][nx] = b[y][x];
        //    //        }
        //    //        else
        //    //        {
        //    //            nr[ny][nx] = ng[ny][nx] = nb[ny][nx] = 0;
        //    //        }
        //    //    }
        //    //}


        //    return r;
        //}

        public IEnumerator<KeyValuePair<int, ContourData>> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        public static ImageContour ParsePenContour(IImageContext Stroke, out IPixel StrokeColor)
        {
            StrokeColor = default;
            bool FoundColor = false;
            ImageContour Contour = new ImageContour();
            for (int j = 0; j < Stroke.Height; j++)
            {
                bool IsFoundLeft = false;
                for (int i = 0; i < Stroke.Width; i++)
                {
                    IPixel Pixel = Stroke[i, j];
                    if (IsFoundLeft)
                    {
                        if (Pixel.A == 0)
                        {
                            Contour[j].AddRight(i - 1);
                            IsFoundLeft = false;
                        }
                    }
                    else
                    {
                        if (Pixel.A > 0)
                        {
                            if (!FoundColor)
                            {
                                StrokeColor = Pixel;
                                FoundColor = true;
                            }

                            Contour[j].AddLeft(i);
                            IsFoundLeft = true;
                        }
                    }
                }

                if (IsFoundLeft)
                    Contour[j].AddRight(Stroke.Width - 1);
            }

            return Contour;
        }

        public static ImageContour CreateFillEllipse(int Cx, int Cy, int Rx, int Ry)
        {
            ImageContour Ellipse = new ImageContour();

            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry, (Dx, Dy) =>
            {
                int X = Cx + Dx,
                    Y = Cy + Dy;

                ContourData TData = Ellipse[Y];
                if (TData.Count == 0)
                {
                    TData.Datas.Add(X);
                    TData.Datas.Add(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            });

            return Ellipse;
        }

        public static ImageContour CreateFillSector(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise)
        {
            ImageContour Contour = new ImageContour();

            void AddData(int X, int Y)
            {
                ContourData TData = Contour[Y];
                if (TData.Count == 0)
                {
                    TData.Datas.Add(X);
                    TData.Datas.Add(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            }

            int DSx = Sx - Cx,
                DSy = Sy - Cy,
                DEx = Ex - Cx,
                DEy = Ey - Cy;

            GraphicAlgorithm.CalculateBresenhamLine(DSx, DSy, DSx.Abs(), DSy.Abs(), (Dx, Dy) => AddData(Cx + Dx, Cy + Dy));
            GraphicAlgorithm.CalculateBresenhamLine(DEx, DEy, DEx.Abs(), DEy.Abs(), (Dx, Dy) => AddData(Cx + Dx, Cy + Dy));

            GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, (Dx, Dy) => AddData(Cx + Dx, Cy + Dy));

            return Contour;
        }

        public static ImageContour CreateFillObround(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Obround = new ImageContour();
            void AddData(int X, int Y)
            {
                ContourData TData = Obround[Y];
                if (TData.Count == 0)
                {
                    TData.Datas.Add(X);
                    TData.Datas.Add(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            }

            int Length;

            // Horizontal
            if (HalfHeight < HalfWidth)
            {
                Length = HalfWidth - HalfHeight;

                GraphicAlgorithm.CalculateBresenhamEllipse(HalfHeight, HalfHeight, (Dx, Dy) => AddData(Cx + Dx + (Dx > 0 ? Length : -Length), Cy + Dy));
                return Obround;
            }

            Length = HalfHeight - HalfWidth;

            // Vertical
            GraphicAlgorithm.CalculateBresenhamEllipse(HalfWidth, HalfWidth, (Dx, Dy) => AddData(Cx + Dx, Cy + Dy + (Dy > 0 ? Length : -Length)));

            int Left = Cx - HalfWidth,
                Right = Cx + HalfWidth;
            for (int Y = Cy - Length; Y <= Cy + Length; Y++)
                Obround[Y].Union(Left, Right);

            return Obround;
        }
        public static ImageContour CreateFillObround(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta)
        {
            double Alpth = Theta % Math.PI;

            if (Math.Round(Alpth, 5) == 0d)
                return CreateFillObround(Cx, Cy, HalfWidth, HalfHeight);

            if (Math.Round(Alpth % MathHelper.HalfPI, 5) == 0d)
                return CreateFillObround(Cx, Cy, HalfHeight, HalfWidth);

            ImageContour Pen;
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            int Sx, Sy, Ex, Ey,
                Dx, Dy;
            if (HalfWidth < HalfHeight)
            {
                Pen = CreateFillEllipse(0, 0, HalfWidth, HalfWidth);

                HalfHeight -= HalfWidth;
                Dx = (int)Math.Round(HalfHeight * Sin);
                Dy = (int)Math.Round(HalfHeight * Cos);

                Sx = Cx - Dx;
                Sy = Cy + Dy;
                Ex = Cx + Dx;
                Ey = Cy - Dy;
            }
            else
            {
                Pen = CreateFillEllipse(0, 0, HalfHeight, HalfHeight);

                HalfWidth -= HalfHeight;
                Dx = (int)Math.Round(HalfWidth * Cos);
                Dy = (int)Math.Round(HalfWidth * Sin);

                Sx = Cx - Dx;
                Sy = Cy - Dy;
                Ex = Cx + Dx;
                Ey = Cy + Dy;
            }

            return CreateLineContour(Sx, Sy, Ex, Ey, Pen);
        }

        public static ImageContour CreateFillDiamond(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Diamond = new ImageContour();

            int X = Cx - HalfWidth;
            GraphicAlgorithm.CalculateBresenhamLine(HalfWidth, HalfHeight, HalfWidth, HalfHeight, (Dx, Dy) => Diamond[Cy + Dy].AddLeft(X + Dx));
            GraphicAlgorithm.CalculateBresenhamLine(HalfWidth, -HalfHeight, HalfWidth, HalfHeight, (Dx, Dy) => Diamond[Cy + Dy].AddLeft(X + Dx));

            X = Cx + HalfWidth;
            GraphicAlgorithm.CalculateBresenhamLine(-HalfWidth, HalfHeight, HalfWidth, HalfHeight, (Dx, Dy) => Diamond[Cy + Dy][1] = X + Dx);
            GraphicAlgorithm.CalculateBresenhamLine(-HalfWidth, -HalfHeight, HalfWidth, HalfHeight, (Dx, Dy) => Diamond[Cy + Dy][1] = X + Dx);

            return Diamond;
        }
        public static ImageContour CreateFillDiamond(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta)
        {
            if (Math.Round(Theta % MathHelper.TwoPI, 5) == 0d)
                return CreateFillDiamond(Cx, Cy, HalfWidth, HalfHeight);

            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);

            int Dx1 = (int)Math.Round(-HalfHeight * Sin),
                Dy1 = (int)Math.Round(HalfHeight * Cos),
                Dx2 = (int)Math.Round(HalfWidth * Cos),
                Dy2 = (int)Math.Round(HalfWidth * Sin);

            return CreateFillRectangle(Cx + Dx1, Cy + Dy1, Cx + Dx2, Cy + Dy2, Cx - Dx1, Cy - Dy1, Cx - Dx2, Cy - Dy2);
        }

        public static ImageContour CreateFillTriangle(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Triangle = new ImageContour();

            int Height = HalfHeight << 1;
            GraphicAlgorithm.CalculateBresenhamLine(-HalfWidth, -Height, HalfWidth, Height, (Dx, Dy) => Triangle[Cy + HalfHeight + Dy].Datas.Add(Cx + Dx));
            GraphicAlgorithm.CalculateBresenhamLine(HalfWidth, -Height, HalfWidth, Height, (Dx, Dy) => Triangle[Cy + HalfHeight + Dy].Datas.Add(Cx + Dx));

            return Triangle;
        }
        public static ImageContour CreateFillTriangle(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta),
                   XCos = HalfWidth * Cos,
                   XSin = HalfWidth * Sin,
                   YCos = HalfHeight * Cos,
                   YSin = HalfHeight * Sin,
                   X1 = -YSin,
                   Y1 = YCos,
                   X2 = XCos + YSin,
                   Y2 = XSin - YCos,
                   X3 = -XCos + YSin,
                   Y3 = -XSin - YCos;

            return CreateFillTriangle(Cx + (int)X1, Cy + (int)Y1, Cx + (int)X2, Cy + (int)Y2, Cx + (int)X3, Cy + (int)Y3);
        }
        public static ImageContour CreateFillTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3)
        {
            ImageContour Triangle = new ImageContour();
            void AddData(int X, int Y)
            {
                ContourData TData = Triangle[Y];
                if (TData.Count == 0)
                {
                    TData.Datas.Add(X);
                    TData.Datas.Add(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            }

            // (X1, Y1) => (X2, Y2)
            int DeltaX = X2 - X1,
                DeltaY = Y2 - Y1;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X1 + Dx, Y1 + Dy));

            // (X2, Y2) => (X3, Y3)
            DeltaX = X3 - X2;
            DeltaY = Y3 - Y2;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X2 + Dx, Y2 + Dy));

            // (X3, Y3) => (X1, Y1)
            DeltaX = X1 - X3;
            DeltaY = Y1 - Y3;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X3 + Dx, Y3 + Dy));

            return Triangle;
        }

        public static ImageContour CreateFillRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Rectangle = new ImageContour();

            int Left = Cx - HalfWidth,
                Right = Cx + HalfWidth;

            for (int i = 0; i < HalfHeight; i++)
            {
                Rectangle[Cy - i] = new ContourData(Left, Right);
                Rectangle[Cy + i] = new ContourData(Left, Right);
            }

            return Rectangle;
        }
        public static ImageContour CreateFillRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta)
        {
            double Alpth = Theta % Math.PI;

            if (Math.Round(Alpth, 5) == 0d)
                return CreateFillRectangle(Cx, Cy, HalfWidth, HalfHeight);

            if (Math.Round(Alpth % MathHelper.HalfPI, 5) == 0d)
                return CreateFillRectangle(Cx, Cy, HalfHeight, HalfWidth);

            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta),
                   XCos = HalfWidth * Cos,
                   XSin = HalfWidth * Sin,
                   YCos = HalfHeight * Cos,
                   YSin = HalfHeight * Sin;

            int Dx1 = (int)Math.Round(XCos - YSin),
                Dy1 = (int)Math.Round(XSin + YCos),
                Dx2 = (int)Math.Round(-XCos - YSin),
                Dy2 = (int)Math.Round(-XSin + YCos);

            return CreateFillRectangle(Cx + Dx1, Cy + Dy1, Cx + Dx2, Cy + Dy2, Cx - Dx1, Cy - Dy1, Cx - Dx2, Cy - Dy2);
        }
        public static ImageContour CreateFillRectangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4)
        {
            ImageContour Rectangle = new ImageContour();
            void AddData(int X, int Y)
            {
                ContourData TData = Rectangle[Y];
                if (TData.Count == 0)
                {
                    TData.Datas.Add(X);
                    TData.Datas.Add(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            }

            // (X1, Y1) => (X2, Y2)
            int DeltaX = X2 - X1,
                DeltaY = Y2 - Y1;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X1 + Dx, Y1 + Dy));

            // (X2, Y2) => (X3, Y3)
            DeltaX = X3 - X2;
            DeltaY = Y3 - Y2;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X2 + Dx, Y2 + Dy));

            // (X3, Y3) => (X4, Y4)
            DeltaX = X4 - X3;
            DeltaY = Y4 - Y3;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X3 + Dx, Y3 + Dy));

            // (X4, Y4) => (X1, Y1)
            DeltaX = X1 - X4;
            DeltaY = Y1 - Y4;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X4 + Dx, Y4 + Dy));

            return Rectangle;
        }

        public static ImageContour CreateFillRoundedRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, int CornerRadius)
        {
            ImageContour Contour = new ImageContour();
            int CornerRx = Cx + HalfWidth - CornerRadius,
                CornerLx = Cx - HalfWidth + CornerRadius,
                CornerTy = Cy - HalfHeight + CornerRadius,
                CornerBy = Cy + HalfHeight - CornerRadius;

            int MinDx = CornerRadius - HalfWidth,
                MinDy = CornerRadius - HalfHeight;
            GraphicAlgorithm.CalculateBresenhamEllipseQuadrantI(CornerRadius, CornerRadius, (Dx, Dy) =>
            {
                if (Dy < MinDy || Dx < MinDx)
                    return;

                int TRx = CornerRx + Dx,
                    TLx = CornerLx - Dx,
                    TTy = CornerTy - Dy,
                    TBy = CornerBy + Dy;

                Contour[TTy].Union(TLx, TRx);
                Contour[TBy] = Contour[TTy];
            });

            int Lx = Cx - HalfWidth,
                Rx = Cx + HalfWidth;
            for (int j = CornerTy + 1; j < CornerBy; j++)
            {
                Contour[j].Datas.Add(Lx);
                Contour[j].Datas.Add(Rx);
            }

            return Contour;

            //=> CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, CornerRadius, CornerRadius, CornerRadius, CornerRadius);
        }
        public static ImageContour CreateFillRoundedRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, int CornerRadius1, int CornerRadius2, int CornerRadius3, int CornerRadius4)
        {
            ImageContour Rectangle = new ImageContour();

            int Left = Cx - HalfWidth,
                Right = Cx + HalfWidth,
                Top = Cy - HalfHeight,
                Bottom = Cy + HalfHeight,
                Y1, Y2, Y3, Y4;

            ImageContour Corner = null;
            int LastRadius = -1;

            // Left & Top
            Y1 = Top + CornerRadius1;
            if (CornerRadius1 > 0)
            {
                LastRadius = CornerRadius1;
                Corner = CreateFillEllipse(0, 0, CornerRadius1, CornerRadius1);

                Rectangle.Union(Offset(Corner, Left + CornerRadius1, Y1));
            }

            // Right & Top
            Y2 = Top + CornerRadius2;
            if (CornerRadius2 > 0)
            {
                if (CornerRadius2 != LastRadius)
                {
                    Corner = CreateFillEllipse(0, 0, CornerRadius2, CornerRadius2);
                    LastRadius = CornerRadius2;
                }
                Rectangle.Union(Offset(Corner, Right - CornerRadius2, Y2));
            }

            // Right & Bottom
            Y3 = Bottom - CornerRadius3;
            if (CornerRadius3 > 0)
            {
                if (CornerRadius3 != LastRadius)
                {
                    Corner = CreateFillEllipse(0, 0, CornerRadius3, CornerRadius3);
                    LastRadius = CornerRadius3;
                }
                Rectangle.Union(Offset(Corner, Right - CornerRadius3, Y3));
            }

            // Left & Bottom
            Y4 = Bottom - CornerRadius4;
            if (CornerRadius4 > 0)
            {
                if (CornerRadius4 != LastRadius)
                    Corner = CreateFillEllipse(0, 0, CornerRadius4, CornerRadius4);

                Rectangle.Union(Offset(Corner, Left + CornerRadius4, Y4));
            }

            for (int j = Y1; j <= Y4; j++)
                Rectangle[j].AddLeft(Left);

            for (int j = Y2; j <= Y3; j++)
                Rectangle[j].AddRight(Right);

            foreach (int j in Rectangle.Datas.Keys.ToArray())
            {
                if (j < Top || Bottom < j)
                {
                    Rectangle.Datas.Remove(j);
                    continue;
                }

                int Length = Rectangle.Datas[j].Count;
                if (Length > 2)
                {
                    Length--;
                    for (int i = 1; i < Length; i++)
                        Rectangle.Datas[j].Datas.RemoveAt(1);
                }
            }

            return Rectangle;
        }
        public static ImageContour CreateFillRoundedRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta, int CornerRadius)
        {
            if (Theta == 0d)
                return CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, CornerRadius);

            return CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, Theta, CornerRadius, CornerRadius, CornerRadius, CornerRadius);
        }
        public static ImageContour CreateFillRoundedRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta, int CornerRadius1, int CornerRadius2, int CornerRadius3, int CornerRadius4)
        {
            if (Math.Round(Theta % MathHelper.HalfPI, 5) == 0d)
                return CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, CornerRadius1, CornerRadius2, CornerRadius3, CornerRadius4);

            ImageContour Rectangle = new ImageContour();
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);
            int X1 = Cx - HalfWidth,
                X2 = X1 + CornerRadius1,
                X4 = Cx + HalfWidth,
                X3 = X4 - CornerRadius2,
                X5 = X4,
                X6 = X4 - CornerRadius3,
                X7 = X1 + CornerRadius4,
                X8 = X1,
                Y2 = Cy - HalfHeight,
                Y1 = Y2 + CornerRadius1,
                Y3 = Y2,
                Y4 = Y2 + CornerRadius2,
                Y6 = Cy + HalfHeight,
                Y5 = Y6 - CornerRadius3,
                Y7 = Y6,
                Y8 = Y6 - CornerRadius4;

            ImageContour Corner = null;
            int LastRadius = -1;

            // Left & Top
            if (CornerRadius1 > 0)
            {
                LastRadius = CornerRadius1;
                Corner = CreateFillEllipse(0, 0, CornerRadius1, CornerRadius1);

                Point<int>.Rotate(X2, Y1, Sin, Cos, out int Tx, out int Ty);
                Rectangle.Union(Offset(Corner, Tx, Ty));
            }

            // Right & Top
            if (CornerRadius2 > 0)
            {
                if (CornerRadius2 != LastRadius)
                {
                    Corner = CreateFillEllipse(0, 0, CornerRadius2, CornerRadius2);
                    LastRadius = CornerRadius2;
                }

                Point<int>.Rotate(X3, Y4, Sin, Cos, out int Tx, out int Ty);
                Rectangle.Union(Offset(Corner, Tx, Ty));
            }

            // Right & Bottom
            if (CornerRadius3 > 0)
            {
                if (CornerRadius3 != LastRadius)
                {
                    Corner = CreateFillEllipse(0, 0, CornerRadius3, CornerRadius3);
                    LastRadius = CornerRadius3;
                }

                Point<int>.Rotate(X6, Y5, Sin, Cos, out int Tx, out int Ty);
                Rectangle.Union(Offset(Corner, Tx, Ty));
            }

            // Left & Bottom
            if (CornerRadius4 > 0)
            {
                if (CornerRadius4 != LastRadius)
                    Corner = CreateFillEllipse(0, 0, CornerRadius4, CornerRadius4);

                Point<int>.Rotate(X7, Y8, Sin, Cos, out int Tx, out int Ty);
                Rectangle.Union(Offset(Corner, Tx, Ty));
            }

            // Filter
            foreach (KeyValuePair<int, ContourData> Item in Rectangle.Datas)
            {
                ContourData TData = Item.Value;
                int Length = TData.Count;
                for (int i = 2; i < Length; i++)
                    TData.Datas.RemoveAt(1);
            }

            void AddData(int X, int Y)
            {
                ContourData TData = Rectangle[Y];
                if (TData.Count == 0)
                {
                    TData.Datas.Add(X);
                    TData.Datas.Add(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            }

            // Rotate
            Point<int>.Rotate(X1, Y1, Sin, Cos, out X1, out Y1);
            Point<int>.Rotate(X2, Y2, Sin, Cos, out X2, out Y2);
            Point<int>.Rotate(X4, Y4, Sin, Cos, out X4, out Y4);
            Point<int>.Rotate(X3, Y3, Sin, Cos, out X3, out Y3);
            Point<int>.Rotate(X5, Y5, Sin, Cos, out X5, out Y5);
            Point<int>.Rotate(X6, Y6, Sin, Cos, out X6, out Y6);
            Point<int>.Rotate(X7, Y7, Sin, Cos, out X7, out Y7);
            Point<int>.Rotate(X8, Y8, Sin, Cos, out X8, out Y8);

            // (X8, Y8) => (X1, Y1)
            int DeltaX = X1 - X8,
                DeltaY = Y1 - Y8;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X8 + Dx, Y8 + Dy));

            // (X2, Y2) => (X3, Y3)
            DeltaX = X3 - X2;
            DeltaY = Y3 - Y2;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X2 + Dx, Y2 + Dy));

            // (X4, Y4) => (X5, Y5)
            DeltaX = X5 - X4;
            DeltaY = Y5 - Y4;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X4 + Dx, Y4 + Dy));

            // (X6, Y6) => (X7, Y7)
            DeltaX = X7 - X6;
            DeltaY = Y7 - Y6;
            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX.Abs(), DeltaY.Abs(), (Dx, Dy) => AddData(X6 + Dx, Y6 + Dy));

            return Rectangle;
        }

        public static ImageContour CreateFillRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, double StartTheta = 0d)
        {
            if (VertexNum < 3)
                throw new ArgumentException($"VertexNum must more than or equal 3.");

            ImageContour Polygon = new ImageContour();
            void AddData(int X, int Y)
            {
                ContourData TData = Polygon[Y];
                if (TData.Count == 0)
                {
                    TData.AddLeft(X);
                    return;
                }

                if (X < TData[0])
                    TData[0] = X;
                else if (TData[1] < X)
                    TData[1] = X;
            }

            double DeltaTheta = 360d / VertexNum * MathHelper.UnitTheta,
                   LastTheta = StartTheta;

            int P0x = (int)Math.Ceiling(Radius * Math.Cos(LastTheta)),
                P0y = (int)Math.Ceiling(Radius * Math.Sin(LastTheta)),
                LastPx = P0x,
                LastPy = P0y,
                DLPx, DLPy;

            for (int i = 1; i < VertexNum; i++)
            {
                LastTheta += DeltaTheta;
                int Px = (int)Math.Ceiling(Radius * Math.Cos(LastTheta)),
                    Py = (int)Math.Ceiling(Radius * Math.Sin(LastTheta));
                DLPx = Px - LastPx;
                DLPy = Py - LastPy;

                GraphicAlgorithm.CalculateBresenhamLine(DLPx, DLPy, DLPx.Abs(), DLPy.Abs(), (Dx, Dy) => AddData(Cx + LastPx + Dx, Cy + LastPy + Dy));

                LastPx = Px;
                LastPy = Py;
            }

            DLPx = P0x - LastPx;
            DLPy = P0y - LastPy;

            GraphicAlgorithm.CalculateBresenhamLine(DLPx, DLPy, DLPx.Abs(), DLPy.Abs(), (Dx, Dy) => AddData(Cx + LastPx + Dx, Cy + LastPy + Dy));

            return Polygon;
        }

        public static ImageContour CreateFillPolygon(IList<int> VerticeDatas, int OffsetX, int OffsetY)
        {
            ImageContour Polygon = new ImageContour();
            int pn = VerticeDatas.Count,
                pnh = VerticeDatas.Count >> 1;

            int[] intersectionsX = new int[pnh];

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = int.MaxValue,
                yMax = 0;
            for (int i = 1; i < pn; i += 2)
            {
                int py = VerticeDatas[i] + OffsetY;
                if (py < yMin)
                    yMin = py;
                if (py > yMax)
                    yMax = py;
            }

            // Scan line from min to max
            for (int y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                float vxi = VerticeDatas[0] + OffsetX,
                      vyi = VerticeDatas[1] + OffsetY;

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                int intersectionCount = 0;
                for (int i = 2; i < pn; i += 2)
                {
                    // Next point x, y
                    float vxj = VerticeDatas[i] + OffsetX,
                          vyj = VerticeDatas[i + 1] + OffsetY;

                    // Is the scanline between the two points
                    if (vyi < y && vyj >= y ||
                        vyj < y && vyi >= y)
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        intersectionsX[intersectionCount++] = (int)(vxi + (y - vyi) * (vxj - vxi) / (vyj - vyi));
                    }
                    vxi = vxj;
                    vyi = vyj;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                int t, j;
                for (int i = 1; i < intersectionCount; i++)
                {
                    t = intersectionsX[i];
                    j = i;
                    while (j > 0 && intersectionsX[j - 1] > t)
                    {
                        intersectionsX[j] = intersectionsX[j - 1];
                        j -= 1;
                    }
                    intersectionsX[j] = t;
                }

                // Add Datas
                for (int i = 0; i < intersectionCount - 1;)
                {
                    Polygon[y].Datas.Add(intersectionsX[i++]);
                    Polygon[y].Datas.Add(intersectionsX[i++]);
                }
            }

            return Polygon;
        }

        public static ImageContour CreateLineContour(int X0, int Y0, int X1, int Y1, ImageContour Pen)
        {
            Bound<int> Bound = Pen.Bound;
            if (Bound.IsEmpty)
                return null;

            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = DeltaY.Abs();

            bool IsHollow = false;
            ImageContour LineContour = new ImageContour();

            #region Pen Bound
            int PCx = (Bound.Left + Bound.Right) >> 1,
                PCy = (Bound.Top + Bound.Bottom) >> 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            foreach (KeyValuePair<int, ContourData> Item in Pen)
            {
                int j = Item.Key;
                ContourData Data = Item.Value;
                if (Data.Count > 2)
                {
                    IsHollow = true;
                    LineContour.Clear();
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

                    LineContour[Ty + Y0].AddLeft(Tx + X0);  // StartPoint
                    LineContour[Ty + Y1].AddLeft(Tx + X1);  // EndPoint
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

                    LineContour[Ty + Y0].AddRight(Tx + X0);  // StartPoint
                    LineContour[Ty + Y1].AddRight(Tx + X1);  // EndPoint
                }
            }

            #endregion

            if (IsHollow)
            {
                #region Line Body Bound
                ImageContour Stroke = Offset(Pen, X0 - PCx, Y0 - PCy);

                int LastDx = 0,
                    LastDy = 0;

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY,
                    (Dx, Dy) =>
                    {
                        Stroke.Offset(Dx - LastDx, Dy - LastDy);
                        LineContour.Union(Stroke);

                        LastDx = Dx;
                        LastDy = Dy;
                    });
                #endregion
            }
            else
            {
                #region Line Body Bound
                int Ux = X0 + DUx,
                    Uy = Y0 + DUy,
                    Lx = X0 + DLx,
                    Ly = Y0 + DLy;

                if (DeltaX == 0 && DeltaY < 0)
                {
                    MathHelper.Swap(ref Ux, ref Lx);
                    MathHelper.Swap(ref Uy, ref Ly);
                }

                GraphicDeltaHandler FoundLineBodyBound = DeltaX * DeltaY < 0 ?
                    new GraphicDeltaHandler(
                        (Dx, Dy) =>
                        {
                            LineContour[Ly + Dy].AddLeft(Lx + Dx);
                            LineContour[Uy + Dy].AddRight(Ux + Dx);
                        }) :
                        (Dx, Dy) =>
                        {
                            LineContour[Uy + Dy].AddLeft(Ux + Dx);
                            LineContour[Ly + Dy].AddRight(Lx + Dx);
                        };

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, FoundLineBodyBound);

                #endregion
            }

            return LineContour;
        }

        public static ImageContour CreateTextContour(int X, int Y, string Text, string FontName, int CharSize, double Angle = 0, FontWeightType Weight = FontWeightType.Normal, bool Italic = false)
        {
            FontData Font = new FontData
            {
                FaceName = string.IsNullOrEmpty(FontName) ? "System" : FontName,
                Height = CharSize,
                Weight = Weight,
                Escapement = (int)(Angle * 10),
                Italic = Italic,
            };

            return Graphic.CreateTextContour(X, Y, Text, Font);
        }

        public static ImageContour operator +(ImageContour This, ImageContour Contour)
        {
            This.Union(Contour);
            return This;
        }
        public static ImageContour operator -(ImageContour This, ImageContour Contour)
        {
            This.Difference(Contour);
            return This;
        }

        public static ImageContour operator +(ImageContour This, Vector<int> Offset)
        {
            This.Offset(Offset.X, Offset.Y);
            return This;
        }
        public static ImageContour operator -(ImageContour This, Vector<int> Offset)
        {
            This.Offset(-Offset.X, -Offset.Y);
            return This;
        }

    }
}
