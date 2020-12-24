using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingResponse : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            get => _UID;
            set => _UID = value;
        }

        public DateTime SendTime { get; }

        public DateTime ReceivedTime { get; }

        public PingResponse(DateTime SendTime)
        {
            this.SendTime = SendTime;
            this.ReceivedTime = DateTime.Now;
        }

        public static Stream Encode(PingResponse Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.SendTime.ToBinary()), 0, sizeof(long));

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static PingResponse Decode(Stream Stream)
        {
            // Decode UID
            int UID = Stream.Read<int>();

            // Decode SendTime
            long SendTime = Stream.Read<long>();

            return new PingResponse(DateTime.FromBinary(SendTime)) { _UID = UID };
        }

    }
}
