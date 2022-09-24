using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a constant threshold.
    /// </summary>
    public sealed class ConstThreshold : ImageThreshold
    {
        /// <summary>
        /// The specified constant of threshold.
        /// </summary>
        public byte Threshold { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Threshold">The specified constant of threshold.</param>
        public ConstThreshold(byte Threshold)
        {
            this.Threshold = Threshold;
        }

        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
            => new RangesThresholdingPixelAdapter<T>(Source, Threshold, byte.MaxValue);

    }
}