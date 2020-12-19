using System;
using System.IO;

namespace MenthaAssembly.Network
{
    public class SuccessMessage : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            get => _UID;
            set => _UID = value;
        }

        public bool Success { get; }

        public SuccessMessage(bool Success)
        {
            this.Success = Success;
        }

        public static Stream Encode(SuccessMessage Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.Success), 0, sizeof(bool));

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static SuccessMessage Decode(Stream Stream)
        {
            // UID
            byte[] Buffer = new byte[sizeof(int)];
            Stream.Read(Buffer, 0, Buffer.Length);

            int UID = BitConverter.ToInt32(Buffer, 0);

            // Decode Message
            Buffer = new byte[sizeof(bool)];
            Stream.Read(Buffer, 0, Buffer.Length);

            return new SuccessMessage(BitConverter.ToBoolean(Buffer, 0)) { _UID = UID };
        }

    }
}
