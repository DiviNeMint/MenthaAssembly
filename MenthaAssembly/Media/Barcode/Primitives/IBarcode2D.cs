using MenthaAssembly.Media.Imaging;

namespace MenthaAssembly.Media.Barcode
{
    public interface IBarcode2D : IBarcode
    {
        public bool TryCreateContour(string Context, int X, int Y, int Width, int Height, double Theta, out ImageContour Contour);

        public void GetBarcodeSize(string Context, out int Width, out int Height);

    }
}