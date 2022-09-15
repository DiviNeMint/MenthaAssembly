using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public class ConservativeSmoothingFilter : ImageFilter
    {
        public int Level { get; }

        public ConservativeSmoothingFilter(int Level)
        {
            this.Level = Level;

            int L = (Level << 1) + 1;
            PatchWidth = L;
            PatchHeight = L;
        }

        public override void Filter(ImagePatch Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            List<byte> DatasA = Args.ByteListA,
                       DatasR = Args.ByteListR,
                       DatasG = Args.ByteListG,
                       DatasB = Args.ByteListB;

            IReadOnlyPixel p;
            int W = PatchWidth,
                H = PatchHeight;

            // Datas
            for (int i = Args.Handled ? W - 1 : 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    p = Patch[i, j];
                    AddData(DatasA, p.A);
                    AddData(DatasR, p.R);
                    AddData(DatasG, p.G);
                    AddData(DatasB, p.B);
                }
            }

            int Length = DatasA.Count - 1,
                LastIndex = Length - 1;
            p = Patch[Level, Level];
            byte Ta = p.A,
                 Tr = p.R,
                 Tg = p.G,
                 Tb = p.B;

            A = Ta == DatasA[0] ? DatasA[1] : Ta == DatasA[Length] ? DatasA[LastIndex] : Ta;
            R = Tr == DatasR[0] ? DatasR[1] : Tr == DatasR[Length] ? DatasR[LastIndex] : Tr;
            G = Tg == DatasG[0] ? DatasG[1] : Tg == DatasG[Length] ? DatasG[LastIndex] : Tg;
            B = Tb == DatasB[0] ? DatasB[1] : Tb == DatasB[Length] ? DatasB[LastIndex] : Tb;

            // Remove Left
            for (int j = 0; j < H; j++)
            {
                p = Patch[0, j];
                DatasA.Remove(p.A);
                DatasR.Remove(p.R);
                DatasG.Remove(p.G);
                DatasB.Remove(p.B);
            }

            // Args
            Args.ByteListA = DatasA;
            Args.ByteListR = DatasR;
            Args.ByteListG = DatasG;
            Args.ByteListB = DatasB;
            Args.Handled = true;
        }

        private static void AddData(List<byte> Datas, byte Data)
        {
            int Count = Datas.Count;
            for (int i = 0; i < Count; i++)
            {
                if (Data < Datas[i])
                {
                    Datas.Insert(i, Data);
                    return;
                }
            }

            Datas.Add(Data);
        }

    }
}
