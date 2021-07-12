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

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.B = B;
            this.G = G;
            this.R = R;
            this.A = A;
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == 0)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
            this.A = (byte)(Alpha / 255);
        }

        public override string ToString()
            => $"{{ B : {this.B}, G : {this.G}, R : {this.R}, A : {this.A}}}";

        public static implicit operator BGRA(RGB Target) => new BGRA(Target.B, Target.G, Target.R, 0xFF);
        public static implicit operator BGRA(BGR Target) => new BGRA(Target.B, Target.G, Target.R, 0xFF);
        public static implicit operator BGRA(ARGB Target) => new BGRA(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(ABGR Target) => new BGRA(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(RGBA Target) => new BGRA(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(Gray8 Target) => new BGRA(Target.B, Target.G, Target.R, 0xFF);
        public static implicit operator int(BGRA This) => This.A << 24 | This.R << 16 | This.G << 8 | This.B;
    }
}
