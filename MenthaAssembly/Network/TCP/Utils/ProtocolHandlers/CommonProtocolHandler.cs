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
                PingRequest PingRequest => new ConcatStream(new byte[] { 0 }, PingRequest.Encode(PingRequest)),
                PingResponse PingResponse => new ConcatStream(new byte[] { 1 }, PingResponse.Encode(PingResponse)),
                SendMessageRequest SendMessageRequest => new ConcatStream(new byte[] { 2 }, SendMessageRequest.Encode(SendMessageRequest)),
                SendMessageResponse SendMessageResponse => new ConcatStream(new byte[] { 3 }, SendMessageResponse.Encode(SendMessageResponse)),
                SendSerializeObjectRequest SendSerializeObjectRequest => new ConcatStream(new byte[] { 4 }, SendSerializeObjectRequest.Encode(SendSerializeObjectRequest)),
                SendSerializeObjectResponse SendSerializeObjectResponse => new ConcatStream(new byte[] { 5 }, SendSerializeObjectResponse.Encode(SendSerializeObjectResponse)),
                _ => null
            };

        public IMessage Decode(Stream Stream)
            => (Stream.ReadByte()) switch
            {
                0 => PingRequest.Decode(Stream),
                1 => PingResponse.Decode(Stream),
                2 => SendMessageRequest.Decode(Stream),
                3 => SendMessageResponse.Decode(Stream),
                4 => SendSerializeObjectRequest.Decode(Stream),
                5 => SendSerializeObjectResponse.Decode(Stream),
                _ => null
            };

    }
}
