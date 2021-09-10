using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public delegate void GraphicDeltaHandler(int DeltaX, int DeltaY);
    public delegate void GraphicDoubleDeltaHandler(double DeltaX, double DeltaY);
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

        public static void CalculateBresenhamEllipseQuadrantI(int Rx, int Ry, GraphicDeltaHandler Handler)
        {
            // Avoid endless loop
            if (Rx < 1 || Ry < 1)
                return;

            checked
            {
                // Init vars
                int x = Rx,
                    y = 0;
                long LRx = Rx,
                     LRy = Ry,
                     xrSqTwo = (LRx * LRx) << 1,
                     yrSqTwo = (LRy * LRy) << 1,
                     xChg = LRy * LRy * (1 - (LRx << 1)),
                     yChg = LRx * LRx,
                     err = 0,
                     xStopping = yrSqTwo * LRx,
                     yStopping = 0;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xStopping >= yStopping)
                {
                    Handler(x, y);      // Quadrant I  (Actually an octant)

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
                xChg = LRy * LRy;
                yChg = LRx * LRx * (1 - (LRy << 1));
                err = 0;
                xStopping = 0;
                yStopping = xrSqTwo * LRy;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
                {
                    Handler(x, y);      // Quadrant I  (Actually an octant)

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
        }
        public static void CalculateBresenhamEllipse(int Rx, int Ry, GraphicDeltaHandler Handler)
            => CalculateBresenhamEllipseQuadrantI(Rx, Ry, (Dx, Dy) =>
            {
                Handler(Dx, Dy);      // Quadrant I  (Actually an octant)
                Handler(-Dx, Dy);     // Quadrant II
                Handler(-Dx, -Dy);    // Quadrant III
                Handler(Dx, -Dy);     // Quadrant IV
            });

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

            checked
            {
                // Init vars
                int x = Rx,
                    y = 0;
                long LRx = Rx,
                     LRy = Ry,
                     xrSqTwo = (LRx * LRx) << 1,
                     yrSqTwo = (LRy * LRy) << 1,
                     xChg = LRy * LRy * (1 - (LRx << 1)),
                     yChg = LRx * LRx,
                     err = 0,
                     xStopping = yrSqTwo * LRx,
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
                                    Filter2 = (Dx, Dy) => false;
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
                xChg = LRy * LRy;
                yChg = LRx * LRx * (1 - (LRy << 1));
                err = 0;
                xStopping = 0;
                yStopping = xrSqTwo * LRy;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xStopping <= yStopping)
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
        }

        public static void CalculateArcPolygonPoints(double DSx, double DSy, double DEx, double DEy, double Rx, double Ry, bool Clockwise, bool IgnoreEndPoint, GraphicDoubleDeltaHandler Handler)
        {
            double Sa = Math.Atan2(DSy, DSx),
                   TwoPI = 2 * Math.PI,
                   Omega;
            if (Sa < 0)
                Sa += TwoPI;

            if (DSx == DEx && DSy == DEy)
            {
                Omega = Clockwise ? TwoPI : -TwoPI;
            }
            else
            {
                double Ea = Math.Atan2(DEy, DEx);
                if (Ea < 0)
                    Ea += TwoPI;

                Omega = Clockwise ? (Ea > Sa ? Ea - Sa : Ea - Sa + TwoPI) :
                                    (Ea > Sa ? Ea - Sa - TwoPI : Ea - Sa);
            }

            int pts = ((int)Math.Ceiling(Math.Max(Rx, Ry) * Omega)).Abs();
            if (pts < 2)
                pts = 2;

            if (pts > 400)
                pts = 400;

            double Delta = Omega / pts,
                   Theta = Sa;

            for (int i = 0; i < pts; i++)
            {
                Handler(Rx * Math.Cos(Theta), Ry * Math.Sin(Theta));
                Theta += Delta;
            }

            if (!IgnoreEndPoint)
                Handler(Rx * Math.Cos(Theta), Ry * Math.Sin(Theta));
        }
        public static void CalculateArcPolygonPoints(double Sa, double Ea, double Rx, double Ry, bool Clockwise, bool IgnoreEndPoint, GraphicDoubleDeltaHandler Handler)
        {
            double Omega = Sa == Ea ? (Clockwise ? MathHelper.TwoPI : -MathHelper.TwoPI) :
                                      (Clockwise ? (Ea > Sa ? Ea - Sa : Ea - Sa + MathHelper.TwoPI) :
                                                   (Ea > Sa ? Ea - Sa - MathHelper.TwoPI : Ea - Sa));

            int pts = ((int)Math.Ceiling(Math.Max(Rx, Ry) * Omega)).Abs();
            if (pts < 2)
                pts = 2;

            if (pts > 400)
                pts = 400;

            double Delta = Omega / pts,
                   Theta = Sa;

            for (int i = 0; i < pts; i++)
            {
                Handler(Rx * Math.Cos(Theta), Ry * Math.Sin(Theta));
                Theta += Delta;
            }

            if (!IgnoreEndPoint)
                Handler(Rx * Math.Cos(Theta), Ry * Math.Sin(Theta));
        }

        public static void CalculateRegularPolygonVertices(double Radius, int VertexNum, double StartTheta, GraphicDoubleDeltaHandler Handler)
        {
            double DeltaTheta = 360d / VertexNum * MathHelper.UnitTheta,
                   LastTheta = StartTheta;

            for (int i = 0; i < VertexNum; i++)
            {
                LastTheta += DeltaTheta;
                Handler(Radius * Math.Cos(LastTheta), Radius * Math.Sin(LastTheta));
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

        /// <summary>
        /// Crop the specified polygon by the specified rectangle. (By Sutherland-Hodgman Algorithm)
        /// </summary>
        /// <param name="Polygon">The specified polygon be cropped.</param>
        /// <param name="MinX">The left of the specified rectangle to crop.</param>
        /// <param name="MinY">The top of the specified rectangle to crop.</param>
        /// <param name="MaxX">The right of the specified rectangle to crop.</param>
        /// <param name="MaxY">The bottom of the specified rectangle to crop.</param>
        public static IList<int> CropPolygon(IList<int> Polygon, int MinX, int MinY, int MaxX, int MaxY)
        {
            if (Polygon.Count < 6)
                throw new ArgumentException($"The polygons passed in must have at least 3 points: subject={Polygon.Count >> 1}");

            List<int> Output = Polygon.ToList(),
                      Input;

            int Sx, Sy, Ex, Ey, Dx, Dy, Tx, Ty, Length;

            // Left
            {
                Input = Output;
                Output = new List<int>();

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
                    return new int[0];

                Input = Output;
                Output = new List<int>();

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
                    return new int[0];

                Input = Output;
                Output = new List<int>();

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
                    return new int[0];

                Input = Output;
                Output = new List<int>();

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

            // Check Close Region
            if (Output.Count > 0)
            {
                Length = Output.Count;
                Sx = Output[Length - 2];
                Sy = Output[Length - 1];
                Ex = Output[0];
                Ey = Output[1];

                if (!Sx.Equals(Ex) | !Sy.Equals(Ey))
                {
                    Output.Add(Ex);
                    Output.Add(Ey);
                }
            }

            return Output;
        }
        /// <summary>
        /// Crop the specified polygon by the specified rectangle. (By Sutherland-Hodgman Algorithm)
        /// </summary>
        /// <param name="Polygon">The specified polygon be cropped.</param>
        /// <param name="MinX">The left of the specified rectangle to crop.</param>
        /// <param name="MinY">The top of the specified rectangle to crop.</param>
        /// <param name="MaxX">The right of the specified rectangle to crop.</param>
        /// <param name="MaxY">The bottom of the specified rectangle to crop.</param>
        public static IList<Point<int>> CropPolygon(IList<Point<int>> Polygon, int MinX, int MinY, int MaxX, int MaxY)
        {
            if (Polygon.Count < 3)
                throw new ArgumentException($"The polygons passed in must have at least 3 points: subject={Polygon.Count}");

            List<Point<int>> Output = Polygon.ToList(),
                        Input;

            Point<int> S, E;
            int Sx, Sy, Ex, Ey, Dx, Dy, Tx, Ty, Length;

            // Left
            {
                Input = Output;
                Output = new List<Point<int>>();

                Length = Input.Count;
                S = Input[Length - 1];
                Sx = S.X;
                Sy = S.Y;

                for (int i = 0; i < Length; i++)
                {
                    E = Input[i];
                    Ex = E.X;
                    Ey = E.Y;

                    if (MinX <= Ex)
                    {
                        if (Sx < MinX)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Tx = MinX - Sx;

                            int Cy = Sy + Dy * Tx / Dx;

                            Output.Add(new Point<int>(MinX, Cy));
                        }

                        Output.Add(E);
                    }
                    else if (MinX <= Sx)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Tx = MinX - Sx;

                        int Cy = Sy + Dy * Tx / Dx;

                        Output.Add(new Point<int>(MinX, Cy));
                    }

                    Sx = Ex;
                    Sy = Ey;
                }

            }

            // Top
            {
                if (Output.Count == 0)
                    return new Point<int>[0];

                Input = Output;
                Output = new List<Point<int>>();

                Length = Input.Count;
                S = Input[Length - 1];
                Sx = S.X;
                Sy = S.Y;

                for (int i = 0; i < Length; i++)
                {
                    E = Input[i];
                    Ex = E.X;
                    Ey = E.Y;

                    if (MinY <= Ey)
                    {
                        if (Sy < MinY)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Ty = MinY - Sy;

                            int Cx = Sx + Dx * Ty / Dy;
                            Output.Add(new Point<int>(Cx, MinY));
                        }

                        Output.Add(E);
                    }
                    else if (MinY <= Sy)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Ty = MinY - Sy;

                        int Cx = Sx + Dx * Ty / Dy;
                        Output.Add(new Point<int>(Cx, MinY));
                    }

                    Sx = Ex;
                    Sy = Ey;
                }

            }

            // Right
            {
                if (Output.Count == 0)
                    return new Point<int>[0];

                Input = Output;
                Output = new List<Point<int>>();

                Length = Input.Count;
                S = Input[Length - 1];
                Sx = S.X;
                Sy = S.Y;

                for (int i = 0; i < Length; i++)
                {
                    E = Input[i];
                    Ex = E.X;
                    Ey = E.Y;

                    if (Ex <= MaxX)
                    {
                        if (MaxX < Sx)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Tx = MaxX - Sx;

                            int Cy = Sy + Dy * Tx / Dx;

                            Output.Add(new Point<int>(MaxX, Cy));
                        }

                        Output.Add(E);
                    }
                    else if (Sx <= MaxX)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Tx = MaxX - Sx;

                        int Cy = Sy + Dy * Tx / Dx;

                        Output.Add(new Point<int>(MaxX, Cy));
                    }

                    Sx = Ex;
                    Sy = Ey;
                }

            }

            // Bottom
            {
                if (Output.Count == 0)
                    return new Point<int>[0];

                Input = Output;
                Output = new List<Point<int>>();

                Length = Input.Count;
                S = Input[Length - 1];
                Sx = S.X;
                Sy = S.Y;

                for (int i = 0; i < Length; i++)
                {
                    E = Input[i];
                    Ex = E.X;
                    Ey = E.Y;

                    if (Ey <= MaxY)
                    {
                        if (MaxY < Sy)
                        {
                            Dx = Ex - Sx;
                            Dy = Ey - Sy;
                            Ty = MaxY - Sy;

                            int Cx = Sx + Dx * Ty / Dy;
                            Output.Add(new Point<int>(Cx, MaxY));
                        }

                        Output.Add(E);
                    }
                    else if (Sy <= MaxY)
                    {
                        Dx = Ex - Sx;
                        Dy = Ey - Sy;
                        Ty = MaxY - Sy;

                        int Cx = Sx + Dx * Ty / Dy;
                        Output.Add(new Point<int>(Cx, MaxY));
                    }

                    Sx = Ex;
                    Sy = Ey;
                }

            }

            // Check Close Region
            if (Output.Count > 0)
            {
                Length = Output.Count;
                S = Output[Length - 1];
                E = Output[0];

                if (!S.Equals(E))
                    Output.Add(E);
            }

            return Output;
        }

        /// <summary>
        /// This clips the subject polygon against the clip polygon (gets the intersection of the two polygons)
        /// </summary>
        /// <remarks>
        /// Based on the psuedocode from:
        /// http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman
        /// </remarks>
        /// <param name="Polygon">Can be concave or convex</param>
        /// <param name="ClipPolygon">Must be convex</param>
        /// <returns>The intersection of the two polygons (or null)</returns>
        public static int[] CalculateSutherlandHodgmanPolygon(int[] Polygon, int[] ClipPolygon)
        {
            if (Polygon.Length < 6 || ClipPolygon.Length < 6)
                throw new ArgumentException($"The polygons passed in must have at least 3 points: subject={Polygon.Length >> 1}, clip={ClipPolygon.Length >> 1}");

            List<int> Output = ClockwisePolygon(Polygon).ToList();
            int[] Clip = ClockwisePolygon(ClipPolygon).ToArray();

            int ClipLength = Clip.Length,
                LCx = Clip[ClipLength - 2],
                LCy = Clip[ClipLength - 1];

            for (int i = 0; i < ClipLength; i++)
            {
                //	Sometimes when the polygons don't intersect, this list goes to zero.  Jump out to avoid an index out of range exception
                if (Output.Count == 0)
                    return new int[0];

                List<int> Input = Output;
                Output = new List<int>();

                int InputLength = Input.Count,
                    Sx = Input[InputLength - 2],
                    Sy = Input[InputLength - 1],
                    Cx = Clip[i++],
                    Cy = Clip[i],
                    Dx = Cx - LCx,
                    Dy = Cy - LCy;

                for (int j = 0; j < InputLength; j++)
                {
                    int Ex = Input[j++],
                        Ey = Input[j];

                    if (Vector<int>.Cross(Dx, Dy, Ex - Cx, Ey - Cy) <= 0)
                    {
                        if (Vector<int>.Cross(Dx, Dy, Sx - Cx, Sy - Cy) > 0)
                        {
                            if (LineSegment<int>.CrossPoint(Sx, Sy, Ex, Ey, LCx, LCy, Cx, Cy).FirstOrNull() is not Point<int> Cross)
                                throw new ApplicationException("Line segments don't intersect or may be colinear.");

                            Output.Add(Cross.X);
                            Output.Add(Cross.Y);
                        }

                        Output.Add(Ex);
                        Output.Add(Ey);
                    }
                    else if (Vector<int>.Cross(Dx, Dy, Sx - Cx, Sy - Cy) <= 0)
                    {
                        if (LineSegment<int>.CrossPoint(Sx, Sy, Ex, Ey, LCx, LCy, Cx, Cy).FirstOrNull() is not Point<int> Cross)
                            throw new ApplicationException("Line segments don't intersect or may be colinear.");

                        Output.Add(Cross.X);
                        Output.Add(Cross.Y);
                    }

                    Sx = Ex;
                    Sy = Ey;
                }

                LCx = Cx;
                LCy = Cy;
            }

            // Check Close Region
            if (Output.Count > 0)
            {
                int Length = Output.Count,
                    Sx = Output[Length - 2],
                    Sy = Output[Length - 1],
                    Ex = Output[0],
                    Ey = Output[1];

                if (!Sx.Equals(Ex) | !Sy.Equals(Ey))
                {
                    Output.Add(Ex);
                    Output.Add(Ey);
                }
            }

            return Output.ToArray();
        }

        private static IEnumerable<int> ClockwisePolygon(int[] Polygon)
        {
            if (IsClockwise(Polygon))
            {
                foreach (int Data in Polygon)
                    yield return Data;
            }
            else
            {
                int Y;
                for (int i = Polygon.Length - 1; i >= 0; i--)
                {
                    Y = Polygon[i--];
                    yield return Polygon[i];
                    yield return Y;
                }
            }
        }
        private static bool IsClockwise(int[] Polygon)
        {
            int Ex = Polygon[2],
                Ey = Polygon[3],
                Dx = Ex - Polygon[0],
                Dy = Ey - Polygon[1];

            for (int i = 4; i < Polygon.Length; i++)
            {
                int Tx = Polygon[i++],
                    Ty = Polygon[i];

                int Cross = Vector<int>.Cross(Dx, Dy, Tx - Ex, Ty - Ey);
                if (Cross == 0)
                    continue;

                return Cross < 0;
            }

            throw new ArgumentException("All the points in the polygon are colinear");
        }
    }

}