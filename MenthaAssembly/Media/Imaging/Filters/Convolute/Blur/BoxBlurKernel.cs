using System;

namespace MenthaAssembly.Media.Imaging
{
    public class BoxBlurKernel : ConvoluteKernel
    {
        public override int[,] Kernel { get; }

        public override int KernelWidth { get; }

        public override int KernelHeight { get; }

        public override int KernelSum { get; }

        /// <summary>
        /// Initializes a kernel of size (n * n) where n =2 * <paramref name="Level"/> + 1.
        /// </summary>
        public BoxBlurKernel(int Level)
        {
            if (Level < 1)
                throw new ArgumentException(@$"Argument ""level"" can't less than 1.");

            int L = (Level << 1) + 1;

            int[,] Kernel = new int[L, L];
            for (int i = 0; i < L; i++)
                for (int j = 0; j < L; j++)
                    Kernel[j, i] = 1;

            this.Kernel = Kernel;
            KernelWidth = L;
            KernelHeight = L;
            KernelSum = L * L;
        }

        public override void Filter<T>(T[][] Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr, Tg, Tb,
                Lr, Lg, Lb;

            T[] Data;

            // Left
            Lr = Lg = Lb = 0;

            Data = Patch[0];
            for (int j = 0; j < KernelHeight; j++)
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

                int Index = KernelWidth - 1;
                Data = Patch[Index];
                for (int j = 0; j < KernelHeight; j++)
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
                for (int i = 1; i < KernelWidth; i++)
                {
                    Data = Patch[i];
                    for (int j = 0; j < KernelHeight; j++)
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

            A = Patch[KernelWidth >> 1][KernelHeight >> 1].A;
            R = (byte)MathHelper.Clamp(Tr / KernelSum, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / KernelSum, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / KernelSum, 0, 255);
        }
        public override void Filter3<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr, Tg, Tb,
                Lr, Lg, Lb;

            byte[] DataR, DataG, DataB;

            // Left
            Lr = Lg = Lb = 0;

            DataR = Patch.DataR[0];
            DataG = Patch.DataG[0];
            DataB = Patch.DataB[0];
            for (int j = 0; j < KernelHeight; j++)
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

                int Index = KernelWidth - 1;
                DataR = Patch.DataR[Index];
                DataG = Patch.DataG[Index];
                DataB = Patch.DataB[Index];
                for (int j = 0; j < KernelHeight; j++)
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
                for (int i = 1; i < KernelWidth; i++)
                {
                    DataR = Patch.DataR[i];
                    DataG = Patch.DataG[i];
                    DataB = Patch.DataB[i];
                    for (int j = 0; j < KernelHeight; j++)
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
            R = (byte)MathHelper.Clamp(Tr / KernelSum, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / KernelSum, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / KernelSum, 0, 255);
        }
        public override void Filter4<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            int Tr, Tg, Tb,
                Lr, Lg, Lb;

            byte[] DataR, DataG, DataB;

            // Left
            Lr = Lg = Lb = 0;

            DataR = Patch.DataR[0];
            DataG = Patch.DataG[0];
            DataB = Patch.DataB[0];
            for (int j = 0; j < KernelHeight; j++)
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

                int Index = KernelWidth - 1;
                DataR = Patch.DataR[Index];
                DataG = Patch.DataG[Index];
                DataB = Patch.DataB[Index];
                for (int j = 0; j < KernelHeight; j++)
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
                for (int i = 1; i < KernelWidth; i++)
                {
                    DataR = Patch.DataR[i];
                    DataG = Patch.DataG[i];
                    DataB = Patch.DataB[i];
                    for (int j = 0; j < KernelHeight; j++)
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

            A = Patch.DataA[KernelWidth >> 1][KernelHeight >> 1];
            R = (byte)MathHelper.Clamp(Tr / KernelSum, 0, 255);
            G = (byte)MathHelper.Clamp(Tg / KernelSum, 0, 255);
            B = (byte)MathHelper.Clamp(Tb / KernelSum, 0, 255);
        }

    }
}
