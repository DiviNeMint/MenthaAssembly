using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImageRectangleContour : ImageShapeContourContext
    {
        private double[] Points;
        public ImageRectangleContour(ImageRectangleContour Contour) : base(Contour)
        {
            Points = [.. Contour.Points];
            //CropBound = Contour.CropBound?.ToArray();
        }
        public ImageRectangleContour(double Cx, double Cy, double Width, double Height)
        {
            double Width2 = Width / 2d,
                   Height2 = Height / 2d;
            Points = [ Width2,  Height2,
                      -Width2,  Height2,
                      -Width2, -Height2,
                       Width2, -Height2];
            OffsetX = Cx;
            OffsetY = Cy;
        }
        public ImageRectangleContour(double Cx, double Cy, double Width, double Height, double Angle)
        {
            double Width2 = Width / 2d,
                   Height2 = Height / 2d,
                   Sin = MathHelper.Sin(Angle),
                   Cos = MathHelper.Cos(Angle),
                   WCos = Width2 * Cos,
                   WSin = Width2 * Sin,
                   HSin = Height2 * Sin,
                   HCos = Height2 * Cos;

            Points = [ WCos - HSin,  WSin + HCos,
                      -WCos - HSin, -WSin + HCos,
                      -WCos + HSin, -WSin - HCos,
                       WCos + HSin,  WSin - HCos];
            OffsetX = Cx;
            OffsetY = Cy;
        }

        public override void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            MinX -= OffsetX;
            MaxX -= OffsetX;
            MinY -= OffsetY;
            MaxY -= OffsetY;
            Points = [.. CropPoints(Points, MinX, MinY, MaxX, MaxY)];
            InvalidateContent();
        }

        public override void Flip(double Cx, double Cy, FlipMode Flip)
        {
            switch (Flip)
            {
                case FlipMode.Vertical:
                    {
                        double Ty = Cy * 2d;

                        for (int i = 1; i < Points.Length; i += 2)
                            Points[i] = Ty - Points[i] - OffsetY;

                        OffsetY = 0d;
                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal:
                    {
                        double Tx = Cx * 2d;

                        for (int i = 0; i < Points.Length; i += 2)
                            Points[i] = Tx - Points[i] - OffsetX;

                        OffsetX = 0d;
                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        double Tx = Cx * 2d,
                               Ty = Cy * 2d;

                        for (int i = 0; i < Points.Length;)
                        {
                            Points[i++] = Tx - Points[i] - OffsetX;
                            Points[i++] = Ty - Points[i] - OffsetY;
                        }

                        OffsetX = 0d;
                        OffsetY = 0d;
                        InvalidateContent();
                        break;
                    }
            }
        }

        public override void Rotate(double Cx, double Cy, double Theta)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta),
                   Px, Py;

            for (int i = 0; i < Points.Length;)
            {
                int i0 = i++,
                    i1 = i++;

                Px = Points[i0] + OffsetX - Cx;
                Py = Points[i1] + OffsetY - Cy;

                Points[i0] = Px * Cos - Py * Sin + Cx;
                Points[i1] = Px * Sin + Py * Cos + Cy;
            }

            OffsetX = 0d;
            OffsetY = 0d;
            InvalidateContent();
        }

        public override void Scale(double ScaleX, double ScaleY)
        {
            for (int i = 0; i < Points.Length;)
            {
                Points[i++] *= ScaleX;
                Points[i++] *= ScaleY;
            }

            OffsetX *= ScaleX;
            OffsetY *= ScaleY;
            InvalidateContent();
        }

        protected override void InternalEnsureContents()
        {
            double Dx = Points[0],
                   Dy = Points[1];

            Points[0] = 0d;
            Points[1] = 0d;

            OffsetX += Dx;
            OffsetY += Dy;

            int Lx = 0, Ly = 0,
                Tx, Ty;

            for (int i = 2; i < Points.Length;)
            {
                Points[i] -= Dx;
                Tx = (int)Math.Round(Points[i++]);

                Points[i] -= Dy;
                Ty = (int)Math.Round(Points[i++]);

                AddContentBound(Lx, Ly, Tx, Ty);

                Lx = Tx;
                Ly = Ty;
            }

            AddContentBound(Lx, Ly, 0, 0);
        }

        private void AddData(int X, int Y)
        {
            ImageContourScanLine ScanLine = this[Y];
            if (ScanLine.Length == 0)
            {
                ScanLine.Datas.Add(X);
                ScanLine.Datas.Add(X);
                return;
            }

            if (X < ScanLine[0])
                ScanLine[0] = X;
            else if (ScanLine[1] < X)
                ScanLine[1] = X;
        }
        private void AddContentBound(int Lx, int Ly, int Tx, int Ty)
        {
            if (Ly != Ty)
            {
                int Dx = Tx - Lx,
                    Dy = Ty - Ly;

                if (Dx == 0)
                {
                    if (Dy > 0)
                    {
                        for (int j = Ly; j <= Ty; j++)
                            AddData(Tx, j);
                    }
                    else
                    {
                        for (int j = Ty; j <= Ly; j++)
                            AddData(Tx, j);
                    }
                }
                else
                {
                    GraphicAlgorithm.CalculateBresenhamLine(Dx, Dy, Dx.Abs(), Dy.Abs(), (TDx, TDy) => AddData(Lx + TDx, Ly + TDy));
                }
            }
        }

        protected internal override ImageShapeContourContext InternalClone()
            => new ImageRectangleContour(this);

        public override string ToString()
        {
            StringBuilder Builder = new($"{{{Points[0] + OffsetX}, {Points[1] + OffsetY}}}");
            try
            {
                for (int i = 2; i < Points.Length;)
                    Builder.Append($", {{{Points[i++] + OffsetX}, {Points[i++] + OffsetY}}}");

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

    }
}