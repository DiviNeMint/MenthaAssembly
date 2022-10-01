namespace MenthaAssembly.Media.Imaging
{
    public enum QuantizationTypes : byte
    {
        /// <summary>
        /// Box Means
        /// </summary>
        Mean = 1,

        /// <summary>
        /// Box Mesian
        /// </summary>
        Median = 2,

        /// <summary>
        /// Cluster K-Means
        /// </summary>
        KMeans = 3,
    }
}