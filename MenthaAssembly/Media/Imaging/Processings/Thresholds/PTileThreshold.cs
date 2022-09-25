using MenthaAssembly.Media.Imaging.Utils;
using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a threshold by the percentage of background area in image.
    /// </summary>
    public sealed class PTileThreshold : ImageThreshold
    {
        /// <summary>
        /// The percentage of background area in image.
        /// </summary>
        /// <value>0 &lt;= Percentage &lt;= 1</value>
        public double Percentage { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Percentage">The percentage of background area in image.
        /// <para/>0 &lt;= value &lt;= 1</param>
        public PTileThreshold(double Percentage)
        {
            this.Percentage = Percentage;
        }

        public override PixelAdapter<T> CreateAdapter<T>(PixelAdapter<T> Source)
        {
            int[] Histogram = CreateHistogram(Source);

            byte Threshold = byte.MinValue;
            int MinAmount = (int)Math.Round((Source.MaxX + 1d) * Percentage * (Source.MaxY + 1d)),
                Amount = 0;

            for (int i = 0; i < Histogram.Length; i++)
            {
                Amount += Histogram[i];
                if (MinAmount <= Amount)
                {
                    Threshold = (byte)i;
                    break;
                }
            }

            return new RangesThresholdingPixelAdapter<T>(Source, Threshold, byte.MaxValue);
        }

        private int[] CreateHistogram<T>(PixelAdapter<T> Adapter)
            where T : unmanaged, IPixel
        {
            int[] Histogram = new int[256];
            foreach (byte Gray in EnumGrays(Adapter))
                Histogram[Gray]++;

            return Histogram;
        }

    }
}