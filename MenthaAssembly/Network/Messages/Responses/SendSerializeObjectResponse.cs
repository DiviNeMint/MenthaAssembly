using MenthaAssembly.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectResponse : SuccessMessage
    {
        public object SerializeObject { get; }

        public SendSerializeObjectResponse(bool Success, object SerializeObject) : base(Success)
        {
            this.SerializeObject = SerializeObject;
        }

        public static Stream Encode(SendSerializeObjectResponse Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // Serialize
            BinaryFormatter BF = new BinaryFormatter();
            BF.Serialize(EncodeStream, Message.SerializeObject);

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return new ConcatStream(SuccessMessage.Encode(Message), EncodeStream);
        }
        
        public static new SendSerializeObjectResponse Decode(Stream Stream)
        {
            //Success
            bool Success = SuccessMessage.Decode(Stream);

            // Deserialize
            BinaryFormatter BF = new BinaryFormatter();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectResponse(Success, SerializeObject);
        }

    }
}
