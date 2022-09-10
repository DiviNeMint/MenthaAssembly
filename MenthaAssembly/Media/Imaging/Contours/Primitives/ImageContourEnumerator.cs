using System;
using System.Collections;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    internal class ImageContourEnumerator : IEnumerator<KeyValuePair<int, ImageContourScanLine>>
    {
        private readonly IEnumerator<KeyValuePair<int, ImageContourScanLine>> Source;
        private readonly int Ox, Oy;
        public ImageContourEnumerator(IImageContour Contour)
        {
            Source = Contour.Contents.GetEnumerator();
            Ox = (int)Math.Round(Contour.OffsetX);
            Oy = (int)Math.Round(Contour.OffsetY);
        }

        public KeyValuePair<int, ImageContourScanLine> Current
        {
            get
            {
                KeyValuePair<int, ImageContourScanLine> Content = Source.Current;
                return new KeyValuePair<int, ImageContourScanLine>(Content.Key + Oy, ImageContourScanLine.Offset(Content.Value, Ox));
            }
        }

        object IEnumerator.Current
            => Current;

        public bool MoveNext()
            => Source.MoveNext();

        public void Reset()
            => Source.Reset();

        public void Dispose()
            => Source.Dispose();

    }
}