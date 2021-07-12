using MenthaAssembly.Media.Imaging.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    public class ImagePalette<Pixel> : IImagePalette
        where Pixel : unmanaged, IPixel
    {
        private readonly List<Pixel> Palette;

        public int Count => Palette.Count;

        public int Capacity { get; }

        public Pixel this[int Index] => Palette[Index];
        IPixel IImagePalette.this[int Index] => this[Index];

        public int this[Pixel Color] => Palette.IndexOf(Color);
        int IImagePalette.this[IPixel Color] => Palette.FindIndex(i => i.A == Color.A && i.R == Color.R && i.G == Color.G && i.B == Color.B);

        public ImagePalette(int BitsPerPixel)
        {
            this.Capacity = 1 << BitsPerPixel;
            this.Palette = new List<Pixel>(Capacity);
        }
        public ImagePalette(int BitsPerPixel, IEnumerable<Pixel> Palette)
        {
            this.Capacity = 1 << BitsPerPixel;
            this.Palette = new List<Pixel>(Palette);
        }

        public bool TryGetOrAdd(Pixel Color, out int Index)
        {
            Index = Palette.IndexOf(Color);
            if (Index != -1)
                return true;

            if (this.Count < Capacity)
            {
                Palette.Add(Color);
                Index = Count - 1;
                return true;
            }

            Index = -1;
            return false;
        }
        bool IImagePalette.TryGetOrAdd(IPixel Color, out int Index)
            => this.TryGetOrAdd(IImageOperator.ToPixel<Pixel>(Color), out Index);

        public static ImagePalette<Pixel> GetSystemPalette<Struct>()
            where Struct : unmanaged, IPixelIndexed
        {
            Struct Temp = default;
            return new ImagePalette<Pixel>(Temp.BitsPerPixel);
        }

        public T[] Extract<T>() where T : unmanaged, IPixel
            => Palette.Select(i => IImageOperator.ToPixel<T>(i))
                      .ToArray();
    }
}
