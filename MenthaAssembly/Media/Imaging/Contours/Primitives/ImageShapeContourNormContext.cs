using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents context of shape's contour in image.
    /// </summary>
    [Serializable]
    public abstract class ImageShapeContourNormContext : ImageShapeContourContext
    {
        protected readonly List<IImageContour> CropRects;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ImageShapeContourNormContext() : base()
        {
            CropRects = [];
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Contour">The initial contour.</param>
        public ImageShapeContourNormContext(ImageShapeContourNormContext Contour) : base(Contour)
        {
            CropRects = new List<IImageContour>(Contour.CropRects.Select(i => i.Clone()));
        }

        public sealed override void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            CropRects.Add(new ImageRectangleContour((MinX + MaxX) / 2d, (MinY + MaxY) / 2d, MaxX - MinX, MaxY - MinY));
            InvalidateContent();
        }

        public sealed override void Flip(double Cx, double Cy, FlipMode Flip)
        {
            foreach (ImageRectangleContour Rect in CropRects)
                Rect.Flip(Cx, Cy, Flip);

            InternalFlip(Cx, Cy, Flip);
        }

        /// <summary>
        /// Rotates the contour about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public sealed override void Rotate(double Cx, double Cy, double Theta)
        {
            foreach (ImageRectangleContour Rect in CropRects)
                Rect.Rotate(Cx, Cy, Theta);

            InternalRotate(Cx, Cy, Theta);
        }

        /// <summary>
        /// Scales this contour around the origin.
        /// </summary>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public sealed override void Scale(double ScaleX, double ScaleY)
        {
            foreach (ImageRectangleContour Rect in CropRects)
                Rect.Scale(ScaleX, ScaleY);

            InternalScale(ScaleX, ScaleY);
        }

        protected abstract void InternalFlip(double Cx, double Cy, FlipMode Flip);

        protected abstract void InternalRotate(double Cx, double Cy, double Theta);

        protected abstract void InternalScale(double ScaleX, double ScaleY);

        public sealed override void Offset(double Dx, double Dy)
        {
            foreach (ImageRectangleContour Rect in CropRects)
                Rect.Offset(Dx, Dy);

            base.Offset(Dx, Dy);
        }

        protected sealed override void InternalEnsureContents()
        {
            InternalEnsureNormContents();

            int Ox, Oy;
            foreach (IImageContour Rect in CropRects)
            {
                Rect.EnsureContents();

                Ox = (int)Math.Round(Rect.OffsetX + OffsetX, MidpointRounding.AwayFromZero);
                Oy = (int)Math.Round(Rect.OffsetY + OffsetY, MidpointRounding.AwayFromZero);
                foreach (int Y in Contents.Keys)
                {
                    if (!Rect.Contents.TryGetValue(Y + Oy, out ImageContourScanLine ScanLine))
                    {
                        Contents.Remove(Y);
                        continue;
                    }

                    ImageContourScanLine ContentLine = Contents[Y];
                    ContentLine.Intersection(ScanLine, Ox);
                    if (ContentLine.Length == 0)
                        Contents.Remove(Y);
                }
            }
        }
        protected abstract void InternalEnsureNormContents();

    }
}