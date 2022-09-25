using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    // https://www.shuangyi-tech.com/news_68.html
    /// <summary>
    /// Represents a threshold in image.
    /// </summary>
    public abstract class ImageThreshold
    {
        /// <summary>
        /// Create the adapter of thresholding image.
        /// </summary>
        /// <typeparam name="T">The pixel type of adapter.</typeparam>
        /// <param name="Source">The adapter of image.</param>
        public abstract PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
            where T : unmanaged, IPixel;

        protected IEnumerable<byte> EnumGrays<T>(PixelAdapter<T> Adapter)
            where T : unmanaged, IPixel
        {
            int Width = Adapter.MaxX + 1,
                Height = Adapter.MaxY + 1;
            Func<byte> GetGray = typeof(T) == typeof(Gray8) ? () => Adapter.R : () => Adapter.ToGray();

            Adapter.InternalMove(0, 0);
            for (int j = 0; j < Height; j++, Adapter.InternalMoveNextLine())
            {
                for (int i = 0; i < Width; i++, Adapter.InternalMoveNext())
                    yield return GetGray();

                Adapter.InternalMoveX(-Width);
            }
        }

        /// <summary>
        /// Represents a threshold by the iteration algorithm.
        /// </summary>
        public static ImageThreshold Iteration { get; } = new IterationThreshold();

        /// <summary>
        /// Represents a threshold by averaging the grays of image.
        /// </summary>
        public static ImageThreshold GrayMean { get; } = new MeanThreshold();

        /// <summary>
        /// Represents a threshold by the Ostu algorithm.
        /// </summary>
        public static ImageThreshold Ostu { get; } = new OstuThreshold();

        /// <summary>
        /// Represents a threshold by 25% of background area in image.
        /// </summary>
        public static ImageThreshold PTile25 { get; } = new PTileThreshold(0.25d);

        /// <summary>
        /// Represents a threshold by 50% of background area in image.
        /// </summary>
        public static ImageThreshold PTile50 { get; } = new PTileThreshold(0.5d);

        /// <summary>
        /// Represents a threshold by 75% of background area in image.
        /// </summary>
        public static ImageThreshold PTile75 { get; } = new PTileThreshold(0.75d);

        /// <summary>
        /// Represents a threshold by the minimum gray between bimodal.
        /// </summary>
        public static ImageThreshold MinimumBimodal { get; } = new MinimumBimodalThreshold();

        /// <summary>
        /// Represents a threshold by averaging the peak gray of bimodal.
        /// </summary>
        public static ImageThreshold MeanBimodal { get; } = new MeanBimodalThreshold();

        /// <summary>
        /// Represents a threshold by averaging the grays in a 3 * 3 neighbourhood.
        /// </summary>
        public static ImageThreshold MeanNeighbor { get; } = new MeanNeighborThreshold(1);

        /// <summary>
        /// Represents a threshold by the median of the grays in a 3 * 3 neighbourhood.
        /// </summary>
        public static ImageThreshold MedianNeighbor { get; } = new MedianNeighborThreshold(1);

        /// <summary>
        /// Represents a threshold by averaging the maximum gray and the minimum gray in a 3 * 3 neighbourhood.
        /// </summary>
        public static ImageThreshold Bernsen { get; } = new BernsenThreshold(1);

        public static implicit operator ImageThreshold(int Threshold)
            => new ConstThreshold((byte)Threshold.Clamp(byte.MinValue, byte.MaxValue));
        public static implicit operator ImageThreshold(byte Threshold)
            => new ConstThreshold(Threshold);

    }
}