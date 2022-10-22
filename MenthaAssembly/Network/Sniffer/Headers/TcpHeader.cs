using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct TcpHeader : IProtocolHeader
    {
        [FieldOffset(0)]
        internal fixed int Context[5];

        [FieldOffset(0)]
        private readonly byte SrcPort1;
        [FieldOffset(1)]
        private readonly byte SrcPort2;
        public int SrcPort
            => (SrcPort1 << 8) | SrcPort2;

        [FieldOffset(2)]
        private readonly byte DestPort1;
        [FieldOffset(3)]
        private readonly byte DestPort2;
        public int DestPort
            => (DestPort1 << 8) | DestPort2;

        [FieldOffset(4)]
        private readonly int _SequenceNumber;
        public int SequenceNumber
            => _SequenceNumber;

        [FieldOffset(8)]
        private readonly int _Acknowledgement;
        public int Acknowledgement
            => _Acknowledgement;

        [FieldOffset(12)]
        private readonly byte HeaderDatas1;
        [FieldOffset(13)]
        private readonly byte HeaderDatas2;

        public int Length
            => (HeaderDatas1 >> 4) << 2;

        public TcpFlags Flags
            => (TcpFlags)(HeaderDatas2 & 0x3F);

        [FieldOffset(14)]
        private readonly short _WindowSize;
        public short WindowSize
            => _WindowSize;

        [FieldOffset(16)]
        private readonly short _Checksum;
        public short Checksum
            => _Checksum;

        [FieldOffset(18)]
        private readonly short _UrgentPointer;
        public short UrgentPointer
            => _UrgentPointer;

        [FieldOffset(20)]
        public fixed int Options[10];

        public int OptionsLength
            => (Length - 20) >> 2;

    }
}