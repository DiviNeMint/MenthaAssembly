﻿using System;
using System.IO;

namespace MenthaAssembly.Network.Messages
{
    public class PingResponse : IIdentityMessage
    {
        public int UID { private set; get; }

        int IIdentityMessage.UID
        {
            set => this.UID = value;
            get => this.UID;
        }
        public DateTime SendTime { get; }

        public DateTime ReceivedTime { get; }

        public PingResponse(DateTime SendTime)
        {
            this.SendTime = SendTime;
            this.ReceivedTime = DateTime.Now;
        }

        public static Stream Encode(PingResponse Message)
        {
            MemoryStream EncodeStream = new MemoryStream();

            // UID
            EncodeStream.Write(BitConverter.GetBytes(Message.UID), 0, sizeof(int));

            // Data
            EncodeStream.Write(BitConverter.GetBytes(Message.SendTime.ToBinary()), 0, sizeof(long));

            // Reset Position
            EncodeStream.Seek(0, SeekOrigin.Begin);

            return EncodeStream;
        }

        public static PingResponse Decode(Stream Stream)
        {
            // Decode UID
            byte[] Buffer = new byte[sizeof(int)];
            Stream.Read(Buffer, 0, Buffer.Length);
            int UID = BitConverter.ToInt32(Buffer, 0);

            // Decode SendTime
            Buffer = new byte[sizeof(long)];
            Stream.Read(Buffer, 0, Buffer.Length);

            return new PingResponse(DateTime.FromBinary(BitConverter.ToInt64(Buffer, 0))) { UID = UID };
        }

    }
}
