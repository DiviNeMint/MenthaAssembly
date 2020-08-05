using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectResponse : SuccessMessage
    {
        public SendSerializeObjectResponse(bool Success) : base(Success)
        {
        }

        public static new SendSerializeObjectResponse Decode(Stream Stream)
            => new SendSerializeObjectResponse(SuccessMessage.Decode(Stream));
    }
}
