namespace MenthaAssembly.Media.Imaging
{
    public abstract class ImageFilter
    {
        public int KernelWidth { get; }

        public int KernelHeight { get; }

        protected ImageFilter(int KernelWidth, int KernelHeight)
        {
            this.KernelWidth = KernelWidth;
            this.KernelHeight = KernelHeight;
        }

        public abstract void Filter<T>(T[][] Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
            where T : unmanaged, IPixel;

        /// <summary>
        /// Filter the 3 channel patch.
        /// </summary>
        public virtual void Filter3<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
            where T : unmanaged, IPixel
        {
            int W = Patch.Width,
                H = Patch.Height;
            T[][] Block = new T[W][];
            byte[][] RBlock = Patch.DataR,
                     GBlock = Patch.DataG,
                     BBlock = Patch.DataB;

            for (int i = 0; i < W; i++)
            {
                T[] Temp = new T[H];
                byte[] TempR = RBlock[i],
                       TempG = GBlock[i],
                       TempB = BBlock[i];
                for (int j = 0; j < H; j++)
                    Temp[j] = PixelHelper.ToPixel<T>(byte.MaxValue, TempR[j], TempG[j], TempB[j]);

                Block[i] = Temp;
            }

            this.Filter(Block, Args, out A, out R, out G, out B);
        }
        /// <summary>
        /// Filter the 4 channel patch.
        /// </summary>
        public virtual void Filter4<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
            where T : unmanaged, IPixel
        {
            int W = Patch.Width,
                H = Patch.Height;
            T[][] Block = new T[W][];
            byte[][] ABlock = Patch.DataA,
                     RBlock = Patch.DataR,
                     GBlock = Patch.DataG,
                     BBlock = Patch.DataB;

            for (int i = 0; i < W; i++)
            {
                T[] Temp = new T[H];
                byte[] TempA = ABlock[i],
                       TempR = RBlock[i],
                       TempG = GBlock[i],
                       TempB = BBlock[i];
                for (int j = 0; j < H; j++)
                    Temp[j] = PixelHelper.ToPixel<T>(TempA[j], TempR[j], TempG[j], TempB[j]);

                Block[i] = Temp; 
            }

            this.Filter(Block, Args, out A, out R, out G, out B);
        }

    }
}
