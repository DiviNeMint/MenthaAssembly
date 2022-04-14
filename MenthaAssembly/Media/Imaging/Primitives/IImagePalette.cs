using System;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Media.Imaging
{
    public interface IImagePalette : IDisposable
    {
        public GCHandle Handle { get; }

        int this[IPixel Color] { get; }

        IPixel this[int Index] { get; }

        bool TryGetOrAdd(IPixel Color, out int Index);

        int Count { get; }

        int Capacity { get; }

        public T[] Extract<T>()
            where T : unmanaged, IPixel;

    }
}
