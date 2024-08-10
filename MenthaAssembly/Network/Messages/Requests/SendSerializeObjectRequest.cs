#if NET5_0_OR_GREATER
using MenthaAssembly.IO;
#else
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectRequest(object SerializeObject) : IMessage
    {
        public object SerializeObject { get; } = SerializeObject;

        public void Encode(Stream Stream)
        {
#if NET5_0_OR_GREATER
            Codec.Encode(Stream, SerializeObject);
#else
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
#endif
        }

        public static SendSerializeObjectRequest Decode(Stream Stream)
        {
#if NET5_0_OR_GREATER
            object SerializeObject = Codec.Decode(Stream);
#else
            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectRequest(null);

            // Deserialize
            BinaryFormatter BF = new();
            object SerializeObject = BF.Deserialize(Stream);
#endif

            return new SendSerializeObjectRequest(SerializeObject);
        }

    }
}
