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

        public override void Filter<T>(T[][] Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            float Tr, Tg, Tb,
                  Lr, Lg, Lb;

            T[] Data;

            // Left
            Lr = Lg = Lb = 0;

            Data = Patch[0];
            for (int j = 0; j < Height; j++)
            {
                Lr += Data[j].R;
                Lg += Data[j].G;
                Lb += Data[j].B;
            }

            // Body
            if (Args.Handled)
            {
                Tr = Args.TokenR;
                Tg = Args.TokenG;
                Tb = Args.TokenB;

                int Index = Width - 1;
                Data = Patch[Index];
                for (int j = 0; j < Height; j++)
                {
                    Tr += Data[j].R;
                    Tg += Data[j].G;
                    Tb += Data[j].B;
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
                    Data = Patch[i];
                    for (int j = 0; j < Height; j++)
                    {
                        Tr += Data[j].R;
                        Tg += Data[j].G;
                        Tb += Data[j].B;
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

            A = Patch[HalfWidth][HalfHeight].A;
            R = (byte)(Tr / KernelSum);
            G = (byte)(Tg / KernelSum);
            B = (byte)(Tb / KernelSum);
        }
        public override void Filter3<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            float Tr, Tg, Tb,
                  Lr, Lg, Lb;

            byte[] DataR, DataG, DataB;

            // Left
            Lr = Lg = Lb = 0;

            DataR = Patch.DataR[0];
            DataG = Patch.DataG[0];
            DataB = Patch.DataB[0];
            for (int j = 0; j < Height; j++)
            {
                Lr += DataR[j];
                Lg += DataG[j];
                Lb += DataB[j];
            }

            // Body
            if (Args.Handled)
            {
                Tr = Args.TokenR;
                Tg = Args.TokenG;
                Tb = Args.TokenB;

                int Index = Width - 1;
                DataR = Patch.DataR[Index];
                DataG = Patch.DataG[Index];
                DataB = Patch.DataB[Index];
                for (int j = 0; j < Height; j++)
                {
                    Tr += DataR[j];
                    Tg += DataG[j];
                    Tb += DataB[j];
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
                    DataR = Patch.DataR[i];
                    DataG = Patch.DataG[i];
                    DataB = Patch.DataB[i];
                    for (int j = 0; j < Height; j++)
                    {
                        Tr += DataR[j];
                        Tg += DataG[j];
                        Tb += DataB[j];
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

            A = byte.MaxValue;
            R = (byte)(Tr / KernelSum);
            G = (byte)(Tg / KernelSum);
            B = (byte)(Tb / KernelSum);
        }
        public override void Filter4<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            float Tr, Tg, Tb,
                  Lr, Lg, Lb;

            byte[] DataR, DataG, DataB;

            // Left
            Lr = Lg = Lb = 0;

            DataR = Patch.DataR[0];
            DataG = Patch.DataG[0];
            DataB = Patch.DataB[0];
            for (int j = 0; j < Height; j++)
            {
                Lr += DataR[j];
                Lg += DataG[j];
                Lb += DataB[j];
            }

            // Body
            if (Args.Handled)
            {
                Tr = Args.TokenR;
                Tg = Args.TokenG;
                Tb = Args.TokenB;

                int Index = Width - 1;
                DataR = Patch.DataR[Index];
                DataG = Patch.DataG[Index];
                DataB = Patch.DataB[Index];
                for (int j = 0; j < Height; j++)
                {
                    Tr += DataR[j];
                    Tg += DataG[j];
                    Tb += DataB[j];
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
                    DataR = Patch.DataR[i];
                    DataG = Patch.DataG[i];
                    DataB = Patch.DataB[i];
                    for (int j = 0; j < Height; j++)
                    {
                        Tr += DataR[j];
                        Tg += DataG[j];
                        Tb += DataB[j];
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

            A = Patch.DataA[HalfWidth][HalfHeight];
            R = (byte)(Tr / KernelSum);
            G = (byte)(Tg / KernelSum);
            B = (byte)(Tb / KernelSum);
        }

    }
}
