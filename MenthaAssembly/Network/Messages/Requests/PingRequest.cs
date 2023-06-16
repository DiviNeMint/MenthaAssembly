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

        public void Encode(Stream Stream)
        {
            // SendTime
            Stream.Write(SendTime);
        }

        public static PingRequest Decode(Stream Stream)
        {
            // Decode SendTime
            return new(Stream.Read<DateTime>());
        }

    }
}