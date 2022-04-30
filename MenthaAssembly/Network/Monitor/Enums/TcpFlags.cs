using System;

namespace MenthaAssembly.Network.Primitives
{
    [Flags]
    public enum TcpFlags
    {
        FIN = 1,
        SYN = 2,
        RST = 4,
        PSH = 8,
        ACK = 16,
        URG = 32,
    }
}
