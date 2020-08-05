using System.IO;

namespace MenthaAssembly.Network
{
    public interface IProtocolHandler
    {
        /// <summary>
        /// Encode Message.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns>
        /// when message can't encode or not support, return null.
        /// </returns>
        Stream Encode(IMessage Message);

        IMessage Decode(Stream Stream);

    }
}
