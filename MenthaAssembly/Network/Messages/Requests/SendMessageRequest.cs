using MenthaAssembly.Network;
using MenthaAssembly.Utils;
using System;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network.Messages
{
    public class SendMessageRequest : IMessage
    {
        public string Message { get; }

        public SendMessageRequest(string Message)
        {
            this.Message = Message;
        }

        public static Stream Encode(SendMessageRequest Message)
        {
            byte[] Datas = Encoding.Default.GetBytes(Message.Message);
            return new ConcatStream(BitConverter.GetBytes(Datas.Length), 0, sizeof(int), new MemoryStream(Datas));
        }

        public static SendMessageRequest Decode(Stream Stream)
        {
            // Decode Size
            byte[] Datas = new byte[sizeof(int)];
            Stream.Read(Datas, 0, Datas.Length);
            int Size = BitConverter.ToInt32(Datas, 0);

            // Decode Message
            Datas = new byte[Size];
            Stream.Read(Datas, 0, Size);

            return new SendMessageRequest(Encoding.Default.GetString(Datas));
        }

    }

}
