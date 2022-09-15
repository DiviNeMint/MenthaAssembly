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
            => new ConvoluteKernel(new float[,] {{-1,-1,-1},
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
        public static LaplacianEdgeKernel Edge_Laplacian { get; } = new LaplacianEdgeKernel(1);

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
             => new ConvoluteKernel(new float[,] {{ 0,-1, 0},
                                                  {-1, 5,-1},
                                                  { 0,-1, 0}});

        /// <summary>
        /// {-1,-1,-1}<para/>
        /// {-1, 9,-1}<para/>
        /// {-1,-1,-1}
        /// </summary>
        public static ConvoluteKernel Sharpen_3x3_2
             => new ConvoluteKernel(new float[,] {{-1,-1,-1},
                                                  {-1, 9,-1},
                                                  {-1,-1,-1}});

        /// <summary>
        /// {1, 1, 1}<para/>
        /// {1,-7, 1}<para/>
        /// {1, 1, 1}
        /// </summary>
        public static ConvoluteKernel Sharpen_3x3_3
             => new ConvoluteKernel(new float[,] {{1, 1, 1},
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
             => new ConvoluteKernel(new float[,] {{-1,-1,-1,-1,-1},
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
             => new ConvoluteKernel(new float[,] {{-2,-1, 0},
                                                  {-1, 1, 1},
                                                  { 0, 1, 2}});


        #endregion

        public virtual float[,] Matrix { get; }

        public int Width => PatchWidth;

        public int Height => PatchHeight;

        protected float KernelSum;
        protected int HalfWidth, HalfHeight;

        internal ConvoluteKernel() { }
        protected ConvoluteKernel(int Width, int Height)
        {
            if ((Width & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((Height & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            PatchWidth = Width;
            PatchHeight = Height;
        }
        public ConvoluteKernel(float[,] Datas)
        {
            int W = Datas.GetUpperBound(1) + 1,
                H = Datas.GetUpperBound(0) + 1;

            if ((W & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((H & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            float Sum = 0;
            for (int j = 0; j < Height; j++)
                for (int i = 0; i < Width; i++)
                    Sum += Datas[j, i];

            Matrix = Datas;
            PatchWidth = W;
            PatchHeight = H;
            KernelSum = Sum == 0f ? 1f : Sum;
            HalfWidth = W >> 1;
            HalfHeight = H >> 1;
        }

        public override void Filter(ImagePatch Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            float Tr = 0,
                  Tg = 0,
                  Tb = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    float k = Matrix[j, i];
                    if (k == 0)
                        continue;

                    IReadOnlyPixel Pixel = Patch[i, j];
                    Tr += Pixel.R * k;
                    Tg += Pixel.G * k;
                    Tb += Pixel.B * k;
                }
            }

            A = Patch[HalfWidth, HalfHeight].A;
            R = CalculateR(Tr);
            G = CalculateG(Tg);
            B = CalculateB(Tb);
        }

        protected virtual byte CalculateR(float FactorR)
            => (byte)MathHelper.Clamp(FactorR / KernelSum, 0f, 255f);
        protected virtual byte CalculateG(float FactorG)
            => (byte)MathHelper.Clamp(FactorG / KernelSum, 0f, 255f);
        protected virtual byte CalculateB(float FactorB)
            => (byte)MathHelper.Clamp(FactorB / KernelSum, 0f, 255f);

    }
}