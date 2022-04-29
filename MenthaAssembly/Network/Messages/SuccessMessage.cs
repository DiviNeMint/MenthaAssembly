using System;
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

        public static Stream Encode(SuccessMessage Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.Success), 0, sizeof(bool));

            return EncodeStream;
        }

        public static SuccessMessage Decode(Stream Stream)
        {
            // Decode Size
            bool Success = Stream.Read<bool>();

            return new SuccessMessage(Success);
        }

    }
}
