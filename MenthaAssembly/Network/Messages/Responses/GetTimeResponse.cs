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
            => new MemoryStream(BitConverter.GetBytes(Message.Time.ToBinary()));

        public static GetTimeResponse Decode(Stream Stream)
        {
            // Decode SendTime
            byte[] Datas = new byte[sizeof(long)];
            Stream.Read(Datas, 0, Datas.Length);

            return new GetTimeResponse(DateTime.FromBinary(BitConverter.ToInt64(Datas, 0)));
        }
    }
}
