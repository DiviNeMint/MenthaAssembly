using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingResponse : IMessage
    {
        public DateTime SendTime { get; }

        public DateTime ReceivedTime { get; }

        public PingResponse(PingRequest Request)
        {
            SendTime = Request.SendTime;
            ReceivedTime = DateTime.Now;
        }
        public PingResponse(DateTime SendTime)
        {
            this.SendTime = SendTime;
            ReceivedTime = DateTime.Now;
        }

        public void Encode(Stream Stream)
        {
            // Data
            Stream.Write(SendTime);
        }

        public static PingResponse Decode(Stream Stream)
        {
            // Decode SendTime
            return new(Stream.Read<DateTime>());
        }

    }
}