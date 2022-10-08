using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenthaAssembly
{
    public sealed class QuantizationBox
    {
        public int Dimension { get; }

        public int TotalDatas { private set; get; } = 0;

        public int[] MinBounds { get; }

        public int[] MaxBounds { get; }

        private readonly Dictionary<int, int>[] Histos;
        private readonly int[] MinValues, MaxValues;

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
        private QuantizationBox(QuantizationBox Box, int[] MinBounds, int[] MaxBounds)
        {
            Dimension = Box.Dimension;
            Histos = new Dictionary<int, int>[Dimension];
            this.MinBounds = MinBounds;
            this.MaxBounds = MaxBounds;
            MinValues = MinBounds;
            MaxValues = MaxBounds;

            for (int i = 0; i < Dimension; i++)
            {
                Dictionary<int, int> Histo = new Dictionary<int, int>();
                for (int j = MinBounds[i]; j < MaxBounds[i]; j++)
                    Histo[j] = Box.Histos[i][j];

                Histos[i] = Histo;
            }
            TotalDatas = Histos[0].Sum(i => i.Value);
        }

        public bool TryAddDatas(int[] Datas)
            => Datas.Length >= Dimension && InternalTryAddDatas(Datas, 1);
        public bool TryAddDatas(int[] Datas, int DataCount)
            => Datas.Length >= Dimension && InternalTryAddDatas(Datas, DataCount);
        internal bool InternalTryAddDatas(int[] Datas, int DataCount)
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
                    Histo[Data] += DataCount;
                else
                    Histo[Data] = DataCount;
            }

            TotalDatas += DataCount;
            ValueCenter = null;
            return true;
        }

        private int[] ValueCenter = null;
        public int[] GetValueCenter()
        {
            if (ValueCenter != null)
                return ValueCenter;

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

            ValueCenter = Center;
            return Center;
        }

        public int[] GetBoxCenter()
        {
            int[] Center = new int[Dimension];
            for (int i = 0; i < Dimension; i++)
                Center[i] = (MinBounds[i] + MaxBounds[i]) >> 1;

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

        public IEnumerable<QuantizationBox> MedianSplitContent()
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

                    yield return new QuantizationBox(this, MinBound, MaxBound);

                    MinBound = MinValues.ToArray();
                    MaxBound = MaxValues.ToArray();
                    MinBound[Index] = NMax + 1;
                    break;
                }
            }

            yield return new QuantizationBox(this, MinBound, MaxBound);
        }

        private void EnsureMaxAndMin()
        {
            for (int i = 0; i < Dimension; i++)
            {
                if (MaxValues[i] == MinBounds[i])
                    MaxValues[i] = MinValues[i];
                else if (MinValues[i] == MaxBounds[i])
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

    public sealed class QuantizationBox<T>
    {
        public int Dimension { get; }

        public int TotalDatas { private set; get; } = 0;

        private readonly Dictionary<T, int>[] Histos;
        private readonly Operatorable<T>[] MinValues, MaxValues,
                                           MinBounds, MaxBounds;

        public QuantizationBox(int Dimension, T MinValue, T MaxValue, IEnumerable<T> Keys)
        {
            this.Dimension = Dimension;
            Histos = new Dictionary<T, int>[Dimension];
            MinValues = new Operatorable<T>[Dimension];
            MaxValues = new Operatorable<T>[Dimension];
            MinBounds = new Operatorable<T>[Dimension];
            MaxBounds = new Operatorable<T>[Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                Histos[i] = Keys.ToDictionary(i => i, i => 0);
                MinValues[i] = MaxValue;
                MaxValues[i] = MinValue;
                MinBounds[i] = MinValue;
                MaxBounds[i] = MaxValue;
            }
        }
        private QuantizationBox(Operatorable<T>[] MinBounds, Operatorable<T>[] MaxBounds, IEnumerable<T> Keys)
        {
            Dimension = MinBounds.Length;
            Histos = new Dictionary<T, int>[Dimension];
            this.MinBounds = MinBounds;
            this.MaxBounds = MaxBounds;
            MinValues = new Operatorable<T>[Dimension];
            MaxValues = new Operatorable<T>[Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                Histos[i] = Keys.ToDictionary(i => i, i => 0);
                MinValues[i] = MaxBounds[i];
                MaxValues[i] = MinBounds[i];
            }
        }

        public bool TryAddDatas(T[] Datas)
            => Datas.Length >= Dimension && InternalTryAddDatas(Datas);
        internal bool InternalTryAddDatas(T[] Datas)
        {
            // Check Bounds
            if (!Contain(Datas))
                return false;

            T Data;
            for (int i = 0; i < Dimension; i++)
            {
                Data = Datas[i];

                // Compare with Min & Max
                if (Data < MinValues[i])
                    MinValues[i] = Data;

                if (MaxValues[i] < Data)
                    MaxValues[i] = Data;

                // Add Datas
                Dictionary<T, int> Histo = Histos[i];
                if (Histo.ContainsKey(Data))
                    Histo[Data]++;
                else
                    Histo[Data] = 1;
            }

            TotalDatas++;
            Center = null;
            return true;
        }

        private T[] Center = null;
        public T[] GetCenter()
        {
            if (this.Center != null)
                return this.Center;

            T[] Center = new T[Dimension];
            Operatorable<T> Sum,
                            Frax = Operatorable<T>.Cast(TotalDatas >> 1);
            for (int i = 0; i < Dimension; i++)
            {
                Sum = Frax;
                foreach (KeyValuePair<T, int> HistoData in Histos[i])
                    Sum += new Operatorable<T>(HistoData.Key) * HistoData.Value;

                Center[i] = Sum / TotalDatas;
            }

            this.Center = Center;
            return Center;
        }

        public IEnumerable<QuantizationBox<T>> Split(int Count)
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
            Operatorable<T>[] MinBound = MinValues.ToArray(),
                              MaxBound = MaxValues.ToArray();

            Dictionary<T, int> Histo = Histos[Index];
            int ColorCount = 0,
                TCount;
            Operatorable<T> TMax = MaxValues[Index],
                            j = MinValues[Index];
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

                        yield return new QuantizationBox<T>(MinBound, MaxBound, Histo.Keys);

                        MinBound = MinValues.ToArray();
                        MaxBound = MaxValues.ToArray();
                        MinBound[Index] = j + 1;
                        break;
                    }
                }
            }

            yield return new QuantizationBox<T>(MinBound, MaxBound, Histo.Keys);
        }

        public IEnumerable<QuantizationBox<T>> MeanSplit()
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
            Operatorable<T>[] MinBound = MinValues.ToArray(),
                              MaxBound = MaxValues.ToArray();
            Operatorable<T> Mean = (MinValues[Index] + MaxValues[Index]) / 2;
            if (MinValues[Index] == Mean)
            {
                yield return this;
                yield break;
            }

            MaxBound[Index] = Mean;
            yield return new QuantizationBox<T>(MinBound, MaxBound, Histos[0].Keys);

            MinBound = MinValues.ToArray();
            MaxBound = MaxValues.ToArray();
            MinBound[Index] = Mean++;

            yield return new QuantizationBox<T>(MinBound, MaxBound, Histos[0].Keys);
        }

        public IEnumerable<QuantizationBox<T>> MedianSplit()
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
            Operatorable<T>[] MinBound = MinValues.ToArray(),
                  MaxBound = MaxValues.ToArray();

            Dictionary<T, int> Histo = Histos[Index];
            int DatasCount = 0,
                TCount = TotalDatas >> 1;
            Operatorable<T> TMax = MaxValues[Index],
                            j = MinValues[Index];

            for (; j <= TMax; j++)
            {
                if (!Histo.ContainsKey(j))
                    continue;

                DatasCount += Histo[j];
                if (TCount < DatasCount)
                {
                    Operatorable<T> NMax = j == TMax ? j - 1 : j;
                    MaxBound[Index] = NMax;

                    yield return new QuantizationBox<T>(MinBound, MaxBound, Histos[0].Keys);

                    MinBound = MinValues.ToArray();
                    MaxBound = MaxValues.ToArray();
                    MinBound[Index] = NMax + 1;
                    break;
                }
            }

            yield return new QuantizationBox<T>(MinBound, MaxBound, Histos[0].Keys);
        }

        private void EnsureMaxAndMin()
        {
            for (int i = 0; i < Dimension; i++)
            {
                if (MaxValues[i] == MinBounds[i])
                    MaxValues[i] = MinValues[i];
                else if (MinValues[i] == MaxBounds[i])
                    MinValues[i] = MaxValues[i];
            }
        }
        private int GetMaxDeltaDimension()
        {
            // Calculate the maximum delta of dimension.
            int DimensionlIndex = 0;
            Operatorable<T> D = MaxValues[0] - MinValues[0],
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

            return D == default ? -1 : DimensionlIndex;
        }

        public T GetValueSize()
        {
            EnsureMaxAndMin();

            T Size = MaxValues[0] - MinValues[0];
            for (int i = 1; i < Dimension; i++)
                Size *= MaxValues[i] - MinValues[i];

            return Size;
        }

        public bool Contain(params T[] Datas)
        {
            for (int i = 0; i < Dimension; i++)
            {
                T Data = Datas[i];
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