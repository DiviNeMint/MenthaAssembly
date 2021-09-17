using System;

namespace MenthaAssembly.Media.Imaging
{
    public class ConvoluteKernel : ImageFilter
    {
        #region Edge Detection
        /// <summary>
        /// {-1,-1,-1}<para/>
        /// {-1, 8,-1}<para/>
        /// {-1,-1,-1}
        /// </summary>
        public static ConvoluteKernel Edge_Detection
           => new ConvoluteKernel(new int[,] {{-1,-1,-1},
                                              {-1, 8,-1},
                                              {-1,-1,-1}});

        /// <summary>
        /// { 1, 2, 1}<para/>
        /// { 0, 0, 0}<para/>
        /// {-1,-2,-1}
        /// </summary>
        public static SobelEdgeKernel Edge_Sobel_Top { get; } = new SobelEdgeKernel(1, Sides.Top);

        /// <summary>
        /// { 1, 0,-1}<para/>
        /// { 2, 0,-2}<para/>
        /// { 1, 0,-1}
        /// </summary>
        public static SobelEdgeKernel Edge_Sobel_Left { get; } = new SobelEdgeKernel(1, Sides.Left);

        /// <summary>
        /// {-1,-2,-1}<para/>
        /// { 0, 0, 0}<para/>
        /// { 1, 2, 1}
        /// </summary>
        public static SobelEdgeKernel Edge_Sobel_Bottom { get; } = new SobelEdgeKernel(1, Sides.Bottom);

        /// <summary>
        /// {-1, 0, 1}<para/>
        /// {-2, 0, 2}<para/>
        /// {-1, 0, 1}
        /// </summary>
        public static SobelEdgeKernel Edge_Sobel_Right { get; } = new SobelEdgeKernel(1, Sides.Right);

        /// <summary>
        /// {0,-1,0}<para/>
        /// {-1, 4,-1}<para/>
        /// {0,-1,0}
        /// </summary>
        public static ConvoluteKernel Edge_Laplacian3x3
             => new ConvoluteKernel(new int[,] {{ 0,-1, 0},
                                                {-1, 4,-1},
                                                { 0,-1, 0}});

        /// <summary>
        /// { 0, 0,-1, 0, 0}<para/>
        /// { 0,-1,-2,-1, 0}<para/>
        /// {-1,-2,16,-2,-1}<para/>
        /// { 0,-1,-2,-1, 0}<para/>
        /// { 0, 0,-1, 0, 0}
        /// </summary>
        public static ConvoluteKernel Edge_Laplacian5x5
             => new ConvoluteKernel(new int[,] {{ 0, 0,-1, 0, 0},
                                                { 0,-1,-2,-1, 0},
                                                {-1,-2,16,-2,-1},
                                                { 0,-1,-2,-1, 0},
                                                { 0, 0,-1, 0, 0}});

        #endregion

        #region Blur
        /// <summary>
        /// {1, 1, 1}<para/>
        /// {1, 1, 1}<para/>
        /// {1, 1, 1}
        /// </summary>
        public static BoxBlurKernel Blur_Box { get; } = new BoxBlurKernel(1);

        /// <summary>
        /// {1, 2, 1}<para/>
        /// {2, 4, 2}<para/>
        /// {1, 2, 1}
        /// </summary>
        public static GaussianBlurKernel Blur_Gaussian { get; } = new GaussianBlurKernel(1);

        #endregion

        #region Sharpen
        /// <summary>
        /// { 0,-1, 0}<para/>
        /// {-1, 5,-1}<para/>
        /// { 0,-1, 0}
        /// </summary>
        public static ConvoluteKernel Sharpen_3x3_1
             => new ConvoluteKernel(new int[,] {{ 0,-1, 0},
                                                {-1, 5,-1},
                                                { 0,-1, 0}});

        /// <summary>
        /// {-1,-1,-1}<para/>
        /// {-1, 9,-1}<para/>
        /// {-1,-1,-1}
        /// </summary>
        public static ConvoluteKernel Sharpen_3x3_2
             => new ConvoluteKernel(new int[,] {{-1,-1,-1},
                                                {-1, 9,-1},
                                                {-1,-1,-1}});

        /// <summary>
        /// {1, 1, 1}<para/>
        /// {1,-7, 1}<para/>
        /// {1, 1, 1}
        /// </summary>
        public static ConvoluteKernel Sharpen_3x3_3
             => new ConvoluteKernel(new int[,] {{1, 1, 1},
                                                {1,-7, 1},
                                                {1, 1, 1}});

        /// <summary>
        /// {-1,-1,-1,-1,-1}<para/>
        /// {-1, 2, 2, 2,-1}<para/>
        /// {-1, 2, 8, 2,-1}<para/>
        /// {-1, 2, 2, 2,-1}<para/>
        /// {-1,-1,-1,-1,-1}
        /// </summary>
        public static ConvoluteKernel Sharpen_5x5
             => new ConvoluteKernel(new int[,] {{-1,-1,-1,-1,-1},
                                                {-1, 2, 2, 2,-1},
                                                {-1, 2, 8, 2,-1},
                                                {-1, 2, 2, 2,-1},
                                                {-1,-1,-1,-1,-1}});

        #endregion

        #region Other
        /// <summary>
        /// {-2,-1, 0}<para/>
        /// {-1, 1, 1}<para/>
        /// { 0, 1, 2}
        /// </summary>
        public static ConvoluteKernel Emboss
             => new ConvoluteKernel(new int[,] {{-2,-1, 0},
                                                {-1, 1, 1},
                                                { 0, 1, 2}});


        #endregion

        public virtual int[,] Kernel { get; }

        public override int KernelWidth { get; }

        public override int KernelHeight { get; }

        public virtual int KernelSum { get; }

        public virtual int Offset { get; }

        protected int HalfWidth, HalfHeight;

        protected ConvoluteKernel() { }
        public ConvoluteKernel(int[,] Datas, int Offset)
        {
            int W = Datas.GetUpperBound(1) + 1,
                H = Datas.GetUpperBound(0) + 1;

            if ((W & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((H & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            int Sum = 0;
            for (int j = 0; j < KernelHeight; j++)
                for (int i = 0; i < KernelWidth; i++)
                    Sum += Datas[j, i];

            Kernel = Datas;
            KernelWidth = W;
            KernelHeight = H;
            KernelSum = Math.Max(Sum, 1);
            this.Offset = Offset;
            HalfWidth = W >> 1;
            HalfHeight = H >> 1;
        }
        public ConvoluteKernel(int[,] Datas) : this(Datas, 0)
        {

        }

        public override void Filter<T>(T[][] Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr = 0,
                Tg = 0,
                Tb = 0;
            T[] Data;
            for (int i = 0; i < KernelWidth; i++)
            {
                Data = Patch[i];
                for (int j = 0; j < KernelHeight; j++)
                {
                    int k = Kernel[j, i];
                    if (k == 0)
                        continue;

                    T Pixel = Data[j];

                    Tr += Pixel.R * k;
                    Tg += Pixel.G * k;
                    Tb += Pixel.B * k;
                }
            }

            A = Patch[HalfWidth][HalfHeight].A;
            R = (byte)MathHelper.Clamp(Tr / KernelSum + Offset, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / KernelSum + Offset, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / KernelSum + Offset, 0, 255);
        }
        public override void Filter3<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr = 0,
                Tg = 0,
                Tb = 0;

            byte[] DataR, DataG, DataB;
            for (int i = 0; i < KernelWidth; i++)
            {
                DataR = Patch.DataR[i];
                DataG = Patch.DataG[i];
                DataB = Patch.DataB[i];
                for (int j = 0; j < KernelHeight; j++)
                {
                    int k = Kernel[j, i];
                    if (k == 0)
                        continue;

                    Tr += DataR[j] * k;
                    Tg += DataG[j] * k;
                    Tb += DataB[j] * k;
                }
            }

            A = byte.MaxValue;
            R = (byte)MathHelper.Clamp(Tr / KernelSum + Offset, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / KernelSum + Offset, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / KernelSum + Offset, 0, 255);
        }
        public override void Filter4<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr = 0,
                Tg = 0,
                Tb = 0;

            byte[] DataR, DataG, DataB;
            for (int i = 0; i < KernelWidth; i++)
            {
                DataR = Patch.DataR[i];
                DataG = Patch.DataG[i];
                DataB = Patch.DataB[i];
                for (int j = 0; j < KernelHeight; j++)
                {
                    int k = Kernel[j, i];
                    if (k == 0)
                        continue;

                    Tr += DataR[j] * k;
                    Tg += DataG[j] * k;
                    Tb += DataB[j] * k;
                }
            }

            A = Patch.DataA[HalfWidth][HalfHeight];
            R = (byte)MathHelper.Clamp(Tr / KernelSum + Offset, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / KernelSum + Offset, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / KernelSum + Offset, 0, 255);
        }

    }
}