using MenthaAssembly.Media.Imaging.Utils;
using System;

namespace MenthaAssembly.Media.Imaging
{
    public abstract class ImageThreshold : ICloneable
    {
        protected static readonly Type GrayType = typeof(Gray8);

        public bool IsThreadingSafe { get; }

        protected ImageThreshold(bool IsThreadingSafe)
        {
            this.IsThreadingSafe = IsThreadingSafe;
        }

        public virtual bool Predict(IReadOnlyPixelAdapter Adapter)
            => InternalPredict(Adapter.X, Adapter.Y, Adapter.PixelType == GrayType ? Adapter.A : Adapter.ToGray());

        protected internal abstract bool InternalPredict(int X, int Y, byte Gray);

        protected int[] CreateHistogram<T>(PixelAdapter<T> Adapter, out byte AverageGray)
            where T : unmanaged, IPixel
        {
            int Width = Adapter.MaxX + 1,
                Height = Adapter.MaxY + 1;
            Func<byte> GetGray = typeof(T) == typeof(Gray8) ? () => Adapter.R : () => Adapter.ToGray();

            long S = 0;
            int[] Histogram = new int[256];

            Adapter.InternalMove(0, 0);
            for (int j = 0; j < Height; j++, Adapter.InternalMoveNextLine())
            {
                for (int i = 0; i < Width; i++, Adapter.InternalMoveNext())
                {
                    byte Gray = GetGray();
                    Histogram[Gray]++;
                    S += Gray;
                }

                Adapter.InternalMoveX(-Width);
            }

            AverageGray = (byte)(S / ((long)Width * Height));
            return Histogram;
        }

        public abstract ImageThreshold Clone();
        object ICloneable.Clone()
            => Clone();

        public static implicit operator ImageThreshold(int Threshold) => new ConstThreshold((byte)Threshold);
        public static implicit operator ImageThreshold(byte Threshold) => new ConstThreshold(Threshold);

    }
}