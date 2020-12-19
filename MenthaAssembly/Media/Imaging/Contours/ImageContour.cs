using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public class ImageContour : IEnumerable<KeyValuePair<int, ContourData>>
    {
        internal readonly SortedList<int, ContourData> Datas = new SortedList<int, ContourData>();

        public int Count => Datas.Count;

        public Int32Bound Bound
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
                                return Int32Bound.Empty;

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

                        return new Int32Bound(Left, Top, Right, Bottom);
                    }

                    return Int32Bound.Empty;
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

        public IEnumerator<KeyValuePair<int, ContourData>> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static ImageContour Parse(IImageContext Stroke, out IPixel StrokeColor)
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
                    TData.AddLeft(X);
                    return;
                }

                TData[X <= TData[0] ? 0 : 1] = X;
            });

            return Ellipse;
        }
        public static ImageContour CreateFillObround(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Obround = new ImageContour();
            void AddData(int X, int Y)
            {
                ContourData TData = Obround[Y];
                if (TData.Count == 0)
                {
                    TData.AddLeft(X);
                    return;
                }

                TData[X <= TData[0] ? 0 : 1] = X;
            }

            // Horizontal
            if (HalfHeight < HalfWidth)
            {
                GraphicAlgorithm.CalculateBresenhamEllipse(HalfHeight, HalfHeight, (Dx, Dy) => AddData(Cx + Dx + (Dx > 0 ? HalfWidth : -HalfWidth), Cy + Dy));
                return Obround;
            }

            // Vertical
            GraphicAlgorithm.CalculateBresenhamEllipse(HalfWidth, HalfWidth, (Dx, Dy) => AddData(Cx + Dx, Cy + Dy + (Dy > 0 ? HalfHeight : -HalfHeight)));

            int Left = Cx - HalfWidth,
                Right = Cx + HalfWidth;
            for (int Y = Cy - HalfHeight; Y <= Cy + HalfHeight; Y++)
                Obround[Y].Union(Left, Right);

            return Obround;
        }
        public static ImageContour CreateFillRectangle(int Cx, int Cy, int HalfWidth, int HalfHeight)
        {
            ImageContour Rectangle = new ImageContour();

            for (int i = 0; i < HalfHeight; i++)
            {
                int Left = Cx - HalfWidth,
                    Right = Cx + HalfWidth;
                Rectangle[Cy - i].Union(Left, Right);
                Rectangle[Cy + i].Union(Left, Right);
            }

            return Rectangle;
        }
        public static ImageContour CreateFillRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, double StartAngle = 0d)
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

                TData[X <= TData[0] ? 0 : 1] = X;
            }

            double DeltaTheta = 360d / VertexNum,
                   LastAngle = StartAngle;

            int P0x = (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                P0y = (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta)),
                LastPx = P0x,
                LastPy = P0y,
                DLPx, DLPy;

            for (int i = 1; i < VertexNum; i++)
            {
                LastAngle += DeltaTheta;
                int Px = (int)Math.Ceiling(Radius * Math.Cos(LastAngle * MathHelper.UnitTheta)),
                    Py = (int)Math.Ceiling(Radius * Math.Sin(LastAngle * MathHelper.UnitTheta));
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

    }
}
