namespace MenthaAssembly.Media.Imaging
{
    public class ImagePatch<T>
        where T : unmanaged, IPixel
    {
        public T[][] Data0 { private set; get; }

        public byte[][] DataA { private set; get; }

        public byte[][] DataR { private set; get; }

        public byte[][] DataG { private set; get; }

        public byte[][] DataB { private set; get; }

        public int Width { get; }

        public int Height { get; }

        public ImagePatch(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        private int Index = 0;
        internal void Enqueue(T[] Data)
        {
            if (this.Data0 is null)
            {
                this.Data0 = new T[this.Width][];
                this.Data0[Index++] = Data;
                return;
            }

            if (Index < this.Width)
            {
                this.Data0[Index++] = Data;
            }
            else
            {
                int Last = this.Width - 1,
                    j;
                for (int i = 0; i < Last;)
                {
                    j = i + 1;
                    this.Data0[i] = this.Data0[j];
                    i = j;
                }

                this.Data0[Last] = Data;
            }
        }
        internal void Enqueue(byte[] DataR, byte[] DataG, byte[] DataB)
        {
            if (this.DataR is null)
            {
                this.DataR = new byte[this.Width][];
                this.DataG = new byte[this.Width][];
                this.DataB = new byte[this.Width][];

                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else if (Index < this.Width)
            {
                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else
            {
                int Last = this.Width - 1,
                    j;
                for (int i = 0; i < Last;)
                {
                    j = i + 1;
                    this.DataR[i] = this.DataR[j];
                    this.DataG[i] = this.DataG[j];
                    this.DataB[i] = this.DataB[j];

                    i = j;
                }

                this.DataR[Last] = DataR;
                this.DataG[Last] = DataG;
                this.DataB[Last] = DataB;
            }
        }
        internal void Enqueue(byte[] DataA, byte[] DataR, byte[] DataG, byte[] DataB)
        {
            if (this.DataA is null)
            {
                this.DataA = new byte[this.Width][];
                this.DataR = new byte[this.Width][];
                this.DataG = new byte[this.Width][];
                this.DataB = new byte[this.Width][];

                this.DataA[Index] = DataA;
                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else if (Index < this.Width)
            {
                this.DataA[Index] = DataA;
                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else
            {
                int Last = this.Width - 1,
                    j;
                for (int i = 0; i < Last;)
                {
                    j = i + 1;
                    this.DataA[i] = this.DataA[j];
                    this.DataR[i] = this.DataR[j];
                    this.DataG[i] = this.DataG[j];
                    this.DataB[i] = this.DataB[j];

                    i = j;
                }

                this.DataA[Last] = DataA;
                this.DataR[Last] = DataR;
                this.DataG[Last] = DataG;
                this.DataB[Last] = DataB;
            }
        }

    }
}
