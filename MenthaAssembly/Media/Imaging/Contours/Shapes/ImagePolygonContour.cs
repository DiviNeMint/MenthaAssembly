using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImagePolygonContour : ImageShapeContourContext
    {
        private List<double> Points;
        public ImagePolygonContour(ImagePolygonContour Contour) : base(Contour)
        {
            Points = [.. Contour.Points];
        }
        public ImagePolygonContour(IEnumerable<double> Vertices) : this(Vertices, 0d, 0d)
        {
        }
        public ImagePolygonContour(IEnumerable<double> Vertices, double OffsetX, double OffsetY)
        {
            Points = Vertices is List<double> List ? List : Vertices.ToList();

            // Check Close Region
            if (Points.Count > 0)
            {
                int Length = Points.Count;
                double Sx = Points[Length - 2],
                       Sy = Points[Length - 1],
                       Ex = Points[0],
                       Ey = Points[1];

                if (!Sx.Equals(Ex) | !Sy.Equals(Ey))
                {
                    Points.Add(Ex);
                    Points.Add(Ey);
                }
            }

            this.OffsetX = OffsetX;
            this.OffsetY = OffsetY;
        }

        public override void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            Points = ImageContourHelper.CropPoints(Points, MinX, MaxX, MinY, MaxY);
            InvalidateContent();
        }

        public override void Flip(double Cx, double Cy, FlipMode Flip)
        {
            Cx -= OffsetX;
            Cy -= OffsetY;

            switch (Flip)
            {
                case FlipMode.Vertical:
                    {
                        double Ty = Cy * 2d;

                        for (int i = 1; i < Points.Count; i += 2)
                            Points[i] = Ty - Points[i] - OffsetY;

                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal:
                    {
                        double Tx = Cx * 2d;

                        for (int i = 0; i < Points.Count; i += 2)
                            Points[i] = Tx - Points[i] - OffsetX;

                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        double Tx = Cx * 2d,
                               Ty = Cy * 2d;

                        for (int i = 0; i < Points.Count;)
                        {
                            Points[i++] = Tx - Points[i] - OffsetX;
                            Points[i++] = Ty - Points[i] - OffsetY;
                        }

                        InvalidateContent();
                        break;
                    }
            }
        }

        public override void Rotate(double Cx, double Cy, double Theta)
        {
            Cx -= OffsetX;
            Cy -= OffsetY;

            double Sin = Math.Sin(Theta),
            Cos = Math.Cos(Theta),
                   Px, Py;
            for (int i = 0; i < Points.Count;)
            {
                int i0 = i++,
                    i1 = i++;

                Px = Points[i0] - Cx;
                Py = Points[i1] - Cy;

                Points[i0] = Px * Cos - Py * Sin + Cx;
                Points[i1] = Px * Sin + Py * Cos + Cy;
            }

            InvalidateContent();
        }

        public override void Scale(double ScaleX, double ScaleY)
        {
            for (int i = 0; i < Points.Count;)
            {
                Points[i++] *= ScaleX;
                Points[i++] *= ScaleY;
            }

            OffsetX *= ScaleX;
            OffsetY *= ScaleY;
            InvalidateContent();
        }

        protected override void InternalEnsureContents()
            => ImageContourHelper.EnsurePolygonContents(this, ref Points, ref OffsetX, ref OffsetY);

        public ImagePolygonContour Clone()
            => new(this);
        protected override IImageContour InternalClone()
            => Clone();

        public override string ToString()
        {
            StringBuilder Builder = new($"{{{Points[0] + OffsetX}, {Points[1] + OffsetY}}}");
            try
            {
                for (int i = 2; i < Points.Count;)
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