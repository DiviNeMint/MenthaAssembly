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
            Contents = new Dictionary<int, ImageContourScanLine>();
        }
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Contour">The initial contour.</param>
        public ImageShapeContourContext(ImageShapeContourContext Contour)
        {
            Contents = new Dictionary<int, ImageContourScanLine>(Contour.Contents.ToDictionary(i => i.Key, i => i.Value.Clone()));
            OffsetX = Contour.OffsetX;
            OffsetY = Contour.OffsetY;
        }

        public abstract void Flip(double Cx, double Cy, FlipMode Flip);

        /// <summary>
        /// Rotates the contour about the specified point.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the center of rotation.</param>
        /// <param name="Cy">The y-coordinate of the center of rotation.</param>
        /// <param name="Theta">The angle to rotate specifed in radians.</param>
        public abstract void Rotate(double Cx, double Cy, double Theta);

        /// <summary>
        /// Scales this contour around the origin.
        /// </summary>
        /// <param name="ScaleX">The scale factor in the x dimension.</param>
        /// <param name="ScaleY">The scale factor in the y dimension.</param>
        public abstract void Scale(double ScaleX, double ScaleY);

        public abstract void Crop(double MinX, double MaxX, double MinY, double MaxY);

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

        protected internal void EnsureContents()
        {
            if (IsContentValid)
                return;

            InternalEnsureContents();
            IsContentValid = true;
        }
        protected abstract void InternalEnsureContents();

        void IImageContour.EnsureContents()
            => EnsureContents();

        protected internal abstract ImageShapeContourContext InternalClone();
        IImageContour IImageContour.Clone()
            => InternalClone();
        object ICloneable.Clone()
            => InternalClone();

        /// <summary>
        /// Creates a ImageContour that is a copy of the current instance.
        /// </summary>
        public ImageContour ToImageContour()
            => new ImageContour(this);

        internal static List<double> CropPoints(double[] Points, double MinX, double MinY, double MaxX, double MaxY)
        {
            List<double> Output = Points.ToList(),
                         Input;

            double Sx, Sy, Ex, Ey, Dx, Dy, Tx, Ty;
            int Length;

            // Left
            {
                Input = Output;
                Output = new List<double>();

                Length = Input.Count;
                Sx = Input[Length - 2];
                Sy = Input[Length - 1];

                for (int i = 0; i < Length; i++)
                {
                    Ex = Input[i++];
                    Ey = Input[i];

                    if (MinX <= Ex)
                    {
                        if (Sx < MinX)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Tx = MinX - Sx;

                            Output.Add(MinX);
                            Output.Add(Sy + Dy * Tx / Dx);
                        }

                        Output.Add(Ex);
                        Output.Add(Ey);
                    }
                    else if (MinX <= Sx)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Tx = MinX - Sx;

                        Output.Add(MinX);
                        Output.Add(Sy + Dy * Tx / Dx);
                    }

                    Sx = Ex;
                    Sy = Ey;
                }
            }

            // Top
            {
                if (Output.Count == 0)
                    return new List<double>(0);

                Input = Output;
                Output = new List<double>();

                Length = Input.Count;
                Sx = Input[Length - 2];
                Sy = Input[Length - 1];

                for (int i = 0; i < Length; i++)
                {
                    Ex = Input[i++];
                    Ey = Input[i];

                    if (MinY <= Ey)
                    {
                        if (Sy < MinY)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Ty = MinY - Sy;

                            Output.Add(Sx + Dx * Ty / Dy);
                            Output.Add(MinY);
                        }

                        Output.Add(Ex);
                        Output.Add(Ey);
                    }
                    else if (MinY <= Sy)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Ty = MinY - Sy;

                        Output.Add(Sx + Dx * Ty / Dy);
                        Output.Add(MinY);
                    }

                    Sx = Ex;
                    Sy = Ey;
                }
            }

            // Right
            {
                if (Output.Count == 0)
                    return new List<double>(0);

                Input = Output;
                Output = new List<double>();

                Length = Input.Count;
                Sx = Input[Length - 2];
                Sy = Input[Length - 1];

                for (int i = 0; i < Length; i++)
                {
                    Ex = Input[i++];
                    Ey = Input[i];

                    if (Ex <= MaxX)
                    {
                        if (MaxX < Sx)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Tx = MaxX - Sx;

                            Output.Add(MaxX);
                            Output.Add(Sy + Dy * Tx / Dx);
                        }

                        Output.Add(Ex);
                        Output.Add(Ey);
                    }
                    else if (Sx <= MaxX)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Tx = MaxX - Sx;

                        Output.Add(MaxX);
                        Output.Add(Sy + Dy * Tx / Dx);
                    }

                    Sx = Ex;
                    Sy = Ey;
                }
            }

            // Bottom
            {
                if (Output.Count == 0)
                    return new List<double>(0);

                Input = Output;
                Output = new List<double>();

                Length = Input.Count;
                Sx = Input[Length - 2];
                Sy = Input[Length - 1];

                for (int i = 0; i < Length; i++)
                {
                    Ex = Input[i++];
                    Ey = Input[i];

                    if (Ey <= MaxY)
                    {
                        if (MaxY < Sy)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Ty = MaxY - Sy;

                            Output.Add(Sx + Dx * Ty / Dy);
                            Output.Add(MaxY);
                        }

                        Output.Add(Ex);
                        Output.Add(Ey);
                    }
                    else if (Sy <= MaxY)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Ty = MaxY - Sy;

                        Output.Add(Sx + Dx * Ty / Dy);
                        Output.Add(MaxY);
                    }

                    Sx = Ex;
                    Sy = Ey;
                }
            }

            return Output;
        }

    }
}
