using System.Net;

namespace MenthaAssembly.Network
{
    public interface IMessageHandler
    {
        /// <summary>
        /// Handle Message.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Message"></param>
        /// <returns>
        /// Return null when not reply message.
        /// </returns>
        IMessage HandleMessage(IPEndPoint Address, IMessage Message);

    }
}
