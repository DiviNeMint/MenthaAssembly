namespace MenthaAssembly.Media.Imaging
{
    public struct RGB : IPixel
    {
        public byte A => byte.MaxValue;

        public byte R { set; get; }

        public byte G { set; get; }

        public byte B { set; get; }

        public int BitsPerPixel => 24;

        public RGB(byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public override string ToString()
            => $"{{ R : {this.R}, G : {this.G}, B : {this.B}}}";

        public static implicit operator RGB(BGR Target) => new RGB(Target.R, Target.G, Target.B);
        public static implicit operator RGB(ARGB Target) => new RGB(Target.R, Target.G, Target.B);
        public static implicit operator RGB(ABGR Target) => new RGB(Target.R, Target.G, Target.B);
        public static implicit operator RGB(RGBA Target) => new RGB(Target.R, Target.G, Target.B);
        public static implicit operator RGB(BGRA Target) => new RGB(Target.R, Target.G, Target.B);
        public static implicit operator RGB(Gray8 Target) => new RGB(Target.R, Target.G, Target.B);
        //public static implicit operator RGB(int Target) => -16777216 | Target.R << 16 | Target.G << 8 | Target.B;
    }
}