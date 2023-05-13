using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImagePatch : IImageAdapter, ICloneable
    {
        public int Width { get; }

        public int Height { get; }

        public int X { private set; get; }

        public int Y { private set; get; }

        public int XLength { get; }

        public int YLength { get; }

        public IReadOnlyPixel this[int X, int Y]
        {
            get
            {
                if (Adapters[Y, X] is IReadOnlyPixel Pixel)
                    return Pixel;

                IPixelAdapter Adapter = Adapters[Cy, Cx].Clone();
                Adapter.Move(this.X + X - Cx, this.Y + Y - Cy);
                Adapters[Y, X] = Adapter;
                return Adapter;
            }
        }

        private readonly int Cx, Cy;
        private readonly IPixelAdapter[,] Adapters;
        public ImagePatch(ImagePatch Patch)
        {
            Width = Patch.Width;
            Height = Patch.Height;

            X = Patch.X;
            Y = Patch.Y;
            XLength = Patch.XLength;
            YLength = Patch.YLength;
            Cx = Patch.Cx;
            Cy = Patch.Cy;

            Adapters = new IPixelAdapter[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i] = Patch.Adapters[j, i]?.Clone();
        }
        public ImagePatch(IImageContext Context, int PatchWidth, int PatchHeight)
        {
            Width = PatchWidth;
            Height = PatchHeight;


            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            IPixelAdapter Adapter = Context.GetAdapter(X, Y);
            Adapters = new IPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;

            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
        }
        public ImagePatch(IImageContext Context, int X, int Y, int PatchWidth, int PatchHeight)
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

            IPixelAdapter Adapter = Context.GetAdapter(X, Y);
            Adapters = new IPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;

            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
        }
        public ImagePatch(IPixelAdapter Adapter, int PatchWidth, int PatchHeight)
        {
            Width = PatchWidth;
            Height = PatchHeight;

            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            Adapters = new IPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;
            Adapter.Move(0, 0);
        }
        public ImagePatch(IPixelAdapter Adapter, int X, int Y, int PatchWidth, int PatchHeight)
        {
            if (X < 0 || Adapter.XLength <= X ||
                Y < 0 || Adapter.YLength <= Y)
                throw new IndexOutOfRangeException();

            Width = PatchWidth;
            Height = PatchHeight;

            this.X = X;
            this.Y = Y;
            XLength = Adapter.XLength;
            YLength = Adapter.YLength;
            Cx = PatchWidth >> 1;
            Cy = PatchHeight >> 1;

            Adapters = new IPixelAdapter[PatchWidth, PatchHeight];
            Adapters[Cy, Cx] = Adapter;
            Adapter.Move(X, Y);
        }

        public void Move(int X, int Y)
        {
            X = X.Clamp(0, XLength - 1);
            Y = Y.Clamp(0, YLength - 1);

            for (int i = 0; i < Width; i++)
            {
                int Tx = X + i - Cx;
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.Move(Tx, Y + j - Cx);
            }

            IPixelAdapter Adapter = Adapters[Cy, Cx];
            this.X = Adapter.X;
            this.Y = Adapter.Y;
        }

        public void OffsetX(int Delta)
            => throw new NotImplementedException();
        public void OffsetY(int Delta)
            => throw new NotImplementedException();

        public void MoveNextX()
        {
            int Tx = X + 1;
            if (XLength <= Tx)
                return;

            for (int i = X == 0 ? 1 : 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.MoveNextX();

            X = Tx;
        }
        public void MoveNextY()
        {
            int Ty = Y + 1;
            if (YLength <= Ty)
                return;

            for (int i = 0; i < Width; i++)
                for (int j = Y == 0 ? 1 : 0; j < Height; j++)
                    Adapters[j, i]?.MoveNextY();

            Y = Ty;
        }

        public void MovePreviousX()
        {
            if (X <= 0)
                return;

            int Ex = X < XLength - 1 ? Width : Width - 1;
            for (int i = 0; i < Ex; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.MovePreviousX();

            X--;
        }
        public void MovePreviousY()
        {
            if (Y <= 0)
                return;

            int Ey = Y < YLength - 1 ? Height : Height - 1;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Ey; j++)
                    Adapters[j, i]?.MovePreviousY();

            Y--;
        }

        public ImagePatch Clone()
            => new(this);
        object ICloneable.Clone()
            => Clone();

        public override string ToString()
        {
            StringBuilder Builder = new();
            try
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Adapters[j, 0] is IPixelAdapter Adapter)
                        Builder.Append($"{{{Adapter.X}, {Adapter.Y}}}");
                    else
                        Builder.Append($"      ");

                    for (int i = 1; i < Width; i++)
                    {
                        if (Adapters[j, i] is IPixelAdapter Adapter2)
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