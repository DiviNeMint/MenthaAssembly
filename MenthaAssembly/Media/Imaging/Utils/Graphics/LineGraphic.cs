using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        #region Line
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLine(Int32Point P0, Int32Point P1, Pixel Color)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Color);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Color">The color for the line.</param>
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
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawLine(Int32Point P0, Int32Point P1, IImageContext Pen)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Pen);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
        {
            int X = X0 - (Pen.Width >> 1),
                Y = Y0 - (Pen.Height >> 1);

            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0;

            GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, Math.Abs(DeltaX), Math.Abs(DeltaY), (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen));
        }
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawLine(Int32Point P0, Int32Point P1, ImageContour Contour, Pixel Fill)
            => DrawLine(P0.X, P0.Y, P1.X, P1.Y, Contour, Fill);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, ImageContour Contour, Pixel Fill)
        {
            Int32Bound Bound = Contour.Bound;
            if (Bound.IsEmpty)
                return;

            if (Bound.Width == 1 && Bound.Height == 1)
            {
                DrawLine(X0, Y0, X1, Y1, Fill);
                return;
            }

            if (X1 < X0)
            {
                MathHelper.Swap(ref X0, ref X1);
                MathHelper.Swap(ref Y0, ref Y1);
            }
            int DeltaX = X1 - X0,
                DeltaY = Y1 - Y0,
                AbsDeltaY = Math.Abs(DeltaY);

            bool IsHollow = false;
            Dictionary<int, int> LeftBound = new Dictionary<int, int>(),
                                 RightBound = new Dictionary<int, int>();
            #region Pen Bound
            int MaxX = this.Width - 1,
                PCx = Bound.Width >> 1,
                PCy = Bound.Height >> 1,
                DUx = 0,
                DUy = 0,
                DLx = 0,
                DLy = 0,
                UpperDistance = 0,
                LowerDistance = 0;

            foreach (KeyValuePair<int, ContourData> Item in Contour)
            {
                int j = Item.Key;
                ContourData Data = Item.Value;
                if (Data.Count > 2)
                {
                    IsHollow = true;
                    LeftBound.Clear();
                    RightBound.Clear();
                    break;
                }

                int Ty = j - PCy;
                // Found Left Bound
                {
                    int Tx = Data[0] - PCx,
                        Predict = DeltaX * Ty - DeltaY * Tx,
                        Distance = Math.Abs(Predict);

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
                    if (-1 < Ry && Ry < this.Height &&
                        (!LeftBound.TryGetValue(Ry, out int LastRx) || LastRx > Rx))
                        LeftBound[Ry] = Rx;

                    // EndPoint
                    Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                    Ry = Ty + Y1;
                    if (-1 < Ry && Ry < this.Height &&
                        (!LeftBound.TryGetValue(Ry, out LastRx) || LastRx > Rx))
                        LeftBound[Ry] = Rx;
                }

                // Found Right Bound
                {
                    int Tx = Data[Data.Count - 1] - PCx,
                        Predict = DeltaX * Ty - DeltaY * Tx,
                        Distance = Math.Abs(Predict);

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

                    if (-1 < Ry && Ry < this.Height &&
                        (!RightBound.TryGetValue(Ry, out int LastRx) || LastRx < Rx))
                        RightBound[Ry] = Rx;

                    // EndPoint
                    Rx = Math.Min(Math.Max(Tx + X1, 0), MaxX);
                    Ry = Ty + Y1;

                    if (-1 < Ry && Ry < this.Height &&
                        (!RightBound.TryGetValue(Ry, out LastRx) || LastRx < Rx))
                        RightBound[Ry] = Rx;
                }
            }

            #endregion

            if (IsHollow)
            {
                #region Line Body Bound
                ImageContour LineContour = new ImageContour(),
                             Stroke = ImageContour.Offset(Contour, X0 - PCx, Y0 - PCy);

                int LastDx = 0,
                    LastDy = 0;

                GraphicAlgorithm.CalculateBresenhamLine(DeltaX, DeltaY, DeltaX, AbsDeltaY,
                    (Dx, Dy) =>
                    {
                        Stroke.Offset(Dx - LastDx, Dy - LastDy);
                        LineContour.Union(Stroke);

                        LastDx = Dx;
                        LastDy = Dy;
                    });
                #endregion

                this.Operator.ContourOverlay(this, LineContour, Fill, 0, 0);
            }
            else
            {
                #region Line Body Bound
                int Ux = X0 + DUx,
                    Uy = Y0 + DUy,
                    Lx = X0 + DLx,
                    Ly = Y0 + DLy,
                    RTx, RTy;

                if (DeltaX == 0 && DeltaY < 0)
                {
                    MathHelper.Swap(ref Ux, ref Lx);
                    MathHelper.Swap(ref Uy, ref Ly);
                }

                GraphicDeltaHandler FoundLineBodyBound = DeltaX * DeltaY < 0 ?
                    new GraphicDeltaHandler(
                        (Dx, Dy) =>
                        {
                            // Right
                            RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                            RTy = Uy + Dy;
                            if (-1 < RTy && RTy < this.Height &&
                                (!RightBound.TryGetValue(RTy, out int LastRx) || LastRx < RTx))
                                RightBound[RTy] = RTx;

                            // Left
                            RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                            RTy = Ly + Dy;
                            if (-1 < RTy && RTy < this.Height &&
                                (!LeftBound.TryGetValue(RTy, out LastRx) || LastRx > RTx))
                                LeftBound[RTy] = RTx;
                        }) :
                        (Dx, Dy) =>
                        {
                            // Left
                            RTx = Math.Min(Math.Max(Ux + Dx, 0), MaxX);
                            RTy = Uy + Dy;
                            if (-1 < RTy && RTy < this.Height &&
                                (!LeftBound.TryGetValue(RTy, out int LastRx) || LastRx > RTx))
                                LeftBound[RTy] = RTx;

                            // Right
                            RTx = Math.Min(Math.Max(Lx + Dx, 0), MaxX);
                            RTy = Ly + Dy;
                            if (-1 < RTy && RTy < this.Height &&
                                (!RightBound.TryGetValue(RTy, out LastRx) || LastRx < RTx))
                                RightBound[RTy] = RTx;
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
                        this.Operator.ScanLineOverlay(this, TLx, Y, TRx - TLx + 1, Fill);
                    }
                    else
                    {
                        this.Operator.SetPixel(this, TRx, Y, Fill);
                    }
                }
                RightBound.Clear();

                foreach (KeyValuePair<int, int> Data in LeftBound)
                    this.Operator.SetPixel(this, Data.Value, Data.Key, Fill);

                LeftBound.Clear();

                #endregion
            }
        }

        #endregion

        #region Arc
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Start">The coordinate of the start.</param>
        /// <param name="End">The coordinate of the end.</param>
        /// <param name="Center">The coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Color">The color for the arc.</param>
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, Pixel Color)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Color);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start.</param>
        /// <param name="Sy">The y-coordinate of the start.</param>
        /// <param name="Ex">The x-coordinate of the end.</param>
        /// <param name="Ey">The y-coordinate of the end.</param>
        /// <param name="Cx">The x-coordinate of the arc center point.</param>
        /// <param name="Cy">The y-coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Color">The color for the arc.</param>
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, Pixel Color)
            => GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy,
                                                      Ex - Cx, Ey - Cy,
                                                      Rx, Ry,
                                                      Clockwise,
                                                      (Dx, Dy) => this.Operator.SetPixel(this, Cx + Dx, Cy + Dy, Color));
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Start">The coordinate of the start.</param>
        /// <param name="End">The coordinate of the end.</param>
        /// <param name="Center">The coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>        
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Pen);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start.</param>
        /// <param name="Sy">The y-coordinate of the start.</param>
        /// <param name="Ex">The x-coordinate of the end.</param>
        /// <param name="Ey">The y-coordinate of the end.</param>
        /// <param name="Cx">The x-coordinate of the arc center point.</param>
        /// <param name="Cy">The y-coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            int X = Cx - (Pen.Width >> 1),
                Y = Cy - (Pen.Height >> 1);

            GraphicAlgorithm.CalculateBresenhamArc(Sx - Cx, Sy - Cy, Ex - Cx, Ey - Cy, Rx, Ry, Clockwise,
                (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen));
        }
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Start">The coordinate of the start.</param>
        /// <param name="End">The coordinate of the end.</param>
        /// <param name="Center">The coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, Pixel Fill)
            => DrawArc(Start.X, Start.Y, End.X, End.Y, Center.X, Center.Y, Rx, Ry, Clockwise, Contour, Fill);
        /// <summary>
        /// Draw an Arc.
        /// </summary>
        /// <param name="Sx">The x-coordinate of the start.</param>
        /// <param name="Sy">The y-coordinate of the start.</param>
        /// <param name="Ex">The x-coordinate of the end.</param>
        /// <param name="Ey">The y-coordinate of the end.</param>
        /// <param name="Cx">The x-coordinate of the arc center point.</param>
        /// <param name="Cy">The y-coordinate of the arc center point.</param>
        /// <param name="Rx">The x-length of the radius.</param>
        /// <param name="Ry">The y-length of the radius.</param>
        /// <param name="Clockwise">The clockwise for the arc.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, Pixel Fill)
        {
            ImageContour ArcContour = new ImageContour();
            Int32Bound Bound = Contour.Bound;

            if (Bound.IsEmpty)
                return;

            if (Bound.Width == 1 && Bound.Height == 1)
            {
                DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Fill);
                return;
            }

            bool IsHollow = Contour.Any(i => i.Value.Count > 2);
            int MaxX = this.Width - 1,
                PCx = Bound.Width >> 1,
                PCy = Bound.Height >> 1,
                DSx = Sx - Cx,
                DSy = Sy - Cy,
                DEx = Ex - Cx,
                DEy = Ey - Cy;

            if (IsHollow)
            {
                ImageContour Stroke = ImageContour.Offset(Contour, Cx - PCx, Cy - PCy);

                int LastDx = 0,
                    LastDy = 0;
                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise,
                    (Dx, Dy) =>
                    {
                        Stroke.Offset(Dx - LastDx, Dy - LastDy);
                        ArcContour.Union(Stroke);

                        LastDx = Dx;
                        LastDy = Dy;
                    });
            }
            else
            {
                Dictionary<int, int> LargeLeftBound = new Dictionary<int, int>(),
                                     LargeRightBound = new Dictionary<int, int>(),
                                     SmallLeftBound = new Dictionary<int, int>(),
                                     SmallRightBound = new Dictionary<int, int>();

                GraphicAlgorithm.CalculateBresenhamArc(DSx, DSy, DEx, DEy, Rx, Ry, Clockwise,
                   (Dx, Dy) =>
                   {
                       int OffsetX = Dx + Cx - PCx,
                           OffsetY = Dy + Cy - PCy;
                       if (Dx < 0)
                       {
                           foreach (KeyValuePair<int, ContourData> item in Contour)
                           {
                               ContourData Data = item.Value;
                               int Ty = item.Key + OffsetY;

                               if (Ty < 0)
                                   continue;

                               if (this.Height <= Ty)
                                   break;

                               int LLTx = Data[0] + OffsetX,
                                   MLTx = Data[1] + OffsetX;

                               if (!LargeLeftBound.TryGetValue(Ty, out int RLLx) || LLTx < RLLx)
                                   LargeLeftBound[Ty] = LLTx;

                               if (!SmallLeftBound.TryGetValue(Ty, out int RMLx) || RMLx < MLTx)
                                   SmallLeftBound[Ty] = MLTx;
                           }
                       }
                       else
                       {
                           foreach (KeyValuePair<int, ContourData> item in Contour)
                           {
                               ContourData Data = item.Value;
                               int Ty = item.Key + OffsetY;

                               if (Ty < 0)
                                   continue;

                               if (this.Height <= Ty)
                                   break;

                               int LRTx = Data[1] + OffsetX,
                                   MRTx = Data[0] + OffsetX;

                               if (!LargeRightBound.TryGetValue(Ty, out int RLRx) || RLRx < LRTx)
                                   LargeRightBound[Ty] = LRTx;

                               if (!SmallRightBound.TryGetValue(Ty, out int RMRx) || MRTx < RMRx)
                                   SmallRightBound[Ty] = MRTx;
                           }
                       }
                   });

                foreach (KeyValuePair<int, int> item in LargeLeftBound)
                {
                    int X0 = item.Value,
                        Y = item.Key;

                    if (SmallLeftBound.TryGetValue(Y, out int X1))
                    {
                        ArcContour[Y].Union(X0, X1);
                        SmallLeftBound.Remove(Y);
                        continue;
                    }

                    if (LargeRightBound.TryGetValue(Y, out X1))
                    {
                        ArcContour[Y].Union(X0, X1);
                        LargeRightBound.Remove(Y);
                    }
                }
                LargeLeftBound.Clear();

                foreach (KeyValuePair<int, int> item in SmallRightBound)
                {
                    int X0 = item.Value,
                        Y = item.Key;

                    if (LargeRightBound.TryGetValue(Y, out int X1))
                    {
                        ArcContour[Y].Union(X0, X1);
                        LargeRightBound.Remove(Y);
                    }
                }
                SmallRightBound.Clear();
            }

            this.Operator.ContourOverlay(this, ArcContour, Fill, 0, 0);
        }

        #endregion

        #region Curve
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

        #endregion

        #region Bezier
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

        #endregion

        #region Other
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

        #endregion

    }
}
