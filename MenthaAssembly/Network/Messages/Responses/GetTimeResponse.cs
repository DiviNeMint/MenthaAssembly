using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class GetTimeResponse : IMessage
    {
        public DateTime Time { get; }

        public GetTimeResponse(DateTime Time)
        {
            this.Time = Time;
        }

        public static Stream Encode(GetTimeResponse Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.Time.ToBinary()), 0, sizeof(long));

            return EncodeStream;
        }

        public static GetTimeResponse Decode(Stream Stream)
        {
            // Decode SendTime
            long SendTime = Stream.Read<long>();

            return new GetTimeResponse(DateTime.FromBinary(SendTime));
        }
    }
}
