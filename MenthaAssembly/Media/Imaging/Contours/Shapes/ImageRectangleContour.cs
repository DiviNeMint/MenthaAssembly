﻿using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImageRectangleContour : ImageShapeContourNormContext
    {
        private double[] Points;
        public ImageRectangleContour(ImageRectangleContour Contour) : base(Contour)
        {
            Points = [.. Contour.Points];
        }
        public ImageRectangleContour(double Cx, double Cy, double Width, double Height) : base()
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
        public ImageRectangleContour(double Cx, double Cy, double Width, double Height, double Theta) : base()
        {
            double Width2 = Width / 2d,
                   Height2 = Height / 2d,
                   Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta),
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

        protected override void InternalFlip(double Cx, double Cy, FlipMode Flip)
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

        protected override void InternalRotate(double Cx, double Cy, double Theta)
        {
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta);
            for (int i = 0; i < Points.Length;)
            {
                int i0 = i++,
                    i1 = i++;
                Point<double>.Rotate(Points[i0] + OffsetX, Points[i1] + OffsetY, Cx, Cy, Sin, Cos, out Points[i0], out Points[i1]);
            }

            OffsetX = 0d;
            OffsetY = 0f;
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
        public ImageRectangleContour Clone()
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