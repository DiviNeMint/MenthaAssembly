using System.Collections.Generic;

namespace MenthaAssembly.Media.Imaging
{
    public class ImageFilterArgs
    {
        public bool Handled { set; get; }

        public float TokenA { set; get; }

        public float TokenR { set; get; }

        public float TokenG { set; get; }

        public float TokenB { set; get; }

        // More kernel need?
        // Need List :
        // Gaussian

        public List<byte> ByteListA { set; get; }

        public List<byte> ByteListR { set; get; }

        public List<byte> ByteListG { set; get; }

        public List<byte> ByteListB { set; get; }

    }
}
