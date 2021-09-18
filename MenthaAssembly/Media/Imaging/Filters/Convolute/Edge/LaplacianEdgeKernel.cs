using System;

namespace MenthaAssembly.Media.Imaging
{
    public class LaplacianEdgeKernel : ConvoluteKernel
    {
        public override float[,] Matrix { get; }

        /// <summary>
        /// Initializes a kernel of size(n* n) where n = 2 * < paramref name="Level"/> + 1.
        /// </summary>
        public LaplacianEdgeKernel(int Level)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int Length = Level << 1,
                L = Length + 1;

            float[,] Kernel = new float[L, L];
            float Sum = 0f;
            int t = Length - Level;
            for (int i = 0; i < Level; i++)
            {
                for (int j = i + 1; j <= Level; j++)
                {
                    float v = i == 0 ? 1 : MathHelper.Combination((j - 1) << 1, i);
                    int t1 = i - j;

                    Kernel[j, Level + t1] = v;
                    Kernel[Length - j, t - t1] = v;
                    Kernel[Level + t1, Length - j] = v;
                    Kernel[t - t1, j] = v;

                    Sum += v;
                }
            }
            Kernel[Level, Level] = -Sum * 4f;

            Matrix = Kernel;
            base.PatchWidth = L;
            base.PatchHeight = L;
            HalfWidth = Level;
            HalfHeight = Level;
        }

        protected override byte CalculateR(float FactorR)
            => (byte)FactorR;
        protected override byte CalculateG(float FactorG)
            => (byte)FactorG;
        protected override byte CalculateB(float FactorB)
            => (byte)FactorB;
    }
}
