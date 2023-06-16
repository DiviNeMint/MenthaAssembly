using System.IO;

namespace MenthaAssembly.Network
{
    public interface ISessionHandler
    {
        void EncodeHeader(Stream Stream, int UID, bool Reply, IMessage Message);

        object DecodeHeader(Stream Stream, out int UID, out bool Reply);

        void EncodeMessage(Stream Stream, IMessage Message);

        IMessage DecodeMessage(Stream Stream);

    }
}