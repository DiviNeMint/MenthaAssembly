namespace MenthaAssembly.Media.Imaging
{
    public struct RGBA : IPixel
    {
        public byte R { set; get; }

        public byte G { set; get; }

        public byte B { set; get; }

        public byte A { set; get; }

        public int BitsPerPixel => 32;

        public RGBA(byte R, byte G, byte B, byte A)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public override string ToString()
            => $"{{ R : {this.R}, G : {this.G}, B : {this.B}, A : {this.A}}}";

        public static implicit operator RGBA(RGB Target) => new RGBA(Target.R, Target.G, Target.B, 0xFF);
        public static implicit operator RGBA(BGR Target) => new RGBA(Target.R, Target.G, Target.B, 0xFF);
        public static implicit operator RGBA(ARGB Target) => new RGBA(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(ABGR Target) => new RGBA(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(BGRA Target) => new RGBA(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(Gray8 Target) => new RGBA(Target.R, Target.G, Target.B, 0xFF);
    
    }
}
