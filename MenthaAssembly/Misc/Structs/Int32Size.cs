namespace System.Windows
{
    public struct Int32Size
    {
        public static Int32Size Empty => new Int32Size();

        public int Width { get; }

        public int Height { get; }

        public bool IsEmpty => Width.Equals(0) || Height.Equals(0);

        public Int32Size(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        public Int32Size(double Width, double Height) : this((int)Width,(int)Height)
        {
        }

        public static Int32Size operator *(Int32Size This, double Factor)
            => new Int32Size((int)(This.Width * Factor), (int)(This.Height * Factor));

        public static Int32Size operator /(Int32Size This, double Factor)
            => new Int32Size((int)(This.Width / Factor), (int)(This.Height / Factor));

        public override string ToString()
            => $@"{{ Width : {this.Width}, Height : {this.Height} }}";

    }
}
