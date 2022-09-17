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

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.A = (byte)(Alpha / 255);
            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
        }

        public override string ToString()
            => $"A : {A}, R : {R}, G : {G}, B : {B}";

        public static implicit operator ARGB(RGB Target) => new ARGB(0xFF, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(BGR Target) => new ARGB(0xFF, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(RGBA Target) => new ARGB(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(BGRA Target) => new ARGB(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(ABGR Target) => new ARGB(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(Gray8 Target) => new ARGB(0xFF, Target.R, Target.G, Target.B);
        //public static implicit operator ARGB(int This) => new ARGB((byte)(This >> 24), (byte)(This >> 16), (byte)(This >> 8), (byte)This);
    }
}
