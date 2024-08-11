using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Text;

namespace MenthaAssembly.Media.Imaging
{
    public sealed class ImageEllipseContour : ImageShapeContourNormContext
    {
        private double Rx, Ry;
        private double Theta;

        private readonly bool IsCircle;
        public ImageEllipseContour(ImageEllipseContour Contour) : base(Contour)
        {
            IsCircle = Contour.IsCircle;
            Rx = Contour.Rx;
            Ry = Contour.Ry;
            OffsetX = Contour.OffsetX;
            OffsetY = Contour.OffsetY;
        }
        public ImageEllipseContour(double Cx, double Cy, double Rx, double Ry) : this(Cx, Cy, Rx, Ry, 0d)
        {

        }
        public ImageEllipseContour(double Cx, double Cy, double Rx, double Ry, double Theta) : base()
        {
            IsCircle = Rx == Ry;
            this.Rx = Rx;
            this.Ry = Ry;
            OffsetX = Cx;
            OffsetY = Cy;
        }

        protected override void InternalFlip(double Cx, double Cy, FlipMode Flip)
        {
            switch (Flip)
            {
                case FlipMode.Vertical:
                    {
                        if (Theta == 0d)
                            Theta += MathHelper.HalfPI;

                        OffsetY = Cy * 2d - OffsetY;
                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal:
                    {
                        if (Theta == 0d)
                            Theta += MathHelper.HalfPI;

                        OffsetX = Cx * 2d - OffsetX;
                        InvalidateContent();
                        break;
                    }
                case FlipMode.Horizontal | FlipMode.Vertical:
                    {
                        if (Theta == 0d)
                            Theta += Math.PI;

                        OffsetX = Cx * 2d - OffsetX;
                        OffsetY = Cy * 2d - OffsetY;

                        InvalidateContent();
                        break;
                    }
            }
        }

        protected override void InternalRotate(double Cx, double Cy, double Theta)
        {
            if (Cx != OffsetX || Cy != OffsetY)
            {
                double Sin = Math.Sin(Theta),
                       Cos = Math.Cos(Theta);
                Point<double>.Rotate(OffsetX, OffsetY, Cx, Cy, Sin, Cos, out OffsetX, out OffsetY);
            }

            if (!IsCircle)
            {
                this.Theta += Theta;
                InvalidateContent();
            }
        }

        protected override void InternalScale(double ScaleX, double ScaleY)
        {
            Rx *= ScaleX;
            Ry *= ScaleY;
            OffsetX *= ScaleX;
            OffsetY *= ScaleY;
            InvalidateContent();
        }

        protected override void InternalEnsureNormContents()
            => ImageContourHelper.EnsureEllipseContents(this, ref OffsetX, ref OffsetY, Rx, Ry, Theta, IsCircle);

        public ImageEllipseContour Clone()
            => new(this);
        protected override IImageContour InternalClone()
            => Clone();

        public override string ToString()
        {
            StringBuilder Builder = new($"{{{OffsetX}, {OffsetY}}}");
            try
            {
                Builder.Append($", {Rx}");
                if (!IsCircle)
                    Builder.Append($", {Ry}");

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

    }
}