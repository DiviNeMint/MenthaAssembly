using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network
{
    public class OperationMessage : IMessage
    {
        public static OperationMessage DoNothing { get; } = new OperationMessage("Do nothing.");

        public string Message { get; }

        public OperationMessage(string Message)
        {
            this.Message = Message;
        }

        public static Stream Encode(OperationMessage Message)
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

        public static OperationMessage Decode(Stream Stream)
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

            return new OperationMessage(Message);
        }

        public override int GetHashCode()
            => 460171812 + EqualityComparer<string>.Default.GetHashCode(Message);

        public override bool Equals(object obj)
            => obj is OperationMessage Item && this.Message.Equals(Item.Message);
    }
}
