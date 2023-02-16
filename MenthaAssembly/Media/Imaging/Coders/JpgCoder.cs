using System.IO;

namespace MenthaAssembly.Media.Imaging
{
    // https://github.com/corkami/formats/blob/master/image/JPEGRGB_dissected.png
    public static unsafe class JpgCoder
    {
        public const int IdentifyHeaderSize = 2;

        public static bool TryGetImageSize(string FilePath, out int Width, out int Height)
        {
            using Stream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            return TryGetImageSize(Stream, out Width, out Height);
        }
        public static bool TryGetImageSize(Stream Stream, out int Width, out int Height)
        {
            byte[] Datas = new byte[IdentifyHeaderSize];

            Stream.Read(Datas, 0, Datas.Length);

            // Identify
            if (!Identify(Datas))
            {
                Width = 0;
                Height = 0;
                return false;
            }

            byte[] LengthData = new byte[2];
            while (Stream.Position < Stream.Length)
            {
                Stream.Read(Datas, 0, Datas.Length);
                if (Datas[0] != 0xFF)
                {
                    Width = 0;
                    Height = 0;
                    return false;
                }

                // Length
                Stream.Read(LengthData, 0, LengthData.Length);
                int Length = LengthData[0] << 8 | LengthData[1];

                // Check Start of Frame
                if (Datas[1] != 0xC0)
                {
                    Stream.Seek(Length - LengthData.Length, SeekOrigin.Current);
                    continue;
                }

                Datas = new byte[5];
                Stream.Read(Datas, 0, Datas.Length);

                Height = Datas[1] << 8 | Datas[2];
                Width = Datas[3] << 8 | Datas[4];

                return true;
            }

            Width = 0;
            Height = 0;
            return false;
        }

        public static bool Identify(byte[] Data)
            => Data.Length >= IdentifyHeaderSize && Data[0].Equals(0xFF) && Data[1].Equals(0xD8);

    }
}