using System;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network
{
    public class ErrorMessage : IMessage
    {
        public static ErrorMessage Timeout => new("Timeout.");

        public static ErrorMessage NotSupport => new("Not support.");

        public static ErrorMessage NotConnected => new("Not connected.");

        public static ErrorMessage Disconnected => new("Disconnected.");

        public static ErrorMessage OperationCanceled => new("Operation Canceled.");

        public static ErrorMessage EncodeException => new("Happen exception when encode request.");

        public static ErrorMessage ReceivingNotSupport => new("The receiving side not support this request.");

        public static ErrorMessage ReceivingEncodeException => new("The receiving side happen exception when encode response.");

        public static ErrorMessage ReceivingHandleException => new("The receiving side happen exception when handle request.");

        public string Message { get; }

        public ErrorMessage(string Message)
        {
            this.Message = Message;
        }

        public override int GetHashCode()
            => Message.GetHashCode();

        public override bool Equals(object obj)
            => obj is ErrorMessage Item && Message.Equals(Item.Message);

        public override string ToString()
            => Message;

        public void Encode(Stream Stream)
        {
            // Message
            if (Message is null)
            {
                Stream.Write(new byte[] { 0, 0, 0, 0 }, 0, sizeof(int));
            }
            else
            {
                byte[] Buffer = Encoding.Unicode.GetBytes(Message);
                Stream.Write(BitConverter.GetBytes(Buffer.Length), 0, sizeof(int));
                Stream.Write(Buffer, 0, Buffer.Length);
            }
        }

        public static ErrorMessage Decode(Stream Stream)
        {
            // Decode Size
            int Size = Stream.Read<int>();

            // Decode Message
            return new(Size > 0 && Stream.TryReadString(Size, Encoding.Unicode, out string Message) ? Message : null);
        }

    }
}