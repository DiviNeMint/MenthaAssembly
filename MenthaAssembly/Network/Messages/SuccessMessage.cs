using System.IO;

namespace MenthaAssembly.Network
{
    public class SuccessMessage : IMessage
    {
        public bool Success { get; }

        public SuccessMessage(bool Success)
        {
            this.Success = Success;
        }

        public virtual void Encode(Stream Stream)
        {
            // Data
            Stream.Write(Success);
        }

        public static SuccessMessage Decode(Stream Stream)
        {
            // Decode Size
            bool Success = Stream.Read<bool>();

            return new SuccessMessage(Success);
        }

    }
}