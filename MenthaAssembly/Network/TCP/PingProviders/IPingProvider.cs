namespace MenthaAssembly.Network
{
    /// <summary>
    /// Implement Auto-Ping System's PingMessage provider.
    /// </summary>
    public interface IPingProvider
    {
        /// <summary>
        /// Provide PingMessage(Request).
        /// </summary>
        IMessage Provide();

    }
}
