using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a pixel with an alpha component, a red component, a green component, and a blue component.
    /// </summary>
    [Serializable]
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
            if (A == byte.MinValue)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
            this.A = (byte)(Alpha / 255);
        }

        public override int GetHashCode()
        {
            int hashCode = 1642429756;
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is BGRA v && B == v.B && G == v.G && R == v.R && A == v.A;

        public override string ToString()
            => $"B : {B}, G : {G}, R : {R}, A : {A}";

        public static implicit operator BGRA(RGB Target)
            => new(Target.B, Target.G, Target.R, byte.MaxValue);
        public static implicit operator BGRA(BGR Target)
            => new(Target.B, Target.G, Target.R, byte.MaxValue);
        public static implicit operator BGRA(ARGB Target)
            => new(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(ABGR Target)
            => new(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(RGBA Target)
            => new(Target.B, Target.G, Target.R, Target.A);
        public static implicit operator BGRA(Gray8 Target)
            => new(Target.Gray, Target.Gray, Target.Gray, byte.MaxValue);

        public static bool operator ==(BGRA This, BGRA Target)
            => This.Equals(Target);
        public static bool operator !=(BGRA This, BGRA Target)
            => !This.Equals(Target);

    }
}