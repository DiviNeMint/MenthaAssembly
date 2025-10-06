namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the options used for pre-filtering candidate regions before template matching.
    /// These thresholds help reduce the number of locations that require full template comparison,
    /// improving performance by eliminating unlikely matches early.
    /// </summary>
    public sealed class TemplateMatchPreFilterOptions
    {
        /// <summary>
        /// Indicates whether the pre-filtering step for template matching is enabled.<para/>
        /// When set to true, the filter will evaluate candidate regions based on mean intensity,
        /// variance, and edge similarity thresholds before performing full template matching.<para/>
        /// When set to false, all candidate regions are passed through without pre-filtering.<para/>
        /// Default is true.
        /// </summary>
        public bool Enabled { get; set; } = true;

        private double _MeanThreshold = 5.0;
        /// <summary>
        /// The mean intensity difference threshold between the template and an image region.<para/>
        /// Value range is 0.0–255.0.<para/>
        /// A higher value makes the filter looser (allows more difference), while a lower value makes the filter stricter.<para/>
        /// Default is 5.
        /// </summary>
        public double MeanThreshold
        {
            get => _MeanThreshold;
            set => _MeanThreshold = MathHelper.Clamp(value, 0.0, 255.0);
        }

        private double _VarianceThreshold = 0.8;
        /// <summary>
        /// The variance similarity threshold between the template and an image region.<para/>
        /// Value range is between 0.0 and 1.0. <para/>
        /// A value closer to 1.0 makes the filter stricter (only regions with similar variance pass),
        /// while a smaller value makes the filter looser (allows larger variance differences).<para/>
        /// Default is 0.8.
        /// </summary>
        public double VarianceThreshold
        {
            get => _VarianceThreshold;
            set => _VarianceThreshold = MathHelper.Clamp(value, 0.0, 1.0);
        }

        private double _EdgeThreshold = 0.9;
        /// <summary>
        /// The edge similarity threshold between the template and an image region.<para/>
        /// Value range is between 0.0 and 1.0.<para/>
        /// A value closer to 1.0 makes the filter stricter (requires stronger edge similarity),
        /// while a smaller value makes the filter looser (allows greater differences in edge strength).<para/>
        /// Default is 0.9.
        /// </summary>
        public double EdgeThreshold
        {
            get => _EdgeThreshold;
            set => _EdgeThreshold = MathHelper.Clamp(value, 0.0, 1.0);
        }

        internal TemplateMatchPreFilterOptions()
        {

        }
    }
}