using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MenthaAssembly.Media.Imaging
{
    public class ImagePalette<Pixel> : IImagePalette, IDisposable
        where Pixel : unmanaged, IPixel
    {
        protected readonly object LockObject = new object();
        internal readonly List<Pixel> Datas;

        public int Count => Datas.Count;

        public int Capacity { get; }

        public Pixel this[int Index]
            => Index < Datas.Count ? Datas[Index] : default;
        IReadOnlyPixel IImagePalette.this[int Index]
            => this[Index];

        public int this[Pixel Color] => Datas.IndexOf(Color);
        int IImagePalette.this[IReadOnlyPixel Color] => Datas.FindIndex(i => i.A == Color.A && i.R == Color.R && i.G == Color.G && i.B == Color.B);

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

        public bool TryGetOrAdd(Pixel Color, out int Index)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObject, ref Token);

                Index = Datas.IndexOf(Color);
                if (Index != -1)
                    return true;

                if (Count < Capacity)
                {
                    Datas.Add(Color);
                    Index = Count - 1;
                    return true;
                }

                Index = -1;
                return false;
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObject);
            }
        }
        bool IImagePalette.TryGetOrAdd(IReadOnlyPixel Color, out int Index)
            => TryGetOrAdd(Color.ToPixel<Pixel>(), out Index);

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