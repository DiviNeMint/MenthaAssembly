using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    [Serializable]
    public abstract class ImageShapeContour : IImageContour, ICloneable
    {
        private readonly Dictionary<int, ImageContourScanLine> Contents;
        IReadOnlyDictionary<int, ImageContourScanLine> IImageContour.Contents
            => Contents;
        private readonly List<Tuple<Bitwises, ImageShapeContour>> Children;

        public Bound<int> Bound
        {
            get
            {
                if (!IsContentValid)
                    EnsureContents();

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
                        T = Content.Key;
                        if (T < Y0)
                            Y0 = T;
                        else if (Y1 < T)
                            Y1 = T;

                        T = XDatas[0];
                        if (T < X0)
                            X0 = T;

                        T = XDatas[XDatas.Count - 1];
                        if (X1 < T)
                            X1 = T;
                    }
                }

                return X0 != int.MaxValue || X1 != int.MinValue || Y0 != int.MaxValue || Y1 != int.MinValue ? new Bound<int>(X0, Y0, X1, Y1) : Bound<int>.Empty;
            }
        }

        private double OffsetX = 0d;
        double IImageContour.OffsetX
            => OffsetX;

        private double OffsetY = 0d;
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

        public ImageShapeContour()
        {
            Contents = new Dictionary<int, ImageContourScanLine>();
            Children = new List<Tuple<Bitwises, ImageShapeContour>>();
        }
        public ImageShapeContour(ImageShapeContour Contour)
        {
            Contents = new Dictionary<int, ImageContourScanLine>(Contour.Contents.ToDictionary(i => i.Key, i => i.Value.Clone()));
            Children = new List<Tuple<Bitwises, ImageShapeContour>>(Contour.Children.Select(i => new Tuple<Bitwises, ImageShapeContour>(i.Item1, i.Item2.Clone())));
            OffsetX = Contour.OffsetX;
            OffsetY = Contour.OffsetY;
        }

        public void Union(ImageShapeContour Contour)
        {
            if (Contents.Count > 0 && Children.Count == 0)
                ConvertToChild();

            Children.Add(new Tuple<Bitwises, ImageShapeContour>(Bitwises.Or, Contour.Clone()));
            InvalidateContent(true);
        }

        public void Intersection(ImageShapeContour Contour)
        {
            if (Contents.Count > 0 && Children.Count == 0)
                ConvertToChild();

            Children.Add(new Tuple<Bitwises, ImageShapeContour>(Bitwises.And, Contour.Clone()));
            InvalidateContent(true);
        }

        public void Difference(ImageShapeContour Contour)
        {
            if (Contents.Count > 0 && Children.Count == 0)
                ConvertToChild();

            Children.Add(new Tuple<Bitwises, ImageShapeContour>(Bitwises.Not, Contour.Clone()));
            InvalidateContent(true);
        }

        public void SymmetricDifference(ImageShapeContour Contour)
        {
            if (Contents.Count > 0 && Children.Count == 0)
                ConvertToChild();

            Children.Add(new Tuple<Bitwises, ImageShapeContour>(Bitwises.Xor, Contour.Clone()));
            InvalidateContent(true);
        }

        private void ConvertToChild()
            => Children.Add(new Tuple<Bitwises, ImageShapeContour>(Bitwises.Or, Clone()));

        public void Flip(double CenterX, double CenterY, FlipMode Flip)
        {
            foreach (Tuple<Bitwises, ImageShapeContour> Item in Children)
                Item.Item2.Flip(CenterX, CenterY, Flip);

            InternalFlip(CenterX, CenterY, Flip);
        }
        public abstract void InternalFlip(double CenterX, double CenterY, FlipMode Flip);

        public void Rotate(double CenterX, double CenterY, double Angle)
        {
            foreach (Tuple<Bitwises, ImageShapeContour> Item in Children)
                Item.Item2.Rotate(CenterX, CenterY, Angle);

            InternalRotate(CenterX, CenterY, Angle);
        }
        public abstract void InternalRotate(double CenterX, double CenterY, double Angle);

        public void Scale(double ScaleX, double ScaleY)
        {
            foreach (Tuple<Bitwises, ImageShapeContour> Item in Children)
                Item.Item2.Scale(ScaleX, ScaleY);

            InternalScale(ScaleX, ScaleY);
        }
        public abstract void InternalScale(double ScaleX, double ScaleY);

        public void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            foreach (Tuple<Bitwises, ImageShapeContour> Item in Children)
                Item.Item2.Crop(MinX, MaxX, MinY, MaxY);

            InternalCrop(MinX, MaxX, MinY, MaxY);
        }
        public abstract void InternalCrop(double MinX, double MaxX, double MinY, double MaxY);

        public void Offset(double DeltaX, double DeltaY)
        {
            OffsetX += DeltaX;
            OffsetY += DeltaY;

            foreach (Tuple<Bitwises, ImageShapeContour> Item in Children)
                Item.Item2.Offset(DeltaX, DeltaY);

            InvalidateContent(false);
        }

        private bool IsContentValid = false;
        protected void InvalidateContent(bool ClearContents)
        {
            IsContentValid = false;

            if (ClearContents)
                Contents.Clear();
        }

        public IEnumerator<KeyValuePair<int, ImageContourScanLine>> GetEnumerator()
        {
            if (!IsContentValid)
                EnsureContents();

            return new ImageContourEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        protected abstract void EnsureContents();

        void IImageContour.EnsureContents()
        {
            if (!IsContentValid)
                EnsureContents();
        }

        internal void Build()
        {
            // Offset
            double TOx = Math.Round(OffsetX),
                   TOy = Math.Round(OffsetY);
            OffsetX -= TOx;
            OffsetY -= TOy;

            int Ox = (int)TOx,
                Oy = (int)TOy;
            if (Children.Count == 0)
            {
                if (Oy == 0)
                {
                    foreach (KeyValuePair<int, ImageContourScanLine> Content in Contents)
                        Content.Value.Offset(Ox);
                }
                else
                {
                    ImageContourScanLine ScanLine;
                    IEnumerable<int> Keys = Oy < 0 ? Contents.Keys.OrderBy(i => i) :
                                                     Contents.Keys.OrderByDescending(i => i);
                    foreach (int Y in Keys)
                    {
                        ScanLine = Contents[Y];
                        ScanLine.Offset(Ox);
                        Contents.Add(Y + Oy, ScanLine);
                        Contents.Remove(Y);
                    }
                }
            }
            else
            {




            }

            IsContentValid = true;
        }

        public abstract ImageShapeContour Clone();
        IImageContour IImageContour.Clone()
            => Clone();
        object ICloneable.Clone()
            => Clone();

        public ImageContour ToImageContour()
            => new ImageContour(this);

    }
}