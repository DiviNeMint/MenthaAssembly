using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network
{
    public class OperationMessage : IIdentityMessage
    {
        public static OperationMessage DoNothing { get; } = new OperationMessage("Do nothing.");

        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            get => _UID;
            set => _UID = value;
        }

        public string Message { get; }

        public OperationMessage(string Message)
        {
            this.Message = Message;
        }

        public static Stream Encode(OperationMessage Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

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

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static OperationMessage Decode(Stream Stream)
        {
            // Decode UID
            int UID = Stream.Read<int>();

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

            return new OperationMessage(Message) { _UID = UID };
        }

        public override int GetHashCode()
            => 460171812 + EqualityComparer<string>.Default.GetHashCode(Message);

        public override bool Equals(object obj)
            => obj is OperationMessage Item && this.Message.Equals(Item.Message);
    }
}
