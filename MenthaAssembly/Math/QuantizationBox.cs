using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenthaAssembly
{
    public sealed class QuantizationBox
    {
        public int Dimension { get; }
        private readonly Dictionary<int, int>[] Histos;
        internal int TotalDatas = 0;
        private readonly int[] MinValues, MaxValues,
                               MinBounds, MaxBounds;

        public QuantizationBox(int Dimension, int MinValue, int MaxValue)
        {
            this.Dimension = Dimension;
            Histos = new Dictionary<int, int>[Dimension];
            MinValues = new int[Dimension];
            MaxValues = new int[Dimension];
            MinBounds = new int[Dimension];
            MaxBounds = new int[Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                Dictionary<int, int> Histo = new Dictionary<int, int>();
                for (int j = MinValue; j < MaxValue; j++)
                    Histo[j] = 0;

                Histos[i] = Histo;
                MinValues[i] = MaxValue;
                MaxValues[i] = MinValue;
                MinBounds[i] = MinValue;
                MaxBounds[i] = MaxValue;
            }
        }
        private QuantizationBox(int[] MinBounds, int[] MaxBounds)
        {
            Dimension = MinBounds.Length;
            Histos = new Dictionary<int, int>[Dimension];
            this.MinBounds = MinBounds;
            this.MaxBounds = MaxBounds;

            MinValues = new int[Dimension];
            MaxValues = new int[Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                Dictionary<int, int> Histo = new Dictionary<int, int>();
                for (int j = MinBounds[i]; j < MaxBounds[i]; j++)
                    Histo[j] = 0;

                Histos[i] = Histo;
                MinValues[i] = MaxBounds[i];
                MaxValues[i] = MinBounds[i];
            }
        }

        public bool TryAddDatas(params int[] Datas)
            => Datas.Length == Dimension && InternalTryAddDatas(Datas);
        internal bool InternalTryAddDatas(params int[] Datas)
        {
            // Check Bounds
            if (!Contain(Datas))
                return false;

            int Data;
            for (int i = 0; i < Dimension; i++)
            {
                Data = Datas[i];

                // Compare with Min & Max
                if (Data < MinValues[i])
                    MinValues[i] = Data;

                if (MaxValues[i] < Data)
                    MaxValues[i] = Data;

                // Add Datas
                Dictionary<int, int> Histo = Histos[i];
                if (Histo.ContainsKey(Data))
                    Histo[Data]++;
                else
                    Histo[Data] = 1;
            }

            TotalDatas++;
            Center = null;
            return true;
        }

        private int[] Center = null;
        public int[] GetCenter()
        {
            if (this.Center != null)
                return this.Center;

            int[] Center = new int[Dimension];
            int Sum,
                Frax = TotalDatas >> 1;
            for (int i = 0; i < Dimension; i++)
            {
                Sum = Frax;
                foreach (KeyValuePair<int, int> HistoData in Histos[i])
                    Sum += HistoData.Key * HistoData.Value;

                Center[i] = Sum / TotalDatas;
            }

            this.Center = Center;
            return Center;
        }

        public IEnumerable<QuantizationBox> Split(int Count)
        {
            if (Count <= 1)
            {
                yield return this;
                yield break;
            }

            // Check Min & Max
            EnsureMaxAndMin();

            // Calculate the maximum delta of dimension.
            int Index = GetMaxDeltaDimension();
            if (Index < 0)
            {
                yield return this;
                yield break;
            }

            // Split
            int[] MinBound = MinValues.ToArray(),
                  MaxBound = MaxValues.ToArray();

            Dictionary<int, int> Histo = Histos[Index];
            int ColorCount = 0,
                TMax = MaxValues[Index],
                j = MinValues[Index],
                TCount;
            for (int i = 1; i < Count; i++)
            {
                TCount = TotalDatas * i / Count;
                for (; j <= TMax; j++)
                {
                    if (!Histo.ContainsKey(j))
                        continue;

                    ColorCount += Histo[j];
                    if (TCount < ColorCount)
                    {
                        MaxBound[Index] = j;

                        yield return new QuantizationBox(MinBound, MaxBound);

                        MinBound = MinValues.ToArray();
                        MaxBound = MaxValues.ToArray();
                        MinBound[Index] = j + 1;
                        break;
                    }
                }
            }

            yield return new QuantizationBox(MinBound, MaxBound);
        }

        public IEnumerable<QuantizationBox> MeanSplit()
        {
            // Check Min & Max
            EnsureMaxAndMin();

            // Calculate the maximum delta of dimension.
            int Index = GetMaxDeltaDimension();
            if (Index < 0)
            {
                yield return this;
                yield break;
            }

            // Split
            int[] MinBound = MinValues.ToArray(),
                  MaxBound = MaxValues.ToArray();
            int Mean = (MinValues[Index] + MaxValues[Index]) >> 1;

            if (MinValues[Index] == Mean)
            {
                yield return this;
                yield break;
            }

            MaxBound[Index] = Mean;
            yield return new QuantizationBox(MinBound, MaxBound);

            MinBound = MinValues.ToArray();
            MaxBound = MaxValues.ToArray();
            MinBound[Index] = Mean + 1;

            yield return new QuantizationBox(MinBound, MaxBound);
        }

        public IEnumerable<QuantizationBox> MedianSplit()
        {
            // Check Min & Max
            EnsureMaxAndMin();

            // Calculate the maximum delta of dimension.
            int Index = GetMaxDeltaDimension();
            if (Index < 0)
            {
                yield return this;
                yield break;
            }

            // Split
            int[] MinBound = MinValues.ToArray(),
                  MaxBound = MaxValues.ToArray();

            Dictionary<int, int> Histo = Histos[Index];
            int DatasCount = 0,
                TMax = MaxValues[Index],
                j = MinValues[Index],
                TCount = TotalDatas >> 1;

            for (; j <= TMax; j++)
            {
                if (!Histo.ContainsKey(j))
                    continue;

                DatasCount += Histo[j];
                if (TCount < DatasCount)
                {
                    int NMax = j == TMax ? j - 1 : j;
                    MaxBound[Index] = NMax;

                    yield return new QuantizationBox(MinBound, MaxBound);

                    MinBound = MinValues.ToArray();
                    MaxBound = MaxValues.ToArray();
                    MinBound[Index] = NMax + 1;
                    break;
                }
            }

            yield return new QuantizationBox(MinBound, MaxBound);
        }

        private void EnsureMaxAndMin()
        {
            for (int i = 0; i < Dimension; i++)
            {
                if (MaxValues[i] == byte.MinValue)
                    MaxValues[i] = MinValues[i];
                else if (MinValues[i] == byte.MaxValue)
                    MinValues[i] = MaxValues[i];
            }
        }
        private int GetMaxDeltaDimension()
        {
            // Calculate the maximum delta of dimension.
            int DimensionlIndex = 0,
                D = MaxValues[0] - MinValues[0],
                Dt;
            for (int i = 1; i < Dimension; i++)
            {
                Dt = MaxValues[i] - MinValues[i];
                if (D < Dt)
                {
                    DimensionlIndex = i;
                    D = Dt;
                }
            }

            return D == 0 ? -1 : DimensionlIndex;
        }

        public int GetValueSize()
        {
            EnsureMaxAndMin();

            int Size = MaxValues[0] - MinValues[0];
            for (int i = 1; i < Dimension; i++)
                Size *= MaxValues[i] - MinValues[i];

            return Size;
        }

        public bool Contain(params int[] Datas)
        {
            for (int i = 0; i < Dimension; i++)
            {
                int Data = Datas[i];
                if (Data < MinBounds[i] || MaxBounds[i] < Data)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            if (Dimension < 1)
                return string.Empty;

            StringBuilder Builder = new StringBuilder();
            try
            {
                Builder.Append($"{MinBounds[0]} <= Data[{0}] <= {MaxBounds[0]}");
                for (int i = 1; i < Dimension; i++)
                    Builder.Append($", {MinBounds[i]} <= Data[{i}] <= {MaxBounds[i]}");

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }

        }

    }
}