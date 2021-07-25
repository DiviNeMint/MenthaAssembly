using System;

namespace MenthaAssembly.Media.Imaging
{
    public class ConvoluteKernel
    {
        internal readonly int[,] Datas;

        public int FactorSum { get; }

        public int Offset { get; }

        public int Height { get; }

        public int Width { get; }

        public int this[int X, int Y]
            => Datas[Y, X];

        public ConvoluteKernel(int[,] Datas, int KernelOffsetSum)
        {
            this.Datas = Datas;

            int HL = Datas.GetUpperBound(0) + 1,
                WL = Datas.GetUpperBound(1) + 1;

            if ((WL & 1) == 0)
                throw new InvalidOperationException("Kernel width must be odd!");

            if ((HL & 1) == 0)
                throw new InvalidOperationException("Kernel height must be odd!");

            int Sum = 0;
            for (int j = 0; j < HL; j++)
                for (int i = 0; i < WL; i++)
                    Sum += Datas[j, i];

            this.FactorSum = Math.Max(Sum, 1);
            this.Offset = KernelOffsetSum;

            this.Height = HL;
            this.Width = WL;
        }
        public ConvoluteKernel(int[,] Datas) : this(Datas, 0)
        {

        }

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

    }
}
