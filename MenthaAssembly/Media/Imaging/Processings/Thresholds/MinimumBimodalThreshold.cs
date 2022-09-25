using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    internal sealed class MinimumBimodalThreshold : ImageThreshold
    {
        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
        {
            Histogram<double> Histo = CreateHistogram(Source);
            HistogramCrest FirstCrest = default;

            int i = 0;
            for (; i < 1000; i++)
            {
                int Count = 0;
                foreach (HistogramCrest Crest in Histo.Crests)
                {
                    if (Count == 0)
                        FirstCrest = Crest;

                    if (++Count > 2)
                        break;
                }

                if (Count == 2)
                    break;

                Histo.Smooth((Prev, Curr, Next) => (Prev + Curr + Next) / 3d);
            }

            byte Threshold = (byte)FirstCrest.End;
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