using System;
using System.Collections;
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
        private readonly List<Tuple<Bitwises, ImageShapeContourContext>> Children;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ImageShapeContour() : base()
        {
            Children = new List<Tuple<Bitwises, ImageShapeContourContext>>();
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Contour">The initial contour.</param>
        public ImageShapeContour(ImageShapeContour Contour) : base(Contour)
        {
            Children = new List<Tuple<Bitwises, ImageShapeContourContext>>(Contour.Children.Select(i => new Tuple<Bitwises, ImageShapeContourContext>(i.Item1, i.Item2.InternalClone())));
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Context">The initial contour context.</param>
        public ImageShapeContour(ImageShapeContourContext Context) : base(Context)
        {
            Children = new List<Tuple<Bitwises, ImageShapeContourContext>> { new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Or, Context) };
        }

        public void Union(ImageShapeContour Contour)
        {
            ImageShapeContourContext Context = Contour.Children.Count == 1 ? Contour.Children[0].Item2.InternalClone() : Contour.Clone();
            Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Or, Context));
            InvalidateContent();
        }

        public void Intersection(ImageShapeContour Contour)
        {
            ImageShapeContourContext Context = Contour.Children.Count == 1 ? Contour.Children[0].Item2.InternalClone() : Contour.Clone();
            Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.And, Context));
            InvalidateContent();
        }

        public void Difference(ImageShapeContour Contour)
        {
            ImageShapeContourContext Context = Contour.Children.Count == 1 ? Contour.Children[0].Item2.InternalClone() : Contour.Clone();
            Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Not, Context));
            InvalidateContent();
        }

        public void SymmetricDifference(ImageShapeContour Contour)
        {
            ImageShapeContourContext Context = Contour.Children.Count == 1 ? Contour.Children[0].Item2.InternalClone() : Contour.Clone();
            Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Xor, Context));
            InvalidateContent();
        }

        public override void Flip(double Cx, double Cy, FlipMode Flip)
        {
            foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
                Item.Item2.Flip(Cx, Cy, Flip);

            InvalidateContent();
        }

        public override void Rotate(double Cx, double Cy, double Theta)
        {
            foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
                Item.Item2.Rotate(Cx, Cy, Theta);

            InvalidateContent();
        }

        public override void Scale(double ScaleX, double ScaleY)
        {
            foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
                Item.Item2.Scale(ScaleX, ScaleY);

            InvalidateContent();
        }

        public override void Crop(double MinX, double MaxX, double MinY, double MaxY)
        {
            foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
                Item.Item2.Crop(MinX, MaxX, MinY, MaxY);

            InvalidateContent();
        }

        public override void Offset(double Dx, double Dy)
        {
            foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
                Item.Item2.Offset(Dx, Dy);

            OffsetX += Dx;
            OffsetY += Dy;
        }

        protected override void InternalEnsureContents()
        {
            //int i = 0,
            //    Count = Children.Count;

            //double Ox, Oy;
            //for (; i < Count; i++)
            //{
            //    Tuple<Bitwises, ImageShapeContourContext> Data = Children[i];
            //    if (Data.Item1 == Bitwises.Or)
            //    {
            //        ImageShapeContourContext Context = Data.Item2;
            //        Context.EnsureContents();

            //        foreach (var item in Context.Contents)
            //        {



            //        }

            //        break;
            //    }
            //}
        }

        /// <summary>
        /// Creates a new contour that is a copy of the current instance.
        /// </summary>
        public ImageShapeContour Clone()
            => new ImageShapeContour(this);
        protected internal override ImageShapeContourContext InternalClone()
            => Clone();

        private void Build()
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

        }

    }


    ///// <summary>
    ///// Represents a contour of shape in image.
    ///// </summary>
    //[Serializable]
    //public abstract class ImageShapeContour : IImageContour, ICloneable
    //{
    //    protected readonly Dictionary<int, ImageContourScanLine> Contents;
    //    IReadOnlyDictionary<int, ImageContourScanLine> IImageContour.Contents
    //        => Contents;
    //    private readonly List<Tuple<Bitwises, ImageShapeContourContext>> Children;

    //    public virtual Bound<int> Bound
    //    {
    //        get
    //        {
    //            if (!IsContentValid)
    //                EnsureContents();

    //            int X0 = int.MaxValue,
    //                X1 = int.MinValue,
    //                Y0 = int.MaxValue,
    //                Y1 = int.MinValue,
    //                T;

    //            foreach (KeyValuePair<int, ImageContourScanLine> Content in Contents)
    //            {
    //                List<int> XDatas = Content.Value.Datas;
    //                if (XDatas.Count > 0)
    //                {
    //                    T = Content.Key;
    //                    if (T < Y0)
    //                        Y0 = T;
    //                    else if (Y1 < T)
    //                        Y1 = T;

    //                    T = XDatas[0];
    //                    if (T < X0)
    //                        X0 = T;

    //                    T = XDatas[XDatas.Count - 1];
    //                    if (X1 < T)
    //                        X1 = T;
    //                }
    //            }

    //            return X0 != int.MaxValue || X1 != int.MinValue || Y0 != int.MaxValue || Y1 != int.MinValue ? new Bound<int>(X0, Y0, X1, Y1) : Bound<int>.Empty;
    //        }
    //    }

    //    protected double OffsetX = 0d;
    //    double IImageContour.OffsetX
    //        => OffsetX;

    //    protected double OffsetY = 0d;
    //    double IImageContour.OffsetY
    //        => OffsetY;

    //    public ImageContourScanLine this[int Y]
    //    {
    //        get
    //        {
    //            if (!Contents.TryGetValue(Y, out ImageContourScanLine ScanLine))
    //            {
    //                ScanLine = new ImageContourScanLine();
    //                Contents.Add(Y, ScanLine);
    //            }

    //            return ScanLine;
    //        }
    //    }

    //    /// <summary>
    //    /// Initializes a new instance.
    //    /// </summary>
    //    public ImageShapeContour()
    //    {
    //        Contents = new Dictionary<int, ImageContourScanLine>();
    //        Children = new List<Tuple<Bitwises, ImageShapeContourContext>>();
    //    }
    //    /// <summary>
    //    /// Initializes a new instance.
    //    /// </summary>
    //    /// <param name="Contour">The initial contour.</param>
    //    public ImageShapeContour(ImageShapeContour Contour)
    //    {
    //        Contents = new Dictionary<int, ImageContourScanLine>(Contour.Contents.ToDictionary(i => i.Key, i => i.Value.Clone()));
    //        Children = new List<Tuple<Bitwises, ImageShapeContourContext>>(Contour.Children.Select(i => new Tuple<Bitwises, ImageShapeContourContext>(i.Item1, i.Item2.Clone())));
    //        OffsetX = Contour.OffsetX;
    //        OffsetY = Contour.OffsetY;
    //    }

    //    public void Union(ImageShapeContour Contour)
    //    {
    //        if (Children.Count == 0)
    //            ConvertToChild();

    //        Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Or, Contour.Clone()));
    //        InvalidateContent();
    //    }

    //    public void Intersection(ImageShapeContour Contour)
    //    {
    //        if (Children.Count == 0)
    //            ConvertToChild();

    //        Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.And, Contour.Clone()));
    //        InvalidateContent();
    //    }

    //    public void Difference(ImageShapeContour Contour)
    //    {
    //        if (Children.Count == 0)
    //            ConvertToChild();

    //        Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Not, Contour.Clone()));
    //        InvalidateContent();
    //    }

    //    public void SymmetricDifference(ImageShapeContour Contour)
    //    {
    //        if (Children.Count == 0)
    //            ConvertToChild();

    //        Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Xor, Contour.Clone()));
    //        InvalidateContent();
    //    }

    //    private void ConvertToChild()
    //        => Children.Add(new Tuple<Bitwises, ImageShapeContourContext>(Bitwises.Or, Clone()));

    //    public void Flip(double Cx, double Cy, FlipMode Flip)
    //    {
    //        foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
    //            Item.Item2.Flip(Cx, Cy, Flip);

    //        InternalFlip(Cx, Cy, Flip);
    //    }
    //    protected internal abstract void InternalFlip(double Cx, double Cy, FlipMode Flip);

    //    /// <summary>
    //    /// Rotates the contour about the specified point.
    //    /// </summary>
    //    /// <param name="Cx">The x-coordinate of the center of rotation.</param>
    //    /// <param name="Cy">The y-coordinate of the center of rotation.</param>
    //    /// <param name="Theta">The angle to rotate specifed in radians.</param>
    //    public void Rotate(double Cx, double Cy, double Theta)
    //    {
    //        foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
    //            Item.Item2.Rotate(Cx, Cy, Theta);

    //        InternalRotate(Cx, Cy, Theta);
    //    }
    //    protected internal abstract void InternalRotate(double Cx, double Cy, double Theta);

    //    /// <summary>
    //    /// Scales this contour around the origin.
    //    /// </summary>
    //    /// <param name="ScaleX">The scale factor in the x dimension.</param>
    //    /// <param name="ScaleY">The scale factor in the y dimension.</param>
    //    public void Scale(double ScaleX, double ScaleY)
    //    {
    //        foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
    //            Item.Item2.Scale(ScaleX, ScaleY);

    //        InternalScale(ScaleX, ScaleY);
    //    }
    //    protected internal abstract void InternalScale(double ScaleX, double ScaleY);

    //    public void Crop(double MinX, double MaxX, double MinY, double MaxY)
    //    {
    //        foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
    //            Item.Item2.Crop(MinX, MaxX, MinY, MaxY);

    //        InternalCrop(MinX, MaxX, MinY, MaxY);

    //        if (IsContentValid)
    //        {
    //            MinY -= OffsetY;
    //            MaxY -= OffsetY;
    //            foreach (int Key in Contents.Keys.ToArray())
    //            {
    //                if (Key < MinY || MaxY < Key)
    //                {
    //                    Contents.Remove(Key);
    //                }
    //                else
    //                {
    //                    int Lx = (int)Math.Floor(MinX - OffsetX),
    //                        Rx = (int)Math.Floor(MaxX - OffsetX);

    //                    Contents[Key].Crop(Lx, Rx);
    //                }
    //            }
    //        }
    //    }
    //    protected internal abstract void InternalCrop(double MinX, double MaxX, double MinY, double MaxY);

    //    public void Offset(double Dx, double Dy)
    //    {
    //        OffsetX += Dx;
    //        OffsetY += Dy;

    //        foreach (Tuple<Bitwises, ImageShapeContourContext> Item in Children)
    //            Item.Item2.Offset(Dx, Dy);

    //        InvalidateContent(false);
    //    }

    //    private bool IsContentValid = false;
    //    protected void InvalidateContent(bool ClearContents)
    //    {
    //        IsContentValid = false;

    //        if (ClearContents)
    //            Contents.Clear();
    //    }

    //    public IEnumerator<KeyValuePair<int, ImageContourScanLine>> GetEnumerator()
    //    {
    //        if (!IsContentValid)
    //            EnsureContents();

    //        return new ImageContourEnumerator(this);
    //    }
    //    IEnumerator IEnumerable.GetEnumerator()
    //        => GetEnumerator();

    //    protected abstract void EnsureContents();

    //    void IImageContour.EnsureContents()
    //    {
    //        if (!IsContentValid)
    //            EnsureContents();
    //    }

    //    private void Build()
    //    {
    //        // Offset
    //        double TOx = Math.Round(OffsetX),
    //               TOy = Math.Round(OffsetY);
    //        OffsetX -= TOx;
    //        OffsetY -= TOy;

    //        int Ox = (int)TOx,
    //            Oy = (int)TOy;
    //        if (Children.Count == 0)
    //        {
    //            if (Oy == 0)
    //            {
    //                foreach (KeyValuePair<int, ImageContourScanLine> Content in Contents)
    //                    Content.Value.Offset(Ox);
    //            }
    //            else
    //            {
    //                ImageContourScanLine ScanLine;
    //                IEnumerable<int> Keys = Oy < 0 ? Contents.Keys.OrderBy(i => i) :
    //                                                 Contents.Keys.OrderByDescending(i => i);
    //                foreach (int Y in Keys)
    //                {
    //                    ScanLine = Contents[Y];
    //                    ScanLine.Offset(Ox);
    //                    Contents.Add(Y + Oy, ScanLine);
    //                    Contents.Remove(Y);
    //                }
    //            }
    //        }
    //        else
    //        {




    //        }

    //        IsContentValid = true;
    //    }

    //    /// <summary>
    //    /// Creates a new contour that is a copy of the current instance.
    //    /// </summary>
    //    public abstract ImageShapeContour Clone();
    //    IImageContour IImageContour.Clone()
    //        => Clone();
    //    object ICloneable.Clone()
    //        => Clone();

    //    /// <summary>
    //    /// Creates a ImageContour that is a copy of the current instance.
    //    /// </summary>
    //    public ImageContour ToImageContour()
    //        => new ImageContour(this);

    //    internal static List<double> CropPoints(double[] Points, double MinX, double MinY, double MaxX, double MaxY)
    //    {
    //        List<double> Output = Points.ToList(),
    //                     Input;

    //        double Sx, Sy, Ex, Ey, Dx, Dy, Tx, Ty;
    //        int Length;

    //        // Left
    //        {
    //            Input = Output;
    //            Output = new List<double>();

    //            Length = Input.Count;
    //            Sx = Input[Length - 2];
    //            Sy = Input[Length - 1];

    //            for (int i = 0; i < Length; i++)
    //            {
    //                Ex = Input[i++];
    //                Ey = Input[i];

    //                if (MinX <= Ex)
    //                {
    //                    if (Sx < MinX)
    //                    {
    //                        Dx = Ex - Sx;
    //                        Dy = Ey - Sy;
    //                        Tx = MinX - Sx;

    //                        Output.Add(MinX);
    //                        Output.Add(Sy + Dy * Tx / Dx);
    //                    }

    //                    Output.Add(Ex);
    //                    Output.Add(Ey);
    //                }
    //                else if (MinX <= Sx)
    //                {
    //                    Dx = Ex - Sx;
    //                    Dy = Ey - Sy;
    //                    Tx = MinX - Sx;

    //                    Output.Add(MinX);
    //                    Output.Add(Sy + Dy * Tx / Dx);
    //                }

    //                Sx = Ex;
    //                Sy = Ey;
    //            }
    //        }

    //        // Top
    //        {
    //            if (Output.Count == 0)
    //                return new List<double>(0);

    //            Input = Output;
    //            Output = new List<double>();

    //            Length = Input.Count;
    //            Sx = Input[Length - 2];
    //            Sy = Input[Length - 1];

    //            for (int i = 0; i < Length; i++)
    //            {
    //                Ex = Input[i++];
    //                Ey = Input[i];

    //                if (MinY <= Ey)
    //                {
    //                    if (Sy < MinY)
    //                    {
    //                        Dx = Ex - Sx;
    //                        Dy = Ey - Sy;
    //                        Ty = MinY - Sy;

    //                        Output.Add(Sx + Dx * Ty / Dy);
    //                        Output.Add(MinY);
    //                    }

    //                    Output.Add(Ex);
    //                    Output.Add(Ey);
    //                }
    //                else if (MinY <= Sy)
    //                {
    //                    Dx = Ex - Sx;
    //                    Dy = Ey - Sy;
    //                    Ty = MinY - Sy;

    //                    Output.Add(Sx + Dx * Ty / Dy);
    //                    Output.Add(MinY);
    //                }

    //                Sx = Ex;
    //                Sy = Ey;
    //            }
    //        }

    //        // Right
    //        {
    //            if (Output.Count == 0)
    //                return new List<double>(0);

    //            Input = Output;
    //            Output = new List<double>();

    //            Length = Input.Count;
    //            Sx = Input[Length - 2];
    //            Sy = Input[Length - 1];

    //            for (int i = 0; i < Length; i++)
    //            {
    //                Ex = Input[i++];
    //                Ey = Input[i];

    //                if (Ex <= MaxX)
    //                {
    //                    if (MaxX < Sx)
    //                    {
    //                        Dx = Ex - Sx;
    //                        Dy = Ey - Sy;
    //                        Tx = MaxX - Sx;

    //                        Output.Add(MaxX);
    //                        Output.Add(Sy + Dy * Tx / Dx);
    //                    }

    //                    Output.Add(Ex);
    //                    Output.Add(Ey);
    //                }
    //                else if (Sx <= MaxX)
    //                {
    //                    Dx = Ex - Sx;
    //                    Dy = Ey - Sy;
    //                    Tx = MaxX - Sx;

    //                    Output.Add(MaxX);
    //                    Output.Add(Sy + Dy * Tx / Dx);
    //                }

    //                Sx = Ex;
    //                Sy = Ey;
    //            }
    //        }

    //        // Bottom
    //        {
    //            if (Output.Count == 0)
    //                return new List<double>(0);

    //            Input = Output;
    //            Output = new List<double>();

    //            Length = Input.Count;
    //            Sx = Input[Length - 2];
    //            Sy = Input[Length - 1];

    //            for (int i = 0; i < Length; i++)
    //            {
    //                Ex = Input[i++];
    //                Ey = Input[i];

    //                if (Ey <= MaxY)
    //                {
    //                    if (MaxY < Sy)
    //                    {
    //                        Dx = Ex - Sx;
    //                        Dy = Ey - Sy;
    //                        Ty = MaxY - Sy;

    //                        Output.Add(Sx + Dx * Ty / Dy);
    //                        Output.Add(MaxY);
    //                    }

    //                    Output.Add(Ex);
    //                    Output.Add(Ey);
    //                }
    //                else if (Sy <= MaxY)
    //                {
    //                    Dx = Ex - Sx;
    //                    Dy = Ey - Sy;
    //                    Ty = MaxY - Sy;

    //                    Output.Add(Sx + Dx * Ty / Dy);
    //                    Output.Add(MaxY);
    //                }

    //                Sx = Ex;
    //                Sy = Ey;
    //            }
    //        }

    //        return Output;
    //    }

    //}
}