#if NET5_0_OR_GREATER
using MenthaAssembly.IO;
#else
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectResponse(bool Success, object SerializeObject) : SuccessMessage(Success)
    {
        public object SerializeObject { get; } = SerializeObject;

        public SendSerializeObjectResponse(bool Success) : this(Success, null)
        {
        }

        public override void Encode(Stream Stream)
        {
            // Success
            Stream.Write(Success);

#if NET5_0_OR_GREATER
            Codec.Encode(Stream, SerializeObject);
#else
            // Datas
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

        public static new SendSerializeObjectResponse Decode(Stream Stream)
        {
            // Success
            bool Success = Stream.Read<bool>();

#if NET5_0_OR_GREATER
            object SerializeObject = Codec.Decode(Stream);
#else
            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectResponse(Success, null);

            // Deserialize
            BinaryFormatter BF = new();
            object SerializeObject = BF.Deserialize(Stream);
#endif

            return new SendSerializeObjectResponse(Success, SerializeObject);
        }

    }
}