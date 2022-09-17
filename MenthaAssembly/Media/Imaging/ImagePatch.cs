using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImagePatch : ICloneable
    {
        public int Width { get; }

        public int Height { get; }

        public IReadOnlyPixel this[int X, int Y]
        {
            get
            {
                if (Adapters[Y, X] is IReadOnlyPixel Pixel)
                    return Pixel;

                IReadOnlyPixelAdapter Adapter = Adapters[Cy, Cx].Clone();
                Adapter.Move(this.X + X - Cx, this.Y + Y - Cy);
                Adapters[Y, X] = Adapter;
                return Adapter;
            }
        }

        internal int X, Y;
        private readonly int Cx, Cy, MaxX, MaxY;
        private readonly IReadOnlyPixelAdapter[,] Adapters;
        public ImagePatch(ImagePatch Patch)
        {
            Width = Patch.Width;
            Height = Patch.Height;

            X = Patch.X;
            Y = Patch.Y;
            MaxX = Patch.MaxX;
            MaxY = Patch.MaxY;
            Cx = Patch.Cx;
            Cy = Patch.Cy;

            Adapters = new IReadOnlyPixelAdapter[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i] = Patch.Adapters[j, i]?.Clone();
        }
        public ImagePatch(IReadOnlyImageContext Context, int PatchWidth, int PatchHeight)
        {
            Width = PatchWidth;
            Height = PatchHeight;


            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            IReadOnlyPixelAdapter Adapter = Context.GetAdapter(X, Y);
            Adapters = new IReadOnlyPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;

            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
        }
        public ImagePatch(IReadOnlyImageContext Context, int X, int Y, int PatchWidth, int PatchHeight)
        {
            if (X < 0 || Context.Width <= X ||
                Y < 0 || Context.Height <= Y)
                throw new IndexOutOfRangeException();

            Width = PatchWidth;
            Height = PatchHeight;

            this.X = X;
            this.Y = Y;
            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            IReadOnlyPixelAdapter Adapter = Context.GetAdapter(X, Y);
            Adapters = new IReadOnlyPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;

            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
        }
        public ImagePatch(IReadOnlyPixelAdapter Adapter, int PatchWidth, int PatchHeight)
        {
            Width = PatchWidth;
            Height = PatchHeight;

            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            Adapters = new IReadOnlyPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;
            Adapter.Move(0, 0);
        }
        public ImagePatch(IReadOnlyPixelAdapter Adapter, int X, int Y, int PatchWidth, int PatchHeight)
        {
            if (X < 0 || Adapter.MaxX < X ||
                Y < 0 || Adapter.MaxY < Y)
                throw new IndexOutOfRangeException();

            Width = PatchWidth;
            Height = PatchHeight;

            this.X = X;
            this.Y = Y;
            MaxX = Adapter.MaxX;
            MaxY = Adapter.MaxY;
            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            Adapters = new IReadOnlyPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;
            Adapter.Move(X, Y);
        }

        public void Move(int X, int Y)
        {
            for (int i = 0; i < Width; i++)
            {
                int Tx = X + i - Cx;
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.Move(Tx, Y + j - Cx);
            }

            IReadOnlyPixelAdapter Adapter = Adapters[Cy, Cx];
            this.X = Adapter.X;
            this.Y = Adapter.Y;
        }

        public void MoveNext()
        {
            if (MaxX <= X)
                return;

            for (int i = X == 0 ? 1 : 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.MoveNext();

            X++;
        }
        public void MovePrevious()
        {
            if (X <= 0)
                return;

            int Ex = X < MaxX ? Width : Width - 1;
            for (int i = 0; i < Ex; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.MovePrevious();

            X--;
        }

        public void MoveNextLine()
        {
            if (MaxY <= Y)
                return;

            for (int i = 0; i < Width; i++)
                for (int j = Y == 0 ? 1 : 0; j < Height; j++)
                    Adapters[j, i]?.MoveNextLine();

            Y++;
        }
        public void MovePreviousLine()
        {
            if (Y <= 0)
                return;

            int Ey = Y < MaxY ? Height : Height - 1;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Ey; j++)
                    Adapters[j, i]?.MovePreviousLine();

            Y--;
        }

        public ImagePatch Clone()
            => new ImagePatch(this);
        object ICloneable.Clone()
            => Clone();

        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder();
            try
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Adapters[j, 0] is IReadOnlyPixelAdapter Adapter)
                        Builder.Append($"{{{Adapter.X}, {Adapter.Y}}}");
                    else
                        Builder.Append($"      ");

                    for (int i = 1; i < Width; i++)
                    {
                        if (Adapters[j, i] is IReadOnlyPixelAdapter Adapter2)
                            Builder.Append($" {{{Adapter2.X}, {Adapter2.Y}}}");
                        else
                            Builder.Append($"       ");
                    }
                    Builder.AppendLine();
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

    }
}