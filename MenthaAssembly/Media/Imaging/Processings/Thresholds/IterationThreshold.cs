using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class IterationThreshold : ImageThreshold
    {
        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
        {
            int[] Histogram = CreateHistogram(Source, out byte Threshold);

            long S0, S1, N0, N1, Delta;
            do
            {
                // Reset
                S0 = S1 = N0 = N1 = 0;

                int i = 0;
                for (; i < Threshold; i++)
                {
                    S0 += Histogram[i] * i;
                    N0 += Histogram[i];
                }

                for (; i < 256; i++)
                {
                    S1 += Histogram[i] * i;
                    N1 += Histogram[i];
                }

                if (N0 == 0 || N1 == 0)
                    break;

                byte NextThreshold = (byte)((S0 / N0 + S1 / N1) >> 1);
                Delta = NextThreshold > Threshold ? NextThreshold - Threshold : Threshold - NextThreshold;
                Threshold = NextThreshold;

            } while (Delta < 1);

            return new RangesThresholdingPixelAdapter<T>(Source, Threshold, byte.MaxValue);
        }

        private int[] CreateHistogram<T>(PixelAdapter<T> Adapter, out byte AverageGray)
            where T : unmanaged, IPixel
        {
            int[] Histogram = new int[256];
            long S = 0;
            foreach (byte Gray in EnumGrays(Adapter))
            {
                Histogram[Gray]++;
                S += Gray;
            }

            AverageGray = (byte)(S / ((long)(Adapter.MaxX + 1) * (Adapter.MaxY + 1)));
            return Histogram;
        }

    }
}