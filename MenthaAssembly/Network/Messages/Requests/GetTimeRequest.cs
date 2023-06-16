using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class GetTimeRequest : IMessage
    {
        public void Encode(Stream Stream)
        {

        }

        //public static GetTimeRequest Decode(Stream Stream) 
        //    => new GetTimeRequest() { _UID = Stream.Read<int>() };

    }
}