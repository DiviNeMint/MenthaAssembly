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
        /// Cluster K-Means++
        /// </summary>
        KMeans = 3,

        // https://github.com/mcychan/nQuantCpp
        // https://github.com/jsummers/imageworsener/issues/2
        // https://github.com/JeremyAnsel/JeremyAnsel.ColorQuant/blob/master/JeremyAnsel.ColorQuant/JeremyAnsel.ColorQuant/WuColorQuantizer.cs

        ///// <summary>
        ///// Fast pairwise nearest neighbor based algorithm
        ///// </summary>
        //PNN = 4,

    }
}