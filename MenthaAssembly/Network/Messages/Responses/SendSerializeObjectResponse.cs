using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectResponse : SuccessMessage
    {
        public object SerializeObject { get; }

        public SendSerializeObjectResponse(bool Success) : this(Success, null)
        {
        }
        public SendSerializeObjectResponse(bool Success, object SerializeObject) : base(Success)
        {
            this.SerializeObject = SerializeObject;
        }

        public override void Encode(Stream Stream)
        {
            // Success
            Stream.Write(Success);

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
        }

        public static new SendSerializeObjectResponse Decode(Stream Stream)
        {
            // Success
            bool Success = Stream.Read<bool>();

            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectResponse(Success, null);

            // Deserialize
            BinaryFormatter BF = new();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectResponse(Success, SerializeObject);
        }

    }
}