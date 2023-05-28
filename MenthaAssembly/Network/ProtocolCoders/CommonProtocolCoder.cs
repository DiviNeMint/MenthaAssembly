using MenthaAssembly.IO;
using MenthaAssembly.Network.Messages;
using System.IO;

namespace MenthaAssembly.Network
{
    public class CommonProtocolCoder : IProtocolCoder
    {
        public static CommonProtocolCoder Instance { get; } = new CommonProtocolCoder();

        public Stream Encode(IMessage Message)
            => Message switch
            {
                PingRequest PingRequest => new ConcatStream(new byte[] { 1 }, PingRequest.Encode(PingRequest)),
                PingResponse PingResponse => new ConcatStream(new byte[] { 2 }, PingResponse.Encode(PingResponse)),
                SendMessageRequest SendMessageRequest => new ConcatStream(new byte[] { 3 }, SendMessageRequest.Encode(SendMessageRequest)),
                SendMessageResponse SendMessageResponse => new ConcatStream(new byte[] { 4 }, SendMessageResponse.Encode(SendMessageResponse)),
                GetTimeRequest => new MemoryStream(new byte[] { 5 }),
                GetTimeResponse GetTimeResponse => new ConcatStream(new byte[] { 6 }, GetTimeResponse.Encode(GetTimeResponse)),
                SendSerializeObjectRequest SendSerializeObjectRequest => new ConcatStream(new byte[] { 7 }, SendSerializeObjectRequest.Encode(SendSerializeObjectRequest)),
                SendSerializeObjectResponse SendSerializeObjectResponse => new ConcatStream(new byte[] { 8 }, SendSerializeObjectResponse.Encode(SendSerializeObjectResponse)),
                SuccessMessage SuccessMessage => new ConcatStream(new byte[] { 9 }, SuccessMessage.Encode(SuccessMessage)),
                ErrorMessage SuccessMessage => new ConcatStream(new byte[] { byte.MaxValue }, ErrorMessage.Encode(SuccessMessage)),
                _ => null
            };

        public IMessage Decode(Stream Stream)
            => Stream.ReadByte() switch
            {
                1 => PingRequest.Decode(Stream),
                2 => PingResponse.Decode(Stream),
                3 => SendMessageRequest.Decode(Stream),
                4 => SendMessageResponse.Decode(Stream),
                5 => new GetTimeRequest(),
                6 => GetTimeResponse.Decode(Stream),
                7 => SendSerializeObjectRequest.Decode(Stream),
                8 => SendSerializeObjectResponse.Decode(Stream),
                9 => SuccessMessage.Decode(Stream),
                byte.MaxValue => ErrorMessage.Decode(Stream),
                _ => null
            };

    }
}
