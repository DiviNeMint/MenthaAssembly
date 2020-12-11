using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        public void DrawLine(Int32Point P0, Int32Point P1, Pixel Color)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Color);
        public void DrawLine(int X0, int Y0, int X1, int Y1, Pixel Color)
        {
            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            Dictionary<int, int> LeftBound = new Dictionary<int, int>(),
                                 RightBound = new Dictionary<int, int>();
            #region Line Body Bound
            int MaxX = this.Width - 1,
                RTx,
                RTy;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, (Dx, Dy) =>
            {
                RTy = Y0 + Dy;
                if (-1 < RTy && RTy < this.Height)
                {
                    RTx = Math.Min(Math.Max(X0 + Dx, 0), MaxX);

                    // Left
                    if (LeftBound.TryGetValue(RTy, out int LastRx))
                    {
                        if (LastRx > RTx)
                            LeftBound[RTy] = RTx;
                    }
                    else
                    {
                        LeftBound[RTy] = RTx;
                    }

                    // Right
                    if (RightBound.TryGetValue(RTy, out LastRx))
                    {
                        if (LastRx < RTx)
                            RightBound[RTy] = RTx;
                    }
                    else
                    {
                        RightBound[RTy] = RTx;
                    }
                }
            });

            #endregion
            #region Fill
            foreach (KeyValuePair<int, int> Data in RightBound)
            {
                int Y = Data.Key,
                    TRx = Data.Value;
                if (LeftBound.TryGetValue(Y, out int TLx))
                {
                    LeftBound.Remove(Y);
                    this.Operator.ScanLineOverlay(this, TLx, Y, TRx - TLx + 1, Color);
                }
                else
                {
                    this.Operator.SetPixel(this, TRx, Y, Color);
                }
            }
            RightBound.Clear();

            foreach (KeyValuePair<int, int> Data in LeftBound)
                this.Operator.SetPixel(this, Data.Value, Data.Key, Color);

            LeftBound.Clear();

            #endregion
        }
        public void DrawLine(Int32Point P0, Int32Point P1, IImageContext Pen)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Pen);
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
        {
            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            Pixel? Color = null;
            Dictionary<int, int> LeftBound = new Dictionary<int, int>(),
                                 RightBound = new Dictionary<int, int>();
            #region Pen Bound
            int MaxX = this.Width - 1,
                PCx = Pen.Width >> 1,
                PCy = Pen.Height >> 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            for (int j = 0; j < Pen.Height; j++)
            {
                // Found Left Bound
                for (int i = 0; i < Pen.Width; i++)
                {
                    IPixel Pixel = Pen[i, j];
                    if (Pixel.A > 0)
                    {
                        if (!Color.HasValue)
                            Color = this.Operator.ToPixel(Pixel.A, Pixel.R, Pixel.G, Pixel.B);

                        int Tx = i - PCx,
                            Ty = j - PCy;

                        int Predict = DeltaX * Ty - DeltaY * Tx;
                        int Distance = Math.Abs(Predict);

                        if (Predict > 0)    // UpperLine
                        {
                            if (UpperDistance < Distance)
                            {
                                UpperDistance = Distance;
                                DUx = Tx;
                                DUy = Ty;
                            }
                        }
                        else                // LowerLine
                        {
                            if (LowerDistance < Distance)
                            {
                                LowerDistance = Distance;
                                DLx = Tx;
                                DLy = Ty;
                            }
                        }

                        // StartPoint
                        int Rx = Math.Min(Math.Max(Tx + X0, 0), MaxX),
                            Ry = Ty + Y0;
                        if (-1 < Ry && Ry < this.Height)
                        {
                            if (LeftBound.TryGetValue(Ry, out int LastRx))
                            {
                                if (LastRx > Rx)
                                    LeftBound[Ry] = Rx;
                            }
                            else
                            {
                                LeftBound[Ry] = Rx;
                            }
                        }

                        // EndPoint
                        Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                        Ry = Ty + Y1;
                        if (-1 < Ry && Ry < this.Height)
                        {
                            if (LeftBound.TryGetValue(Ry, out int LastRx))
                            {
                                if (LastRx > Rx)
                                    LeftBound[Ry] = Rx;
                            }
                            else
                            {
                                LeftBound[Ry] = Rx;
                            }
                        }
                        break;
                    }
                }

                // Found Right Bound
                for (int i = Pen.Width - 1; i >= 0; i--)
                {
                    byte Alpha = Pen[i, j].A;
                    if (Alpha > 0)
                    {
                        int Tx = i - PCx,
                            Ty = j - PCy;

                        int Predict = DeltaX * Ty - DeltaY * Tx;
                        int Distance = Math.Abs(Predict);

                        if (Predict > 0)    // UpperLine
                        {
                            if (UpperDistance < Distance)
                            {
                                UpperDistance = Distance;
                                DUx = Tx;
                                DUy = Ty;
                            }
                        }
                        else                // LowerLine
                        {
                            if (LowerDistance < Distance)
                            {
                                LowerDistance = Distance;
                                DLx = Tx;
                                DLy = Ty;
                            }
                        }

                        // StartPoint
                        int Rx = Math.Min(Math.Max(Tx + X0, 0), MaxX),
                            Ry = Ty + Y0;

                        if (-1 < Ry && Ry < this.Height)
                        {
                            if (RightBound.TryGetValue(Ry, out int LastRx))
                            {
                                if (LastRx < Rx)
                                    RightBound[Ry] = Rx;
                            }
                            else
                            {
                                RightBound[Ry] = Rx;
                            }
                        }
                        // EndPoint
                        Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                        Ry = Ty + Y1;

                        if (-1 < Ry && Ry < this.Height)
                        {
                            if (RightBound.TryGetValue(Ry, out int LastRx))
                            {
                                if (LastRx < Rx)
                                    RightBound[Ry] = Rx;
                            }
                            else
                            {
                                RightBound[Ry] = Rx;
                            }
                        }
                        break;
                    }
                }
            }

            #endregion
            #region Line Body Bound
            int Ux = X0 + DUx,
                Uy = Y0 + DUy,
                Lx = X0 + DLx,
                Ly = Y0 + DLy,
                RTx, RTy;

            GraphicDeltaHandler FoundLineBodyBound = DeltaX * DeltaY < 0 ?
                new GraphicDeltaHandler(
                    (Dx, Dy) =>
                    {
                        // Right
                        RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                        RTy = Uy + Dy;
                        if (-1 < RTy && RTy < this.Height)
                        {
                            if (RightBound.TryGetValue(RTy, out int LastRx))
                            {
                                if (LastRx < RTx)
                                    RightBound[RTy] = RTx;
                            }
                            else
                            {
                                RightBound[RTy] = RTx;
                            }
                        }

                        // Left
                        RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                        RTy = Ly + Dy;
                        if (-1 < RTy && RTy < this.Height)
                        {
                            if (LeftBound.TryGetValue(RTy, out int LastRx))
                            {
                                if (LastRx > RTx)
                                    LeftBound[RTy] = RTx;
                            }
                            else
                            {
                                LeftBound[RTy] = RTx;
                            }
                        }
                    }) :
                    (Dx, Dy) =>
                    {
                        // Left
                        RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                        RTy = Uy + Dy;
                        if (-1 < RTy && RTy < this.Height)
                        {
                            if (LeftBound.TryGetValue(RTy, out int LastRx))
                            {
                                if (LastRx > RTx)
                                    LeftBound[RTy] = RTx;
                            }
                            else
                            {
                                LeftBound[RTy] = RTx;
                            }
                        }
                        // Right
                        RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                        RTy = Ly + Dy;
                        if (-1 < RTy && RTy < this.Height)
                        {
                            if (RightBound.TryGetValue(RTy, out int LastRx))
                            {
                                if (LastRx < RTx)
                                    RightBound[RTy] = RTx;
                            }
                            else
                            {
                                RightBound[RTy] = RTx;
                            }
                        }
                    };

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, FoundLineBodyBound);

            #endregion
            #region Fill
            foreach (KeyValuePair<int, int> Data in RightBound)
            {
                int Y = Data.Key,
                    TRx = Data.Value;
                if (LeftBound.TryGetValue(Y, out int TLx))
                {
                    LeftBound.Remove(Y);
                    this.Operator.ScanLineOverlay(this, TLx, Y, TRx - TLx + 1, Color.Value);
                }
                else
                {
                    this.Operator.SetPixel(this, TRx, Y, Color.Value);
                }
            }
            RightBound.Clear();

            foreach (KeyValuePair<int, int> Data in LeftBound)
                this.Operator.SetPixel(this, Data.Value, Data.Key, Color.Value);

            LeftBound.Clear();

            #endregion
        }
        public void DrawLineWithStamp(int X0, int Y0, int X1, int Y1, IImageContext Stamp)
        {
            int X = X0 - (Stamp.Width >> 1),
                Y = Y0 - (Stamp.Height >> 1);

            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0;


            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, Math.Abs(DeltaX), Math.Abs(DeltaY), (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Stamp));
        }

        public ImageContour CreateLineContour(int X0, int Y0, int X1, int Y1, IImageContext Pen)
        {
            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            Pixel? Color = null;
            ImageContour Contour = new ImageContour();

            #region Pen Bound
            int MaxX = this.Width - 1,
                PCx = Pen.Width >> 1,
                PCy = Pen.Height >> 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            for (int j = 0; j < Pen.Height; j++)
            {
                // Found Left Bound
                for (int i = 0; i < Pen.Width; i++)
                {
                    IPixel Pixel = Pen[i, j];
                    if (Pixel.A > 0)
                    {
                        if (!Color.HasValue)
                            Color = this.Operator.ToPixel(Pixel.A, Pixel.R, Pixel.G, Pixel.B);

                        int Tx = i - PCx,
                            Ty = j - PCy;

                        int Predict = DeltaX * Ty - DeltaY * Tx;
                        int Distance = Math.Abs(Predict);

                        if (Predict > 0)    // UpperLine
                        {
                            if (UpperDistance < Distance)
                            {
                                UpperDistance = Distance;
                                DUx = Tx;
                                DUy = Ty;
                            }
                        }
                        else                // LowerLine
                        {
                            if (LowerDistance < Distance)
                            {
                                LowerDistance = Distance;
                                DLx = Tx;
                                DLy = Ty;
                            }
                        }

                        // StartPoint
                        int Ry = Ty + Y0;
                        if (-1 < Ry && Ry < this.Height)
                            Contour[Ry].AddLeft(Math.Min(Math.Max(Tx + X0, 0), MaxX));

                        // EndPoint
                        Ry = Ty + Y1;
                        if (-1 < Ry && Ry < this.Height)
                            Contour[Ry].AddLeft(Math.Min(Math.Max(Tx + X1, 0), MaxX));

                        break;
                    }
                }

                // Found Right Bound
                for (int i = Pen.Width - 1; i >= 0; i--)
                {
                    byte Alpha = Pen[i, j].A;
                    if (Alpha > 0)
                    {
                        int Tx = i - PCx,
                            Ty = j - PCy;

                        int Predict = DeltaX * Ty - DeltaY * Tx;
                        int Distance = Math.Abs(Predict);

                        if (Predict > 0)    // UpperLine
                        {
                            if (UpperDistance < Distance)
                            {
                                UpperDistance = Distance;
                                DUx = Tx;
                                DUy = Ty;
                            }
                        }
                        else                // LowerLine
                        {
                            if (LowerDistance < Distance)
                            {
                                LowerDistance = Distance;
                                DLx = Tx;
                                DLy = Ty;
                            }
                        }

                        // StartPoint
                        int Ry = Ty + Y0;
                        if (-1 < Ry && Ry < this.Height)
                            Contour[Ry].AddRight(Math.Min(Math.Max(Tx + X0, 0), MaxX));

                        // EndPoint
                        Ry = Ty + Y1;
                        if (-1 < Ry && Ry < this.Height)
                            Contour[Ry].AddRight(Math.Min(Math.Max(Tx + X1, 0), MaxX));

                        break;
                    }
                }
            }

            #endregion
            #region Line Body Bound
            int Ux = X0 + DUx,
                Uy = Y0 + DUy,
                Lx = X0 + DLx,
                Ly = Y0 + DLy,
                RTy;

            GraphicDeltaHandler FoundLineBodyBound = DeltaX * DeltaY < 0 ?
                new GraphicDeltaHandler(
                    (Dx, Dy) =>
                    {
                        // Right
                        RTy = Uy + Dy;
                        if (-1 < RTy && RTy < this.Height)
                            Contour[RTy].AddRight(Math.Min(Math.Max(Ux + Dx, 0), MaxX));

                        // Left
                        RTy = Ly + Dy;
                        if (-1 < RTy && RTy < this.Height)
                            Contour[RTy].AddLeft(Math.Min(Math.Max(Lx + Dx, 0), MaxX));
                    }) :
                    (Dx, Dy) =>
                    {
                        // Left
                        RTy = Uy + Dy;
                        if (-1 < RTy && RTy < this.Height)
                            Contour[RTy].AddLeft(Math.Min(Math.Max(Ux + Dx, 0), MaxX));

                        // Right
                        RTy = Ly + Dy;
                        if (-1 < RTy && RTy < this.Height)
                            Contour[RTy].AddRight(Math.Min(Math.Max(Lx + Dx, 0), MaxX));
                    };

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY, FoundLineBodyBound);

            #endregion

            return Contour;
        }

        public void DrawStamp(int X, int Y, IImageContext Stamp)
        {
            int Bx = X + Stamp.Width,
                By = Y + Stamp.Height,
                OffsetX = 0,
                OffsetY = 0;

            if (X < 0)
            {
                OffsetX = -X;
                X = 0;
            }

            if (Y < 0)
            {
                OffsetY = -Y;
                Y = 0;
            }

            int Width = Math.Min(Bx, this.Width) - X;
            if (Width < 1)
                return;

            int Height = Math.Min(By, this.Width) - Y;
            if (Height < 1)
                return;

            Operator.BlockOverlay(this, X, Y, Stamp, OffsetX, OffsetY, Width, Height);
        }

        /// <summary>
        /// Draws a colored dotted line
        /// </summary>
        /// <param name="X0">The x-coordinate of the start point.</param>
        /// <param name="Y0">The y-coordinate of the start point.</param>
        /// <param name="X1">The x-coordinate of the end point.</param>
        /// <param name="Y1">The y-coordinate of the end point.</param>
        /// <param name="DotSpace">length of space between each line segment</param>
        /// <param name="DotLength">length of each line segment</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawDottedLine(int X0, int Y0, int X1, int Y1, int DotSpace, int DotLength, Pixel Color)
        {
            if (X0 == X1)       // Vertically
            {
                if (Y1 < Y0)
                    MathHelper.Swap(ref Y0, ref Y1);

                // Draw
                {
                    if (X0 < 0 || X0 > Width)
                        return;

                    bool on = true;
                    int spaceCnt = 0;
                    for (int i = Y0; i <= Y1; i++)
                    {
                        if (i < 1)
                            continue;

                        if (i >= Height)
                            break;

                        if (on)
                        {
                            this.Operator.SetPixel(this, X0, i - 1, Color);

                            on = i % DotLength != 0;
                            spaceCnt = 0;
                        }
                        else
                        {
                            spaceCnt++;
                            on = spaceCnt % DotSpace == 0;
                        }
                    }
                }
            }
            else if (Y0 == Y1)  // Horizontally
            {
                if (X1 < X0)
                    MathHelper.Swap(ref X0, ref X1);

                // Draw
                {
                    if (Y0 < 0 || Y0 > Height)
                        return;

                    bool on = true;
                    int spaceCnt = 0;
                    for (int i = X0; i <= X1; i++)
                    {
                        if (i < 1)
                            continue;

                        if (i >= Width)
                            break;

                        if (Y0 >= Height)
                            break;

                        if (on)
                        {
                            this.Operator.SetPixel(this, i - 1, Y0, Color);

                            on = i % DotLength != 0;
                            spaceCnt = 0;
                        }
                        else
                        {
                            spaceCnt++;
                            on = spaceCnt % DotSpace == 0;
                        }
                    }
                }
            }
            else
            {
                if (X1 < X0)
                {
                    MathHelper.Swap(ref X0, ref X1);
                    MathHelper.Swap(ref Y0, ref Y1);
                }

                float m = (Y1 - Y0) / (float)(X1 - X0),
                      n = Y0 - m * X0;

                bool on = true;
                int spaceCnt = 0;
                for (int i = X0; i <= Width; i++)
                {
                    if (i == 0)
                        continue;

                    int y = (int)(m * i + n);
                    if (y <= 0)
                        continue;

                    if (y >= Height || i >= X1)
                        continue;

                    if (on)
                    {
                        this.Operator.SetPixel(this, i - 1, y - 1, Color);

                        spaceCnt = 0;
                        on = i % DotLength != 0;
                    }
                    else
                    {
                        spaceCnt++;
                        on = spaceCnt % DotSpace == 0;
                    }
                }
            }
        }

        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurve(int[] Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0], Points[1],
                                                   Points[0], Points[1],
                                                   Points[2], Points[3],
                                                   Points[4], Points[5],
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < Points.Length - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                       Points[i], Points[i + 1],
                                                       Points[i + 2], Points[i + 3],
                                                       Points[i + 4], Points[i + 5],
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 2], Points[i - 1],
                                                   Points[i], Points[i + 1],
                                                   Points[i + 2], Points[i + 3],
                                                   Points[i + 2], Points[i + 3],
                                                   Tension,
                                                   DrawHandler);
        }

        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurve(Int32Point[] Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[0].X, Points[0].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < Points.Length - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                   Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public void DrawCurveClosed(int[] points, float tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

            int pn = points.Length;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(points[pn - 2], points[pn - 1],
                                                   points[0], points[1],
                                                   points[2], points[3],
                                                   points[4], points[5],
                                                   tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 2; i < pn - 4; i += 2)
                GraphicAlgorithm.CalculateCurveSegment(points[i - 2],
                                                       points[i - 1], points[i],
                                                       points[i + 1], points[i + 2],
                                                       points[i + 3], points[i + 4],
                                                       points[i + 5],
                                                       tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(points[i - 2],
                                                   points[i - 1], points[i],
                                                   points[i + 1], points[i + 2],
                                                   points[i + 3], points[0],
                                                   points[1],
                                                   tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(points[i], points[i + 1],
                                                   points[i + 2], points[i + 3],
                                                   points[0], points[1],
                                                   points[2], points[3],
                                                   tension,
                                                   DrawHandler);
        }

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurveClosed(Int32Point[] Points, float Tension, Pixel Color)
        {
            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

            int pn = Points.Length;

            // First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[pn - 1].X, Points[pn - 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Points[2].X, Points[2].Y,
                                                   Tension,
                                                   DrawHandler);

            // Middle segments
            int i;
            for (i = 1; i < pn - 2; i++)
                GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X, Points[i - 1].Y,
                                                       Points[i].X, Points[i].Y,
                                                       Points[i + 1].X, Points[i + 1].Y,
                                                       Points[i + 2].X, Points[i + 2].Y,
                                                       Tension,
                                                       DrawHandler);

            // Last segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i - 1].X,
                                                   Points[i - 1].Y, Points[i].X,
                                                   Points[i].Y, Points[i + 1].X,
                                                   Points[i + 1].Y, Points[0].X,
                                                   Points[0].Y,
                                                   Tension,
                                                   DrawHandler);

            // Last-to-First segment
            GraphicAlgorithm.CalculateCurveSegment(Points[i].X, Points[i].Y,
                                                   Points[i + 1].X, Points[i + 1].Y,
                                                   Points[0].X, Points[0].Y,
                                                   Points[1].X, Points[1].Y,
                                                   Tension,
                                                   DrawHandler);
        }

        /// <summary>
        /// Draws a cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="X1">The x-coordinate of the start point.</param>
        /// <param name="Y1">The y-coordinate of the start point.</param>
        /// <param name="Cx1">The x-coordinate of the 1st control point.</param>
        /// <param name="Cy1">The y-coordinate of the 1st control point.</param>
        /// <param name="Cx2">The x-coordinate of the 2nd control point.</param>
        /// <param name="Cy2">The y-coordinate of the 2nd control point.</param>
        /// <param name="X2">The x-coordinate of the end point.</param>
        /// <param name="Y2">The y-coordinate of the end point.</param>
        /// <param name="Color">The color.</param>
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, Pixel Color)
            => GraphicAlgorithm.CalculateBezierLinePoints(X1, Y1,
                                                          Cx1, Cy1,
                                                          Cx2, Cy2,
                                                          X2, Y2,
                                                          (Px1, Py1, Px2, Py2) => DrawLine(Px1, Py1, Px2, Py2, Color));

        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawBeziers(int[] Points, Pixel Color)
        {
            int x1 = Points[0],
                y1 = Points[1],
                x2, y2;

            void DrawHandler(int Px1, int Py1, int Px2, int Py2)
                => DrawLine(Px1, Py1, Px2, Py2, Color);

            for (int i = 2; i + 5 < Points.Length; i += 6)
            {
                x2 = Points[i + 4];
                y2 = Points[i + 5];
                GraphicAlgorithm.CalculateBezierLinePoints(x1, y1,
                                                           Points[i], Points[i + 1],
                                                           Points[i + 2], Points[i + 3],
                                                           x2, y2,
                                                           DrawHandler);

                x1 = x2;
                y1 = y2;
            }
        }

        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, Pixel Color)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Color);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, Pixel Color)
            => GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy,
                                                      Ex - Cx, Ey - Cy,
                                                      Rx, Ry,
                                                      Clockwise,
                                                      (Dx, Dy) => this.Operator.SetPixel(this, Cx + Dx, Cy + Dy, Color));
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Pen);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            ImageContour PenContour = ImageContour.Parse(Pen, out IPixel PenColor);

            ImageContour Contour = new ImageContour();
            GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy, Ex - Cx, Ey - Cy, Rx, Ry, Clockwise, (Dx, Dy) => Contour.Union(PenContour.Offset(Cx + Dx, Cy + Dy)));

            this.Operator.ContourOverlay(this, Contour, this.Operator.ToPixel(PenColor.A, PenColor.R, PenColor.G, PenColor.B));
        }

    }
}
