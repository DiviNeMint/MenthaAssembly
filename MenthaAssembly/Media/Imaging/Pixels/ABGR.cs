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

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.B = B;
            this.G = G;
            this.R = R;
        }
        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == 0)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.A = (byte)(Alpha / 255);
            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
        }

        public override string ToString()
            => $"A : {this.A}, B : {this.B}, G : {this.G}, R : {this.R}";


        public static implicit operator ABGR(RGB Target) => new ABGR(0xFF, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(BGR Target) => new ABGR(0xFF, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(RGBA Target) => new ABGR(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(BGRA Target) => new ABGR(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(ARGB Target) => new ABGR(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(Gray8 Target) => new ABGR(0xFF, Target.B, Target.G, Target.R);

    }
}
