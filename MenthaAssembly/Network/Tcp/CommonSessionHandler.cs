using MenthaAssembly.Network.Messages;
using System.IO;

namespace MenthaAssembly.Network.Primitives
{
    public class CommonSessionHandler : ISessionHandler
    {
        public static CommonSessionHandler Instance { get; } = new();

        public void EncodeHeader(Stream Stream, int UID, bool Reply, IMessage Message)
        {
            Stream.Write(UID);
            Stream.Write(Reply);
        }

        public void EncodeMessage(Stream Stream, IMessage Message)
        {
            byte MessageUID = Message switch
            {
                PingRequest => 0x01,
                PingResponse => 0x02,
                SendMessageRequest => 0x03,
                SendMessageResponse => 0x04,
                GetTimeRequest => 0x05,
                GetTimeResponse => 0x06,
                SendSerializeObjectRequest => 0x07,
                SendSerializeObjectResponse => 0x08,
                SuccessMessage => 0x09,
                _ => byte.MaxValue
            };

            if (MessageUID == byte.MaxValue &&
                Message is not ErrorMessage)
                Message = ErrorMessage.NotSupport;

            Stream.WriteByte(MessageUID);
            Message.Encode(Stream);
        }

        public object DecodeHeader(Stream Stream, out int UID, out bool Reply)
        {
            UID = Stream.Read<int>();
            Reply = Stream.Read<bool>();
            return null;
        }

        public IMessage DecodeMessage(Stream Stream)
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