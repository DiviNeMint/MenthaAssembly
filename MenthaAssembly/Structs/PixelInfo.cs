namespace MenthaAssembly
{
    public struct PixelInfo
    {
        public static PixelInfo Empty { get; } = new PixelInfo(-1, -1, 0, 0, 0, 0);

        public int X { get; }

        public int Y { get; }

        public byte A { get; }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public bool IsEmpty
            => X is -1 && 
               Y is -1 && 
               A is 0 && 
               R is 0 && 
               G is 0 && 
               B is 0;

        public PixelInfo(int X, int Y, int PixelData) : this(X, Y, (byte)(PixelData >> 24), (byte)(PixelData >> 16), (byte)(PixelData >> 8), (byte)PixelData)
        {
        }
        public PixelInfo(int X, int Y, byte A, byte R, byte G, byte B)
        {
            this.X = X;
            this.Y = Y;
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public override string ToString()
            => $"X : {X}, Y : {Y}, A : {A}, R : {R}, G : {G}, B : {B}";
    }
}
