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
            MemoryStream EncodeStream = new MemoryStream();

            // Message
            if (Message.Message is null)
            {
                EncodeStream.Write(new byte[] { 0, 0, 0, 0 }, 0, sizeof(int));
            }
            else
            {
                byte[] Buffer = Encoding.Default.GetBytes(Message.Message);
                EncodeStream.Write(BitConverter.GetBytes(Buffer.Length), 0, sizeof(int));
                EncodeStream.Write(Buffer, 0, Buffer.Length);
            }

            return EncodeStream;
        }

        public static SendMessageResponse Decode(Stream Stream)
        {            
            // Decode Size
            int Size = Stream.Read<int>();

            // Decode Message
            string Message = null;
            if (Size > 0)
            {
                byte[] Datas = new byte[Size];
                Stream.ReadBuffer(Datas);
                Message = Encoding.Default.GetString(Datas);
            }

            return new SendMessageResponse(Message);
        }

    }

}
