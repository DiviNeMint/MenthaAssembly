﻿using System;
using System.IO;
using System.Text;

namespace MenthaAssembly.Network.Messages
{
    public class SendMessageRequest : IIdentityMessage
    {
        internal int _UID;
        public int UID => _UID;

        int IIdentityMessage.UID
        {
            set => _UID = value;
            get => _UID;
        }

        public string Message { get; }

        public SendMessageRequest(string Message)
        {
            this.Message = Message;
        }

        public static Stream Encode(SendMessageRequest Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

            // Message
            if (Message.Message is null)
            {
                EncodeStream.Write(new byte[] { 0, 0, 0, 0 }, 0, sizeof(int));
            }
            else
            {
                byte[] Buffer = Encoding.Default.GetBytes(Message.Message);
                EncodeStream.Write(BitConverter.GetBytes(Buffer.Length), 0, sizeof(int));
                EncodeStream.Write(Buffer, 0, Buffer.Length);
            }

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static SendMessageRequest Decode(Stream Stream)
        {
            // Decode UID
            byte[] Buffer = new byte[sizeof(int)];
            Stream.Read(Buffer, 0, Buffer.Length);
            int UID = BitConverter.ToInt32(Buffer, 0);

            // Decode Size
            Stream.Read(Buffer, 0, Buffer.Length);
            int Size = BitConverter.ToInt32(Buffer, 0);

            // Decode Message
            string Message = null;
            if (Size > 0)
            {
                Buffer = new byte[Size];
                Stream.Read(Buffer, 0, Size);
                Message = Encoding.Default.GetString(Buffer);
            }

            return new SendMessageRequest(Message) { _UID = UID };
        }

    }

}