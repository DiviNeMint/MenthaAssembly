using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a threshold decided by averaging the neighbor grays (n * n).
    /// </summary>
    public sealed class MeanNeighborThreshold : ImageThreshold
    {
        /// <summary>
        /// The specified value to decide the size of neighbor (n * n). <para/>n = (2 * Level) + 1.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Level">The specified value to decide the size of neighbor (n * n). <para/>n = (2 * Level) + 1.</param>
        public MeanNeighborThreshold(int Level)
        {
            this.Level = Level;
        }

        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
            => new MeanNeighborThresholdingPixelAdapter<T>(Source, Level);

    }
}