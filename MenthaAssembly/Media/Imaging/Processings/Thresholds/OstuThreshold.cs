using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    internal sealed class OstuThreshold : ImageThreshold
    {
        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
        {
            int[] Histogram = CreateHistogram(Source, out int SumGrays, out int SumPixels);

            int Threshold = 0;
            double Sigma = -1d, TSigma, DeltaMicro;
            int N,
                N0 = 0,
                N1 = SumPixels,
                SumGray,
                SumGray0 = 0,
                SumGray1 = SumGrays;
            for (int i = 0; i < 256; i++)
            {
                N = Histogram[i];
                if (N == 0)
                    continue;

                N0 += N;
                N1 -= N;

                SumGray = N * i;
                SumGray0 += SumGray;
                SumGray1 -= SumGray;

                DeltaMicro = ((double)SumGray1 / N1 - (double)SumGray0 / N0) / SumPixels;
                TSigma = DeltaMicro * DeltaMicro * N0 * N1;
                if (Sigma < TSigma)
                {
                    Sigma = TSigma;
                    Threshold = i;
                }
            }

            return new RangesThresholdingPixelAdapter<T>(Source, (byte)Threshold, byte.MaxValue);
        }

        private int[] CreateHistogram<T>(PixelAdapter<T> Adapter, out int SumGrays, out int SumPixels)
            where T : unmanaged, IPixel
        {
            int[] Histogram = new int[256];
            SumGrays = 0;
            foreach (byte Gray in EnumGrays(Adapter))
            {
                Histogram[Gray]++;
                SumGrays += Gray;
            }

            SumPixels = (Adapter.MaxX + 1) * (Adapter.MaxY + 1);
            return Histogram;
        }

    }
}
