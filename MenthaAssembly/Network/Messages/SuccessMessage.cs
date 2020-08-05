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
            => new MemoryStream(BitConverter.GetBytes(Message.Success));

        protected static bool Decode(Stream Stream)
        {
            // Decode Message
            byte[] Datas = new byte[sizeof(bool)];
            Stream.Read(Datas, 0, Datas.Length);

            return BitConverter.ToBoolean(Datas, 0);
        }

    }
}
