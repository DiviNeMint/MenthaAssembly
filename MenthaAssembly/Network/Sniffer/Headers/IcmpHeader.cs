using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IcmpHeader : IProtocolHeader
    {
        [FieldOffset(0)]
        internal unsafe fixed int Context[1];

        [FieldOffset(0)]
        private readonly byte _Type;
        public byte Type => _Type;

        [FieldOffset(1)]
        private readonly byte _Code;
        public byte Code => _Code;

        [FieldOffset(2)]
        private readonly short _Checksum;
        public short Checksum => _Checksum;

        public int Length => 4;

    }
}
