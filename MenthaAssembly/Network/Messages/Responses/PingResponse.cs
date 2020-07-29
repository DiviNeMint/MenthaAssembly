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
            => new MemoryStream(BitConverter.GetBytes(Message.SendTime.ToBinary()));

        public static PingResponse Decode(Stream Stream)
        {
            // Decode SendTime
            byte[] Datas = new byte[sizeof(long)];
            Stream.Read(Datas, 0, Datas.Length);

            return new PingResponse(DateTime.FromBinary(BitConverter.ToInt64(Datas, 0)));
        }

    }
}
