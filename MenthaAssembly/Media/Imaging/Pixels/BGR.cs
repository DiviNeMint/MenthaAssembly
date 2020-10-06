namespace MenthaAssembly.Media.Imaging
{
    public struct BGR : IPixel
    {
        public byte B { set; get; }

        public byte G { set; get; }

        public byte R { set; get; }

        public byte A => byte.MaxValue;

        public int BitsPerPixel => 24;

        public BGR(byte B, byte G, byte R)
        {
            this.B = B;
            this.G = G;
            this.R = R;
        }

        public override string ToString()
            => $"{{ B : {this.B}, G : {this.G}, R : {this.R}}}";

        public static implicit operator BGR(RGB Target) => new BGR(Target.B, Target.G, Target.R);
        public static implicit operator BGR(ABGR Target) => new BGR(Target.B, Target.G, Target.R);
        public static implicit operator BGR(ARGB Target) => new BGR(Target.B, Target.G, Target.R);
        public static implicit operator BGR(BGRA Target) => new BGR(Target.B, Target.G, Target.R);
        public static implicit operator BGR(RGBA Target) => new BGR(Target.B, Target.G, Target.R);
        public static implicit operator BGR(Gray8 Target) => new BGR(Target.B, Target.G, Target.R);

    }
}
