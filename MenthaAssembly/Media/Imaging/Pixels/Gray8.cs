﻿namespace MenthaAssembly.Media.Imaging
{
    public struct Gray8 : IPixel
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
            this.Gray = (byte)((R * 30 + G * 59 + B * 11 + 50) / 100);
        }

        public static implicit operator Gray8(RGB Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(BGR Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(ARGB Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(ABGR Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(RGBA Target) => new Gray8(Target.R, Target.G, Target.B);
        public static implicit operator Gray8(BGRA Target) => new Gray8(Target.R, Target.G, Target.B);

    }
}