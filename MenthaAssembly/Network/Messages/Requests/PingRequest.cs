using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingRequest : IMessage
    {
        public DateTime SendTime { get; }

        public PingRequest()
        {
            this.SendTime = DateTime.Now;
        }
        private PingRequest(DateTime SendTime)
        {
            this.SendTime = SendTime;
        }

        public static Stream Encode(PingRequest Message)
            => new MemoryStream(BitConverter.GetBytes(Message.SendTime.ToBinary()));

        public static PingRequest Decode(Stream Stream)
        {
            // Decode SendTime
            byte[] Datas = new byte[sizeof(long)];
            Stream.Read(Datas, 0, Datas.Length);

            return new PingRequest(DateTime.FromBinary(BitConverter.ToInt64(Datas, 0)));
        }

    }
}
