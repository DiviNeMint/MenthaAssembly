namespace MenthaAssembly.Media.Imaging
{
    public unsafe static class PixelHelper
    {
        public static T ToPixel<T>(this IReadOnlyPixel Color)
            where T : unmanaged, IPixel
            => ToPixel<T>(Color.A, Color.R, Color.G, Color.B);

        public static T ToPixel<T>(byte A, byte R, byte G, byte B)
            where T : unmanaged, IPixel
        {
            T Pixel = default;
            Pixel.Override(A, R, G, B);
            return Pixel;
        }

        public static byte ToGray(this IReadOnlyPixel Pixel)
            => ToGray(Pixel.A, Pixel.R, Pixel.G, Pixel.B);
        public static byte ToGray(byte A, byte R, byte G, byte B)
            => (byte)((R * 30 + G * 59 + B * 11 + 50) * A / 25500);

        public static void Overlay(ref byte* pDestR, ref byte* pDestG, ref byte* pDestB, byte A, byte R, byte G, byte B)
        {
            int rA = 255 - A;
            *pDestR = (byte)((R * A + *pDestR * rA) / 255);
            *pDestG = (byte)((G * A + *pDestG * rA) / 255);
            *pDestB = (byte)((B * A + *pDestB * rA) / 255);
        }
        public static void Overlay(ref byte* pDestA, ref byte* pDestR, ref byte* pDestG, ref byte* pDestB, byte A, byte R, byte G, byte B)
        {
            int A1 = *pDestA,
                rA = 255 - A,
                Alpha = 65025 - rA * (255 - A1);

            *pDestA = (byte)(Alpha / 255);
            *pDestR = (byte)((R * A * 255 + *pDestR * A1 * rA) / Alpha);
            *pDestG = (byte)((G * A * 255 + *pDestG * A1 * rA) / Alpha);
            *pDestB = (byte)((B * A * 255 + *pDestB * A1 * rA) / Alpha);
        }

    }
}