using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MenthaAssembly.Media.Imaging
{
    public class ImagePalette<Pixel> : IImagePalette, IDisposable
        where Pixel : unmanaged, IPixel
    {
        protected readonly object LockObject = new();
        internal readonly List<Pixel> Datas;

        public int Count
            => Datas.Count;

        public int Capacity { get; }

        public Pixel this[int Index]
            => Datas[Index];
        IReadOnlyPixel IImagePalette.this[int Index]
            => this[Index];

        public int this[Pixel Color]
            => Datas.IndexOf(Color);
        int IImagePalette.this[IReadOnlyPixel Color]
            => Datas.FindIndex(i => i.A == Color.A && i.R == Color.R && i.G == Color.G && i.B == Color.B);

        public ImagePalette(int BitsPerPixel)
        {
            Capacity = 1 << BitsPerPixel;
            Datas = new List<Pixel>(Capacity);
        }
        public ImagePalette(int BitsPerPixel, IEnumerable<Pixel> Palette)
        {
            Capacity = 1 << BitsPerPixel;
            Datas = new List<Pixel>(Palette);
        }

        public Pixel GetOrAdd(Pixel Color, out int Index)
        {
            if (Count < Capacity)
            {
                bool Token = false;
                try
                {
                    Monitor.Enter(LockObject, ref Token);

                    Index = Datas.IndexOf(Color);
                    if (Index != -1)
                        return Color;

                    Datas.Add(Color);
                    Index = Count - 1;
                    return Color;
                }
                finally
                {
                    if (Token)
                        Monitor.Exit(LockObject);
                }
            }
            else
            {
                Index = Datas.IndexOf(Color);
                if (Index != -1)
                    return Color;

                // Finds the color that minimum distance.
                Index = 0;
                Pixel Data = Datas[0];
                int Da = Data.A - Color.A,
                    Dr = Data.R - Color.R,
                    Dg = Data.G - Color.G,
                    Db = Data.B - Color.B,
                    D = Da * Da + Dr * Dr + Dg * Dg + Db * Db,
                    TD;

                for (int i = 1; i < Capacity; i++)
                {
                    Data = Datas[i];
                    Da = Data.A - Color.A;
                    Dr = Data.R - Color.R;
                    Dg = Data.G - Color.G;
                    Db = Data.B - Color.B;
                    TD = Da * Da + Dr * Dr + Dg * Dg + Db * Db;
                    if (TD < D)
                    {
                        Index = i;
                        D = TD;
                    }
                }

                return Datas[Index];
            }
        }
        IReadOnlyPixel IImagePalette.GetOrAdd(IReadOnlyPixel Color, out int Index)
            => GetOrAdd(Color.ToPixel<Pixel>(), out Index);

        public static ImagePalette<Pixel> GetSystemPalette<Struct>()
            where Struct : unmanaged, IPixelIndexed
        {
            Struct Temp = default;
            return new ImagePalette<Pixel>(Temp.BitsPerPixel);
        }

        public T[] Extract<T>() where T : unmanaged, IPixel
            => Datas.Select(i => i.ToPixel<T>())
                    .ToArray();

        private bool IsDisposed;
        protected virtual void Dispose(bool Disposing)
        {
            if (!IsDisposed)
            {
                if (Disposing)
                    Datas.Clear();

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImagePalette()
        {
            Dispose(false);
        }

    }
}