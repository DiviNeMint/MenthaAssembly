using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a contour of shape in image.
    /// </summary>
    [Serializable]
    public sealed class ImageShapeContour : ImageShapeContourContext
    {
        private readonly List<(SetsOperation Operation, IImageContour Contour)> Children;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ImageShapeContour() : base()
        {
            Children = [];
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Contour">The initial contour.</param>
        public ImageShapeContour(ImageShapeContour Contour) : base(Contour)
        {
            Children = new List<(SetsOperation Operation, IImageContour Contour)>(Contour.Children.Select(i => (i.Operation, i.Contour.Clone())));
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Context">The initial contour context.</param>
        public ImageShapeContour(ImageShapeContourContext Context) : base(Context)
        {
            Children = [(SetsOperation.Union, Context)];
        }

        public void Union(ImageShapeContour Contour)
            => SetsOoperation(Contour, SetsOperation.Union);
        public void Union(ImageShapeContourContext Context)
            => SetsOoperation(Context, SetsOperation.Union);

        public void Intersection(ImageShapeContour Contour)
            => SetsOoperation(Contour, SetsOperation.Intersection);
        public void Intersection(ImageShapeContourContext Context)
            => SetsOoperation(Context, SetsOperation.Intersection);

        public void Difference(ImageShapeContour Contour)
            => SetsOoperation(Contour, SetsOperation.Difference);
        public void Difference(ImageShapeContourContext Context)
            => SetsOoperation(Context, SetsOperation.Difference);

        public void SymmetricDifference(ImageShapeContour Contour)
            => SetsOoperation(Contour, SetsOperation.SymmetricDifference);
        public void SymmetricDifference(ImageShapeContourContext Context)
            => SetsOoperation(Context, SetsOperation.SymmetricDifference);

        private void SetsOoperation(ImageShapeContour Contour, SetsOperation SetsOperation)
            => SetsOoperation(Contour.Children.Count == 1 ? Contour.Children[0].Contour.Clone() : Contour.Clone(), SetsOperation);
        private void SetsOoperation(IImageContour Context, SetsOperation SetsOperation)
        {
            Children.Add((SetsOperation, Context));
            InvalidateContent();
        }

        public override void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            foreach ((_, IImageContour Contour) in Children)
                Contour.Crop(MinX, MaxX, MinY, MaxY);

            InvalidateContent();
        }

        public override void Flip(double Cx, double Cy, FlipMode Flip)
        {
            foreach ((_, IImageContour Contour) in Children)
                Contour.Flip(Cx, Cy, Flip);

            InvalidateContent();
        }

        public override void Rotate(double Cx, double Cy, double Theta)
        {
            foreach ((_, IImageContour Contour) in Children)
                Contour.Rotate(Cx, Cy, Theta);

            InvalidateContent();
        }

        public override void Scale(double ScaleX, double ScaleY)
        {
            foreach ((_, IImageContour Contour) in Children)
                Contour.Scale(ScaleX, ScaleY);

            InvalidateContent();
        }

        public override void Offset(double Dx, double Dy)
        {
            foreach ((_, IImageContour Contour) in Children)
                Contour.Offset(Dx, Dy);
        }

        protected override void InternalEnsureContents()
        {
            int i = 0,
                Count = Children.Count;

            int Ox, Oy;
            for (; i < Count; i++)
            {
                (SetsOperation Operation, IImageContour Context) = Children[i];
                if (Operation is SetsOperation.Union or SetsOperation.SymmetricDifference)
                {
                    Context.EnsureContents();

                    Ox = (int)Math.Round(Context.OffsetX, MidpointRounding.AwayFromZero);
                    Oy = (int)Math.Round(Context.OffsetY, MidpointRounding.AwayFromZero);
                    foreach (KeyValuePair<int, ImageContourScanLine> Data in Context.Contents)
                        Contents[Data.Key + Oy] = ImageContourScanLine.Offset(Data.Value, Ox);

                    i++;
                    break;
                }
            }

            for (; i < Count; i++)
            {
                (SetsOperation Operation, IImageContour Context) = Children[i];

                Context.EnsureContents();

                Ox = (int)Math.Round(Context.OffsetX, MidpointRounding.AwayFromZero);
                Oy = (int)Math.Round(Context.OffsetY, MidpointRounding.AwayFromZero);
                switch (Operation)
                {
                    case SetsOperation.Union:
                        {
                            foreach (KeyValuePair<int, ImageContourScanLine> Data in Context.Contents)
                            {
                                int Y = Data.Key + Oy;
                                if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
                                {
                                    Contents[Y] = ImageContourScanLine.Offset(Data.Value, Ox);
                                    continue;
                                }

                                ScanLine.Union(Data.Value, Ox);
                            }
                            break;
                        }
                    case SetsOperation.Intersection:
                        {
                            foreach (int Y in Contents.Keys)
                            {
                                if (!Context.Contents.TryGetValue(Y + Oy, out ImageContourScanLine ScanLine))
                                {
                                    Contents.Remove(Y);
                                    continue;
                                }

                                ImageContourScanLine ContentLine = Contents[Y];
                                ContentLine.Intersection(ScanLine, Ox);
                                if (ContentLine.Length == 0)
                                    Contents.Remove(Y);
                            }

                            break;
                        }
                    case SetsOperation.Difference:
                        {
                            foreach (KeyValuePair<int, ImageContourScanLine> Data in Context.Contents)
                                if (Contents.TryGetValue(Data.Key + Oy, out ImageContourScanLine ScanLine))
                                    ScanLine.Difference(Data.Value, Ox);
                            break;
                        }
                    case SetsOperation.SymmetricDifference:
                        {
                            foreach (KeyValuePair<int, ImageContourScanLine> Data in Context.Contents)
                            {
                                int Y = Data.Key + Oy;
                                if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
                                {
                                    Contents[Y] = ImageContourScanLine.Offset(Data.Value, Ox);
                                    continue;
                                }

                                ScanLine.SymmetricDifference(Data.Value, Ox);
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Creates a new contour that is a copy of the current instance.
        /// </summary>
        public ImageShapeContour Clone()
            => new(this);
        protected override IImageContour InternalClone()
            => Clone();

    }
}