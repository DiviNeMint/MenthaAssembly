using MenthaAssembly.Media.Imaging.Utils;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Defines options for template matching, including the image channel to use,
    /// pre-filter settings, and the similarity threshold.
    /// </summary>
    public class TemplateMatchOptions
    {
        /// <summary>
        /// Pre-filter settings to optimize matching speed and accuracy.
        /// </summary>
        public TemplateMatchPreFilterOptions PreFilter { get; } = new();

        /// <summary>
        /// The similarity threshold for template matching, ranging from 0 to 1. 
        /// Only matches with a similarity above this threshold are considered successful. 
        /// Default is 0.8.
        /// </summary>
        public double Threshold { get; set; } = 0.8;

        /// <summary>
        /// Specifies the algorithm used for template matching.
        /// </summary>
        public TemplateMatchMode Mode { set; get; } = TemplateMatchMode.Auto;

    }
}