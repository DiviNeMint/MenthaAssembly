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
            if (A == byte.MinValue)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
            this.A = (byte)(Alpha / 255);
        }

        public override int GetHashCode()
        {
            int hashCode = 1960784236;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is RGBA v && R == v.R && G == v.G && B == v.B && A == v.A;

        public override string ToString()
            => $"R : {R}, G : {G}, B : {B}, A : {A}";

        public static implicit operator RGBA(RGB Target)
            => new(Target.R, Target.G, Target.B, byte.MaxValue);
        public static implicit operator RGBA(BGR Target)
            => new(Target.R, Target.G, Target.B, byte.MaxValue);
        public static implicit operator RGBA(ARGB Target)
            => new(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(ABGR Target)
            => new(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(BGRA Target)
            => new(Target.R, Target.G, Target.B, Target.A);
        public static implicit operator RGBA(Gray8 Target)
            => new(Target.Gray, Target.Gray, Target.Gray, byte.MaxValue);

        public static bool operator ==(RGBA This, RGBA Target)
            => This.Equals(Target);
        public static bool operator !=(RGBA This, RGBA Target)
            => !This.Equals(Target);

    }
}