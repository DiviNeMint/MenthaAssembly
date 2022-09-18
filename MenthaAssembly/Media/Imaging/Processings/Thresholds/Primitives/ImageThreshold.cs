using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
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
        /// The special threshold calculated by iteration algorithm.
        /// </summary>
        public static ImageThreshold Iteration { get; } = new IterationThreshold();

        /// <summary>
        /// The special threshold calculated by averaging all gray.
        /// </summary>
        public static ImageThreshold GrayMean { get; } = new MeanThreshold();

        /// <summary>
        /// The special threshold calculated by 25% of background area in image.
        /// </summary>
        public static ImageThreshold PTile25 { get; } = new PTileThreshold(0.25d);

        /// <summary>
        /// The special threshold calculated by 50% of background area in image.
        /// </summary>
        public static ImageThreshold PTile50 { get; } = new PTileThreshold(0.5d);

        /// <summary>
        /// The special threshold calculated by 75% of background area in image.
        /// </summary>
        public static ImageThreshold PTile75 { get; } = new PTileThreshold(0.75d);

        public static implicit operator ImageThreshold(int Threshold)
            => new ConstThreshold((byte)Threshold.Clamp(byte.MinValue, byte.MaxValue));
        public static implicit operator ImageThreshold(byte Threshold)
            => new ConstThreshold(Threshold);

    }
}