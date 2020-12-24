using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate void GraphicDeltaHandler(int DeltaX, int DeltaY);
    public delegate void GraphicPointsHandler(int X1, int Y1, int X2, int Y2);

    public static class GraphicAlgorithm
    {
        public static void CalculateBresenhamLine(int DeltaX, int DeltaY, int AbsDeltaX, int AbsDeltaY, GraphicDeltaHandler Handler)
        {
            int Inx = DeltaX < 0 ? -1 : DeltaX > 0 ? 1 : 0,
                Iny = DeltaY < 0 ? -1 : DeltaY > 0 ? 1 : 0;

            int pDx, pDy, oDx, oDy, es, el;
            if (AbsDeltaX > AbsDeltaY)
            {
                pDx = Inx;
                pDy = 0;
                oDx = Inx;
                oDy = Iny;
                es = AbsDeltaY;
                el = AbsDeltaX;
            }
            else
            {
                pDx = 0;
                pDy = Iny;
                oDx = Inx;
                oDy = Iny;
                es = AbsDeltaX;
                el = AbsDeltaY;
            }

            int X = 0,
                Y = 0,
                error = el >> 1;

            Handler(X, Y);

            for (int i = 0; i < el; i++)
            {
                error -= es;

                if (error < 0)
                {
                    error += el;
                    X += oDx;
                    Y += oDy;
                }
                else
                {
                    X += pDx;
                    Y += pDy;
                }

                Handler(X, Y);
            }

        }

        public static void CalculateBresenhamEllipse(int Rx, int Ry, GraphicDeltaHandler Handler)
        {
            // Avoid endless loop
            if (Rx < 1 || Ry < 1)
                return;

            // Init vars
            int x = Rx,
                y = 0,
                xrSqTwo = (Rx * Rx) << 1,
                yrSqTwo = (Ry * Ry) << 1,
                xChg = Ry * Ry * (1 - (Rx << 1)),
                yChg = Rx * Rx,
                err = 0,
                xStopping = yrSqTwo * Rx,
                yStopping = 0;

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                Handler(x, y);      // Quadrant I  (Actually an octant)
                Handler(-x, y);     // Quadrant II
                Handler(-x, -y);    // Quadrant III
                Handler(x, -y);     // Quadrant IV

                y++;
                yStopping += xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
                if ((xChg + (err << 1)) > 0)
                {
                    x--;
                    xStopping -= yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                }
            }

            // ReInit vars
            x = 0;
            y = Ry;
            xChg = Ry * Ry;
            yChg = Rx * Rx * (1 - (Ry << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo * Ry;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                Handler(x, y);      // Quadrant I  (Actually an octant)
                Handler(x, -y);     // Quadrant IV
                Handler(-x, y);     // Quadrant II
                Handler(-x, -y);    // Quadrant III

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }

        public static void CalculateBresenhamArc(int DSx, int DSy, int DEx, int DEy, int Rx, int Ry, bool Clockwise, GraphicDeltaHandler Handler)
        {
            // Avoid endless loop
            if (Rx < 1 || Ry < 1)
                return;

            if (DSx == DEx && DSy == DEy)
            {
                CalculateBresenhamEllipse(Rx, Ry, Handler);
                return;
            }

            // Init vars
            int x = Rx,
                y = 0,
                xrSqTwo = (Rx * Rx) << 1,
                yrSqTwo = (Ry * Ry) << 1,
                xChg = Ry * Ry * (1 - (Rx << 1)),
                yChg = Rx * Rx,
                err = 0,
                xStopping = yrSqTwo * Rx,
                yStopping = 0;

            Func<int, int, bool> Filter1, Filter2, Filter3, Filter4;

            if (DSx > 0)
            {
                if (DSy > 0)
                {
                    if (DEx > 0)
                    {
                        if (DEy > 0)
                        {
                            bool Temp = DSx < DEx || DEy < DSy;
                            if (!Temp)
                            {
                                MathHelper.Swap(ref DEx, ref DSx);
                                MathHelper.Swap(ref DEy, ref DSy);
                            }

                            if (Clockwise == Temp)
                            {
                                Filter1 = (Dx, Dy) => (Dx <= DSx && DSy <= Dy) || (DEx <= Dx && Dy <= DEy);
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => true;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => DSx <= Dx && Dx <= DEx && DEy <= Dy && Dy <= DSy;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => false;
                            }
                        }
                        else
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => Dx <= DSx && DSy <= Dy;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => Dx <= DEx && Dy <= DEy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => DSx <= Dx && Dy <= DSy;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => DEx <= Dx && DEy <= Dy;
                            }
                        }
                    }
                    else
                    {
                        if (DEy > 0)
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => Dx <= DSx && DSy <= Dy;
                                Filter2 = (Dx, Dy) => DEx <= Dx && DEy <= Dy;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => false;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => DSx <= Dx && Dy <= DSy;
                                Filter2 = (Dx, Dy) => Dx <= DEx && Dy <= DEy;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => true;
                            }
                        }
                        else
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => Dx <= DSx && DSy <= Dy;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => Dx <= DEx && DEy <= Dy;
                                Filter4 = (Dx, Dy) => false;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => DSx <= Dx && Dy <= DSy;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => DEx <= Dx && Dy <= DEy;
                                Filter4 = (Dx, Dy) => false;
                            }
                        }
                    }
                }
                else
                {
                    if (DEx > 0)
                    {
                        if (DEy > 0)
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => DEx <= Dx && Dy <= DEy;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => DSx <= Dx && DSy <= Dy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => Dx <= DEx && DEy <= Dy;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => Dx <= DSx && Dy <= DSy;
                            }
                        }
                        else
                        {
                            bool Temp = DSx < DEx || DSy < DEy;
                            if (!Temp)
                            {
                                MathHelper.Swap(ref DEx, ref DSx);
                                MathHelper.Swap(ref DEy, ref DSy);
                            }

                            if (Clockwise == Temp)
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => DSx <= Dx && Dx <= DEx && DSy <= Dy && Dy <= DEy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => (Dx <= DSx && Dy <= DSy) || (DEx <= Dx && DEy <= Dy);
                            }
                        }
                    }
                    else
                    {
                        if (DEy > 0)
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => DEx <= Dx && DEy <= Dy;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => DSx <= Dx && DSy <= Dy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => Dx <= DEx && Dy <= DEy;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => Dx <= DSx && Dy <= DSy;
                            }
                        }
                        else
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => Dx <= DEx && DEy <= Dy;
                                Filter4 = (Dx, Dy) => DSx <= Dx && DSy <= Dy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => DEx <= Dx && Dy <= DEy;
                                Filter4 = (Dx, Dy) => Dx <= DSx && Dy <= DSy;
                            }
                        }
                    }
                }
            }
            else
            {
                if (DSy > 0)
                {
                    if (DEx > 0)
                    {
                        if (DEy > 0)
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => DEx <= Dx && Dy <= DEy;
                                Filter2 = (Dx, Dy) => Dx <= DSx && Dy <= DSy;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => true;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => Dx <= DEx && DEy <= Dy;
                                Filter2 = (Dx, Dy) => DSx <= Dx && DSy <= Dy;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => false;
                            }
                        }
                        else
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => Dx <= DSx && Dy <= DSy;
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => Dx <= DEx && Dy <= DEy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => DSx <= Dx && DSy <= Dy;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => DEx <= Dx && DEy <= Dy;
                            }
                        }
                    }
                    else
                    {
                        if (DEy > 0)
                        {
                            bool Temp = DSx < DEx || DSy < DEy;
                            if (!Temp)
                            {
                                MathHelper.Swap(ref DEx, ref DSx);
                                MathHelper.Swap(ref DEy, ref DSy);
                            }

                            if (Clockwise == Temp)
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => (Dx <= DSx && Dy <= DSy) || (DEx <= Dx && DEy <= Dy);
                                Filter3 = (Dx, Dy) => true;
                                Filter4 = (Dx, Dy) => true;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => DSx <= Dx && Dx <= DEx && DSy <= Dy && Dy <= DEy;
                                Filter3 = (Dx, Dy) => false;
                                Filter4 = (Dx, Dy) => false;
                            }
                        }
                        else
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => Dx <= DSx && Dy <= DSy;
                                Filter3 = (Dx, Dy) => Dx <= DEx && DEy <= Dy;
                                Filter4 = (Dx, Dy) => false;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => DSx <= Dx && DSy <= Dy;
                                Filter3 = (Dx, Dy) => DEx <= Dx && Dy <= DEy;
                                Filter4 = (Dx, Dy) => true;
                            }
                        }
                    }
                }
                else
                {
                    if (DEx > 0)
                    {
                        if (DEy > 0)
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => DEx <= Dx && Dy <= DEy;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => DSx <= Dx && Dy <= DSy;
                                Filter4 = (Dx, Dy) => true;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => Dx <= DEx && DEy <= Dy;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => Dx <= DSx && DSy <= Dy;
                                Filter4 = (Dx, Dy) => false;
                            }
                        }
                        else
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => DSx <= Dx && Dy <= DSy;
                                Filter4 = (Dx, Dy) => Dx <= DEx && Dy <= DEy;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => Dx <= DSx && DSy <= Dy;
                                Filter4 = (Dx, Dy) => DEx <= Dx && DEy <= Dy;
                            }
                        }
                    }
                    else
                    {
                        if (DEy > 0)
                        {
                            if (Clockwise)
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => DEx <= Dx && DEy <= Dy;
                                Filter3 = (Dx, Dy) => DSx <= Dx && Dy <= DSy;
                                Filter4 = (Dx, Dy) => true;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => Dx <= DEx && Dy <= DEy;
                                Filter3 = (Dx, Dy) => Dx <= DSx && DSy <= Dy;
                                Filter4 = (Dx, Dy) => false;
                            }
                        }
                        else
                        {
                            bool Temp = DSx < DEx || DEy < DSy;
                            if (!Temp)
                            {
                                MathHelper.Swap(ref DEx, ref DSx);
                                MathHelper.Swap(ref DEy, ref DSy);
                            }

                            if (Clockwise == Temp)
                            {
                                Filter1 = (Dx, Dy) => false;
                                Filter2 = (Dx, Dy) => false;
                                Filter3 = (Dx, Dy) => DSx <= Dx && Dx <= DEx && DEy <= Dy && Dy <= DSy;
                                Filter4 = (Dx, Dy) => false;
                            }
                            else
                            {
                                Filter1 = (Dx, Dy) => true;
                                Filter2 = (Dx, Dy) => true;
                                Filter3 = (Dx, Dy) => (Dx <= DSx && DSy <= Dy) || (DEx <= Dx && Dy <= DEy);
                                Filter4 = (Dx, Dy) => true;
                            }
                        }
                    }
                }
            }

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                if (Filter1(x, y))      // Quadrant I  (Actually an octant)
                    Handler(x, y);
                if (Filter2(-x, y))     // Quadrant II
                    Handler(-x, y);
                if (Filter3(-x, -y))    // Quadrant III
                    Handler(-x, -y);
                if (Filter4(x, -y))     // Quadrant IV
                    Handler(x, -y);

                y++;
                yStopping += xrSqTwo;
                err += yChg;
                yChg += xrSqTwo;
                if ((xChg + (err << 1)) > 0)
                {
                    x--;
                    xStopping -= yrSqTwo;
                    err += xChg;
                    xChg += yrSqTwo;
                }
            }

            // ReInit vars
            x = 0;
            y = Ry;
            xChg = Ry * Ry;
            yChg = Rx * Rx * (1 - (Ry << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo * Ry;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                if (Filter1(x, y))      // Quadrant I  (Actually an octant)
                    Handler(x, y);
                if (Filter4(x, -y))     // Quadrant IV
                    Handler(x, -y);
                if (Filter2(-x, y))     // Quadrant II
                    Handler(-x, y);
                if (Filter3(-x, -y))    // Quadrant III
                    Handler(-x, -y);

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }

        /// <summary>
        /// Calculate a cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="X1">The x-coordinate of the start point.</param>
        /// <param name="Y1">The y-coordinate of the start point.</param>
        /// <param name="Cx1">The x-coordinate of the 1st control point.</param>
        /// <param name="Cy1">The y-coordinate of the 1st control point.</param>
        /// <param name="Cx2">The x-coordinate of the 2nd control point.</param>
        /// <param name="Cy2">The y-coordinate of the 2nd control point.</param>
        /// <param name="X2">The x-coordinate of the end point.</param>
        /// <param name="Y2">The y-coordinate of the end point.</param>
        public static void CalculateBezierLinePoints(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, GraphicPointsHandler Handler)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            int MinX = Math.Min(X1, Math.Min(Cx1, Math.Min(Cx2, X2))),
                MinY = Math.Min(Y1, Math.Min(Cy1, Math.Min(Cy2, Y2))),
                MaxX = Math.Max(X1, Math.Max(Cx1, Math.Max(Cx2, X2))),
                MaxY = Math.Max(Y1, Math.Max(Cy1, Math.Max(Cy2, Y2)));

            // Get slope
            int Len = Math.Max(MaxY - MinY, MaxX - MinX);

            // Prevent division by zero
            if (Len != 0)
            {
                // Init vars
                float Step = 2f / Len;
                int Tx1 = X1,
                    Ty1 = Y1,
                    Tx2, Ty2;

                // Interpolate
                for (float t = Step; t <= 1; t += Step)
                {
                    float tSq = t * t,
                          t1 = 1 - t,
                          t1Sq = t1 * t1;

                    Tx2 = (int)(t1 * t1Sq * X1 + 3 * t * t1Sq * Cx1 + 3 * t1 * tSq * Cx2 + t * tSq * X2);
                    Ty2 = (int)(t1 * t1Sq * Y1 + 3 * t * t1Sq * Cy1 + 3 * t1 * tSq * Cy2 + t * tSq * Y2);

                    Handler(Tx1, Ty1, Tx2, Ty2);

                    Tx1 = Tx2;
                    Ty1 = Ty2;
                }

                // Prevent rounding gap
                Handler(Tx1, Ty1, X2, Y2);
            }
        }

        /// <summary>
        /// Calculate a segment of a Cardinal spline (cubic) defined by four control points.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st control point.</param>
        /// <param name="Y1">The y-coordinate of the 1st control point.</param>
        /// <param name="X2">The x-coordinate of the 2nd control point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd control point.</param>
        /// <param name="X3">The x-coordinate of the 3rd control point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd control point.</param>
        /// <param name="X4">The x-coordinate of the 4th control point.</param>
        /// <param name="Y4">The y-coordinate of the 4th control point.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        public static void CalculateCurveSegment(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, float Tension, GraphicPointsHandler Handler)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            int MinX = Math.Min(X1, Math.Min(X2, Math.Min(X3, X4))),
                MinY = Math.Min(Y1, Math.Min(Y2, Math.Min(Y3, Y4))),
                MaxX = Math.Max(X1, Math.Max(X2, Math.Max(X3, X4))),
                MaxY = Math.Max(Y1, Math.Max(Y2, Math.Max(Y3, Y4)));

            // Get slope
            int Len = Math.Max(MaxY - MinY, MaxX - MinX);

            // Prevent division by zero
            if (Len != 0)
            {
                // Init vars
                float Step = 2f / Len;
                int Tx1 = X2,
                    Ty1 = Y2,
                    Tx2,
                    Ty2;

                // Calculate factors
                float Sx1 = Tension * (X3 - X1),
                      Sy1 = Tension * (Y3 - Y1),
                      Sx2 = Tension * (X4 - X2),
                      Sy2 = Tension * (Y4 - Y2),
                      Ax = Sx1 + Sx2 + 2 * X2 - 2 * X3,
                      Ay = Sy1 + Sy2 + 2 * Y2 - 2 * Y3,
                      Bx = -2 * Sx1 - Sx2 - 3 * X2 + 3 * X3,
                      By = -2 * Sy1 - Sy2 - 3 * Y2 + 3 * Y3;

                // Interpolate
                for (float t = Step; t <= 1; t += Step)
                {
                    float TSq = t * t;

                    Tx2 = (int)(Ax * TSq * t + Bx * TSq + Sx1 * t + X2);
                    Ty2 = (int)(Ay * TSq * t + By * TSq + Sy1 * t + Y2);

                    // Draw line
                    Handler(Tx1, Ty1, Tx2, Ty2);
                    Tx1 = Tx2;
                    Ty1 = Ty2;
                }

                // Prevent rounding gap
                Handler(Tx1, Ty1, X3, Y3);
            }
        }

        public static FloatBound CalculateLineBound(float X0, float Y0, float X1, float Y1)
            => X1 < X0 ? (Y1 < Y0 ? new FloatBound(X1, Y1, X0, Y0) :
                                    new FloatBound(X1, Y0, X0, Y1)) :
                         (Y1 < Y0 ? new FloatBound(X0, Y1, X1, Y0) :
                                    new FloatBound(X0, Y0, X1, Y1));
        public static FloatBound CalculateArcBound(float Sx, float Sy, float Ex, float Ey, float Cx, float Cy, bool Clockwise, out float Radius)
        {
            double SCx = Sx - Cx,
                   SCy = Sy - Cy;
            Radius = (float)Math.Sqrt(SCx * SCx + SCy * SCy);

            if (Cx < Sx)
            {
                if (Cy < Sy)
                {
                    if (Cx < Ex)
                    {
                        if (Cy < Ey)
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius) :
                                                       Temp ? new FloatBound(Sx, Ey, Ex, Sy) :
                                                              new FloatBound(Ex, Sy, Sx, Ey);
                        }
                        else
                        {
                            return Clockwise ? new FloatBound(Cx - Radius, Cy - Radius, Sx < Ex ? Ex : Sx, Cy + Radius) :
                                               new FloatBound(Sx < Ex ? Sx : Ex, Ey, Cx + Radius, Sy);
                        }
                    }
                    else
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Ex, Sy < Ey ? Sy : Ey, Sx, Cy + Radius) :
                                                      new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Sy < Ey ? Ey : Sy)) :
                                         (Clockwise ? new FloatBound(Cx - Radius, Ey, Sx, Cy + Radius) :
                                                      new FloatBound(Ex, Cy - Radius, Cx + Radius, Sy));
                    }
                }
                else
                {
                    if (Cx < Ex)
                    {
                        if (Cy < Ey)
                        {
                            return Clockwise ? new FloatBound(Sx < Ex ? Sx : Ex, Sy, Cx + Radius, Ey) :
                                               new FloatBound(Cx - Radius, Cy - Radius, Sx < Ex ? Ex : Sx, Cy + Radius);
                        }
                        else
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? (Temp ? new FloatBound(Sx, Sy, Ex, Ey) :
                                                               new FloatBound(Ex, Ey, Sx, Sy)) :
                                                       new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius);
                        }
                    }
                    else
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Ex, Sy, Cx + Radius, Cy + Radius) :
                                                      new FloatBound(Cx - Radius, Cy - Radius, Sx, Ey)) :
                                         (Clockwise ? new FloatBound(Cx - Radius, Sy < Ey ? Sy : Ey, Cx + Radius, Cy + Radius) :
                                                      new FloatBound(Ex, Cy - Radius, Sx, Sy < Ey ? Ey : Sy));
                    }
                }
            }
            else
            {
                if (Cy < Sy)
                {
                    if (Cx < Ex)
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Sy < Ey ? Ey : Sy) :
                                                      new FloatBound(Sx, Sy < Ey ? Sy : Ey, Ex, Cy + Radius)) :
                                         (Clockwise ? new FloatBound(Cx - Radius, Cy - Radius, Ex, Sy) :
                                                      new FloatBound(Sx, Ey, Cx + Radius, Cy + Radius));
                    }
                    else
                    {
                        if (Cy < Ey)
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius) :
                                                       Temp ? new FloatBound(Sx, Sy, Ex, Ey) :
                                                              new FloatBound(Ex, Ey, Sx, Sy);
                        }
                        else
                        {
                            return Clockwise ? new FloatBound(Cx - Radius, Ey, Sx < Ex ? Ex : Sx, Sy) :
                                               new FloatBound(Sx < Ex ? Sx : Ex, Cy - Radius, Cx + Radius, Cy + Radius);
                        }
                    }
                }
                else
                {
                    if (Cx < Ex)
                    {
                        return Cy < Ey ? (Clockwise ? new FloatBound(Sx, Cy - Radius, Cx + Radius, Ey) :
                                                      new FloatBound(Cx - Radius, Sy, Ex, Cy + Radius)) :
                                         (Clockwise ? new FloatBound(Sx, Cy - Radius, Ex, Sy < Ey ? Ey : Sy) :
                                                      new FloatBound(Cx - Radius, Sy < Ey ? Sy : Ey, Cx + Radius, Cy + Radius));
                    }
                    else
                    {
                        if (Cy < Ey)
                        {
                            return Clockwise ? new FloatBound(Sx < Ex ? Sx : Ex, Cy - Radius, Cx + Radius, Cy + Radius) :
                                               new FloatBound(Cx - Radius, Sy, Sx < Ex ? Ex : Sx, Ey);
                        }
                        else
                        {
                            bool Temp = Sx < Ex;
                            return Clockwise == Temp ? (Temp ? new FloatBound(Sx, Ey, Ex, Sy) :
                                                               new FloatBound(Ex, Sy, Sx, Ey)) :
                                                       new FloatBound(Cx - Radius, Cy - Radius, Cx + Radius, Cy + Radius);
                        }
                    }
                }
            }
        }

    }
}
