using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a pixel with an alpha component, a red component, a green component, and a blue component.
    /// </summary>
    [Serializable]
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
            if (A == byte.MinValue)
                return;

            int A1 = this.A,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            this.A = (byte)(Alpha / 255);
            this.R = (byte)((R * A * 255 + this.R * A1 * rA) / Alpha);
            this.G = (byte)((G * A * 255 + this.G * A1 * rA) / Alpha);
            this.B = (byte)((B * A * 255 + this.B * A1 * rA) / Alpha);
        }

        public override int GetHashCode()
        {
            int hashCode = -1749689076;
            hashCode = hashCode * -1521134295 + A.GetHashCode();
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is ARGB v && A == v.A && R == v.R && G == v.G && B == v.B;

        public override string ToString()
            => $"A : {A}, R : {R}, G : {G}, B : {B}";

        public static implicit operator ARGB(RGB Target)
            => new(byte.MaxValue, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(BGR Target)
            => new(byte.MaxValue, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(RGBA Target)
            => new(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(BGRA Target)
            => new(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(ABGR Target)
            => new(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator ARGB(Gray8 Target)
            => new(byte.MaxValue, Target.Gray, Target.Gray, Target.Gray);

        public static bool operator ==(ARGB This, ARGB Target)
            => This.Equals(Target);
        public static bool operator !=(ARGB This, ARGB Target)
            => !This.Equals(Target);

    }
}