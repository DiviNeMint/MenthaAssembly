using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents context of  shape's contour in image.
    /// </summary>
    [Serializable]
    public abstract class ImageShapeContourContext : IImageContour, ICloneable
    {
        protected internal readonly Dictionary<int, ImageContourScanLine> Contents;
        IReadOnlyDictionary<int, ImageContourScanLine> IImageContour.Contents
            => Contents;

        public virtual Bound<int> Bound
        {
            get
            {
                EnsureContents();

                ImageContourHelper.StandardizateOffset(OffsetX, OffsetY, out int Ox, out int Oy, out _, out _);
                int X0 = int.MaxValue,
                    X1 = int.MinValue,
                    Y0 = int.MaxValue,
                    Y1 = int.MinValue,
                    T;

                foreach (KeyValuePair<int, ImageContourScanLine> Content in Contents)
                {
                    List<int> XDatas = Content.Value.Datas;
                    if (XDatas.Count > 0)
                    {
                        T = Content.Key + Oy;
                        if (T < Y0)
                            Y0 = T;
                        if (Y1 < T)
                            Y1 = T;

                        T = XDatas[0] + Ox;
                        if (T < X0)
                            X0 = T;

                        T = XDatas[XDatas.Count - 1] + Ox;
                        if (X1 < T)
                            X1 = T;
                    }
                }

                return X0 != int.MaxValue || X1 != int.MinValue || Y0 != int.MaxValue || Y1 != int.MinValue ? new Bound<int>(X0, Y0, X1, Y1) : Bound<int>.Empty;
            }
        }

        protected double OffsetX = 0d;
        double IImageContour.OffsetX
            => OffsetX;

        protected double OffsetY = 0d;
        double IImageContour.OffsetY
            => OffsetY;

        public ImageContourScanLine this[int Y]
        {
            get
            {
                if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
                {
                    ScanLine = new ImageContourScanLine();
                    Contents.Add(Y, ScanLine);
                }

                return ScanLine;
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ImageShapeContourContext()
        {
            Contents = [];
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Contour">The initial contour.</param>
        public ImageShapeContourContext(ImageShapeContourContext Contour)
        {
            if (Contour.IsContentValid)
            {
                IsContentValid = true;
                Contents = new Dictionary<int, ImageContourScanLine>(Contour.Contents.ToDictionary(i => i.Key, i => i.Value.Clone()));
            }
            else
            {
                Contents = [];
            }

            OffsetX = Contour.OffsetX;
            OffsetY = Contour.OffsetY;
        }

        public abstract void Crop(double MinX, double MaxX, double MinY, double MaxY);

        public abstract void Flip(double Cx, double Cy, FlipMode Flip);

        public abstract void Rotate(double Cx, double Cy, double Theta);

        public abstract void Scale(double ScaleX, double ScaleY);

        public virtual void Offset(double Dx, double Dy)
        {
            OffsetX += Dx;
            OffsetY += Dy;
        }

        private bool IsContentValid = false;
        protected void InvalidateContent()
        {
            IsContentValid = false;
            Contents.Clear();
        }

        public IEnumerator<KeyValuePair<int, ImageContourScanLine>> GetEnumerator()
        {
            EnsureContents();
            return new ImageContourEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        protected abstract void InternalEnsureContents();

        private void EnsureContents()
        {
            if (IsContentValid)
                return;

            InternalEnsureContents();
            IsContentValid = true;
        }
        void IImageContour.EnsureContents()
            => EnsureContents();

        protected abstract IImageContour InternalClone();
        IImageContour IImageContour.Clone()
            => InternalClone();
        object ICloneable.Clone()
            => InternalClone();

        /// <summary>
        /// Creates a ImageContour that is a copy of the current instance.
        /// </summary>
        public ImageContour ToImageContour()
            => new(this);

    }
}