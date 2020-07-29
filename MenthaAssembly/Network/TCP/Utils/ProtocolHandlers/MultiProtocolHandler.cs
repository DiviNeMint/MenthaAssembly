using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace MenthaAssembly.Network.Utils
{
    public class MultiProtocolHandler : IProtocolHandler
    {
        public List<IProtocolHandler> Handlers { get; }

        public MultiProtocolHandler(params IProtocolHandler[] Handlers)
        {
            this.Handlers = new List<IProtocolHandler>(Handlers);
        }
        public MultiProtocolHandler(IEnumerable<IProtocolHandler> Handlers)
        {
            this.Handlers = new List<IProtocolHandler>(Handlers);
        }

        public IMessage Decode(Stream Stream)
        {
            int HeaderSize = CalculateMessageHeaderSize(),
                Offset = 0;
            byte[] Header = new byte[HeaderSize];

            do
            {
                int ReadSize = Stream.Read(Header, Offset, HeaderSize);
                Offset += ReadSize;
                HeaderSize -= ReadSize;
            } while (HeaderSize > 0);

            int Index = DecodeHeader(Header);

            return Handlers[Index].Decode(Stream);
        }

        public Stream Encode(IMessage Message)
        {
            for (int i = Handlers.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (Handlers[i].Encode(Message) is Stream EncodeStream)
                        return new ConcatStream(EncodeHeader(i), EncodeStream);
                }
                catch
                {
                    continue;
                }
            }

            throw new NotImplementedException();
        }

        private int CalculateMessageHeaderSize()
            => Math.Min((Handlers.Count + 255) >> 8, sizeof(int));

        private byte[] EncodeHeader(int Index)
        {
            int HeaderSize = CalculateMessageHeaderSize();
            byte[] Header = new byte[HeaderSize];

            Header[0] = (byte)Index;
            for (int i = 1; i < HeaderSize; i++)
                Header[i] = (byte)(Index >> (8 * i));

            return Header;
        }
        private int DecodeHeader(byte[] Header)
        {
            int Result = Header[0];

            for (int i = 1; i < Header.Length; i++)
                Result += Header[i] << (i * 8);

            return Result;
        }

    }
}
