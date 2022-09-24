using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a threshold decided by averaging the maximum gray and the minimum gray in a n * n neighbourhood.
    /// </summary>
    public sealed class BernsenThreshold : ImageThreshold
    {
        /// <summary>
        /// The specified value to decide the size of neighbourhood (n * n). <para/>n = (2 * Level) + 1.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Level">The specified value to decide the size of neighbourhood (n * n). <para/>n = (2 * Level) + 1.</param>
        public BernsenThreshold(int Level)
        {
            this.Level = Level;
        }

        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
            => new BernsenThresholdingPixelAdapter<T>(Source, Level);

    }
}