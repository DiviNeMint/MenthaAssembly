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

            // SerializeObject
            if (Message.SerializeObject is null)
            {
                // Null
                EncodeStream.WriteByte(0);
            }
            else
            {
                EncodeStream.WriteByte(1);

                // Serialize
                BinaryFormatter BF = new BinaryFormatter();
                BF.Serialize(EncodeStream, Message.SerializeObject);
            }

            return EncodeStream;
        }

        public static SendSerializeObjectRequest Decode(Stream Stream)
        {
            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectRequest(null) ;

            // Deserialize
            BinaryFormatter BF = new BinaryFormatter();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectRequest(SerializeObject) ;
        }

    }
}
