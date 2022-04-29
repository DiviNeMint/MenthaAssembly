using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingResponse : IMessage
    {
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

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.SendTime.ToBinary()), 0, sizeof(long));

            return EncodeStream;
        }

        public static PingResponse Decode(Stream Stream)
        {
            // Decode SendTime
            long SendTime = Stream.Read<long>();

            return new PingResponse(DateTime.FromBinary(SendTime));
        }

    }
}
