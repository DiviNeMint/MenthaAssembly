using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public class ImageContourScanLine : IEnumerable<int>, ICloneable
    {
        internal readonly List<int> Datas;

        public ImageContourScanLine()
        {
            Datas = [];
        }
        public ImageContourScanLine(int Left, int Right)
        {
            if (Right < Left)
                MathHelper.Swap(ref Left, ref Right);

            Datas = [Left, Right];
        }
        public ImageContourScanLine(ImageContourScanLine ScanLine)
        {
            Datas = [.. ScanLine.Datas];
        }

        public int Length
            => Datas.Count;

        public int this[int Index]
        {
            get => Datas[Index];
            internal set => Datas[Index] = value;
        }

        public bool Contain(int X)
        {
            for (int i = 1; i < Length; i += 2)
            {
                if (Datas[i - 1] <= X && X <= Datas[i])
                    return true;
            }

            return false;
        }

        public void Clear()
            => Datas.Clear();

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
            if (Index == Datas.Count - 1)
            {
                if (!Equal)
                {
                    Datas.Add(Left);
                    Datas.Add(Left);
                }
            }
            else if (Index < 0)
            {
                if (Left < Datas[0])
                    Datas[0] = Left;
            }
            else
            {
                if (Equal)
                {
                    Datas.RemoveRange(Index, 2);
                    return;
                }

                // Nearest Left
                Index++;
                if (Left < Datas[Index])
                    Datas[Index] = Left;
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

            int LastIndexOfDatas = Datas.Count - 1;
            if (Index < 0)
            {
                if (Datas[LastIndexOfDatas] < Right)
                    Datas[LastIndexOfDatas] = Right;
            }
            else
            {
                Index--;
                if (Equal)
                {
                    Datas.RemoveRange(Index, 2);
                    return;
                }

                // Nearest Right
                if (Datas[Index] < Right)
                    Datas[Index] = Right;
            }
        }

        public void Union(ImageContourScanLine Data)
        {
            if (Datas.Count == 0)
            {
                Datas.AddRange(Data.Datas);
                return;
            }

            int Index = 0;
            for (int i = 0; i < Data.Length;)
                InternalUnion(Data.Datas[i++], Data.Datas[i++], ref Index);
        }
        public void Union(ImageContourScanLine Data, int Offset)
        {
            if (Datas.Count == 0)
            {
                Datas.AddRange(Data.Datas.Select(i => i + Offset));
                return;
            }

            int Index = 0;
            for (int i = 0; i < Data.Length;)
                InternalUnion(Data.Datas[i++] + Offset, Data.Datas[i++] + Offset, ref Index);
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

            int Index = 0;
            InternalUnion(Left, Right, ref Index);
        }

        public void Intersection(ImageContourScanLine Data)
        {
            if (Length == 0)
                return;

            if (Data.Length == 0)
            {
                Datas.Clear();
                return;
            }

            int Index = 0;
            for (int i = 0; i < Data.Length;)
            {
                InternalIntersection(Data.Datas[i++], Data.Datas[i++], ref Index);
                if (Index >= Length)
                    return;
            }

            for (int i = Length - 1; i >= Index; i--)
                Datas.RemoveAt(i);
        }
        public void Intersection(ImageContourScanLine Data, int Offset)
        {
            if (Length == 0)
                return;

            if (Data.Length == 0)
            {
                Datas.Clear();
                return;
            }

            int Index = 0;
            for (int i = 0; i < Data.Length;)
            {
                InternalIntersection(Data.Datas[i++] + Offset, Data.Datas[i++] + Offset, ref Index);
                if (Index >= Length)
                    return;
            }

            for (int i = Length - 1; i >= Index; i--)
                Datas.RemoveAt(i);
        }

        public void Difference(ImageContourScanLine Data)
        {
            if (Datas.Count == 0)
                return;

            int Index = 0;
            for (int i = 0; i < Data.Length;)
            {
                InternalDifference(Data.Datas[i++], Data.Datas[i++], ref Index);
                if (Index >= Length)
                    return;
            }
        }
        public void Difference(ImageContourScanLine Data, int Offset)
        {
            if (Datas.Count == 0)
                return;

            int Index = 0;
            for (int i = 0; i < Data.Length;)
            {
                InternalDifference(Data.Datas[i++] + Offset, Data.Datas[i++] + Offset, ref Index);
                if (Index >= Length)
                    return;
            }
        }
        public void Difference(int Left, int Right)
        {
            if (Datas.Count == 0)
                return;

            if (Right < Left)
                MathHelper.Swap(ref Left, ref Right);

            int Index = 0;
            InternalDifference(Left, Right, ref Index);
        }

        public void SymmetricDifference(ImageContourScanLine Data)
        {
            if (Datas.Count == 0)
            {
                Datas.AddRange(Data.Datas);
                return;
            }

            ImageContourScanLine Cross = new(this);
            Cross.Intersection(Data);

            int Index = 0;
            for (int i = 0; i < Data.Length;)
                InternalUnion(Data.Datas[i++], Data.Datas[i++], ref Index);

            Index = 0;
            for (int i = 0; i < Cross.Length;)
            {
                InternalDifference(Cross.Datas[i++], Cross.Datas[i++], ref Index);
                if (Index >= Length)
                    return;
            }
        }
        public void SymmetricDifference(ImageContourScanLine Data, int Offset)
        {
            if (Datas.Count == 0)
            {
                Datas.AddRange(Data.Datas);
                return;
            }

            ImageContourScanLine Cross = new(this);
            Cross.Intersection(Data, Offset);

            int Index = 0;
            for (int i = 0; i < Data.Length;)
                InternalUnion(Data.Datas[i++] + Offset, Data.Datas[i++] + Offset, ref Index);

            Index = 0;
            for (int i = 0; i < Cross.Length;)
            {
                InternalDifference(Cross.Datas[i++], Cross.Datas[i++], ref Index);
                if (Index >= Length)
                    return;
            }
        }

        internal void Crop(int Left, int Right)
        {
            int Index = 0;
            for (; Index < Length; Index++)
                if (Left <= Datas[Index])
                    break;

            if ((Index & 0x01) > 0)
            {
                Index--;
                Datas[Index] = Left;
            }

            for (int i = 0; i < Index; i++)
                Datas.RemoveAt(0);

            Index = 1;
            for (; Index < Length; Index++)
                if (Right < Datas[Index])
                    break;

            if ((Index & 0x01) > 0)
                Datas[Index] = Right;

            Index++;
            for (int i = Length - 1; i >= Index; i--)
                Datas.RemoveAt(i);
        }

        public void Offset(int X)
        {
            if (X == 0)
                return;

            for (int i = 0; i < Datas.Count; i++)
                Datas[i] += X;
        }

        public IEnumerator<int> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public ImageContourScanLine Clone()
            => new(this);
        object ICloneable.Clone()
            => Clone();

        public override string ToString()
            => $"{{{string.Join(", ", Datas)}}}";

        private void InternalUnion(int Left, int Right, ref int StartIndex)
        {
            int MinIndex = StartIndex,
                Tx, Lx = int.MinValue;
            do
            {
                if (MinIndex >= Datas.Count)
                {
                    if (Lx++ == Left || Lx == Left)
                    {
                        Datas[MinIndex - 1] = Right;
                    }
                    else
                    {
                        Datas.Add(Left);
                        Datas.Add(Right);
                    }

                    StartIndex = Datas.Count;
                    return;
                }

                Tx = Datas[MinIndex];

                if (Left < Tx)
                    break;

                Lx = Tx;
                MinIndex++;
            } while (true);

            int MaxIndex = MinIndex,
                Rx;
            if ((MinIndex & 0x01) == 1)
            {
                // O || O  O  O
                do
                {
                    // O | O  O  O ||
                    if (MaxIndex >= Datas.Count)
                    {
                        int LastIndex = Datas.Count - 1;
                        Datas[LastIndex--] = Right;

                        for (int i = LastIndex; i >= MinIndex; i--)
                            Datas.RemoveAt(i);

                        StartIndex = MinIndex;
                        return;
                    }

                    Rx = Datas[MaxIndex];

                    if (Right < Rx)
                        break;

                    MaxIndex++;
                } while (true);

                // O |  || O  O  O
                if (MinIndex == MaxIndex)
                {
                    StartIndex = MaxIndex;
                    return;
                }
            }
            else
            {
                if (Lx++ == Left || Lx == Left)
                {
                    // O  |O|  O  O  0  0
                    MinIndex--;

                    do
                    {
                        // O  O | O  O ||
                        if (MaxIndex >= Datas.Count)
                        {
                            int LastIndex = Datas.Count - 1;
                            Datas[LastIndex--] = Right;

                            for (int i = LastIndex; i >= MinIndex; i--)
                                Datas.RemoveAt(i);

                            StartIndex = MinIndex;
                            return;
                        }

                        Rx = Datas[MaxIndex];

                        if (Right < Rx)
                            break;

                        MaxIndex++;
                    } while (true);
                }
                else
                {
                    // O  O || O  O  0  0
                    do
                    {
                        // O  O | O  O ||
                        if (MaxIndex >= Datas.Count)
                        {
                            int LastIndex = Datas.Count - 1;
                            Datas[LastIndex--] = Right;
                            Datas[MinIndex++] = Left;

                            for (int i = LastIndex; i >= MinIndex; i--)
                                Datas.RemoveAt(i);

                            StartIndex = MinIndex;
                            return;
                        }

                        Rx = Datas[MaxIndex];

                        if (Right < Rx)
                            break;

                        MaxIndex++;
                    } while (true);

                    if (MinIndex == MaxIndex)
                    {
                        if (Right + 1 == Rx)
                        {
                            // O  O |  |O|  O
                            Datas[MinIndex] = Left;
                        }
                        else
                        {
                            // O  O |  || O  O
                            Datas.Insert(MinIndex, Right);
                            Datas.Insert(MinIndex, Left);
                        }
                        StartIndex = MinIndex + 1;
                        return;
                    }

                    Datas[MinIndex++] = Left;
                }
            }

            if ((MaxIndex & 0x01) == 0)
            {
                if (Right + 1 == Rx)
                    MaxIndex++;                 // O  O | O  O  |O|  O
                else
                    Datas[--MaxIndex] = Right;  // O  O | O  O || O  O
            }

            for (int i = MaxIndex - 1; i >= MinIndex; i--)
                Datas.RemoveAt(i);

            StartIndex = MinIndex;
        }
        private void InternalIntersection(int Left, int Right, ref int StartIndex)
        {
            int Index = StartIndex;
            for (; Index < Length; Index++)
                if (Left <= Datas[Index])
                    break;

            if ((Index & 0x01) > 0)
            {
                Index--;
                Datas[Index] = Left;
            }

            for (int i = StartIndex; i < Index; i++)
                Datas.RemoveAt(StartIndex);

            if (Length <= StartIndex)
                return;

            StartIndex++;
            for (; StartIndex < Length; StartIndex++)
                if (Right < Datas[StartIndex])
                    break;

            if ((StartIndex & 0x01) > 0)
            {
                Datas.Insert(StartIndex, Right + 1);
                Datas.Insert(StartIndex, Right);
                StartIndex++;
            }
        }
        private void InternalDifference(int Left, int Right, ref int StartIndex)
        {
            for (; StartIndex < Datas.Count; StartIndex++)
                if (Left <= Datas[StartIndex])
                    break;

            int MaxIndex = StartIndex;
            for (; MaxIndex < Datas.Count; MaxIndex++)
                if (Right < Datas[MaxIndex])
                    break;

            Left--;
            Right++;
            if (StartIndex == MaxIndex)
            {
                if ((StartIndex & 0x01) == 0)
                    return;

                Datas.Insert(MaxIndex, Right);
                Datas.Insert(MaxIndex, Left);
                return;
            }

            if ((StartIndex & 0x01) == 1)
            {
                Datas[StartIndex] = Left;
                StartIndex++;
            }

            if ((MaxIndex & 0x01) == 1)
            {
                MaxIndex--;
                Datas[MaxIndex] = Right;
            }

            for (int i = MaxIndex - 1; i >= StartIndex; i--)
                Datas.RemoveAt(i);
        }

        private int LeftIndexWithMoreThanOrEqual(int Right, out bool Equal)
            => LeftIndexWithMoreThanOrEqual(Right, 0, out Equal);
        private int LeftIndexWithMoreThanOrEqual(int Right, int MinLeftIndex, out bool Equal)
        {
            for (int j = MinLeftIndex; j < Datas.Count; j += 2)
            {
                int tData = Datas[j];
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
            for (int j = Datas.Count - 1; j >= MinRightIndex; j -= 2)
            {
                int tData = Datas[j];
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

        public static ImageContourScanLine Union(ImageContourScanLine Data, int Left, int Right)
        {
            ImageContourScanLine Result = new(Data);
            Result.Union(Left, Right);
            return Result;
        }
        public static ImageContourScanLine Union(ImageContourScanLine Data1, ImageContourScanLine Data2)
        {
            ImageContourScanLine Result = new(Data1);
            Result.Union(Data2);
            return Result;
        }

        public static ImageContourScanLine Intersection(ImageContourScanLine Data1, ImageContourScanLine Data2)
        {
            ImageContourScanLine Result = new(Data1);
            Result.Intersection(Data2);
            return Result;
        }

        public static ImageContourScanLine Difference(ImageContourScanLine Data, int Left, int Right)
        {
            ImageContourScanLine Result = new(Data);
            Result.Difference(Left, Right);
            return Result;
        }
        public static ImageContourScanLine Difference(ImageContourScanLine Data1, ImageContourScanLine Data2)
        {
            ImageContourScanLine Result = new(Data1);
            Result.Difference(Data2);
            return Result;
        }

        public static ImageContourScanLine Offset(ImageContourScanLine Data, int X)
        {
            ImageContourScanLine Result = new();
            foreach (int Value in Data.Datas)
                Result.Datas.Add(Value + X);

            return Result;
        }

        public static ImageContourScanLine operator |(ImageContourScanLine This, ImageContourScanLine Data)
        {
            This.Union(Data);
            return This;
        }
        public static ImageContourScanLine operator &(ImageContourScanLine This, ImageContourScanLine Data)
        {
            This.Intersection(Data);
            return This;
        }

        public static ImageContourScanLine operator +(ImageContourScanLine This, ImageContourScanLine Data)
        {
            This.Union(Data);
            return This;
        }
        public static ImageContourScanLine operator -(ImageContourScanLine This, ImageContourScanLine Data)
        {
            This.Difference(Data);
            return This;
        }

        public static ImageContourScanLine operator +(ImageContourScanLine This, int OffsetX)
        {
            This.Offset(OffsetX);
            return This;
        }
        public static ImageContourScanLine operator -(ImageContourScanLine This, int OffsetX)
        {
            This.Offset(-OffsetX);
            return This;
        }

    }
}