using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class GetTimeRequest : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            set => _UID = value;
            get => _UID;
        }

        public static Stream Encode(GetTimeRequest Message) 
            => new MemoryStream(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

        public static GetTimeRequest Decode(Stream Stream)
        {
            // Decode UID
            byte[] Buffer = new byte[sizeof(int)];
            Stream.Read(Buffer, 0, Buffer.Length);
            int UID = BitConverter.ToInt32(Buffer, 0);

            return new GetTimeRequest() { _UID = UID };
        }

    }

}
