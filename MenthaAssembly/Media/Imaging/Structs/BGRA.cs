using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly.Media.Imaging
{
    public struct BGRA : IPixel
    {
        public byte B { set; get; }

        public byte G { set; get; }

        public byte R { set; get; }

        public byte A { set; get; }

        public int BitsPerPixel => 32;

        public BGRA(byte B, byte G, byte R, byte A)
        {
            this.B = B;
            this.G = G;
            this.R = R;
            this.A = A;
        }

        public override string ToString()
            => $"{{ B : {this.B}, G : {this.G}, R : {this.R}, A : {this.A}}}";

        public static implicit operator BGRA(RGB Target) => new BGRA(Target.B, Target.G, Target.R, 0xFF);
        public static implicit operator BGRA(BGR Target) => new BGRA(Target.B, Target.G, Target.R, 0xFF);
        public static implicit operator BGRA(ARGB Target) => new BGRA(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(Gray8 Target) => new BGRA(Target.B, Target.G, Target.R, 0xFF);
        public static implicit operator int(BGRA This) => This.A << 24 | This.R << 16 | This.G << 8 | This.B;
    }
}
