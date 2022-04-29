using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingRequest : IMessage
    {
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

            // SendTime
            byte[] Buffer = BitConverter.GetBytes(Message.SendTime.ToBinary());
            EncodeStream.Write(Buffer, 0, Buffer.Length);

            return EncodeStream;
        }

        public static PingRequest Decode(Stream Stream)
        {
            // Decode SendTime
            long SendTime = Stream.Read<long>();

            return new PingRequest(DateTime.FromBinary(SendTime));
        }

    }
}
