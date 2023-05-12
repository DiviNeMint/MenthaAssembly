using System;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
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
            if (A == byte.MinValue)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.A = (byte)(Alpha / 255);
            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
        }

        public override int GetHashCode()
        {
            int hashCode = 146518540;
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is ABGR v && A == v.A && B == v.B && G == v.G && R == v.R;

        public override string ToString()
            => $"A : {A}, B : {B}, G : {G}, R : {R}";

        public static implicit operator ABGR(RGB Target)
            => new(byte.MaxValue, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(BGR Target)
            => new(byte.MaxValue, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(RGBA Target)
            => new(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(BGRA Target)
            => new(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(ARGB Target)
            => new(Target.A, Target.B, Target.G, Target.R);
        public static implicit operator ABGR(Gray8 Target)
            => new(byte.MaxValue, Target.Gray, Target.Gray, Target.Gray);

        public static bool operator ==(ABGR This, ABGR Target)
            => This.Equals(Target);
        public static bool operator !=(ABGR This, ABGR Target)
            => !This.Equals(Target);

    }
}