using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    public class ImagePalette<Pixel> : IImagePalette
        where Pixel : unmanaged, IPixel
    {
        internal readonly List<Pixel> Datas;

        public int Count => Datas.Count;

        public int Capacity { get; }

        public Pixel this[int Index] => Datas[Index];
        IPixel IImagePalette.this[int Index] => this[Index];

        public int this[Pixel Color] => Datas.IndexOf(Color);
        int IImagePalette.this[IPixel Color] => Datas.FindIndex(i => i.A == Color.A && i.R == Color.R && i.G == Color.G && i.B == Color.B);

        public ImagePalette(int BitsPerPixel)
        {
            this.Capacity = 1 << BitsPerPixel;
            this.Datas = new List<Pixel>(Capacity);
        }
        public ImagePalette(int BitsPerPixel, IEnumerable<Pixel> Palette)
        {
            this.Capacity = 1 << BitsPerPixel;
            this.Datas = new List<Pixel>(Palette);
        }

        public bool TryGetOrAdd(Pixel Color, out int Index)
        {
            Index = Datas.IndexOf(Color);
            if (Index != -1)
                return true;

            if (this.Count < Capacity)
            {
                Datas.Add(Color);
                Index = Count - 1;
                return true;
            }

            Index = -1;
            return false;
        }
        bool IImagePalette.TryGetOrAdd(IPixel Color, out int Index)
            => this.TryGetOrAdd(Color.ToPixel<Pixel>(), out Index);

        public static ImagePalette<Pixel> GetSystemPalette<Struct>()
            where Struct : unmanaged, IPixelIndexed
        {
            Struct Temp = default;
            return new ImagePalette<Pixel>(Temp.BitsPerPixel);
        }

        public T[] Extract<T>() where T : unmanaged, IPixel
            => Datas.Select(i => i.ToPixel<T>())
                    .ToArray();
    }
}
