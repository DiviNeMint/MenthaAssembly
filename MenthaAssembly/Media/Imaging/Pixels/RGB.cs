using System;

namespace MenthaAssembly.Media.Imaging
{
    [NonAlpha]
    [Serializable]
    public struct RGB : IPixel
    {
        public byte A => byte.MaxValue;

        public byte R { set; get; }

        public byte G { set; get; }

        public byte B { set; get; }

        public int BitsPerPixel => 24;

        public RGB(byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MinValue)
                return;

            int rA = 255 - A;
            this.R = (byte)((R * A + this.R * rA) / 255);
            this.G = (byte)((G * A + this.G * rA) / 255);
            this.B = (byte)((B * A + this.B * rA) / 255);
        }

        public override int GetHashCode()
        {
            int hashCode = -1520100960;
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is RGB v && R == v.R && G == v.G && B == v.B;

        public override string ToString()
            => $"R : {R}, G : {G}, B : {B}";

        public static implicit operator RGB(BGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator RGB(ARGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator RGB(ABGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator RGB(RGBA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator RGB(BGRA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator RGB(Gray8 Target)
            => new(Target.Gray, Target.Gray, Target.Gray);

        public static bool operator ==(RGB This, RGB Target)
            => This.Equals(Target);
        public static bool operator !=(RGB This, RGB Target)
            => !This.Equals(Target);

    }
}