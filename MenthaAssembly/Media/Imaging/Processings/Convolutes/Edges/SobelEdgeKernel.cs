using System;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class SobelEdgeKernel : ConvoluteKernel
    {
        public override float[,] Matrix { get; }

        /// <summary>
        /// Initializes a kernel of size(n* n) where n = 2 * < paramref name="Level"/> + 1.
        /// </summary>
        public SobelEdgeKernel(int Level, Sides Side)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1,
                k = 1,
                Tx;

            for (int i = 2; i <= Level; i++)
                k = k * i / MathHelper.GCD(k, i);

            k <<= 1;

            for (int i = 2; i <= Level; i++)
            {
                Tx = i * i;
                for (int j = 1; j < i; j++)
                    k = MathHelper.LCM(k, j * j + Tx);
            }

            float[,] Kernel = new float[L, L];

            Action<int, int, float> AddValue = null;
            switch (Side)
            {
                case Sides.Left:
                    AddValue = (x, y, v) => Kernel[Level + y, Level - x] = v;
                    break;
                case Sides.Right:
                    AddValue = (x, y, v) => Kernel[Level + y, Level + x] = v;
                    break;
                case Sides.Top:
                    AddValue = (x, y, v) => Kernel[Level - x, Level + y] = v;
                    break;
                case Sides.Bottom:
                    AddValue = (x, y, v) => Kernel[Level + x, Level + y] = v;
                    break;
            }

            for (int i = -Level; i <= Level; i++)
            {
                Tx = i * i;
                for (int j = -Level; j <= Level; j++)
                {
                    if (Tx == 0 & j == 0)
                        continue;

                    AddValue(i, j, i * k / (Tx + j * j));
                }
            }

            Matrix = Kernel;
            base.PatchWidth = L;
            base.PatchHeight = L;
            HalfWidth = Level;
            HalfHeight = Level;
        }

        protected override byte CalculateR(float FactorR)
            => (byte)MathHelper.Clamp(FactorR, 0f, 255f);
        protected override byte CalculateG(float FactorG)
            => (byte)MathHelper.Clamp(FactorG, 0f, 255f);
        protected override byte CalculateB(float FactorB)
            => (byte)MathHelper.Clamp(FactorB, 0f, 255f);

    }
}
