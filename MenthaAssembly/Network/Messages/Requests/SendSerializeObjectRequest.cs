using MenthaAssembly.Network;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectRequest : IMessage
    {
        public object SerializeObject { get; }

        public SendSerializeObjectRequest(object SerializeObject)
        {
            this.SerializeObject = SerializeObject;
        }

        public static Stream Encode(SendSerializeObjectRequest Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // Serialize
            BinaryFormatter BF = new BinaryFormatter();
            BF.Serialize(EncodeStream, Message.SerializeObject);

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static SendSerializeObjectRequest Decode(Stream Stream)
        {
            // Deserialize
            BinaryFormatter BF = new BinaryFormatter();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectRequest(SerializeObject);
        }

    }
}
