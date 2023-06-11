using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct IPHeader
    {
        [FieldOffset(0)]
        internal fixed int Context[5];

        [FieldOffset(0)]
        private readonly byte _IPInfoDatas;

        public int Version
            => _IPInfoDatas >> 4;

        public int Length
            => (_IPInfoDatas & 0x0F) << 2;

        [FieldOffset(1)]
        private readonly byte _TOS;
        public byte TOS
            => _TOS;        // Type of service 

        [FieldOffset(2)]
        private readonly byte PacketLength1;
        [FieldOffset(3)]
        private readonly byte PacketLength2;
        public int PacketLength
            => (PacketLength1 << 8) | PacketLength2;

        [FieldOffset(4)]
        private readonly ushort _ID;
        public ushort ID
            => _ID;        // unique identifier 

        [FieldOffset(6)]
        public readonly ushort FlagDatas; // flags and offset 

        [FieldOffset(8)]
        private readonly byte _TTL;
        public byte TTL
            => _TTL;        // Time To Live 

        [FieldOffset(9)]
        private readonly byte _Protocol;
        public ProtocolType Protocol
            => (ProtocolType)_Protocol;

        [FieldOffset(10)]
        private readonly ushort _Checksum;
        public ushort Checksum
            => _Checksum; //IP Header checksum

        [FieldOffset(12)]
        private readonly int _SrcAddress;
        public int SrcAddress
            => _SrcAddress;

        [FieldOffset(16)]
        private readonly int _DestAddress;
        public int DestAddress
            => _DestAddress;

        [FieldOffset(20)]
        public fixed int Options[10];

        public int OptionsLength
            => (Length - 20) >> 2;

    }
}