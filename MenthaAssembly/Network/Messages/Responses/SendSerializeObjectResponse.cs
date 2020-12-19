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
        private SendSerializeObjectResponse(SuccessMessage Message, object SerializeObject) : this(Message.Success, SerializeObject)
        {
            this._UID = Message.UID;
        }

        public static Stream Encode(SendSerializeObjectResponse Message)
        {
            Stream EncodeStream = SuccessMessage.Encode(Message);

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

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static new SendSerializeObjectResponse Decode(Stream Stream)
        {
            SuccessMessage Message = SuccessMessage.Decode(Stream);

            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectResponse(Message, null);

            // Deserialize
            BinaryFormatter BF = new BinaryFormatter();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectResponse(Message, SerializeObject);
        }

    }
}
