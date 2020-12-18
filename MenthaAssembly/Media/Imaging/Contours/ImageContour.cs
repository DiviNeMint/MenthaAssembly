using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public class ImageContour : IEnumerable<KeyValuePair<int, ContourData>>
    {
        internal readonly SortedList<int, ContourData> Datas;

        public int Count => Datas.Count;

        internal readonly bool IsObservable;

        private bool NeedUpdateBound = true;
        private Int32Bound _Bound;
        public Int32Bound Bound
        {
            get
            {
                if (NeedUpdateBound)
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
                                {
                                    _Bound = Int32Bound.Empty;
                                    return _Bound;
                                };

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

                            _Bound = new Int32Bound(Left, Top, Right, Bottom);
                        }
                        else
                        {
                            _Bound = Int32Bound.Empty;
                        }
                    }
                    finally
                    {
                        Enumerator.Dispose();
                        NeedUpdateBound = false;
                    }
                }

                return _Bound;
            }
        }

        public ContourData this[int Y]
        {
            get
            {
                if (Datas.TryGetValue(Y, out ContourData Info))
                    return Info;

                Info = new ContourData();

                if (IsObservable)
                    Info.DatasChanged += OnDatasChanged;

                Datas[Y] = Info;
                return Info;
            }
            set
            {
                if (Datas.TryGetValue(Y, out ContourData Data))
                    Data.DatasChanged -= OnDatasChanged;

                if (value is null)
                {
                    Datas.Remove(Y);
                    return;
                }

                if (IsObservable)
                    value.DatasChanged += OnDatasChanged;

                Datas[Y] = value;
            }
        }

        public ImageContour() : this(true)
        {
        }
        internal ImageContour(bool IsObservable)
        {
            this.IsObservable = IsObservable;
            Datas = new SortedList<int, ContourData>();
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
            Dictionary<int, ContourData> Temp = new Dictionary<int, ContourData>(Datas);
            Datas.Clear();

            foreach (KeyValuePair<int, ContourData> Item in Temp)
            {
                ContourData Data = Item.Value;
                Data.Offset(X);
                Datas.Add(Item.Key + Y, Data);
            }

            NeedUpdateBound = X != 0 && Y != 0;
        }
        public static ImageContour Offset(ImageContour Source, int X, int Y)
            => Offset(Source, X, Y, true);
        internal static ImageContour Offset(ImageContour Source, int X, int Y, bool IsObservable)
        {
            ImageContour Result = new ImageContour(IsObservable);
            foreach (KeyValuePair<int, ContourData> Data in Source.Datas)
                Result[Data.Key + Y] = ContourData.Offset(Data.Value, X);

            return Result;
        }

        public IEnumerator<KeyValuePair<int, ContourData>> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void OnDatasChanged(object sender, EventArgs e)
            => NeedUpdateBound = true;


        public static ImageContour Parse(IImageContext Stroke, out IPixel StrokeColor)
            => Parse(Stroke, out StrokeColor, true);
        internal static ImageContour Parse(IImageContext Stroke, out IPixel StrokeColor, bool IsObservable)
        {
            StrokeColor = default;
            bool FoundColor = false;
            ImageContour Contour = new ImageContour(IsObservable);
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
