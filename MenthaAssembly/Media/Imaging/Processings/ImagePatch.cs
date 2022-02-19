namespace MenthaAssembly.Media.Imaging
{
    public unsafe class ImagePatch<T>
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
            if (Data0 is null)
            {
                Data0 = new T[Width][];
                Data0[Index++] = (Data);
                return;
            }

            if (Index < Width)
            {
                Data0[Index++] = Data;
            }
            else
            {
                int Last = Width - 1,
                    j;
                for (int i = 0; i < Last;)
                {
                    j = i + 1;
                    Data0[i] = Data0[j];
                    i = j;
                }

                Data0[Last] = Data;
            }
        }
        internal void Enqueue(byte[] DataR, byte[] DataG, byte[] DataB)
        {
            if (this.DataR is null)
            {
                this.DataR = new byte[Width][];
                this.DataG = new byte[Width][];
                this.DataB = new byte[Width][];

                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else if (Index < Width)
            {
                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else
            {
                int Last = Width - 1,
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
                this.DataA = new byte[Width][];
                this.DataR = new byte[Width][];
                this.DataG = new byte[Width][];
                this.DataB = new byte[Width][];

                this.DataA[Index] = DataA;
                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else if (Index < Width)
            {
                this.DataA[Index] = DataA;
                this.DataR[Index] = DataR;
                this.DataG[Index] = DataG;
                this.DataB[Index] = DataB;

                Index++;
            }
            else
            {
                int Last = Width - 1,
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
