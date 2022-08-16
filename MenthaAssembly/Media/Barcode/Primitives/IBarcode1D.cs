using MenthaAssembly.Media.Imaging;

namespace MenthaAssembly.Media.Barcode
{
    public interface IBarcode1D : IBarcode
    {
        public bool TryCreateContour(string Context, int X, int Y, int NarrowBarWidth, int WideBarWidth, int Height, double Theta, out ImageContour Contour);

        public double GetBarcodeWidth(string Context, double NarrowBarWidth, double WideBarWidth);

    }
}
