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
        public static ConvoluteKernel EdgeDetection
           => new ConvoluteKernel(new int[,] {{-1,-1,-1},
                                              {-1, 8,-1},
                                              {-1,-1,-1}});

        /// <summary>
        /// { 1, 2, 1}<para/>
        /// { 0, 0, 0}<para/>
        /// {-1,-2,-1}
        /// </summary>
        public static ConvoluteKernel EdgeDetection_Sobel_Top
           => new ConvoluteKernel(new int[,] {{ 1, 2, 1},
                                              { 0, 0, 0},
                                              {-1,-2,-1}});

        /// <summary>
        /// { 1, 0,-1}<para/>
        /// { 2, 0,-2}<para/>
        /// { 1, 0,-1}
        /// </summary>
        public static ConvoluteKernel EdgeDetection_Sobel_Left
           => new ConvoluteKernel(new int[,] {{ 1, 0,-1},
                                              { 2, 0,-2},
                                              { 1, 0,-1}});

        /// <summary>
        /// {-1,-2,-1}<para/>
        /// { 0, 0, 0}<para/>
        /// { 1, 2, 1}
        /// </summary>
        public static ConvoluteKernel EdgeDetection_Sobel_Bottom
           => new ConvoluteKernel(new int[,] {{-1,-2,-1},
                                              { 0, 0, 0},
                                              { 1, 2, 1}});

        /// <summary>
        /// {-1, 0, 1}<para/>
        /// {-2, 0, 2}<para/>
        /// {-1, 0, 1}
        /// </summary>
        public static ConvoluteKernel EdgeDetection_Sobel_Right
           => new ConvoluteKernel(new int[,] {{-1, 0, 1},
                                              {-2, 0, 2},
                                              {-1, 0, 1}});

        /// <summary>
        /// {0,-1,0}<para/>
        /// {-1, 4,-1}<para/>
        /// {0,-1,0}
        /// </summary>
        public static ConvoluteKernel EdgeDetection_Laplacian3x3
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
        public static ConvoluteKernel EdgeDetection_Laplacian5x5
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
        public static ConvoluteKernel Blur_Box
             => new ConvoluteKernel(new int[,] {{1, 1, 1},
                                                {1, 1, 1},
                                                {1, 1, 1}});

        /// <summary>
        /// {1, 2, 1}<para/>
        /// {2, 4, 2}<para/>
        /// {1, 2, 1}
        /// </summary>
        public static ConvoluteKernel Blur_Gaussian3x3
             => new ConvoluteKernel(new int[,] {{1, 2, 1},
                                                {2, 4, 2},
                                                {1, 2, 1}});

        /// <summary>
        /// {1,  4,  6,  4, 1}<para/>
        /// {4, 16, 24, 16, 4}<para/>
        /// {6, 24, 36, 24, 6}<para/>
        /// {4, 16, 24, 16, 4}<para/>
        /// {1,  4,  6,  4, 1}
        /// </summary>
        public static ConvoluteKernel Blur_Gaussian5x5
             => new ConvoluteKernel(new int[,] {{1,  4,  6,  4, 1},
                                                {4, 16, 24, 16, 4},
                                                {6, 24, 36, 24, 6},
                                                {4, 16, 24, 16, 4},
                                                {1,  4,  6,  4, 1}});

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

        public int[,] Kernel { get; }

        public int KernelSum { get; }

        public int Offset { get; }

        public ConvoluteKernel(int[,] Datas, int KernelOffsetSum) : base(Datas.GetUpperBound(1) + 1, Datas.GetUpperBound(0) + 1)
        {
            this.Kernel = Datas;

            if ((this.KernelWidth & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((this.KernelHeight & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            int Sum = 0;
            for (int j = 0; j < this.KernelHeight; j++)
                for (int i = 0; i < this.KernelWidth; i++)
                    Sum += Datas[j, i];

            this.KernelSum = Math.Max(Sum, 1);
            this.Offset = KernelOffsetSum;

        }
        public ConvoluteKernel(int[,] Datas) : this(Datas, 0)
        {

        }

        public override void Filter<T>(T[][] Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Ta = 0,
                Tr = 0,
                Tg = 0,
                Tb = 0;
            for (int i = 0; i < this.KernelWidth; i++)
            {
                T[] Temp = Patch[i];
                for (int j = 0; j < this.KernelHeight; j++)
                {
                    int k = this.Kernel[j, i];
                    if (k == 0)
                        continue;

                    T Pixel = Temp[j];

                    Ta += Pixel.A * k;
                    Tr += Pixel.R * k;
                    Tg += Pixel.G * k;
                    Tb += Pixel.B * k;
                }
            }

            A = (byte)MathHelper.Clamp(Ta / this.KernelSum + this.Offset, 0, 255);
            R = (byte)MathHelper.Clamp(Tr / this.KernelSum + this.Offset, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / this.KernelSum + this.Offset, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / this.KernelSum + this.Offset, 0, 255);
        }
        public override void Filter3<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr = 0,
                Tg = 0,
                Tb = 0;
            for (int i = 0; i < this.KernelWidth; i++)
            {
                byte[] TempR = Patch.DataR[i],
                       TempG = Patch.DataG[i],
                       TempB = Patch.DataB[i];
                for (int j = 0; j < this.KernelHeight; j++)
                {
                    int k = this.Kernel[j, i];
                    if (k == 0)
                        continue;

                    Tr += TempR[j] * k;
                    Tg += TempG[j] * k;
                    Tb += TempB[j] * k;
                }
            }

            A = byte.MaxValue;
            R = (byte)MathHelper.Clamp(Tr / this.KernelSum + this.Offset, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / this.KernelSum + this.Offset, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / this.KernelSum + this.Offset, 0, 255);
        }
        public override void Filter4<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Ta = 0,
                Tr = 0,
                Tg = 0,
                Tb = 0;
            for (int i = 0; i < this.KernelWidth; i++)
            {
                byte[] TempA = Patch.DataA[i],
                       TempR = Patch.DataR[i],
                       TempG = Patch.DataG[i],
                       TempB = Patch.DataB[i];
                for (int j = 0; j < this.KernelHeight; j++)
                {
                    int k = this.Kernel[j, i];
                    if (k == 0)
                        continue;

                    Ta += TempA[j] * k;
                    Tr += TempR[j] * k;
                    Tg += TempG[j] * k;
                    Tb += TempB[j] * k;
                }
            }

            A = (byte)MathHelper.Clamp(Ta / this.KernelSum + this.Offset, 0, 255);
            R = (byte)MathHelper.Clamp(Tr / this.KernelSum + this.Offset, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / this.KernelSum + this.Offset, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / this.KernelSum + this.Offset, 0, 255);
        }

    }
}
