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

        public void Encode(Stream Stream)
        {
            // Time
            //Stream.Write(BitConverter.GetBytes(Time.ToBinary()), 0, sizeof(long));
            Stream.Write(Time);
        }

        public static GetTimeResponse Decode(Stream Stream)
        {
            // Decode Time
            return new GetTimeResponse(Stream.Read<DateTime>());

            //long SendTime = Stream.Read<long>();
            //return new GetTimeResponse(DateTime.FromBinary(SendTime));
        }

    }
}