using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly.Media.Imaging
{
    public class ImageFilterArgs : ICloneable
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

        public ImageFilterArgs Clone()
            => new ImageFilterArgs
            {
                Handled = Handled,
                TokenA = TokenA,
                TokenR = TokenR,
                TokenG = TokenG,
                TokenB = TokenB,
                ByteListA = ByteListA?.ToList(),
                ByteListR = ByteListR?.ToList(),
                ByteListG = ByteListG?.ToList(),
                ByteListB = ByteListB?.ToList(),
            };
        object ICloneable.Clone()
            => Clone();

    }
}
