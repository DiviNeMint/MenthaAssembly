namespace MenthaAssembly.Media.Imaging
{
    public struct Gray8 : ICalculatedPixel
    {
        public byte Gray { set; get; }

        public byte A => byte.MaxValue;

        public byte R => Gray;

        public byte G => Gray;

        public byte B => Gray;

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

        public override string ToString()
            => $"R : {R}, G : {G}, B : {B}";

        public static implicit operator Gray8(RGB Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(BGR Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(ARGB Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(ABGR Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(RGBA Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(BGRA Target) => new Gray8(Target.R, Target.G, Target.B);

    }
}
