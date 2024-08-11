using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImageTriangleContour : ImageShapeContourNormContext
    {
        private double[] Points;
        public ImageTriangleContour(ImageTriangleContour Contour) : base(Contour)
        {
            Points = [.. Contour.Points];
        }
        public ImageTriangleContour(double X1, double Y1, double X2, double Y2, double X3, double Y3) : base()
        {
            Points = [X1, Y1, X2, Y2, X3, Y3];
            OffsetX = 0d;
            OffsetY = 0d;
        }

        protected override void InternalFlip(double Cx, double Cy, FlipMode Flip)
        {
            switch (Flip)
            {
                case FlipMode.Vertical:
                    {
                        double Ty = Cy * 2d;

                        for (int i = 1; i < Points.Length; i += 2)
                            Points[i] = Ty - Points[i];

                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal:
                    {
                        double Tx = Cx * 2d;

                        for (int i = 0; i < Points.Length; i += 2)
                            Points[i] = Tx - Points[i];

                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        double Tx = Cx * 2d,
                               Ty = Cy * 2d;

                        for (int i = 0; i < Points.Length;)
                        {
                            Points[i++] = Tx - Points[i];
                            Points[i++] = Ty - Points[i];
                        }

                        InvalidateContent();
                        break;
                    }
            }
        }

        protected override void InternalRotate(double Cx, double Cy, double Theta)
        {
            Cx -= OffsetX;
            Cy -= OffsetY;

            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);
            for (int i = 0; i < Points.Length;)
            {
                int i0 = i++,
                    i1 = i++;
                Point<double>.Rotate(Points[i0], Points[i1], Cx, Cy, Sin, Cos, out Points[i0], out Points[i1]);
            }

            InvalidateContent();
        }

        protected override void InternalScale(double ScaleX, double ScaleY)
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

        protected override void InternalEnsureNormContents()
            => ImageContourHelper.EnsureSimpleConvexPolygonContents(this, ref Points, ref OffsetX, ref OffsetY);

        /// <summary>
        /// Creates a new contour that is a copy of the current instance.
        /// </summary>
        public ImageTriangleContour Clone()
            => new(this);
        protected override IImageContour InternalClone()
            => Clone();

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