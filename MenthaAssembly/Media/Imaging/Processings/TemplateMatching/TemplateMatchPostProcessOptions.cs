namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Defines post-processing options for template matching results.
    /// Includes non-maximum suppression and subpixel refinement settings.
    /// </summary>
    public class TemplateMatchPostProcessOptions
    {
        /// <summary>
        /// Enables or disables non-maximum suppression (NMS).
        /// When enabled, only the strongest response within a local region is kept.
        /// </summary>
        public bool EnableNonMaximumSuppression { get; set; } = true;

        /// <summary>
        /// The radius (in pixels) used for non-maximum suppression.
        /// Peaks within this radius around a stronger response will be suppressed.
        /// Typical values are between 2 and 5.
        /// </summary>
        public int NmsRadius { get; set; } = 3;

        ///// <summary>
        ///// Enables or disables subpixel peak refinement using quadratic fitting.
        ///// </summary>
        //public bool EnableSubpixelRefine { get; set; } = false;

        ///// <summary>
        ///// The size of the neighborhood used for quadratic surface fitting.
        ///// Common values are 3 (3×3) or 5 (5×5).
        ///// </summary>
        //public int SubpixelWindowSize { get; set; } = 3;

        ///// <summary>
        ///// If true, enforces boundary checks to ensure the refinement window
        ///// stays within the correlation map bounds.
        ///// </summary>
        //public bool ClampSubpixelToBounds { get; set; } = true;

        internal TemplateMatchPostProcessOptions()
        {
        }

    }

}