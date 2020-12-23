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
            => new GetTimeRequest() { _UID = Stream.Read<int>() };

    }

}
