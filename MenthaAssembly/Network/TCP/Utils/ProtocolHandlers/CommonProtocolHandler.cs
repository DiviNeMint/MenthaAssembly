using MenthaAssembly.Network.Messages;
using MenthaAssembly.Utils;
using System.IO;

namespace MenthaAssembly.Network.Utils
{
    public class CommonProtocolHandler : IProtocolHandler
    {
        public static CommonProtocolHandler Instance { get; } = new CommonProtocolHandler();

        public Stream Encode(IMessage Message)
            => Message switch
            {
                PingRequest PingRequest => new ConcatStream(new byte[] { 1 }, PingRequest.Encode(PingRequest)),
                PingResponse PingResponse => new ConcatStream(new byte[] { 2 }, PingResponse.Encode(PingResponse)),
                SendMessageRequest SendMessageRequest => new ConcatStream(new byte[] { 3 }, SendMessageRequest.Encode(SendMessageRequest)),
                SendMessageResponse SendMessageResponse => new ConcatStream(new byte[] { 4 }, SendMessageResponse.Encode(SendMessageResponse)),
                GetTimeRequest GetTimeRequest => new ConcatStream(new byte[] { 5 }, GetTimeRequest.Encode(GetTimeRequest)),
                GetTimeResponse GetTimeResponse => new ConcatStream(new byte[] { 6 }, GetTimeResponse.Encode(GetTimeResponse)),
                SendSerializeObjectRequest SendSerializeObjectRequest => new ConcatStream(new byte[] { 7 }, SendSerializeObjectRequest.Encode(SendSerializeObjectRequest)),
                SendSerializeObjectResponse SendSerializeObjectResponse => new ConcatStream(new byte[] { 8 }, SendSerializeObjectResponse.Encode(SendSerializeObjectResponse)),
                SuccessMessage SuccessMessage => new ConcatStream(new byte[] { 9 }, SuccessMessage.Encode(SuccessMessage)),
                _ => null
            };

        public IMessage Decode(Stream Stream)
            => (Stream.ReadByte()) switch
            {
                1 => PingRequest.Decode(Stream),
                2 => PingResponse.Decode(Stream),
                3 => SendMessageRequest.Decode(Stream),
                4 => SendMessageResponse.Decode(Stream),
                5 => GetTimeRequest.Decode(Stream),
                6 => GetTimeResponse.Decode(Stream),
                7 => SendSerializeObjectRequest.Decode(Stream),
                8 => SendSerializeObjectResponse.Decode(Stream),
                9 => SuccessMessage.Decode(Stream),
                _ => null
            };

    }
}
