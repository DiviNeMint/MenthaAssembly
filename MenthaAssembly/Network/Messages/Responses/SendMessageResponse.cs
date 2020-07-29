using MenthaAssembly.Network;
using MenthaAssembly.Utils;
using System;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network.Messages
{
    public class SendMessageResponse : IMessage
    {
        public string Message { get; }

        public SendMessageResponse(string Message)
        {
            this.Message = Message;
        }

        public static Stream Encode(SendMessageResponse Message)
        {
            byte[] Datas = Encoding.Default.GetBytes(Message.Message);
            return new ConcatStream(BitConverter.GetBytes(Datas.Length), 0, sizeof(int), new MemoryStream(Datas));
        }

        public static SendMessageResponse Decode(Stream Stream)
        {
            // Decode Size
            byte[] Datas = new byte[sizeof(int)];
            Stream.Read(Datas, 0, Datas.Length);
            int Size = BitConverter.ToInt32(Datas, 0);

            // Decode Message
            Datas = new byte[Size];
            Stream.Read(Datas, 0, Size);

            return new SendMessageResponse(Encoding.Default.GetString(Datas));
        }

    }

}
