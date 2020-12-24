using MenthaAssembly.Media.Imaging.Utils;
using System;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st point.</param>
        /// <param name="Y1">The y-coordinate of the 1st point.</param>
        /// <param name="X2">The x-coordinate of the 2nd point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd point.</param>
        /// <param name="X3">The x-coordinate of the 3rd point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd point.</param>
        /// <param name="color">The color.</param>
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, Pixel color)
        {
            DrawLine(X1, Y1, X2, Y2, color);
            DrawLine(X2, Y2, X3, Y3, color);
            DrawLine(X3, Y3, X1, Y1, color);
        }

        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="X1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="Y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="X2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="Y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="Color">The color.</param>
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, Pixel Color)
        {
            // Check boundaries
            if ((X1 < 0 && X2 < 0) ||
                (Y1 < 0 && Y2 < 0) ||
                (X1 >= Width && X2 >= Width) ||
                (Y1 >= Height && Y2 >= Height))
                return;

            // Clamp boundaries
            X1 = Math.Min(Math.Max(0, X1), Width - 1);
            Y1 = Math.Min(Math.Max(0, Y1), Height - 1);
            X2 = Math.Min(Math.Max(0, X2), Width - 1);
            Y2 = Math.Min(Math.Max(0, Y2), Height - 1);

            for (int x = X1; x <= X2; x++)
            {
                this.Operator.SetPixel(this, x, Y1, Color);
                this.Operator.SetPixel(this, x, Y2, Color);
            }

            for (int y = Y1 + 1; y <= Y2; y++)
            {
                this.Operator.SetPixel(this, X1, y, Color);
                this.Operator.SetPixel(this, X2, y, Color);
            }
        }

        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st point.</param>
        /// <param name="Y1">The y-coordinate of the 1st point.</param>
        /// <param name="X2">The x-coordinate of the 2nd point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd point.</param>
        /// <param name="X3">The x-coordinate of the 3rd point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd point.</param>
        /// <param name="X4">The x-coordinate of the 4th point.</param>
        /// <param name="Y4">The y-coordinate of the 4th point.</param>
        /// <param name="Color">The color.</param>
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, Pixel Color)
        {
            DrawLine(X1, Y1, X2, Y2, Color);
            DrawLine(X2, Y2, X3, Y3, Color);
            DrawLine(X3, Y3, X4, Y4, Color);
            DrawLine(X4, Y4, X1, Y1, Color);
        }

        #region Ellipse
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse(Int32Bound Bound, Pixel Color)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Color);
        }
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse(Int32Point Center, int Rx, int Ry, Pixel Color)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Color);
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, Pixel Color)
            => GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry, (Dx, Dy) => this.Operator.SetPixel(this, Cx + Dx, Cy + Dy, Color));
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawEllipse(Int32Bound Bound, IImageContext Pen)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Pen);
        }
        /// <summary>        
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawEllipse(Int32Point Center, int Rx, int Ry, IImageContext Pen)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Pen);
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen)
        {
            int X = Cx - (Pen.Width >> 1),
                Y = Cy - (Pen.Height >> 1);

            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry, (Dx, Dy) => DrawStamp(X + Dx, Y + Dy, Pen));
        }
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawEllipse(Int32Bound Bound, ImageContour Contour, Pixel Fill)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            DrawEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Contour, Fill);
        }
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawEllipse(Int32Point Center, int Rx, int Ry, ImageContour Contour, Pixel Fill)
            => DrawEllipse(Center.X, Center.Y, Rx, Ry, Contour, Fill);
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, Pixel Fill)
        {
            ImageContour EllipseContour = new ImageContour();

            Int32Bound Bound = Contour.Bound;
            ImageContour Stroke = ImageContour.Offset(Contour, Cx - (Bound.Width >> 1), Cy - (Bound.Height >> 1));

            int LastDx = 0,
                LastDy = 0;
            GraphicAlgorithm.CalculateBresenhamEllipse(Rx, Ry,
                (Dx, Dy) =>
                {
                    Stroke.Offset(Dx - LastDx, Dy - LastDy);
                    EllipseContour.Union(Stroke);

                    LastDx = Dx;
                    LastDy = Dy;
                });

            this.Operator.ContourOverlay(this, EllipseContour, Fill, 0, 0);
        }

        /// <summary>
        /// Fill an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Fill">The color for the line.</param>
        public void FillEllipse(Int32Bound Bound, Pixel Fill)
        {
            int Rx = Bound.Width >> 1,
                Ry = Bound.Height >> 1;

            FillEllipse(Bound.Left + Rx, Bound.Top + Ry, Rx, Ry, Fill);
        }
        /// <summary>
        /// Fill an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Fill">The color for the ellipses.</param>
        public void FillEllipse(Int32Point Center, int Rx, int Ry, Pixel Fill)
            => FillEllipse(Center.X, Center.Y, Rx, Ry, Fill);
        /// <summary>
        /// Fill an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Fill">The color for the ellipses.</param>
        public void FillEllipse(int Cx, int Cy, int Rx, int Ry, Pixel Fill)
        {
            // Avoid endless loop
            if (Rx < 1 || Ry < 1)
                return;

            // Skip completly outside objects
            if (Cx - Rx >= Width ||
                Cx + Rx < 0 ||
                Cy - Ry >= Height ||
                Cy + Ry < 0)
                return;

            // Init vars
            int uy, ly, lx, rx;
            int x = Rx;
            int y = 0;
            int xrSqTwo = (Rx * Rx) << 1;
            int yrSqTwo = (Ry * Ry) << 1;
            int xChg = Ry * Ry * (1 - (Rx << 1));
            int yChg = Rx * Rx;
            int err = 0;
            int xStopping = yrSqTwo * Rx;
            int yStopping = 0;

            // Draw first set of points counter clockwise where tangent line slope > -1.
            while (xStopping >= yStopping)
            {
                // Draw 4 quadrant points at once
                // Upper half
                uy = Cy + y;
                // Lower half
                ly = Cy - y - 1;

                // Clip
                if (uy < 0)
                    uy = 0;

                if (uy >= Height)
                    uy = Height - 1;

                if (ly < 0)
                    ly = 0;

                if (ly >= Height)
                    ly = Height - 1;

                rx = Cx + x;
                lx = Cx - x;

                // Clip
                if (rx < 0)
                    rx = 0;
                if (rx >= Width)
                    rx = Width - 1;
                if (lx < 0)
                    lx = 0;
                if (lx >= Width)
                    lx = Width - 1;

                int Length = rx - lx + 1;
                this.Operator.ScanLineOverlay(this, lx, uy, Length, Fill);
                this.Operator.ScanLineOverlay(this, lx, ly, Length, Fill);

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

            // Upper half
            uy = Cy + y;
            // Lower half
            ly = Cy - y;

            // Clip
            if (uy < 0)
                uy = 0;

            if (uy >= Height)
                uy = Height - 1;

            if (ly < 0)
                ly = 0;

            if (ly >= Height)
                ly = Height - 1;

            xChg = Ry * Ry;
            yChg = Rx * Rx * (1 - (Ry << 1));
            err = 0;
            xStopping = 0;
            yStopping = xrSqTwo * Ry;

            // Draw second set of points clockwise where tangent line slope < -1.
            while (xStopping <= yStopping)
            {
                // Draw 4 quadrant points at once
                rx = Cx + x;
                lx = Cx - x;

                // Clip
                if (rx < 0)
                    rx = 0;
                if (rx >= Width)
                    rx = Width - 1;
                if (lx < 0)
                    lx = 0;
                if (lx >= Width)
                    lx = Width - 1;

                // Draw line
                int Length = rx - lx + 1;
                this.Operator.ScanLineOverlay(this, lx, uy, Length, Fill);
                this.Operator.ScanLineOverlay(this, lx, ly, Length, Fill);

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    uy = Cy + y; // Upper half
                    ly = Cy - y; // Lower half
                    if (uy < 0)
                        uy = 0; // Clip
                    if (uy >= Height)
                        uy = Height - 1; // ...
                    if (ly < 0)
                        ly = 0;
                    if (ly >= Height)
                        ly = Height - 1;
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }

        #endregion


        /// <summary>
        /// Draws a filled polygon with or without alpha blending (default = false). 
        /// Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="Points">The points of the polygon in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="Fill">The color for the line.</param>
        public void FillPolygon(int[] Points, Pixel Fill)
        {
            int pn = Points.Length,
                pnh = Points.Length >> 1;

            int[] intersectionsX = new int[pnh];

            // Find y min and max (slightly faster than scanning from 0 to height)
            int yMin = Height;
            int yMax = 0;
            for (int i = 1; i < pn; i += 2)
            {
                int py = Points[i];
                if (py < yMin)
                    yMin = py;
                if (py > yMax)
                    yMax = py;
            }
            if (yMin < 0)
                yMin = 0;
            if (yMax >= Height)
                yMax = Height - 1;

            // Scan line from min to max
            for (int y = yMin; y <= yMax; y++)
            {
                // Initial point x, y
                float vxi = Points[0],
                      vyi = Points[1];

                // Find all intersections
                // Based on http://alienryderflex.com/polygon_fill/
                int intersectionCount = 0;
                for (int i = 2; i < pn; i += 2)
                {
                    // Next point x, y
                    float vxj = Points[i],
                          vyj = Points[i + 1];

                    // Is the scanline between the two points
                    if (vyi < y && vyj >= y ||
                        vyj < y && vyi >= y)
                    {
                        // Compute the intersection of the scanline with the edge (line between two points)
                        intersectionsX[intersectionCount++] = (int)(vxi + (y - vyi) / (vyj - vyi) * (vxj - vxi));
                    }
                    vxi = vxj;
                    vyi = vyj;
                }

                // Sort the intersections from left to right using Insertion sort 
                // It's faster than Array.Sort for this small data set
                int t, j;
                for (int i = 1; i < intersectionCount; i++)
                {
                    t = intersectionsX[i];
                    j = i;
                    while (j > 0 && intersectionsX[j - 1] > t)
                    {
                        intersectionsX[j] = intersectionsX[j - 1];
                        j -= 1;
                    }
                    intersectionsX[j] = t;
                }

                // Fill the pixels between the intersections
                for (int i = 0; i < intersectionCount - 1; i += 2)
                {
                    int x0 = intersectionsX[i],
                        x1 = intersectionsX[i + 1];

                    // Check boundary
                    if (x1 > 0 && x0 < Width)
                    {
                        if (x0 < 0)
                            x0 = 0;

                        if (x1 >= Width)
                            x1 = Width - 1;

                        // Fill the pixels
                        Operator.ScanLineOverlay(this, x0, y, x1 - x0 + 1, Fill);
                    }
                }
            }
        }

        public void FillContour(ImageContour Contour, Pixel Fill, int OffsetX, int OffsetY)
            => this.Operator.ContourOverlay(this, Contour, Fill, OffsetX, OffsetY);

    }

}
