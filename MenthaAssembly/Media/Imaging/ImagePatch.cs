using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImagePatch : IImageAdapter, ICloneable
    {
        public int Width { set; get; }

        public int Height { set; get; }

        public int X { set; get; }

        public int Y { set; get; }

        public int XLength { get; }

        public int YLength { get; }

        public IReadOnlyPixel this[int X, int Y]
        {
            get
            {
                if (Adapters[Y, X] is IReadOnlyPixel Pixel)
                    return Pixel;

                IPixelAdapter Adapter = Adapters[Cy, Cx].Clone();
                Adapter.DangerousMove(MathHelper.Clamp(this.X + X - Cx, 0, XLength - 1), MathHelper.Clamp(this.Y + Y - Cy, 0, YLength - 1));
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

            Adapters = new IPixelAdapter[PatchHeight, PatchWidth];
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

            Adapters = new IPixelAdapter[PatchHeight, PatchWidth];
            Adapters[Cy, Cx] = Adapter;
            Adapter.Move(X, Y);
        }

        public void Move(int X, int Y)
            => DangerousMove(X, Y);

        public void OffsetX(int Delta)
        {
            int Nx = X + Delta;
            if (Nx < 0)
            {
                Delta += Nx;
                Nx = 0;
            }
            else if (Nx >= XLength)
            {
                Nx = XLength - 1;
                Delta = Nx - X;
            }

            if (Delta == 0)
                return;

            DangerousOffsetX(Delta);
            X = Nx;
        }
        public void OffsetY(int Delta)
        {
            int Ny = Y + Delta;
            if (Ny < 0)
            {
                Delta += Ny;
                Ny = 0;
            }
            else if (Ny >= YLength)
            {
                Ny = YLength - 1;
                Delta = Ny - Y;
            }

            if (Delta == 0)
                return;

            DangerousOffsetY(Delta);
            Y = Ny;
        }

        public void MoveNextX()
            => DangerousMoveNextX();
        public void MoveNextY()
            => DangerousMoveNextY();

        public void MovePreviousX()
            => DangerousMovePreviousX();
        public void MovePreviousY()
            => DangerousMovePreviousY();

        public void DangerousMove(int X, int Y)
        {
            int MaxX = XLength - 1,
                MaxY = YLength - 1;

            X = X.Clamp(0, MaxX);
            Y = Y.Clamp(0, MaxY);
            for (int i = 0; i < Width; i++)
            {
                int Tx = MathHelper.Clamp(X + i - Cx, 0, MaxX),
                    Ty = Y - Cy;
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.DangerousMove(Tx, MathHelper.Clamp(Ty + j, 0, MaxY));
            }

            this.X = X;
            this.Y = Y;
        }

        public void DangerousOffsetX(int Delta)
        {
            int MaxX = XLength - 1;
            Delta = MathHelper.Clamp(X + Delta, 0, MaxX) - X;

            if (Delta == 0)
                return;

            int Tx = X - Cx;
            for (int i = 0; i < Width; i++, Tx++)
            {
                int Dx = MathHelper.Clamp(Tx + Delta, 0, MaxX) - Tx.Clamp(0, MaxX);
                if (Dx == 0)
                    continue;

                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.DangerousOffsetX(Dx);
            }

            X += Delta;
        }
        public void DangerousOffsetY(int Delta)
        {
            int MayY = YLength - 1;
            Delta = MathHelper.Clamp(Y + Delta, 0, MayY) - Y;

            if (Delta == 0)
                return;

            int Ty = Y - Cy;
            for (int j = 0; j < Height; j++, Ty++)
            {
                int Dy = MathHelper.Clamp(Ty + Delta, 0, MayY) - Ty.Clamp(0, MayY);
                if (Dy == 0)
                    continue;

                for (int i = 0; i < Width; i++)
                    Adapters[j, i]?.DangerousOffsetY(Dy);
            }

            Y += Delta;
        }

        public void DangerousMoveNextX()
        {
            int Nx = X + 1;
            if (XLength <= Nx)
                return;

            int DL = Cx - X,
                DR = Width - DL - XLength + 1,
                Ex = Width - Math.Max(DR, 0);

            for (int i = Math.Max(DL, 0); i < Ex; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.DangerousMoveNextX();

            X = Nx;
        }
        public void DangerousMoveNextY()
        {
            int Ny = Y + 1;
            if (YLength <= Ny)
                return;

            int DT = Cy - Y,
                DB = Height - DT - YLength + 1,
                Ey = Height - Math.Max(DB, 0);

            for (int j = Math.Max(DT, 0); j < Ey; j++)
                for (int i = 0; i < Width; i++)
                    Adapters[j, i]?.DangerousMoveNextY();

            Y = Ny;
        }

        public void DangerousMovePreviousX()
        {
            if (X <= 0)
                return;

            int DL = Cx - X,
                DR = Width - DL - XLength,
                Ex = Width - Math.Max(DR, 0);

            for (int i = Math.Max(DL + 1, 0); i < Ex; i++)
                for (int j = 0; j < Height; j++)
                    Adapters[j, i]?.DangerousMoveNextX();

            X--;
        }
        public void DangerousMovePreviousY()
        {
            if (Y <= 0)
                return;

            int DT = Cy - Y,
                DB = Height - DT - YLength,
                Ey = Height - Math.Max(DB, 0);

            for (int j = Math.Max(DT + 1, 0); j < Ey; j++)
                for (int i = 0; i < Width; i++)
                    Adapters[j, i]?.DangerousMoveNextY();

            Y--;
        }

        public ImagePatch Clone()
            => new(this);
        IImageAdapter IImageAdapter.Clone()
            => Clone();
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