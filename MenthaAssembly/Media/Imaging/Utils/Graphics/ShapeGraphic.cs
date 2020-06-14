using System;

namespace MenthaAssembly.Media.Imaging.Primitives
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        /// <summary>
        /// Draws a polyline anti-aliased. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public void DrawPolyline(int[] points, Pixel color)
        {
            int x1 = points[0];
            int y1 = points[1];

            for (int i = 2; i < points.Length; i += 2)
            {
                int x2 = points[i];
                int y2 = points[i + 1];

                DrawLine(x1, y1, x2, y2, color);
                x1 = x2;
                y1 = y2;
            }
        }

        ///// <summary>
        ///// Draws a polyline anti-aliased. Add the first point also at the end of the array if the line should be closed.
        ///// </summary>
        ///// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        ///// <param name="color">The color for the line.</param>
        //public void DrawPolylineAa(int[] points, Pixel color)
        //{
        //    using (var context = bmp.GetBitmapContext())
        //    {
        //        // Use refs for faster access (really important!) speeds up a lot!
        //        var w = context.Width;
        //        var h = context.Height;
        //        int x1 = points[0];
        //        int y1 = points[1];

        //        for (int i = 2; i < points.Length; i += 2)
        //        {
        //            int x2 = points[i];
        //            int y2 = points[i + 1];

        //            DrawLineAa(context, w, h, x1, y1, x2, y2, color);
        //            x1 = x2;
        //            y1 = y2;
        //        }
        //    }
        //}

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
                SetPixel(x, Y1, Color);
                SetPixel(x, Y2, Color);
            }

            for (int y = Y1 + 1; y <= Y2; y++)
            {
                SetPixel(X1, y, Color);
                SetPixel(X2, y, Color);
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

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// x2 has to be greater than x1 and y2 has to be less than y1.
        /// </summary>
        /// <param name="X1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="Y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="X2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="Y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse(int X1, int Y1, int X2, int Y2, Pixel Color)
        {
            // Calc center and radius
            int Rx = (X2 - X1) >> 1,
                Ry = (Y2 - Y1) >> 1;

            DrawEllipseCentered(X1 + Rx, Y1 + Ry, Rx, Ry, Color);
        }

        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf 
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipseCentered(int Cx, int Cy, int Rx, int Ry, Pixel Color)
        {
            // Avoid endless loop
            if (Rx < 1 || Ry < 1)
                return;

            // Init vars
            int uy, ly, lx, rx,
                x = Rx,
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
                uy = Cy + y;                  // Upper half
                ly = Cy - y;                  // Lower half

                rx = Cx + x;
                lx = Cx - x;

                if (0 <= uy && uy < Height)
                {
                    if (0 <= rx && rx < Width) SetPixel(rx, uy, Color);      // Quadrant I (Actually an octant)
                    if (0 <= lx && lx < Width) SetPixel(lx, uy, Color);      // Quadrant II
                }

                if (0 <= ly && ly < Height)
                {
                    if (0 <= lx && lx < Width) SetPixel(lx, ly, Color);      // Quadrant III
                    if (0 <= rx && rx < Width) SetPixel(rx, ly, Color);      // Quadrant IV
                }

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
            uy = Cy + y;                  // Upper half
            ly = Cy - y;                  // Lower half
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
                if (0 <= rx && rx < Width)
                {
                    if (0 <= uy && uy < Height) SetPixel(rx, uy, Color);      // Quadrant I (Actually an octant)
                    if (0 <= ly && ly < Height) SetPixel(rx, ly, Color);      // Quadrant IV
                }

                lx = Cx - x;
                if (0 <= lx && lx < Width)
                {
                    if (0 <= uy && uy < Height) SetPixel(lx, uy, Color);      // Quadrant II
                    if (0 <= ly && ly < Height) SetPixel(lx, ly, Color);      // Quadrant III
                }

                x++;
                xStopping += yrSqTwo;
                err += xChg;
                xChg += yrSqTwo;
                if ((yChg + (err << 1)) > 0)
                {
                    y--;
                    uy = Cy + y;                  // Upper half
                    ly = Cy - y;                  // Lower half
                    yStopping -= xrSqTwo;
                    err += yChg;
                    yChg += xrSqTwo;
                }
            }
        }

    }

}
