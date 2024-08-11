using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public sealed class ImageContour : IImageContour, ICloneable
    {
        private readonly Dictionary<int, ImageContourScanLine> Contents;
        IReadOnlyDictionary<int, ImageContourScanLine> IImageContour.Contents
            => Contents;

        private double OffsetX = 0d;
        double IImageContour.OffsetX
            => OffsetX;

        private double OffsetY = 0d;
        double IImageContour.OffsetY
            => OffsetY;

        public Bound<int> Bound
        {
            get
            {
                int X0 = int.MaxValue,
                    X1 = int.MinValue,
                    Y0 = int.MaxValue,
                    Y1 = int.MinValue,
                    T;

                foreach (KeyValuePair<int, ImageContourScanLine> Content in Contents)
                {
                    List<int> XDatas = Content.Value.Datas;
                    if (XDatas.Count > 0)
                    {
                        T = Content.Key;
                        if (T < Y0)
                            Y0 = T;
                        else if (Y1 < T)
                            Y1 = T;

                        T = XDatas[0];
                        if (T < X0)
                            X0 = T;

                        T = XDatas[XDatas.Count - 1];
                        if (X1 < T)
                            X1 = T;
                    }
                }

                return X0 != int.MaxValue || X1 != int.MinValue || Y0 != int.MaxValue || Y1 != int.MinValue ? new Bound<int>(X0, Y0, X1, Y1) : Bound<int>.Empty;
            }
        }

        public ImageContourScanLine this[int Y]
        {
            get
            {
                if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
                {
                    ScanLine = new ImageContourScanLine();
                    Contents.Add(Y, ScanLine);
                }

                return ScanLine;
            }
            private set => Contents[Y] = value;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ImageContour()
        {
            Contents = [];
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Contour">The initial contour.</param>
        public ImageContour(IImageContour Contour)
        {
            Contour.EnsureContents();
            Contents = Contour.Contents.ToDictionary(i => i.Key, i => i.Value.Clone()); // new Dictionary<int, ImageContourScanLine>();
            OffsetX = Contour.OffsetX;
            OffsetY = Contour.OffsetY;
        }

        public void Union(IImageContour Contour)
        {
            Contour.EnsureContents();

            int Ox = (int)Math.Round(OffsetX + Contour.OffsetX),
                Oy = (int)Math.Round(OffsetY + Contour.OffsetY);
            foreach (KeyValuePair<int, ImageContourScanLine> Data in Contour.Contents)
            {
                int Y = Data.Key + Oy;
                if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
                {
                    Contents[Y] = ImageContourScanLine.Offset(Data.Value, Ox);
                    continue;
                }

                ScanLine.Union(Data.Value, Ox);
            }
        }

        public void Intersection(IImageContour Contour)
        {
            Contour.EnsureContents();

            int Ox = (int)Math.Round(OffsetX + Contour.OffsetX),
                Oy = (int)Math.Round(OffsetY + Contour.OffsetY);
            foreach (KeyValuePair<int, ImageContourScanLine> Data in Contour.Contents)
                if (Contents.TryGetValue(Data.Key + Oy, out ImageContourScanLine ScanLine))
                    ScanLine.Intersection(Data.Value, Ox);
        }

        public void Difference(IImageContour Contour)
        {
            Contour.EnsureContents();

            int Ox = (int)Math.Round(OffsetX + Contour.OffsetX),
                Oy = (int)Math.Round(OffsetY + Contour.OffsetY);
            foreach (KeyValuePair<int, ImageContourScanLine> Data in Contour.Contents)
                if (Contents.TryGetValue(Data.Key + Oy, out ImageContourScanLine ScanLine))
                    ScanLine.Difference(Data.Value, Ox);
        }

        public void SymmetricDifference(IImageContour Contour)
        {
            Contour.EnsureContents();

            int Ox = (int)Math.Round(OffsetX + Contour.OffsetX),
                Oy = (int)Math.Round(OffsetY + Contour.OffsetY);
            foreach (KeyValuePair<int, ImageContourScanLine> Data in Contour.Contents)
            {
                int Y = Data.Key + Oy;
                if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
                {
                    Contents[Y] = ImageContourScanLine.Offset(Data.Value, Ox);
                    continue;
                }

                ScanLine.SymmetricDifference(Data.Value, Ox);
            }
        }

        public void Flip(double CenterX, double CenterY, FlipMode Flip)
        {
            switch (Flip)
            {
                case FlipMode.Horizontal:
                    {
                        double TDx = CenterX * 2d;
                        int Dx = (int)Math.Round(TDx);
                        OffsetX += TDx - Dx;

                        foreach (KeyValuePair<int, ImageContourScanLine> Data in Contents)
                        {
                            List<int> XDatas = Data.Value.Datas;
                            int Count = XDatas.Count,
                                Length = Count >> 1;
                            Count--;
                            for (int i = 0; i < Length; i++)
                            {
                                int Temp = Dx - XDatas[i];
                                XDatas[i] = Dx - XDatas[Count - i];
                                XDatas[Count - i] = Temp;
                            }
                        }
                        break;
                    }
                case FlipMode.Vertical:
                    {
                        double TDy = CenterY * 2d;
                        int Dy = (int)Math.Round(TDy);
                        OffsetY += TDy - Dy;

                        List<int> Keys = Contents.Keys.ToList();

                        void Handler(int Ky)
                        {
                            ImageContourScanLine Value = Contents[Ky];
                            Contents.Remove(Ky);
                            Keys.Remove(Ky);

                            int Ny = Dy - Ky;
                            if (Keys.Contains(Ny))
                                Handler(Ny);

                            Contents.Add(Ny, Value);
                        }

                        while (Keys.Count > 0)
                            Handler(Keys[0]);

                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        double TDx = CenterX * 2d,
                               TDy = CenterY * 2d;
                        int Dx = (int)Math.Round(TDx),
                            Dy = (int)Math.Round(TDy);
                        OffsetX += TDx - Dx;
                        OffsetY += TDy - Dy;

                        List<int> Keys = Contents.Keys.ToList();

                        void Handler(int Ky)
                        {
                            ImageContourScanLine Value = Contents[Ky];
                            Contents.Remove(Ky);
                            Keys.Remove(Ky);

                            // FlipX
                            List<int> XDatas = Value.Datas;
                            int Count = XDatas.Count,
                                Length = Count >> 1;
                            Count--;
                            for (int i = 0; i < Length; i++)
                            {
                                int Temp = Dx - XDatas[i];
                                XDatas[i] = Dx - XDatas[Count - i];
                                XDatas[Count - i] = Temp;
                            }

                            // New Y
                            int Ny = Dy - Ky;
                            if (Keys.Contains(Ny))
                                Handler(Ny);

                            Contents.Add(Ny, Value);
                        }

                        while (Keys.Count > 0)
                            Handler(Keys[0]);

                        break;
                    }
            }
        }

        public void Offset(double DeltaX, double DeltaY)
        {
            OffsetX += DeltaX;
            OffsetY += DeltaY;
        }

        public void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            MinX -= OffsetX;
            MinY -= OffsetY;

            int X0 = (int)Math.Round(MinX),
                Y0 = (int)Math.Round(MinY);

            OffsetX += MinX - X0;
            OffsetY += MinY - Y0;

            MaxX -= OffsetX;
            MaxY -= OffsetY;

            int X1 = (int)Math.Round(MaxX),
                Y1 = (int)Math.Round(MaxY);

            foreach (int Y in Contents.Keys.ToArray())
            {
                if (Y < Y0 || Y1 < Y)
                    Contents.Remove(Y);
                else
                    Contents[Y].Crop(X0, X1);
            }
        }

        void IImageContour.Rotate(double Cx, double Cy, double Theta)
            => throw new NotSupportedException();

        void IImageContour.Scale(double ScaleX, double ScaleY)
            => throw new NotSupportedException();

        public bool Contain(int X, int Y)
            => Contents.TryGetValue(Y, out ImageContourScanLine Data) && Data.Contain(X);

        public void Clear()
        {
            foreach (ImageContourScanLine Data in Contents.Values)
                Data.Clear();

            Contents.Clear();
        }

        void IImageContour.EnsureContents()
        {
        }

        public IEnumerator<KeyValuePair<int, ImageContourScanLine>> GetEnumerator()
            => new ImageContourEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Creates a new contour that is a copy of the current instance.
        /// </summary>
        public ImageContour Clone()
            => new ImageContour(this);
        IImageContour IImageContour.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

        public static ImageContour Flip(ImageContour Source, double CenterX, double CenterY, FlipMode Mode)
        {
            ImageContour Contour = new(Source);
            Contour.Flip(CenterX, CenterY, Mode);
            return Contour;
        }

        public static ImageContour Offset(ImageContour Source, double OffsetX, double OffsetY)
        {
            ImageContour Result = new(Source);
            Result.OffsetX += OffsetX;
            Result.OffsetY += OffsetY;
            return Result;
        }

        public static ImageContour ParsePenContour(IImageContext Stroke, out IReadOnlyPixel StrokeColor)
        {
            StrokeColor = default;
            bool FoundColor = false;
            ImageContour Contour = new();
            for (int j = 0; j < Stroke.Height; j++)
            {
                bool IsFoundLeft = false;
                for (int i = 0; i < Stroke.Width; i++)
                {
                    IReadOnlyPixel Pixel = Stroke[i, j];
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
            ImageContour Ellipse = new();

            GraphicAlgorithm.CalculateBresenhamEllipseContourQuadrantI(Rx, Ry, (Dx, Dy) =>
            {
                int X1 = Cx - Dx,
                    X2 = Cx + Dx;
                ImageContourScanLine Data = Ellipse[Cy - Dy];
                Data.Union(X1, X2);
                Data = Ellipse[Cy + Dy];
                Data.Union(X1, X2);
            });

            return Ellipse;
        }

        public static ImageContour CreateFillSector(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise)
        {
            ImageContour Contour = new();

            void AddData(int X, int Y)
            {
                ImageContourScanLine TData = Contour[Y];
                if (TData.Length == 0)
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

            GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, false, (Dx, Dy) => AddData(Cx + Dx, Cy + Dy));

            return Contour;
        }

        public static ImageContour CreateFillObround(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Obround = new();

            int Length;

            // Horizontal
            if (HalfHeight < HalfWidth)
            {
                Length = HalfWidth - HalfHeight;

                GraphicAlgorithm.CalculateBresenhamEllipseContourQuadrantI(HalfHeight, HalfHeight, (Dx, Dy) =>
                {
                    int X1 = Cx - Dx - Length,
                        X2 = Cx + Dx + Length;
                    ImageContourScanLine Data = Obround[Cy - Dy];
                    Data.Datas.Add(X1);
                    Data.Datas.Add(X2);
                    Data = Obround[Cy + Dy];
                    Data.Datas.Add(X1);
                    Data.Datas.Add(X2);
                });
                return Obround;
            }

            Length = HalfHeight - HalfWidth;

            // Vertical
            GraphicAlgorithm.CalculateBresenhamEllipseContourQuadrantI(HalfWidth, HalfWidth, (Dx, Dy) =>
            {
                int X1 = Cx - Dx,
                    X2 = Cx + Dx;
                ImageContourScanLine Data = Obround[Cy - Dy - Length];
                Data.Datas.Add(X1);
                Data.Datas.Add(X2);
                Data = Obround[Cy + Dy + Length];
                Data.Datas.Add(X1);
                Data.Datas.Add(X2);
            });

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
            ImageContour Diamond = new();

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
            ImageContour Triangle = new();

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
            ImageContour Triangle = new();
            void AddData(int X, int Y)
            {
                ImageContourScanLine TData = Triangle[Y];
                if (TData.Length == 0)
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
            ImageContour Rectangle = new();

            int Left = Cx - HalfWidth,
                Right = Cx + HalfWidth;

            for (int i = 0; i <= HalfHeight; i++)
            {
                Rectangle[Cy - i] = new ImageContourScanLine(Left, Right);
                Rectangle[Cy + i] = new ImageContourScanLine(Left, Right);
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
            ImageContour Rectangle = new();
            void AddData(int X, int Y)
            {
                ImageContourScanLine TData = Rectangle[Y];
                if (TData.Length == 0)
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
            ImageContour Contour = new();
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
            ImageContour Rectangle = new();

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

            foreach (int j in Rectangle.Contents.Keys.ToArray())
            {
                if (j < Top || Bottom < j)
                {
                    Rectangle.Contents.Remove(j);
                    continue;
                }

                int Length = Rectangle.Contents[j].Length;
                if (Length > 2)
                {
                    Length--;
                    for (int i = 1; i < Length; i++)
                        Rectangle.Contents[j].Datas.RemoveAt(1);
                }
            }

            return Rectangle;
        }
        public static ImageContour CreateFillRoundedRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta, int CornerRadius)
        {
            return Theta == 0d
                ? CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, CornerRadius)
                : CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, Theta, CornerRadius, CornerRadius, CornerRadius, CornerRadius);
        }
        public static ImageContour CreateFillRoundedRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight, double Theta, int CornerRadius1, int CornerRadius2, int CornerRadius3, int CornerRadius4)
        {
            if (Math.Round(Theta % MathHelper.HalfPI, 5) == 0d)
                return CreateFillRoundedRectangle(Cx, Cy, HalfWidth, HalfHeight, CornerRadius1, CornerRadius2, CornerRadius3, CornerRadius4);

            ImageContour Rectangle = new();
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
            foreach (KeyValuePair<int, ImageContourScanLine> Item in Rectangle.Contents)
            {
                ImageContourScanLine TData = Item.Value;
                int Length = TData.Length;
                for (int i = 2; i < Length; i++)
                    TData.Datas.RemoveAt(1);
            }

            void AddData(int X, int Y)
            {
                ImageContourScanLine TData = Rectangle[Y];
                if (TData.Length == 0)
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

            ImageContour Polygon = new();
            void AddData(int X, int Y)
            {
                ImageContourScanLine TData = Polygon[Y];
                if (TData.Length == 0)
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

        public static ImageContour CreateFillPolygon(IEnumerable<int> VerticeDatas, int OffsetX, int OffsetY)
        {
            ImageContour Polygon = new();
            int[] Datas = VerticeDatas is int[] TArray ? TArray : VerticeDatas.ToArray();
            int pn = Datas.Length,
                pnh = pn >> 1;

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = int.MaxValue,
                yMax = 0;
            for (int i = 1; i < pn; i += 2)
            {
                int py = Datas[i] + OffsetY;
                if (py < yMin)
                    yMin = py;
                if (py > yMax)
                    yMax = py;
            }

            int[] IntersectionsX = new int[pnh - 1],
                  HorizontalX = new int[pn];

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

                ImageContourScanLine Data = Polygon[y];
                // Add Intersections Datas
                for (int i = 0; i < IntersectionCount - 1;)
                {
                    Data.Datas.Add(IntersectionsX[i++]);
                    Data.Datas.Add(IntersectionsX[i++]);
                }

                // Add Horizontal Datas
                for (int i = 0; i < HorizontalCount - 1;)
                    Data.Union(HorizontalX[i++], HorizontalX[i++]);
            }

            return Polygon;
        }
        public static ImageContour CreateFillPolygon(IEnumerable<int> VerticeDatas, int OffsetX, int OffsetY, int MinX, int MaxX, int MinY, int MaxY)
        {
            ImageContour Polygon = new();
            List<int> Datas = GraphicAlgorithm.InternalCropPolygon(VerticeDatas, MinX, MaxX, MinY, MaxY);

            int pn = Datas.Count;
            if (pn == 0)
                return null;

            int pnh = pn >> 1;

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = int.MaxValue,
                yMax = 0;
            for (int i = 1; i < pn; i += 2)
            {
                int py = Datas[i] + OffsetY;
                if (py < yMin)
                    yMin = py;
                if (py > yMax)
                    yMax = py;
            }

            int[] IntersectionsX = new int[pnh - 1],
                  HorizontalX = new int[pn];

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

                ImageContourScanLine Data = Polygon[y];
                // Add Intersections Datas
                for (int i = 0; i < IntersectionCount - 1;)
                {
                    Data.Datas.Add(IntersectionsX[i++]);
                    Data.Datas.Add(IntersectionsX[i++]);
                }

                // Add Horizontal Datas
                for (int i = 0; i < HorizontalCount - 1;)
                    Data.Union(HorizontalX[i++], HorizontalX[i++]);
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
            ImageContour LineContour = new();

            #region Pen Bound
            int PCx = (Bound.Left + Bound.Right) >> 1,
                PCy = (Bound.Top + Bound.Bottom) >> 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            foreach (KeyValuePair<int, ImageContourScanLine> Item in Pen)
            {
                int j = Item.Key;
                ImageContourScanLine Data = Item.Value;
                if (Data.Length > 2)
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

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, (Dx, Dy) =>
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

        public static ImageContour CreateArcContour(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Pen)
        {
            ImageContour Contour = new();
            Bound<int> Bound = Pen.Bound;

            if (Bound.IsEmpty)
                return null;

            bool IsHollow = Pen.Any(i => i.Value.Length > 2);
            int PCx = (Bound.Left + Bound.Right) >> 1,
                PCy = (Bound.Top + Bound.Bottom) >> 1,
                DSx = Sx - Cx,
                DSy = Sy - Cy,
                DEx = Ex - Cx,
                DEy = Ey - Cy;

            if (IsHollow)
            {
                ImageContour Stroke = Offset(Pen, Cx - PCx, Cy - PCy);

                int LastDx = 0,
                    LastDy = 0;
                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, false,
                    (Dx, Dy) =>
                    {
                        Stroke.Offset(Dx - LastDx, Dy - LastDy);
                        Contour.Union(Stroke);

                        LastDx = Dx;
                        LastDy = Dy;
                    });
            }
            else
            {
                Dictionary<int, int> LargeLeftBound = [],
                                     LargeRightBound = [],
                                     SmallLeftBound = [],
                                     SmallRightBound = [];

                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise, false,
                    (Dx, Dy) =>
                    {
                        int OffsetX = Dx + Cx - PCx,
                            OffsetY = Dy + Cy - PCy;
                        if (Dx < 0)
                        {
                            foreach (KeyValuePair<int, ImageContourScanLine> item in Pen)
                            {
                                ImageContourScanLine Data = item.Value;
                                int Ty = item.Key + OffsetY;

                                if (Ty < 0)
                                    continue;

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
                            foreach (KeyValuePair<int, ImageContourScanLine> item in Pen)
                            {
                                ImageContourScanLine Data = item.Value;
                                int Ty = item.Key + OffsetY;

                                if (Ty < 0)
                                    continue;

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
                        Contour[Y].Union(X0, X1);
                        SmallLeftBound.Remove(Y);
                        continue;
                    }

                    if (LargeRightBound.TryGetValue(Y, out X1))
                    {
                        Contour[Y].Union(X0, X1);
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
                        Contour[Y].Union(X0, X1);
                        LargeRightBound.Remove(Y);
                    }
                }
                SmallRightBound.Clear();
            }

            return Contour;
        }

        public static ImageContour CreateTextContour(int X, int Y, string Text, string FontName, int CharSize, double Angle = 0, FontWeightType Weight = FontWeightType.Normal, bool Italic = false)
        {
            FontData Font = new()
            {
                FaceName = string.IsNullOrEmpty(FontName) ? "System" : FontName,
                Height = CharSize,
                Weight = Weight,
                Escapement = (int)(Angle * 10),
                Italic = Italic,
            };

            return Graphic.CreateTextContour(X, Y, Text, Font);
        }

        public static ImageContour operator |(ImageContour This, ImageContour Contour)
        {
            This.Union(Contour);
            return This;
        }
        public static ImageContour operator &(ImageContour This, ImageContour Contour)
        {
            This.Intersection(Contour);
            return This;
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