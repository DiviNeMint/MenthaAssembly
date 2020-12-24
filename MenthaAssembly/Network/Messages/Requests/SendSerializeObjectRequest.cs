using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MenthaAssembly.Network.Messages
{
    public class SendSerializeObjectRequest : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            set => _UID = value;
            get => _UID;
        }

        public object SerializeObject { get; }

        public SendSerializeObjectRequest(object SerializeObject)
        {
            this.SerializeObject = SerializeObject;
        }

        public static Stream Encode(SendSerializeObjectRequest Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

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

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static SendSerializeObjectRequest Decode(Stream Stream)
        {
            // Decode UID
            int UID = Stream.Read<int>();

            // Check null
            if (Stream.ReadByte() == 0)
                return new SendSerializeObjectRequest(null) { _UID = UID };

            // Deserialize
            BinaryFormatter BF = new BinaryFormatter();
            object SerializeObject = BF.Deserialize(Stream);

            return new SendSerializeObjectRequest(SerializeObject) { _UID = UID };
        }

    }
}
