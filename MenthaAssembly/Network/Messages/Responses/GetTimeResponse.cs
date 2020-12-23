using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class GetTimeResponse : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            get => _UID;
            set => _UID = value;
        }

        public DateTime Time { get; }

        public GetTimeResponse(DateTime Time)
        {
            this.Time = Time;
        }

        public static Stream Encode(GetTimeResponse Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.Time.ToBinary()), 0, sizeof(long));

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static GetTimeResponse Decode(Stream Stream)
        {
            // Decode UID
            int UID = Stream.Read<int>();

            // Decode SendTime
            long SendTime = Stream.Read<long>();

            return new GetTimeResponse(DateTime.FromBinary(SendTime)) { _UID = UID };
        }
    }
}
