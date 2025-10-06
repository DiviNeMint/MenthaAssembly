namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Specifies the algorithm used for template matching.
    /// </summary>
    public enum TemplateMatchMode
    {
        /// <summary>
        /// Automatically selects the most suitable matching method based on template and image size.
        /// </summary>
        Auto,

        /// <summary>
        /// Performs direct sliding-window comparison (pixel-wise).
        /// </summary>
        SlidingWindow,

        /// <summary>
        /// Performs matching using Fourier Transform for faster computation on larger images.
        /// </summary>
        Fourier

    }
}