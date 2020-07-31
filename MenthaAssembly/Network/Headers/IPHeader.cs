using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IPHeader
    {
        [FieldOffset(0)]
        private readonly byte _IPInfoDatas;

        public int Version => _IPInfoDatas >> 4;

        public int HeaderLength => (_IPInfoDatas & 0x0F) << 2;

        [FieldOffset(1)]
        private readonly byte _TOS;     // Type of service 
        public byte TOS => _TOS;     // Type of service 

        [FieldOffset(2)]
        private readonly byte PacketLength1;
        [FieldOffset(3)]
        private readonly byte PacketLength2;
        public int PacketLength => (PacketLength1 << 8) | PacketLength2;

        [FieldOffset(4)]
        private readonly ushort _ID; // unique identifier 
        public ushort ID => _ID; // unique identifier 

        [FieldOffset(6)]
        public readonly ushort FlagDatas; // flags and offset 

        [FieldOffset(8)]
        private readonly byte _TTL; // Time To Live 
        public byte TTL => _TTL;

        [FieldOffset(9)]
        private readonly byte _Protocol;
        public ProtocolType Protocol => (ProtocolType)_Protocol;

        [FieldOffset(10)]
        private readonly ushort _Checksum;
        public ushort Checksum => _Checksum; //IP Header checksum

        [FieldOffset(12)]
        private readonly byte SrcAddr1;
        [FieldOffset(13)]
        private readonly byte SrcAddr2;
        [FieldOffset(14)]
        private readonly byte SrcAddr3;
        [FieldOffset(15)]
        private readonly byte SrcAddr4;
        public IPAddress SrcAddr => new IPAddress(new byte[] { SrcAddr1, SrcAddr2, SrcAddr3, SrcAddr4 });

        [FieldOffset(16)]
        private readonly byte DestAddr1;
        [FieldOffset(17)]
        private readonly byte DestAddr2;
        [FieldOffset(18)]
        private readonly byte DestAddr3;
        [FieldOffset(19)]
        private readonly byte DestAddr4;
        public IPAddress DestAddr => new IPAddress(new byte[] { DestAddr1, DestAddr2, DestAddr3, DestAddr4 });

    }

}
