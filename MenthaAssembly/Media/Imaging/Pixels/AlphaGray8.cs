using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a pixel with grayscale and alpha.<para/>
    /// Grayscale = R * 0.299 + G * 0.587 + B * 0.114
    /// </summary>
    [Calculated]
    [Serializable]
    public struct AlphaGray8 : IPixel
    {
        /// <summary>
        /// Gets the grayscale for this pixel.
        /// </summary>
        public byte A { set; get; }

        /// <summary>
        /// Gets the grayscale for this pixel.
        /// </summary>
        public byte Gray { set; get; }

        readonly byte IReadOnlyPixel.R => Gray;

        readonly byte IReadOnlyPixel.G => Gray;

        readonly byte IReadOnlyPixel.B => Gray;

        public readonly int BitsPerPixel => 16;

        public AlphaGray8(byte Alpha, byte Gray)
        {
            A = Alpha;
            this.Gray = Gray;
        }

        public AlphaGray8(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            Gray = PixelHelper.ToGray(byte.MaxValue, R, G, B);
        }

        void IPixel.Override(byte A, byte R, byte G, byte B)
        {
            this.A = A;
            Gray = PixelHelper.ToGray(byte.MaxValue, R, G, B);
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MinValue)
                return;

            int rA = 255 - A;
            Gray = (byte)(((R * A + Gray * rA) * 30 +
                           (G * A + Gray * rA) * 59 +
                           (B * A + Gray * rA) * 11 + 50) / 25500);
        }

        public override readonly int GetHashCode()
            => 1200207562 + Gray.GetHashCode();

        public override readonly bool Equals(object obj)
            => obj is AlphaGray8 v && Gray == v.Gray;

        public override readonly string ToString()
            => $"A : {A}, R : {Gray}, G : {Gray}, B : {Gray}";

        public static implicit operator AlphaGray8(RGB Target)
            => new(byte.MaxValue, Target.R, Target.G, Target.B);
        public static implicit operator AlphaGray8(BGR Target)
            => new(byte.MaxValue, Target.R, Target.G, Target.B);
        public static implicit operator AlphaGray8(ARGB Target)
            => new(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator AlphaGray8(ABGR Target)
            => new(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator AlphaGray8(RGBA Target)
            => new(Target.A, Target.R, Target.G, Target.B);
        public static implicit operator AlphaGray8(BGRA Target)
            => new(Target.A, Target.R, Target.G, Target.B);

        public static bool operator ==(AlphaGray8 This, AlphaGray8 Target)
            => This.Equals(Target);
        public static bool operator !=(AlphaGray8 This, AlphaGray8 Target)
            => !This.Equals(Target);

    }
}