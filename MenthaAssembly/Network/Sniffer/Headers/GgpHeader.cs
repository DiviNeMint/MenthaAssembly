using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public struct GgpHeader : IProtocolHeader
    {
        [FieldOffset(0)]
        internal unsafe fixed int Context[1];

        [FieldOffset(0)]
        private readonly byte _Type;
        public GatewayType Type => (GatewayType)_Type;

        [FieldOffset(1)]
        private readonly byte Reserved;

        [FieldOffset(2)]
        private readonly short _SequenceNumber;
        public short SequenceNumber => _SequenceNumber;

        public int Length => 4;

    }
}
