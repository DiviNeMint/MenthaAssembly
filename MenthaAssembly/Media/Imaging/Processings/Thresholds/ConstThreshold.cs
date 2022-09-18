using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ConstThreshold : ImageThreshold
    {
        public byte Threshold { get; }

        public ConstThreshold(byte Threshold)
        {
            this.Threshold = Threshold;
        }

        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
            => new RangesThresholdingPixelAdapter<T>(Source, Threshold, byte.MaxValue);

    }
}