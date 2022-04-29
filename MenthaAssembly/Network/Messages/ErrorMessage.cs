using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network
{
    public class ErrorMessage : IMessage
    {
        public static ErrorMessage Timeout => new ErrorMessage("Timeout.");

        public static ErrorMessage NotSupport => new ErrorMessage("Not support.");

        public static ErrorMessage EncodeException => new ErrorMessage("Happen exception when encode request.");

        public static ErrorMessage ReceivingNotSupport => new ErrorMessage("The receiving side not support this request.");

        public static ErrorMessage ReceivingEncodeException => new ErrorMessage("The receiving side happen exception when encode response.");

        public static ErrorMessage ReceivingHandleException => new ErrorMessage("The receiving side happen exception when handle request.");

        public static ErrorMessage ClientNotFound => new ErrorMessage("Client not found.");

        public static ErrorMessage Disconnected => new ErrorMessage("Disconnected.");

        public string Message { get; }

        public ErrorMessage(string Message)
        {
            this.Message = Message;
        }

        public static Stream Encode(ErrorMessage Message)
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

        public static ErrorMessage Decode(Stream Stream)
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

            return new ErrorMessage(Message);
        }

        public override int GetHashCode() 
            => 460171812 + EqualityComparer<string>.Default.GetHashCode(Message);

        public override bool Equals(object obj)
            => obj is ErrorMessage Item && this.Message.Equals(Item.Message);

    }
}
