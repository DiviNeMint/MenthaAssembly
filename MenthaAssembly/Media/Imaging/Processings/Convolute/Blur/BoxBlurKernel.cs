using System;

namespace MenthaAssembly.Media.Imaging
{
    public class BoxBlurKernel : ConvoluteKernel
    {
        public override float[,] Matrix { get; }

        /// <summary>
        /// Initializes a kernel of size (n * n) where n = 2 * <paramref name="Level"/> + 1.
        /// </summary>
        public BoxBlurKernel(int Level)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1;

            float[,] Kernel = new float[L, L];
            for (int i = 0; i < L; i++)
                for (int j = 0; j < L; j++)
                    Kernel[j, i] = 1f;

            Matrix = Kernel;
            base.PatchWidth = L;
            base.PatchHeight = L;
            KernelSum = L * L;
            HalfWidth = Level;
            HalfHeight = Level;
        }

        public override void Filter(ImagePatch Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            float Tr, Tg, Tb,
                  Lr, Lg, Lb;

            IReadOnlyPixel p;

            // Left
            Lr = Lg = Lb = 0;

            for (int j = 0; j < Height; j++)
            {
                p = Patch[0, j];
                Lr += p.R;
                Lg += p.G;
                Lb += p.B;
            }

            // Body
            if (Args.Handled)
            {
                Tr = Args.TokenR;
                Tg = Args.TokenG;
                Tb = Args.TokenB;

                int Index = Width - 1;
                for (int j = 0; j < Height; j++)
                {
                    p = Patch[Index, j];
                    Tr += p.R;
                    Tg += p.G;
                    Tb += p.B;
                }

                // Token
                Args.TokenR = Tr - Lr;
                Args.TokenG = Tg - Lg;
                Args.TokenB = Tb - Lb;
            }
            else
            {
                Tr = Tg = Tb = 0;
                for (int i = 1; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        p = Patch[i, j];
                        Tr += p.R;
                        Tg += p.G;
                        Tb += p.B;
                    }
                }

                // Token
                Args.TokenR = Tr;
                Args.TokenG = Tg;
                Args.TokenB = Tb;
                Args.Handled = true;

                // Merge
                Tr += Lr;
                Tg += Lg;
                Tb += Lb;
            }

            A = Patch[HalfWidth, HalfHeight].A;
            R = (byte)(Tr / KernelSum);
            G = (byte)(Tg / KernelSum);
            B = (byte)(Tb / KernelSum);
        }

    }
}