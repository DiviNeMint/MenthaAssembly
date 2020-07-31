namespace MenthaAssembly.Network.Primitives
{
    public enum GatewayType
    {
        EchoReply = 0,
        Acknowledgment = 2,
        EchoRequest = 8,
        NetworkInterfaceStatus = 9,
        NegativeAcknowledgment = 10,
        RoutingUpdate = 12,
    }
}
