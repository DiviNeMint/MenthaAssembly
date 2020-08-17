namespace MenthaAssembly.Win32
{
    internal enum WindowLayeredAttributeFlags
    {
        /// <summary>
        /// Use bAlpha to determine the opacity of the layered window. 
        /// </summary>
        Alpha = 0x02,

        /// <summary>
        /// Use crKey as the transparency color. 
        /// </summary>
        ColorKey = 0x01,
    }
}
