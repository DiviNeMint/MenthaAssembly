namespace MenthaAssembly.Media.Barcode
{
    public static class Barcodes
    {
        // TODO:
        // Other Types
        // https://github.com/Tagliatti/NetBarcode

        public static IBarcode1D Code39 { get; } = new Code39(false);
        
        public static IBarcode1D Code39Mod43 { get; } = new Code39(true);

        public static IBarcode1D Code39Ex { get; } = new Code39Ex(false);

        public static IBarcode1D Code39ExMod43 { get; } = new Code39Ex(true);

    }
}
