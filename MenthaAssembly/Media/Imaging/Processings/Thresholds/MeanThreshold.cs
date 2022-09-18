using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class MeanThreshold : ImageThreshold
    {
        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
        {
            byte Threshold = GetAverageGray(Source);
            return new RangesThresholdingPixelAdapter<T>(Source, Threshold, byte.MaxValue);
        }

        private byte GetAverageGray<T>(PixelAdapter<T> Adapter)
            where T : unmanaged, IPixel
        {
            long S = 0;
            foreach (byte Gray in EnumGrays(Adapter))
                S += Gray;

            return (byte)(S / ((long)(Adapter.MaxX + 1) * (Adapter.MaxY + 1)));
        }

    }
}