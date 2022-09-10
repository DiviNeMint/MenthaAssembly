using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public interface IImageContour : IEnumerable<KeyValuePair<int, ImageContourScanLine>>
    {
        internal IReadOnlyDictionary<int, ImageContourScanLine> Contents { get; }

        internal double OffsetX { get; }

        internal double OffsetY { get; }

        internal void EnsureContents();

        public Bound<int> Bound { get; }

        public void Flip(double CenterX, double CenterY, FlipMode Flip);

        public void Offset(double DeltaX, double DeltaY);

        public void Crop(double MinX, double MaxX, double MinY, double MaxY);

        public IImageContour Clone();

    }
}