using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly
{
    public sealed class QuantizationCluster
    {
        private readonly int Dimension;
        private int[] _Center;
        public int[] Center
        {
            get => _Center;
            set
            {
                _Center = value;

                // Reset
                TotalDatas = 0;
                NextCenter = null;
                foreach (Dictionary<int, int> Histo in Histos)
                    foreach (int Key in Histo.Keys.ToArray())
                        Histo[Key] = 0;
            }
        }

        public int TotalDatas { private set; get; } = 0;

        private readonly Dictionary<int, int>[] Histos;

        public QuantizationCluster(int[] Center, int MinValue, int MaxValue)
        {
            _Center = Center;
            Dimension = Center.Length;
            Histos = new Dictionary<int, int>[Dimension];

            for (int i = 0; i < Dimension; i++)
            {
                Dictionary<int, int> Histo = new Dictionary<int, int>();
                for (int j = MinValue; j < MaxValue; j++)
                    Histo[j] = 0;

                Histos[i] = Histo;
            }
        }

        public bool TryAddDatas(params int[] Datas)
            => Datas.Length >= Dimension && InternalTryAddDatas(Datas);
        internal bool InternalTryAddDatas(params int[] Datas)
        {
            int Data;
            for (int i = 0; i < Dimension; i++)
            {
                Dictionary<int, int> Histo = Histos[i];
                Data = Datas[i];

                if (Histo.ContainsKey(Data))
                    Histo[Data]++;
                else
                    Histo[Data] = 1;
            }

            NextCenter = null;
            TotalDatas++;
            return true;
        }

        private int[] NextCenter;
        public int[] GetNextCenter()
        {
            if (NextCenter != null)
                return NextCenter;

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

            NextCenter = Center;
            return Center;
        }

        public override string ToString()
            => string.Join(", ", Center);

    }
}