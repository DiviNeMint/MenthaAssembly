using MenthaAssembly.Network;
using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectResponse : IMessage
    {
        public bool Success { get; }

        public SendSerializeObjectResponse(bool Success)
        {
            this.Success = Success;
        }

        public static Stream Encode(SendSerializeObjectResponse Message)
            => new MemoryStream(BitConverter.GetBytes(Message.Success));

        public static SendSerializeObjectResponse Decode(Stream Stream)
        {
            // Decode Message
            byte[] Datas = new byte[sizeof(bool)];
            Stream.Read(Datas, 0, Datas.Length);

            return new SendSerializeObjectResponse(BitConverter.ToBoolean(Datas, 0));
        }

    }
}
