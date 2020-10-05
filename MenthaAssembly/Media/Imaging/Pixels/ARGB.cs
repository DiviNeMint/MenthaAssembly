namespace MenthaAssembly.Media.Imaging
{
    public struct ARGB : IPixel
    {
        public byte A { set; get; }

        public byte R { set; get; }

        public byte G { set; get; }

        public byte B { set; get; }

        public int BitsPerPixel => 32;

        public ARGB(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public override string ToString()
            => $"{{ A : {this.A}, R : {this.R}, G : {this.G}, B : {this.B}}}";

        public static implicit operator ARGB(RGB Target) => new ARGB(0xFF, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(BGR Target) => new ARGB(0xFF, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(RGBA Target) => new ARGB(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(BGRA Target) => new ARGB(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(ABGR Target) => new ARGB(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(Gray8 Target) => new ARGB(0xFF, Target.R, Target.G, Target.B);
        //public static implicit operator ARGB(int This) => new ARGB((byte)(This >> 24), (byte)(This >> 16), (byte)(This >> 8), (byte)This);
    }
}
