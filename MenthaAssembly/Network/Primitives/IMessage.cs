using System.IO;

namespace MenthaAssembly.Network
{
    public interface IMessage
    {
        public void Encode(Stream Stream);

    }
}