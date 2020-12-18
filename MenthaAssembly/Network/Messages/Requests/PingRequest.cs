using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingRequest : IIdentityMessage
    {
        public int UID { private set; get; }

        int IIdentityMessage.UID
        {
            set => this.UID = value;
            get => this.UID;
        }

        public DateTime SendTime { get; }

        public PingRequest() : this(DateTime.Now)
        {
        }
        private PingRequest(DateTime SendTime)
        {
            this.SendTime = SendTime;
        }

        public static Stream Encode(PingRequest Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

            // SendTime
            byte[] Buffer = BitConverter.GetBytes(Message.SendTime.ToBinary());
            EncodeStream.Write(Buffer, 0, Buffer.Length);

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static PingRequest Decode(Stream Stream)
        {
            // Decode UID
            byte[] Buffer = new byte[sizeof(int)];
            Stream.Read(Buffer, 0, Buffer.Length);
            int UID = BitConverter.ToInt32(Buffer, 0);

            // Decode SendTime
            Buffer = new byte[sizeof(long)];
            Stream.Read(Buffer, 0, Buffer.Length);

            return new PingRequest(DateTime.FromBinary(BitConverter.ToInt64(Buffer, 0))) { UID = UID };
        }

    }
}
