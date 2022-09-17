﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Media.Imaging
{
    public class ImagePalette<Pixel> : IImagePalette
        where Pixel : unmanaged, IPixel
    {
        internal readonly List<Pixel> Datas;

        public GCHandle Handle { get; }

        public int Count => Datas.Count;

        public int Capacity { get; }

        public Pixel this[int Index] => Datas[Index];
        IReadOnlyPixel IImagePalette.this[int Index] => this[Index];

        public int this[Pixel Color] => Datas.IndexOf(Color);
        int IImagePalette.this[IReadOnlyPixel Color] => Datas.FindIndex(i => i.A == Color.A && i.R == Color.R && i.G == Color.G && i.B == Color.B);

        public ImagePalette(int BitsPerPixel)
        {
            Capacity = 1 << BitsPerPixel;
            Datas = new List<Pixel>(Capacity);
            Handle = GCHandle.Alloc(this);
        }
        public ImagePalette(int BitsPerPixel, IEnumerable<Pixel> Palette)
        {
            Capacity = 1 << BitsPerPixel;
            Datas = new List<Pixel>(Palette);
            Handle = GCHandle.Alloc(this);
        }

        public bool TryGetOrAdd(Pixel Color, out int Index)
        {
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

                if (Handle.IsAllocated)
                    Handle.Free();

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
