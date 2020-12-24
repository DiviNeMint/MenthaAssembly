using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingRequest : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            set => _UID = value;
            get => _UID;
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
            int UID = Stream.Read<int>();

            // Decode SendTime
            long SendTime = Stream.Read<long>();

            return new PingRequest(DateTime.FromBinary(SendTime)) { _UID = UID };
        }

    }
}
