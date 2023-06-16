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

        public void Encode(Stream Stream)
        {
            // SerializeObject
            if (SerializeObject is null)
            {
                // Null
                Stream.WriteByte(0);
            }
            else
            {
                Stream.WriteByte(1);

                // Serialize
                BinaryFormatter BF = new();
                BF.Serialize(Stream, SerializeObject);
            }
        }

        public static SendSerializeObjectRequest Decode(Stream Stream)
        {
            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectRequest(null);

            // Deserialize
            BinaryFormatter BF = new();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectRequest(SerializeObject);
        }

    }
}