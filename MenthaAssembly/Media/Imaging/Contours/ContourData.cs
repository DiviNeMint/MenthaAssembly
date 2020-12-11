using System;
using System.Collections;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public class ContourData : IEnumerable<int>
    {
        internal readonly List<int> Datas = new List<int>();

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
