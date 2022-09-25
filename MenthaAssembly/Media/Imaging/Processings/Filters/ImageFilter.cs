namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a filter in image.
    /// </summary>
    public abstract class ImageFilter
    {
        internal int PatchWidth { set; get; }

        internal int PatchHeight { set; get; }

        internal ImageFilter()
        {

        }
        protected ImageFilter(int PatchWidth, int PatchHeight)
        {
            this.PatchHeight = PatchHeight;
            this.PatchWidth = PatchWidth;
        }

        /// <summary>
        /// Filters the specified patch in image.
        /// </summary>
        /// <param name="Patch">The specified patch to be filtered in image.</param>
        /// <param name="Args">The arguments of the filter.</param>
        /// <param name="A">The Alpha-Channel of filtered pixel.</param>
        /// <param name="R">The R-Channel of filtered pixel.</param>
        /// <param name="G">The G-Channel of filtered pixel.</param>
        /// <param name="B">The B-Channel of filtered pixel.</param>
        public abstract void Filter(ImagePatch Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B);

    }
}
