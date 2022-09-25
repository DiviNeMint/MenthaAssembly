using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    internal sealed class MeanBimodalThreshold : ImageThreshold
    {
        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
        {
            Histogram<double> Histo = CreateHistogram(Source);
            HistogramCrest[] Crests = new HistogramCrest[2];

            int i = 0;
            for (; i < 1000; i++)
            {
                int Count = 0;
                foreach (HistogramCrest Crest in Histo.Crests)
                {
                    if (Count < 2)
                        Crests[Count] = Crest;

                    if (++Count > 2)
                        break;
                }

                if (Count == 2)
                    break;

                Histo.Smooth((Prev, Curr, Next) => (Prev + Curr + Next) / 3d);
            }

            byte Threshold = (byte)((Crests[0].Peak + Crests[1].Peak) >> 1);
            return new RangesThresholdingPixelAdapter<T>(Source, Threshold, byte.MaxValue);
        }

        private Histogram<double> CreateHistogram<T>(PixelAdapter<T> Adapter)
            where T : unmanaged, IPixel
        {
            double[] Datas = new double[256];
            foreach (byte Gray in EnumGrays(Adapter))
                Datas[Gray]++;

            return new Histogram<double>(Datas);
        }

    }
}