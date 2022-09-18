namespace MenthaAssembly.Media.Imaging
{
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

        public abstract void Filter(ImagePatch Patch, ImageFilterArgs Args, out byte A, out byte R, out byte G, out byte B);

    }
}
