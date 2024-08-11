using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal static class ImageContourHelper
    {
        public static void EnsureSimpleConvexPolygonContents(ImageShapeContourContext Context, ref double[] Points, ref double OffsetX, ref double OffsetY)
        {
            StandardizateOffset(Points, ref OffsetX, ref OffsetY, out double Ex, out double Ey);

            Points[0] += OffsetX;
            Points[1] += OffsetY;

            int Sx = (int)Math.Round(Points[0]),
                Sy = (int)Math.Round(Points[1]),
                Lx, Ly, Tx, Ty;

            Lx = Sx;
            Ly = Sy;
            for (int i = 2; i < Points.Length;)
            {
                Points[i] += OffsetX;
                Tx = (int)Math.Round(Points[i++]);

                Points[i] += OffsetY;
                Ty = (int)Math.Round(Points[i++]);

                AddContentBound(Context, Lx, Ly, Tx, Ty);

                Lx = Tx;
                Ly = Ty;
            }

            AddContentBound(Context, Lx, Ly, Sx, Sy);

            OffsetX = Ex;
            OffsetY = Ey;
        }
        private static void AddData(ImageShapeContourContext Context, int X, int Y)
        {
            ImageContourScanLine ScanLine = Context[Y];
            if (ScanLine.Length == 0)
            {
                ScanLine.Datas.Add(X);
                ScanLine.Datas.Add(X);
                return;
            }

            if (X < ScanLine[0])
                ScanLine[0] = X;
            else if (ScanLine[1] < X)
                ScanLine[1] = X;
        }
        private static void AddContentBound(ImageShapeContourContext Context, int X1, int Y1, int X2, int Y2)
        {
            if (Y1 != Y2)
            {
                int Dx = X2 - X1,
                    Dy = Y2 - Y1;

                if (Dx == 0)
                {
                    if (Dy > 0)
                    {
                        for (int j = Y1; j <= Y2; j++)
                            AddData(Context, X2, j);
                    }
                    else
                    {
                        for (int j = Y2; j <= Y1; j++)
                            AddData(Context, X2, j);
                    }
                }
                else
                {
                    GraphicAlgorithm.CalculateBresenhamLine(Dx, Dy, Dx.Abs(), Dy.Abs(), (TDx, TDy) => AddData(Context, X1 + TDx, Y1 + TDy));
                }
            }
        }

        public static void EnsurePolygonContents<T>(ImageShapeContourContext Context, ref T Points, ref double OffsetX, ref double OffsetY)
            where T : IList<double>
        {
            StandardizateOffset(Points, ref OffsetX, ref OffsetY, out double Ex, out double Ey);

            int pn = Points.Count,
                pnh = pn >> 1;

            // Find y min and max (slightly faster than scanning from 0 to height)
            // And set offsets
            int yMin = int.MaxValue,
                yMax = 0;
            for (int i = 0; i < pn;)
            {
                Points[i++] += OffsetX;

                double Ty = Points[i] + OffsetY;
                Points[i++] = Ty;

                int py = (int)Math.Round(Ty);
                if (py < yMin)
                    yMin = py;
                if (py > yMax)
                    yMax = py;
            }

            int[] IntersectionsX = new int[pnh - 1],
                  HorizontalX = new int[pn];

            // Scan line from min to max
            for (int y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                double X0 = Points[0],
                       Y0 = Points[1];

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                int IntersectionCount = 0,
                    HorizontalCount = 0;
                for (int i = 2; i < pn; i += 2)
                {
                    // Next point x, y
                    double X1 = Points[i],
                           Y1 = Points[i + 1];

                    // Is the scanline between the two points
                    if ((Y0 < y && y <= Y1) ||
                        (Y1 < y && y <= Y0))
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        IntersectionsX[IntersectionCount++] = (int)(X0 + (y - Y0) * (X1 - X0) / (Y1 - Y0));
                    }
                    else if (Y0 == Y1 && Y0 == y)
                    {
                        HorizontalX[HorizontalCount++] = (int)X0;
                        HorizontalX[HorizontalCount++] = (int)X1;
                    }

                    X0 = X1;
                    Y0 = Y1;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                int t, j;
                for (int i = 1; i < IntersectionCount; i++)
                {
                    t = IntersectionsX[i];
                    j = i;
                    while (j > 0 && IntersectionsX[j - 1] > t)
                    {
                        IntersectionsX[j] = IntersectionsX[j - 1];
                        j -= 1;
                    }
                    IntersectionsX[j] = t;
                }

                ImageContourScanLine Data = Context[y];
                // Add Intersections Datas
                for (int i = 0; i < IntersectionCount - 1;)
                {
                    Data.Datas.Add(IntersectionsX[i++]);
                    Data.Datas.Add(IntersectionsX[i++]);
                }

                // Add Horizontal Datas
                for (int i = 0; i < HorizontalCount - 1;)
                    Data.Union(HorizontalX[i++], HorizontalX[i++]);
            }

            OffsetX = Ex;
            OffsetY = Ey;
        }

        public static void EnsureEllipseContents(ImageShapeContourContext Context, ref double OffsetX, ref double OffsetY, double Rx, double Ry, double Theta, bool IsCircle)
        {
            StandardizateOffset(OffsetX, OffsetY, out int Cx, out int Cy, out OffsetX, out OffsetY);

            int Ra = (int)Math.Round(Rx, MidpointRounding.AwayFromZero),
                Rb = (int)Math.Round(Ry, MidpointRounding.AwayFromZero);
            GraphicAlgorithm.CalculateBresenhamEllipseContourQuadrantI(Ra, Rb, (Dx, Dy) =>
            {
                int X1 = Cx - Dx,
                    X2 = Cx + Dx;
                ImageContourScanLine Data = Context[Cy - Dy];
                Data.Union(X1, X2);
                Data = Context[Cy + Dy];
                Data.Union(X1, X2);
            });
        }

        public static List<double> CropPoints(List<double> Points, double MinX, double MinY, double MaxX, double MaxY)
        {
            if (Points.Count < 6)
                throw new ArgumentException($"The polygons passed in must have at least 3 points: subject={Points.Count >> 1}");

            List<double> Output = [.. Points],
                         Input;

            double Sx, Sy, Ex, Ey, Dx, Dy, Tx, Ty;
            int Length;

            // Left
            {
                Input = Output;
                Output = [];

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
                    return [];

                Input = Output;
                Output = [];

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
                    return [];

                Input = Output;
                Output = [];

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
                    return [];

                Input = Output;
                Output = [];

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
        /// Standardizate offset<para/>
        /// Real value = Standardizated value + Error value
        /// </summary>
        /// <param name="Points">The coordinate points for deciding on standardization</param>
        /// <param name="OffsetX">The standardizated value in X direction.</param>
        /// <param name="OffsetY">The standardizated value in Y direction.</param>
        /// <param name="ErrorX">The error value in X direction.</param>
        /// <param name="ErrorY">The error value in Y direction.</param>
        public static void StandardizateOffset<T>(T Points, ref double OffsetX, ref double OffsetY, out double ErrorX, out double ErrorY)
            where T : IList<double>
        {
            double MinX = Points[0] + OffsetX,
                   MinY = Points[1] + OffsetY;

            int Length = Points.Count;
            for (int i = 2; i < Length;)
            {
                double Tx = Points[i++] + OffsetX,
                       Ty = Points[i++] + OffsetY;

                if (Tx < MinX)
                    MinX = Tx;

                if (Ty < MinY)
                    MinY = Ty;
            }

            ErrorX = Math.Round(MinX, MidpointRounding.AwayFromZero) - MinX;
            ErrorY = Math.Round(MinY, MidpointRounding.AwayFromZero) - MinY;

            OffsetX -= ErrorX;
            OffsetY -= ErrorY;
        }

        public static void StandardizateOffset(double OffsetX, double OffsetY, out int StandardX, out int StandardY, out double ErrorX, out double ErrorY)
        {
            double DoubleStandardX = Math.Round(OffsetX, MidpointRounding.AwayFromZero),
                   DoubleStandardY = Math.Round(OffsetY, MidpointRounding.AwayFromZero);

            ErrorX = DoubleStandardX - OffsetX;
            ErrorY = DoubleStandardY - OffsetY;

            StandardX = (int)DoubleStandardX;
            StandardY = (int)DoubleStandardY;
        }

    }
}