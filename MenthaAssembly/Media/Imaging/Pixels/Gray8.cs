namespace MenthaAssembly.Media.Imaging
{
    [Calculated]
    public struct Gray8 : IPixel
    {
        public byte Gray { set; get; }

        byte IReadOnlyPixel.A => byte.MaxValue;

        byte IReadOnlyPixel.R => Gray;

        byte IReadOnlyPixel.G => Gray;

        byte IReadOnlyPixel.B => Gray;

        public int BitsPerPixel => 8;

        public Gray8(byte Gray)
        {
            this.Gray = Gray;
        }

        public Gray8(byte R, byte G, byte B)
        {
            Gray = PixelHelper.ToGray(byte.MaxValue, R, G, B);
        }

        void IPixel.Override(byte A, byte R, byte G, byte B)
            => Gray = PixelHelper.ToGray(A, R, G, B);

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
        {
            if (A == byte.MinValue)
                return;

            int rA = 255 - A;
            Gray = (byte)(((R * A + Gray * rA) * 30 +
                           (G * A + Gray * rA) * 59 +
                           (B * A + Gray * rA) * 11 + 50) / 25500);
        }

        public override int GetHashCode()
            => 1200207562 + Gray.GetHashCode();

        public override bool Equals(object obj)
            => obj is Gray8 v && Gray == v.Gray;

        public override string ToString()
            => $"R : {Gray}, G : {Gray}, B : {Gray}";

        public static implicit operator Gray8(RGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(BGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(ARGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(ABGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(RGBA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(BGRA Target)
            => new(Target.R, Target.G, Target.B);

        public static bool operator ==(Gray8 This, Gray8 Target)
            => This.Equals(Target);
        public static bool operator !=(Gray8 This, Gray8 Target)
            => !This.Equals(Target);

    }
}