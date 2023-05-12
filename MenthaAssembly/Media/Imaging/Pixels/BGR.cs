using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a pixel with  a red component, a green component, and a blue component.
    /// </summary>
    [NonAlpha]
    [Serializable]
    public struct BGR : IPixel
    {
        public byte B { set; get; }

        public byte G { set; get; }

        public byte R { set; get; }

        public byte A => byte.MaxValue;

        public int BitsPerPixel => 24;

        public BGR(byte B, byte G, byte R)
        {
            this.B = B;
            this.G = G;
            this.R = R;
        }

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.B = B;
            this.G = G;
            this.R = R;
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MinValue)
                return;

            int rA = 255 - A;
            this.B = (byte)((B * A + this.B * rA) / 255);
            this.G = (byte)((G * A + this.G * rA) / 255);
            this.R = (byte)((R * A + this.R * rA) / 255);
        }

        public override int GetHashCode()
        {
            int hashCode = 931614316;
            hashCode = hashCode * -1521134295 + B.GetHashCode();
            hashCode = hashCode * -1521134295 + G.GetHashCode();
            hashCode = hashCode * -1521134295 + R.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is BGR v && B == v.B && G == v.G && R == v.R;

        public override string ToString()
            => $"B : {B}, G : {G}, R : {R}";

        public static implicit operator BGR(RGB Target)
            => new(Target.B, Target.G, Target.R);
        public static implicit operator BGR(ABGR Target)
            => new(Target.B, Target.G, Target.R);
        public static implicit operator BGR(ARGB Target)
            => new(Target.B, Target.G, Target.R);
        public static implicit operator BGR(BGRA Target)
            => new(Target.B, Target.G, Target.R);
        public static implicit operator BGR(RGBA Target)
            => new(Target.B, Target.G, Target.R);
        public static implicit operator BGR(Gray8 Target)
            => new(Target.Gray, Target.Gray, Target.Gray);

        public static bool operator ==(BGR This, BGR Target)
            => This.Equals(Target);
        public static bool operator !=(BGR This, BGR Target)
            => !This.Equals(Target);

    }
}