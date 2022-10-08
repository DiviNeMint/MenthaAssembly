using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MenthaAssembly
{
    public sealed class QuantizationCluster
    {
        private readonly object LockObject = new object();
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

        public QuantizationCluster(int[] Center)
        {
            _Center = Center;
            Dimension = Center.Length;
            Histos = new Dictionary<int, int>[Dimension];
            for (int i = 0; i < Dimension; i++)
                Histos[i] = new Dictionary<int, int>();
        }

        public void AddDatas(int[] Datas)
        {
            if (Datas.Length < Dimension)
                throw new ArgumentException($"The Length of Datas have to be greater than {Dimension}.");

            InternalAddDatas(Datas, 1);
        }
        public void AddDatas(int[] Datas, int DataCount)
        {
            if (Datas.Length < Dimension)
                throw new ArgumentException($"The Length of Datas have to be greater than {Dimension}.");

            InternalAddDatas(Datas, DataCount);
        }
        internal void InternalAddDatas(int[] Datas, int DataCount)
            => LockAction(() =>
            {
                int Data;
                for (int i = 0; i < Dimension; i++)
                {
                    Dictionary<int, int> Histo = Histos[i];
                    Data = Datas[i];

                    if (Histo.ContainsKey(Data))
                        Histo[Data] += DataCount;
                    else
                        Histo[Data] = DataCount;
                }

                NextCenter = null;
                TotalDatas += DataCount;
            });

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

        private void LockAction(Action Action)
        {
            Monitor.Enter(LockObject);
            try
            {
                Action();
            }
            finally
            {
                Monitor.Exit(LockObject);
            }
        }

        public override string ToString()
            => string.Join(", ", Center);

    }
}