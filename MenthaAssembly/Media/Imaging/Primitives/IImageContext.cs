using MenthaAssembly.Media.Imaging.Primitives;
using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging
{
    public unsafe interface IImageContext
    {
        /// <summary>
        /// Gets the Width of image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the Height of image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the bytes length of a scanline.
        /// </summary>
        public long Stride { get; }

        /// <summary>
        /// Gets the bits length of a pixel.
        /// </summary>
        public int BitsPerPixel { get; }

        /// <summary>
        /// Get the channels of image.
        /// </summary>
        public int Channels { get; }

        /// <summary>
        /// Get the pixel type of image.
        /// </summary>
        public Type PixelType { get; }

        /// <summary>
        /// Get the struct type of image.
        /// </summary>
        public Type StructType { get; }

        public IntPtr Scan0 { get; }

        public IntPtr ScanA { get; }

        public IntPtr ScanR { get; }

        public IntPtr ScanG { get; }

        public IntPtr ScanB { get; }

        /// <summary>
        /// Gets & Setters the pixel at the special location in image.
        /// </summary>
        /// <param name="X">The x-coordinate of the special location.</param>
        /// <param name="Y">The y-coordinate of the special location.</param>
        public IReadOnlyPixel this[int X, int Y] { set; get; }

        /// <summary>
        /// Creates a PixelAdapter.
        /// </summary>
        /// <param name="X">The x-coordinate of start point at adapter.</param>
        /// <param name="Y">The y-coordinate of start point at adapter.</param>
        public PixelAdapter<T> GetAdapter<T>(int X, int Y) where T : unmanaged, IPixel;

        #region Graphic Processing

        #region Line Rendering

        #region Line
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLine<T>(Point<int> P0, Point<int> P1, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawLine<T>(Point<int> P0, Point<int> P1, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="P0">The coordinate of the start.</param>
        /// <param name="P1">The coordinate of the end.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen);
        /// <summary>
        /// Draw an line.
        /// </summary>
        /// <param name="X0">The x-coordinate of the start.</param>
        /// <param name="Y0">The y-coordinate of the start.</param>
        /// <param name="X1">The x-coordinate of the end.</param>
        /// <param name="Y1">The y-coordinate of the end.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen);

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
        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, T Color) where T : unmanaged, IPixel;
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
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, T Color) where T : unmanaged, IPixel;
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
        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
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
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
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
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen);
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
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen);

        #endregion

        #region Curve
        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurve<T>(IList<int> Points, float Tension, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawCurve<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen);
        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawCurve(IList<Point<int>> Points, float Tension, IImageContext Pen);

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public void DrawCurveClosed<T>(IList<int> Points, float Tension, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawCurveClosed<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen);
        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="Points">The points for the curve in x and y.</param>
        /// <param name="Tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IImageContext Pen);

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
        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, T Color) where T : unmanaged, IPixel;
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
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
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
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen);

        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="Color">The color for the spline.</param>
        public void DrawBeziers<T>(IList<int> Points, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawBeziers<T>(IList<int> Points, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="Points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="Pen">The pen with transparent background for the arc.</param>
        public void DrawBeziers(IList<int> Points, IImageContext Pen);

        #endregion

        #endregion

        #region Shape Rendering

        #region Triangle
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st point.</param>
        /// <param name="Y1">The y-coordinate of the 1st point.</param>
        /// <param name="X2">The x-coordinate of the 2nd point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd point.</param>
        /// <param name="X3">The x-coordinate of the 3rd point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd point.</param>
        /// <param name="Color">The color.</param>
        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="P1">The coordinate of the 1st point.</param>
        /// <param name="P2">The coordinate of the 2nd point.</param>
        /// <param name="P3">The coordinate of the 3rd point.</param>
        /// <param name="Color">The color.</param>
        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st point.</param>
        /// <param name="Y1">The y-coordinate of the 1st point.</param>
        /// <param name="X2">The x-coordinate of the 2nd point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd point.</param>
        /// <param name="X3">The x-coordinate of the 3rd point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd point.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="P1">The coordinate of the 1st point.</param>
        /// <param name="P2">The coordinate of the 2nd point.</param>
        /// <param name="P3">The coordinate of the 3rd point.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the 1st point.</param>
        /// <param name="Y1">The y-coordinate of the 1st point.</param>
        /// <param name="X2">The x-coordinate of the 2nd point.</param>
        /// <param name="Y2">The y-coordinate of the 2nd point.</param>
        /// <param name="X3">The x-coordinate of the 3rd point.</param>
        /// <param name="Y3">The y-coordinate of the 3rd point.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen);
        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="P1">The coordinate of the 1st point.</param>
        /// <param name="P2">The coordinate of the 2nd point.</param>
        /// <param name="P3">The coordinate of the 3rd point.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen);

        #endregion

        #region Rectangle
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="Y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="X2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="Y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="Color">The color.</param>
        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="P1">The coordinate of the bounding rectangle's left-top.</param>
        /// <param name="P2">The coordinate of the bounding rectangle's right-bottom.</param>
        /// <param name="Color">The color.</param>
        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="Y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="X2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="Y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="P1">The coordinate of the bounding rectangle's left-top.</param>
        /// <param name="P2">The coordinate of the bounding rectangle's right-bottom.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="X1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="Y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="X2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="Y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen);
        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="P1">The coordinate of the bounding rectangle's left-top.</param>
        /// <param name="P2">The coordinate of the bounding rectangle's right-bottom.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen);

        #endregion

        #region Quad
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
        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="P1">The coordinate of the 1st point.</param>
        /// <param name="P2">The coordinate of the 2nd point.</param>
        /// <param name="P3">The coordinate of the 3rd point.</param>
        /// <param name="P4">The coordinate of the 4th point.</param>
        /// <param name="Color">The color.</param>
        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, T Color) where T : unmanaged, IPixel;
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
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="P1">The coordinate of the 1st point.</param>
        /// <param name="P2">The coordinate of the 2nd point.</param>
        /// <param name="P3">The coordinate of the 3rd point.</param>
        /// <param name="P4">The coordinate of the 4th point.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
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
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen);
        /// <summary>
        /// Draws a quad.
        /// </summary>
        /// <param name="P1">The coordinate of the 1st point.</param>
        /// <param name="P2">The coordinate of the 2nd point.</param>
        /// <param name="P3">The coordinate of the 3rd point.</param>
        /// <param name="P4">The coordinate of the 4th point.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen);

        #endregion

        #region Ellipse
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse<T>(Bound<int> Bound, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Color">The color for the line.</param>
        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Color) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawEllipse<T>(Bound<int> Bound, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawEllipse(Bound<int> Bound, IImageContext Pen);
        /// <summary>        
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen);
        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen);

        /// <summary>
        /// Fill an ellipse.
        /// </summary>
        /// <param name="Bound">The bounding rectangle of ellipse.</param>
        /// <param name="Fill">The color for the line.</param>
        public void FillEllipse<T>(Bound<int> Bound, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Fill an ellipse.
        /// </summary>
        /// <param name="Center">The coordinate of the ellipses center.</param>
        /// <param name="Rx">The radius of the ellipse in x-direction.</param>
        /// <param name="Ry">The radius of the ellipse in y-direction.</param>
        /// <param name="Fill">The color for the ellipses.</param>
        public void FillEllipse<T>(Point<int> Center, int Rx, int Ry, T Fill) where T : unmanaged, IPixel;
        /// <summary>
        /// Fill an ellipse.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the ellipses center.</param>
        /// <param name="Cy">The y-coordinate of the ellipses center.</param>
        /// <param name="LRx">The radius of the ellipse in x-direction.</param>
        /// <param name="LRy">The radius of the ellipse in y-direction.</param>
        /// <param name="Fill">The color for the ellipses.</param>
        public void FillEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Fill) where T : unmanaged, IPixel;

        #endregion

        #region Polygon
        /// <summary>
        /// Draw a regular polygon.
        /// </summary>
        /// <param name="Center">The coordinate of the polygon center.</param>
        /// <param name="Radius">The radius of the polygon.</param>
        /// <param name="VertexNum">The number of the polygons vertex.</param>
        /// <param name="Color">The color for the line.</param>
        /// <param name="StartAngle">The angle of first vertex in the polygon.</param>
        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, T Color, double StartAngle) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw a regular polygon.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the polygon center.</param>
        /// <param name="Cy">The y-coordinate of the polygon center.</param>
        /// <param name="Radius">The radius of the polygon.</param>
        /// <param name="VertexNum">The number of the polygons vertex.</param>
        /// <param name="Color">The color for the line.</param>
        /// <param name="StartAngle">The angle of first vertex in the polygon.</param>
        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, T Color, double StartAngle) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw a regular polygon.
        /// </summary>
        /// <param name="Center">The coordinate of the polygon center.</param>
        /// <param name="Radius">The radius of the polygon.</param>
        /// <param name="VertexNum">The number of the polygons vertex.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        /// <param name="StartAngle">The angle of first vertex in the polygon.</param>
        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, T Fill, double StartAngle) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw a regular polygon.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the polygon center.</param>
        /// <param name="Cy">The y-coordinate of the polygon center.</param>
        /// <param name="Radius">The radius of the polygon.</param>
        /// <param name="VertexNum">The number of the polygons vertex.</param>
        /// <param name="Contour">The stroke for the line.</param>
        /// <param name="Fill">The color for the line.</param>
        /// <param name="StartAngle">The angle of first vertex in the polygon.</param>
        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, T Fill, double StartAngle) where T : unmanaged, IPixel;
        /// <summary>
        /// Draw a regular polygon.
        /// </summary>
        /// <param name="Center">The coordinate of the polygon center.</param>
        /// <param name="Radius">The radius of the polygon.</param>
        /// <param name="VertexNum">The number of the polygons vertex.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        /// <param name="StartAngle">The angle of first vertex in the polygon.</param>
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, double StartAngle);
        /// <summary>
        /// Draw a regular polygon.
        /// </summary>
        /// <param name="Cx">The x-coordinate of the polygon center.</param>
        /// <param name="Cy">The y-coordinate of the polygon center.</param>
        /// <param name="Radius">The radius of the polygon.</param>
        /// <param name="VertexNum">The number of the polygons vertex.</param>
        /// <param name="Pen">The pen with transparent background for the line.</param>
        /// <param name="StartAngle">The angle of first vertex in the polygon.</param>
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, double StartAngle);

        /// <summary>
        /// Fill a polygon.
        /// </summary>
        /// <param name="Vertices">The vertices of the polygon.</param>
        /// <param name="Fill">The color for the line.</param>
        /// <param name="OffsetX">The offset of x-coordinate.</param>
        /// <param name="OffsetY">The offset of y-coordinate.</param>
        public void FillPolygon<T>(IEnumerable<Point<int>> Vertices, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel;
        /// <summary>
        /// Fill a polygon.
        /// </summary>
        /// <param name="VerticeDatas">The vertices of the polygon in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="Fill">The color for the line.</param>
        /// <param name="OffsetX">The offset of x-coordinate.</param>
        /// <param name="OffsetY">The offset of y-coordinate.</param>
        public void FillPolygon<T>(IEnumerable<int> VerticeDatas, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel;

        #endregion

        #region Other
        /// <summary>
        /// Draw a stamp.
        /// </summary>
        /// <param name="Position">The coordinate of center in stamp.</param>
        /// <param name="Stamp">The stamp to draw.</param>
        public void DrawStamp(Point<int> Position, IImageContext Stamp);
        /// <summary>
        /// Draw a stamp.
        /// </summary>
        /// <param name="X">The x-coordinate of center in stamp.</param>
        /// <param name="Y">The y-coordinate of center in stamp.</param>
        /// <param name="Stamp">The stamp to draw.</param>
        public void DrawStamp(int X, int Y, IImageContext Stamp);

        /// <summary>
        /// Fill a contour.
        /// </summary>
        /// <param name="Contour">The contour to draw.</param>
        /// <param name="Fill">The fill color for the contour.</param>
        /// <param name="OffsetX">The offset of x-coordinate.</param>
        /// <param name="OffsetY">The offset of y-coordinate.</param>
        public void FillContour<T>(ImageContour Contour, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel;

        /// <summary>
        /// Fill a region by <paramref name="Predicate"/>.
        /// </summary>
        /// <param name="SeedPoint">The coordinate of seed.</param>
        /// <param name="Fill">The fill color for the region.</param>
        /// <param name="Predicate">The decider of bound.</param>
        public void SeedFill<T>(Point<int> SeedPoint, T Fill, ImagePredicate Predicate) where T : unmanaged, IPixel;
        /// <summary>
        /// Fill a region by <paramref name="Predicate"/>.
        /// </summary>
        /// <param name="SeedX">The x-coordinate of seed.</param>
        /// <param name="SeedY">The y-coordinate of seed.</param>
        /// <param name="Fill">The fill color for the region.</param>
        /// <param name="Predicate">The decider of bound.</param>
        public void SeedFill<T>(int SeedX, int SeedY, T Fill, ImagePredicate Predicate) where T : unmanaged, IPixel;

        #endregion

        #endregion

        #region Text Rendering
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill) where T : unmanaged, IPixel;
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel;
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill) where T : unmanaged, IPixel;
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel;

        #endregion

        #endregion

        #region Transform Processing

        #region Rotate
        /// <summary>
        /// Creates a new rotated <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Interpolation">The algorithm of interpolation.</param>
        /// <param name="Angle">Arbitrary angle (positive = clockwise).</param>
        public ImageContext<T> Rotate<T>(double Angle, InterpolationTypes Interpolation) where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new rotated <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Angle">Arbitrary angle (positive = clockwise).</param>
        /// <param name="Interpolation">The algorithm of interpolation.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Rotate<T>(double Angle, InterpolationTypes Interpolation, ParallelOptions Options) where T : unmanaged, IPixel;

        #endregion

        #region Resize
        /// <summary>
        /// Creates a new resized <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Width">The width of the rectangle that defines the new size.</param>
        /// <param name="Height">The height of the rectangle that defines the new size.</param>
        /// <param name="Interpolation">The algorithm of interpolation.</param>
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation) where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new resized <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Width">The width of the rectangle that defines the new size.</param>
        /// <param name="Height">The height of the rectangle that defines the new size.</param>
        /// <param name="Interpolation">The algorithm of interpolation.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation, ParallelOptions Options) where T : unmanaged, IPixel;

        #endregion

        #region Flip
        /// <summary>
        /// Creates a new flipped <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Mode">The flip mode.</param>
        public ImageContext<T> Flip<T>(FlipMode Mode) where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new flipped <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Mode">The flip mode.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Flip<T>(FlipMode Mode, ParallelOptions Options) where T : unmanaged, IPixel;

        /// <summary>
        /// Creates a new flipped Indexed <see cref="ImageContext{T,U}"/>.
        /// </summary>
        /// <param name="Mode">The flip mode.</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        public ImageContext<T, U> Flip<T, U>(FlipMode Mode) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        /// <summary>
        /// Creates a new flipped Indexed <see cref="ImageContext{T,U}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="Mode">The flip mode.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T, U> Flip<T, U>(FlipMode Mode, ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        #endregion

        #region Crop
        /// <summary>
        /// Creates a new cropped <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height) where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new cropped <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height, ParallelOptions Options) where T : unmanaged, IPixel;

        /// <summary>
        /// Creates a new cropped Indexed <see cref="ImageContext{T,U}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        public ImageContext<T, U> Crop<T, U>(int X, int Y, int Width, int Height) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        /// <summary>
        /// Creates a new cropped Indexed <see cref="ImageContext{T,U}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="X">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="Width">The width of the rectangle that defines the crop region.</param>
        /// <param name="Height">The height of the rectangle that defines the crop region.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T, U> Crop<T, U>(int X, int Y, int Width, int Height, ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        #endregion

        #region Convolute
        /// <summary>
        /// Creates a new convoluted <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Kernel">The kernel used for convolution.</param>
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel) where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new convoluted <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Kernel">The kernel used for convolution.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel, ParallelOptions Options) where T : unmanaged, IPixel;

        #endregion

        #region Filter
        /// <summary>
        /// Creates a new filtered <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Filter">The kernel used for filtering.</param>
        public ImageContext<T> Filter<T>(ImageFilter Filter) where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new filtered <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Filter">The kernel used for filtering.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Filter<T>(ImageFilter Filter, ParallelOptions Options) where T : unmanaged, IPixel;

        #endregion

        #region Cast
        /// <summary>
        /// Creates a new casted <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public ImageContext<T> Cast<T>() where T : unmanaged, IPixel;
        /// <summary>
        /// Creates a new casted <see cref="ImageContext{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T> Cast<T>(ParallelOptions Options) where T : unmanaged, IPixel;

        /// <summary>
        /// Creates a new casted Indexed <see cref="ImageContext{T,U}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        public ImageContext<T, U> Cast<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;
        /// <summary>
        /// Creates a new casted Indexed <see cref="ImageContext{T,U}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public ImageContext<T, U> Cast<T, U>(ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed;

        #endregion

        #region Clear
        public void Clear<T>(T Color) where T : unmanaged, IPixel;
        public void Clear<T>(T Color, ParallelOptions Options) where T : unmanaged, IPixel;

        #endregion

        #endregion

        #region Buffer Processing

        #region BlockCopy
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to a destination array where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in the <paramref name="Dest0"/> where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an unmanaged memory pointer with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific block of pixels to <typeparamref name="T"/> and Copies to an address in memory with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        /// <param name="DestStride">The stride of bytes in <paramref name="Dest0"/>.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel;

        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in the memory pointers.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in the memory pointers.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in target addresses.</param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in target addresses.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options);

        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the destination arrays of specifying channels where starting at a particular offset with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        /// <param name="DestStride">The stride of bytes in destination arrays.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in the memory pointers.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the unmanaged memory pointers of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in the memory pointers.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in target addresses.</param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride);
        /// <summary>
        /// Copies the specific block of pixels to the addresses in memory of specifying channels with a particular stride.
        /// </summary>
        /// <param name="X">The x-coordinate of block.</param>
        /// <param name="Y">The y-coordinate of block.</param>
        /// <param name="Width">The width of block.</param>
        /// <param name="Height">The height of block.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        /// <param name="DestStride">The stride of bytes in target addresses.</param>
        /// <param name="Options">An object that configures the behavior of this operation.<para/>
        /// If it is null, the function will run with default options. </param>
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options);

        #endregion

        #region ScanLineCopy
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, T* Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in destination array where copying should start.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0, int DestOffset) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to the destination array where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The destination buffer.</param>
        /// <param name="DestOffset">The zero-based index in destination array where copying should start.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to an unmanaged memory pointer.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The memory pointer to copy to.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, IntPtr Dest0) where T : unmanaged, IPixel;
        /// <summary>
        /// Casts the specific number of pixels from a particular scanline starting at a particular offset to <typeparamref name="T"/> and Copies to an address in memory.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of pixels to copy.</param>
        /// <param name="Dest0">The target address to copy to.</param>
        public void ScanLineCopy<T>(int X, int Y, int Length, byte* Dest0) where T : unmanaged, IPixel;

        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void ScanLineCopy3(int X, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void ScanLineCopy3(int X, int Y, int Length, byte* DestR, byte* DestG, byte* DestB);

        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the destination arrays of specifying channels where starting at a particular offset.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The destination buffer of A channel.</param>
        /// <param name="DestR">The destination buffer of R channel.</param>
        /// <param name="DestG">The destination buffer of G channel.</param>
        /// <param name="DestB">The destination buffer of B channel.</param>
        /// <param name="DestOffset">The zero-based index in destination arrays where copying should start.</param>
        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the unmanaged memory pointers of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The memory pointer of A channel to copy to.</param>
        /// <param name="DestR">The memory pointer of R channel to copy to.</param>
        /// <param name="DestG">The memory pointer of G channel to copy to.</param>
        /// <param name="DestB">The memory pointer of B channel to copy to.</param>
        public void ScanLineCopy4(int X, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB);
        /// <summary>
        /// Copies the specific number of pixels from a particular scanline starting at a particular offset to the addresses in memory of specifying channels.
        /// </summary>
        /// <param name="X">The zero-based index in scanline where copying should start.</param>
        /// <param name="Y">The y-coordinate of scanline.</param>
        /// <param name="Length">The number of bytes to copy.</param>
        /// <param name="DestA">The target address of A channel to copy to.</param>
        /// <param name="DestR">The target address of R channel to copy to.</param>
        /// <param name="DestG">The target address of G channel to copy to.</param>
        /// <param name="DestB">The target address of B channel to copy to.</param>
        public void ScanLineCopy4(int X, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB);

        #endregion

        #endregion

    }
}