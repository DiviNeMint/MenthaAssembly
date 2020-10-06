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

        public ImageContour Offset(int X, int Y)
        {
            ImageContour Offsetted = new ImageContour();
            foreach (KeyValuePair<int, ContourData> Data in Datas)
                Offsetted[Data.Key + Y] = Data.Value.Offset(X);

            return Offsetted;
        }

        public IEnumerator<KeyValuePair<int, ContourData>> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static ImageContour Parse(IImageContext Pen, out IPixel Color)
        {
            Color = default;
            bool FoundColor = false;
            ImageContour PenContour = new ImageContour();
            for (int j = 0; j < Pen.Height; j++)
            {
                bool IsFoundLeft = false;
                for (int i = 0; i < Pen.Width; i++)
                {
                    IPixel Pixel = Pen[i, j];
                    if (IsFoundLeft)
                    {
                        if (Pixel.A == 0)
                        {
                            PenContour[j].AddRight(i - 1);
                            IsFoundLeft = false;
                        }
                    }
                    else
                    {
                        if (Pixel.A > 0)
                        {
                            if (!FoundColor)
                            {
                                Color = Pixel;
                                FoundColor = true;
                            }

                            PenContour[j].AddLeft(i);
                            IsFoundLeft = true;
                        }
                    }
                }

                if (IsFoundLeft)
                    PenContour[j].AddRight(Pen.Width - 1);
            }

            return PenContour;
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

    public class ContourData : IEnumerable<int>
    {
        private readonly List<int> Datas = new List<int>();

        public ContourData()
        {
        }
        public ContourData(int Left, int Right)
        {
            Union(Left, Right);
        }

        public int Count => Datas.Count;

        public int this[int Index]
        {
            get => Datas[Index];
            internal set => Datas[Index] = value;
        }

        public void AddLeft(int Left)
        {
            if (Datas.Count == 0)
            {
                Datas.Add(Left);
                Datas.Add(Left);
                return;
            }
            else if (Datas.Count == 2)
            {
                if (Left < Datas[0])
                    Datas[0] = Left;

                return;
            }

            int Index = RightIndexWithLessThanOrEqual(Left, out bool Equal);
            if (Index == this.Datas.Count - 1)
            {
                if (!Equal)
                {
                    Datas.Add(Left);
                    Datas.Add(Left);
                }
            }
            else if (Index < 0)
            {
                if (Left < this.Datas[0])
                    this.Datas[0] = Left;
            }
            else
            {
                if (Equal)
                {
                    this.Datas.RemoveRange(Index, 2);
                    return;
                }

                // Nearest Left
                Index++;
                if (Left < this.Datas[Index])
                    this.Datas[Index] = Left;
            }
        }
        public void AddRight(int Right)
        {
            if (Datas.Count == 0)
            {
                Datas.Add(Right);
                Datas.Add(Right);
                return;
            }
            else if (Datas.Count == 2)
            {
                if (Datas[1] < Right)
                    Datas[1] = Right;

                return;
            }

            int Index = LeftIndexWithMoreThanOrEqual(Right, out bool Equal);
            if (Index == 0)
            {
                if (!Equal)
                {
                    Datas.Insert(0, Right);
                    Datas.Insert(0, Right);
                }
                return;
            }

            int LastIndexOfDatas = this.Datas.Count - 1;
            if (Index < 0)
            {
                if (this.Datas[LastIndexOfDatas] < Right)
                    this.Datas[LastIndexOfDatas] = Right;
            }
            else
            {
                Index--;
                if (Equal)
                {
                    this.Datas.RemoveRange(Index, 2);
                    return;
                }

                // Nearest Right
                if (this.Datas[Index] < Right)
                    this.Datas[Index] = Right;
            }
        }

        public void Union(ContourData Info)
        {
            if (Datas.Count == 0)
            {
                Datas.AddRange(Info.Datas);
                return;
            }

            for (int i = 0; i < Info.Datas.Count;)
                this.HandleUnion(Info.Datas[i++], Info.Datas[i++]);
        }
        public void Union(int Left, int Right)
        {
            if (Right < Left)
                MathHelper.Swap(ref Left, ref Right);

            if (Datas.Count == 0)
            {
                Datas.Add(Left);
                Datas.Add(Right);
                return;
            }

            this.HandleUnion(Left, Right);
        }

        public void Difference(ContourData Info)
        {
            if (Datas.Count == 0)
                return;

            for (int i = 0; i < Info.Datas.Count;)
                this.HandleDifference(Info.Datas[i++], Info.Datas[i++]);
        }
        public void Difference(int Left, int Right)
        {
            if (Datas.Count == 0)
                return;

            if (Right < Left)
                MathHelper.Swap(ref Left, ref Right);

            this.HandleDifference(Left, Right);
        }

        public ContourData Offset(int X)
        {
            ContourData Offsetted = new ContourData();
            foreach (int Data in Datas)
                Offsetted.Datas.Add(Data + X);

            return Offsetted;
        }

        public IEnumerator<int> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void HandleUnion(int Left, int Right)
        {
            int LastIndexOfDatas = this.Datas.Count - 1,
                LIndex = RightIndexWithLessThanOrEqual(Left, out bool LEqual);
            if (LIndex < 0)
            {
                int RIndex = LeftIndexWithMoreThanOrEqual(Right, out bool REqual);
                if (RIndex == 0)
                {
                    if (REqual)
                    {
                        this.Datas[0] = Left;
                    }
                    else
                    {
                        this.Datas.Insert(0, Left);
                        this.Datas.Insert(1, Right);
                    }
                }
                else if (RIndex < 0)
                {
                    Left = Math.Min(Left, this.Datas[0]);
                    Right = Math.Max(Right, this.Datas[LastIndexOfDatas]);

                    this.Datas.Clear();
                    this.Datas.Add(Left);
                    this.Datas.Add(Right);
                }
                else
                {
                    if (Left < this.Datas[0])
                        this.Datas[0] = Left;

                    if (!REqual)
                    {
                        // Nearest RIndex
                        RIndex--;
                        int NearestRight = this.Datas[RIndex];
                        if (NearestRight < Right)
                            this.Datas[RIndex] = Right;

                        RIndex--;
                    }

                    for (int i = RIndex; i > 0; i--)
                        this.Datas.RemoveAt(i);
                }
            }
            else if (LIndex == LastIndexOfDatas)
            {
                if (LEqual)
                {
                    this.Datas[LastIndexOfDatas] = Right;
                }
                else
                {
                    this.Datas.Add(Left);
                    this.Datas.Add(Right);
                }
            }
            else
            {
                if (!LEqual)
                {
                    // Nearest LIndex
                    LIndex++;
                    int NearestLeft = this.Datas[LIndex];
                    if (Left < NearestLeft)
                    {
                        if (Right < NearestLeft)
                        {
                            this.Datas.Insert(LIndex, Right);
                            this.Datas.Insert(LIndex, Left);
                            return;
                        }

                        this.Datas[LIndex] = Left;

                        if (Right == NearestLeft)
                            return;

                    }
                }

                int RIndex = LeftIndexWithMoreThanOrEqual(Right, LIndex, out bool REqual);
                if (RIndex < 0)
                {
                    RIndex = LastIndexOfDatas;
                    int NearRight = this.Datas[RIndex];

                    if (NearRight < Right)
                        this.Datas[RIndex] = Right;

                    RIndex--;
                }
                else
                {
                    if (!REqual)
                    {
                        // Nearest RIndex
                        RIndex--;
                        int NearRight = this.Datas[RIndex];
                        if (NearRight < Right)
                            this.Datas[RIndex] = Right;

                        RIndex--;
                    }
                }

                for (int i = RIndex; i > LIndex; i--)
                    this.Datas.RemoveAt(i);
            }
        }
        private void HandleDifference(int Left, int Right)
        {
            int MinIndex = 0;
            for (; MinIndex < Datas.Count; MinIndex++)
                if (Left < Datas[MinIndex])
                    break;

            int MaxIndex = MinIndex;
            for (; MaxIndex < Datas.Count; MaxIndex++)
                if (Right < Datas[MaxIndex])
                    break;

            if (MinIndex == MaxIndex)
            {
                if ((MinIndex & 0x01) == 0)
                    return;

                Datas.Insert(MaxIndex, Right);
                Datas.Insert(MaxIndex, Left);
                return;
            }

            if ((MaxIndex & 0x01) == 1)
            {
                MaxIndex--;
                Datas[MaxIndex] = Right;
            }

            if ((MinIndex & 0x01) == 1)
            {
                Datas[MinIndex] = Left;
                MinIndex++;
            }

            for (int i = MaxIndex - 1; i >= MinIndex; i--)
                Datas.RemoveAt(i);
        }

        private int LeftIndexWithMoreThanOrEqual(int Right, out bool Equal)
            => LeftIndexWithMoreThanOrEqual(Right, 0, out Equal);
        private int LeftIndexWithMoreThanOrEqual(int Right, int MinLeftIndex, out bool Equal)
        {
            for (int j = MinLeftIndex; j < this.Datas.Count; j += 2)
            {
                int tData = this.Datas[j];
                if (Right == tData)
                {
                    Equal = true;
                    return j;
                }

                if (Right < tData)
                {
                    Equal = false;
                    return j;
                }
            }

            Equal = false;
            return -1;
        }
        private int RightIndexWithLessThanOrEqual(int Left, out bool Equal)
            => RightIndexWithLessThanOrEqual(Left, 0, out Equal);
        private int RightIndexWithLessThanOrEqual(int Left, int MinRightIndex, out bool Equal)
        {
            for (int j = this.Datas.Count - 1; j >= MinRightIndex; j -= 2)
            {
                int tData = this.Datas[j];
                if (Left == tData)
                {
                    Equal = true;
                    return j;
                }

                if (tData < Left)
                {
                    Equal = false;
                    return j;
                }
            }

            Equal = false;
            return -1;
        }

    }
}
