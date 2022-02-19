using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public class MedianFilter : ImageFilter
    {

        public int Level { get; }

        public MedianFilter(int Level)
        {
            this.Level = Level;

            int L = (Level << 1) + 1;
            base.PatchWidth = L;
            base.PatchHeight = L;
        }

        public override void Filter<T>(T[][] Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            List<byte> DatasA = Args.ByteListA,
                       DatasR = Args.ByteListR,
                       DatasG = Args.ByteListG,
                       DatasB = Args.ByteListB;

            T[] Data;
            int W = base.PatchWidth,
                H = base.PatchHeight;

            // Datas
            for (int i = Args.Handled ? W - 1 : 0; i < W; i++)
            {
                Data = Patch[i];
                for (int j = 0; j < H; j++)
                {
                    T p = Data[j];
                    AddData(DatasA, p.A);
                    AddData(DatasR, p.R);
                    AddData(DatasG, p.G);
                    AddData(DatasB, p.B);
                }
            }

            int Index = (W * H) << 1;
            A = DatasA[Index];
            R = DatasR[Index];
            G = DatasG[Index];
            B = DatasB[Index];

            // Remove Left
            Data = Patch[0];
            for (int j = 0; j < H; j++)
            {
                T p = Data[j];
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
        public override void Filter3<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            List<byte> DatasR = Args.ByteListR,
                       DatasG = Args.ByteListG,
                       DatasB = Args.ByteListB;

            byte[] DataR, DataG, DataB;
            int W = base.PatchWidth,
                H = base.PatchHeight;

            // Datas
            for (int i = Args.Handled ? W - 1 : 0; i < W; i++)
            {
                DataR = Patch.DataR[i];
                DataG = Patch.DataG[i];
                DataB = Patch.DataB[i];
                for (int j = 0; j < H; j++)
                {
                    AddData(DatasR, DataR[j]);
                    AddData(DatasG, DataG[j]);
                    AddData(DatasB, DataB[j]);
                }
            }

            int Index = Level;
            A = Patch.DataA[Index][Index];

            Index = (W * H) << 1;
            R = DatasR[Index];
            G = DatasG[Index];
            B = DatasB[Index];

            // Remove Left
            DataR = Patch.DataR[0];
            DataG = Patch.DataG[0];
            DataB = Patch.DataB[0];
            for (int j = 0; j < H; j++)
            {
                DatasR.Remove(DataR[j]);
                DatasG.Remove(DataG[j]);
                DatasB.Remove(DataB[j]);
            }

            // Args
            Args.ByteListR = DatasR;
            Args.ByteListG = DatasG;
            Args.ByteListB = DatasB;
            Args.Handled = true;
        }
        public override void Filter4<T>(ImagePatch<T> Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B)
        {
            List<byte> DatasA = Args.ByteListA,
                       DatasR = Args.ByteListR,
                       DatasG = Args.ByteListG,
                       DatasB = Args.ByteListB;

            byte[] DataA, DataR, DataG, DataB;
            int W = base.PatchWidth,
                H = base.PatchHeight;

            // Datas
            for (int i = Args.Handled ? W - 1 : 0; i < W; i++)
            {
                DataA = Patch.DataA[i];
                DataR = Patch.DataR[i];
                DataG = Patch.DataG[i];
                DataB = Patch.DataB[i];
                for (int j = 0; j < H; j++)
                {
                    AddData(DatasA, DataA[j]);
                    AddData(DatasR, DataR[j]);
                    AddData(DatasG, DataG[j]);
                    AddData(DatasB, DataB[j]);
                }
            }

            int Index = (W * H) << 1;
            A = DatasA[Index];
            R = DatasR[Index];
            G = DatasG[Index];
            B = DatasB[Index];

            // Remove Left
            DataA = Patch.DataA[0];
            DataR = Patch.DataR[0];
            DataG = Patch.DataG[0];
            DataB = Patch.DataB[0];
            for (int j = 0; j < H; j++)
            {
                DatasA.Remove(DataA[j]);
                DatasR.Remove(DataR[j]);
                DatasG.Remove(DataG[j]);
                DatasB.Remove(DataB[j]);
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
