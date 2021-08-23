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

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == 0)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
            this.A = (byte)(Alpha / 255);
        }

        public override string ToString()
            => $"R : {this.R}, G : {this.G}, B : {this.B}, A : {this.A}";

        public static implicit operator RGBA(RGB Target) => new RGBA(Target.R, Target.G, Target.B, 0xFF);
        public static implicit operator RGBA(BGR Target) => new RGBA(Target.R, Target.G, Target.B, 0xFF);
        public static implicit operator RGBA(ARGB Target) => new RGBA(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(ABGR Target) => new RGBA(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(BGRA Target) => new RGBA(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(Gray8 Target) => new RGBA(Target.R, Target.G, Target.B, 0xFF);
    
    }
}
