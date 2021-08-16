using System;
using System.Collections;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public class ContourData : IEnumerable<int>, ICloneable
    {
        internal readonly List<int> Datas;

        public ContourData()
        {
            Datas = new List<int>();
        }
        public ContourData(int Left, int Right)
        {
            if (Right < Left)
                MathHelper.Swap(ref Left, ref Right);

            Datas = new List<int> { Left, Right };
        }
        private ContourData(IEnumerable<int> Datas)
        {
            this.Datas = new List<int>(Datas);
        }

        public int Count => Datas.Count;

        public int this[int Index]
        {
            get => Datas[Index];
            internal set => Datas[Index] = value;
        }

        public bool Contain(int X)
        {
            int Index = Datas.FindIndex(i => X < i);
            return Index > 0 && (Index & 1) > 0;
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

            int Index = 0;
            for (int i = 0; i < Info.Datas.Count;)
                this.HandleUnion(Info.Datas[i++], Info.Datas[i++], ref Index);
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
            this.HandleUnion(Left, Right, ref Index);
        }
        public static ContourData Union(ContourData Data, int Left, int Right)
        {
            ContourData Result = new ContourData(Data.Datas);
            Result.Union(Left, Right);
            return Result;
        }
        public static ContourData Union(ContourData Data1, ContourData Data2)
        {
            ContourData Result = new ContourData(Data1.Datas);
            Result.Union(Data2);
            return Result;
        }

        public void Difference(ContourData Info)
        {
            if (Datas.Count == 0)
                return;

            int Index = 0;
            for (int i = 0; i < Info.Datas.Count;)
                this.HandleDifference(Info.Datas[i++], Info.Datas[i++], ref Index);
        }
        public void Difference(int Left, int Right)
        {
            if (Datas.Count == 0)
                return;

            if (Right < Left)
                MathHelper.Swap(ref Left, ref Right);

            int Index = 0;
            this.HandleDifference(Left, Right, ref Index);
        }
        public static ContourData Difference(ContourData Data, int Left, int Right)
        {
            ContourData Result = new ContourData(Data.Datas);
            Result.Difference(Left, Right);
            return Result;
        }
        public static ContourData Difference(ContourData Data1, ContourData Data2)
        {
            ContourData Result = new ContourData(Data1.Datas);
            Result.Difference(Data2);
            return Result;
        }

        public void Offset(int X)
        {
            if (X == 0)
                return;

            for (int i = 0; i < Datas.Count; i++)
                Datas[i] += X;
        }
        public static ContourData Offset(ContourData Data, int X)
        {
            ContourData Result = new ContourData();
            foreach (int Value in Data.Datas)
                Result.Datas.Add(Value + X);

            return Result;
        }

        public IEnumerator<int> GetEnumerator()
            => Datas.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public ContourData Clone()
            => new ContourData(this.Datas);
        object ICloneable.Clone()
            => this.Clone();

        public override string ToString()
            => $"{{{string.Join(", ", Datas)}}}";

        //private void HandleUnion(int Left, int Right)
        //{
        //    int LastIndexOfDatas = this.Datas.Count - 1,
        //        LIndex = RightIndexWithLessThanOrEqual(Left, out bool LEqual);
        //    if (LIndex < 0)
        //    {
        //        int RIndex = LeftIndexWithMoreThanOrEqual(Right, out bool REqual);
        //        if (RIndex == 0)
        //        {
        //            if (REqual)
        //            {
        //                this.Datas[0] = Left;
        //            }
        //            else
        //            {
        //                this.Datas.Insert(0, Left);
        //                this.Datas.Insert(1, Right);
        //            }
        //        }
        //        else if (RIndex < 0)
        //        {
        //            Left = Math.Min(Left, this.Datas[0]);
        //            Right = Math.Max(Right, this.Datas[LastIndexOfDatas]);

        //            this.Datas.Clear();
        //            this.Datas.Add(Left);
        //            this.Datas.Add(Right);
        //        }
        //        else
        //        {
        //            if (Left < this.Datas[0])
        //                this.Datas[0] = Left;

        //            if (!REqual)
        //            {
        //                // Nearest RIndex
        //                RIndex--;
        //                int NearestRight = this.Datas[RIndex];
        //                if (NearestRight < Right)
        //                    this.Datas[RIndex] = Right;

        //                RIndex--;
        //            }

        //            for (int i = RIndex; i > 0; i--)
        //                this.Datas.RemoveAt(i);
        //        }
        //    }
        //    else if (LIndex == LastIndexOfDatas)
        //    {
        //        if (LEqual)
        //        {
        //            this.Datas[LastIndexOfDatas] = Right;
        //        }
        //        else
        //        {
        //            this.Datas.Add(Left);
        //            this.Datas.Add(Right);
        //        }
        //    }
        //    else
        //    {
        //        if (LEqual)
        //        {
        //            LIndex--;
        //        }
        //        else
        //        {
        //            // Nearest LIndex
        //            LIndex++;
        //            int NearestLeft = this.Datas[LIndex];
        //            if (Left < NearestLeft)
        //            {
        //                if (Right < NearestLeft)
        //                {
        //                    this.Datas.Insert(LIndex, Right);
        //                    this.Datas.Insert(LIndex, Left);
        //                    return;
        //                }

        //                this.Datas[LIndex] = Left;

        //                if (Right == NearestLeft)
        //                    return;
        //            }
        //        }

        //        int RIndex = LeftIndexWithMoreThanOrEqual(Right, LIndex, out bool REqual);
        //        if (RIndex < 0)
        //        {
        //            RIndex = LastIndexOfDatas;
        //            int NearRight = this.Datas[RIndex];

        //            if (NearRight < Right)
        //                this.Datas[RIndex] = Right;

        //            RIndex--;
        //        }
        //        else
        //        {
        //            if (!REqual)
        //            {
        //                // Nearest RIndex
        //                RIndex--;
        //                int NearRight = this.Datas[RIndex];
        //                if (NearRight < Right)
        //                    this.Datas[RIndex] = Right;

        //                RIndex--;
        //            }
        //        }

        //        for (int i = RIndex; i > LIndex; i--)
        //            this.Datas.RemoveAt(i);
        //    }
        //}
        private void HandleUnion(int Left, int Right, ref int StartIndex)
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

        //private void HandleDifference(int Left, int Right)
        //{
        //    int MinIndex = 0;
        //    for (; MinIndex < Datas.Count; MinIndex++)
        //        if (Left < Datas[MinIndex])
        //            break;

        //    int MaxIndex = MinIndex;
        //    for (; MaxIndex < Datas.Count; MaxIndex++)
        //        if (Right < Datas[MaxIndex])
        //            break;

        //    if (MinIndex == MaxIndex)
        //    {
        //        if ((MinIndex & 0x01) == 0)
        //            return;

        //        Datas.Insert(MaxIndex, Right);
        //        Datas.Insert(MaxIndex, Left);
        //        return;
        //    }

        //    if ((MaxIndex & 0x01) == 1)
        //    {
        //        MaxIndex--;
        //        Datas[MaxIndex] = Right;
        //    }

        //    if ((MinIndex & 0x01) == 1)
        //    {
        //        Datas[MinIndex] = Left;
        //        MinIndex++;
        //    }

        //    for (int i = MaxIndex - 1; i >= MinIndex; i--)
        //        Datas.RemoveAt(i);
        //}
        private void HandleDifference(int Left, int Right, ref int StartIndex)
        {
            for (; StartIndex < Datas.Count; StartIndex++)
                if (Left <= Datas[StartIndex])
                    break;

            int MaxIndex = StartIndex;
            for (; MaxIndex < Datas.Count; MaxIndex++)
                if (Right < Datas[MaxIndex])
                    break;

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

        public static ContourData operator +(ContourData This, ContourData Data)
        {
            This.Union(Data);
            return This;
        }
        public static ContourData operator -(ContourData This, ContourData Data)
        {
            This.Difference(Data);
            return This;
        }

        public static ContourData operator +(ContourData This, int OffsetX)
        {
            This.Offset(OffsetX);
            return This;
        }
        public static ContourData operator -(ContourData This, int OffsetX)
        {
            This.Offset(-OffsetX);
            return This;
        }

    }
}
