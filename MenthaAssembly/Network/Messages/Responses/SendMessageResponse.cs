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

        public void Encode(Stream Stream)
        {
            // Message
            if (Message is null)
            {
                Stream.Write(0);
            }
            else
            {
                byte[] Buffer = Encoding.Unicode.GetBytes(Message);
                int Length = Buffer.Length;
                Stream.Write(Length);
                Stream.Write(Buffer, 0, Length);
            }
        }

        public static SendMessageResponse Decode(Stream Stream)
        {
            // Decode Size
            int Size = Stream.Read<int>();

            // Decode Message
            return new(Size > 0 && Stream.TryReadString(Size, Encoding.Unicode, out string Message) ? Message : null);
        }

    }
}