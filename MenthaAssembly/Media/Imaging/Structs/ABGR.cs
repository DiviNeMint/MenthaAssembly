namespace MenthaAssembly.Media.Imaging
{
    public struct ABGR : IPixel
    {
        public byte A { set; get; }

        public byte B { set; get; }

        public byte G { set; get; }

        public byte R { set; get; }

        public int BitsPerPixel => 32;

        public ABGR(byte A, byte B, byte G, byte R)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public override string ToString()
            => $"{{ A : {this.A}, B : {this.B}, G : {this.G}, R : {this.R}}}";

        public static implicit operator ABGR(RGB Target) => new ABGR(0xFF, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(BGR Target) => new ABGR(0xFF, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(RGBA Target) => new ABGR(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(BGRA Target) => new ABGR(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(ARGB Target) => new ABGR(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(Gray8 Target) => new ABGR(0xFF, Target.B, Target.G, Target.R);

    }
}
