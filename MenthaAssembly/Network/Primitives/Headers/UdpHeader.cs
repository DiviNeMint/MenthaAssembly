using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public struct UdpHeader : IProtocolHeader
    {
        [FieldOffset(0)]
        private readonly byte SrcPort1;
        [FieldOffset(1)]
        private readonly byte SrcPort2;
        public int SrcPort => (SrcPort1 << 8) | SrcPort2;

        [FieldOffset(2)]
        private readonly byte DestPort1;
        [FieldOffset(3)]
        private readonly byte DestPort2;
        public int DestPort => (DestPort1 << 8) | DestPort2;

        [FieldOffset(4)]
        private readonly byte LengthDatas1;
        [FieldOffset(5)]
        private readonly byte LengthDatas2;

        public int Length => ((LengthDatas1 << 8) | LengthDatas2) << 2;

        [FieldOffset(6)]
        private readonly short _Checksum;
        public short Checksum => _Checksum;

    }
}
