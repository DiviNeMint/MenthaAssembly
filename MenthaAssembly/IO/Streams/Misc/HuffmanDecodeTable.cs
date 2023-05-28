using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MenthaAssembly.IO
{
    public class HuffmanDecodeTable : IEnumerable<KeyValuePair<int, Dictionary<int, byte[]>>>
    {
        private readonly Dictionary<int, Dictionary<int, byte[]>> Context = new();

        public IEnumerable<int> Bits
            => Context.Keys;

        public IEnumerable<int> Codes
            => Context.SelectMany(i => i.Value.Keys);

        public Dictionary<int, byte[]> this[int Bit]
        {
            get => Bit <= 32 ? Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content) ? Content : null :
                   throw new NotSupportedException();
            set => Context[Bit] = value;
        }

        public byte[] this[int Bit, int Code]
        {
            get => Bit <= 32 ? (Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content) &&
                                Content.TryGetValue(Code, out byte[] Values) ? Values : null) :
                   throw new NotSupportedException();
            set
            {
                if (!Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content))
                {
                    Content = new Dictionary<int, byte[]>();
                    Context.Add(Bit, Content);
                }

                Content.Add(Code, value);
            }
        }

        public void Add(int Bit, int Code, params byte[] Values)
        {
            if (!Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content))
            {
                Content = new Dictionary<int, byte[]>();
                Context.Add(Bit, Content);
            }

            Content.Add(Code, Values);
        }

        public void Remove(int Bit)
            => Context.Remove(Bit);
        public void Remove(int Bit, int Code)
        {
            if (Context.TryGetValue(Bit, out Dictionary<int, byte[]> Content))
                Content.Remove(Code);
        }

        public void Clear()
            => Context.Clear();

        public IEnumerator<KeyValuePair<int, Dictionary<int, byte[]>>> GetEnumerator()
            => Context.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Context.GetEnumerator();

        public override string ToString()
        {
            StringBuilder Builder = new();
            try
            {
                foreach (KeyValuePair<int, Dictionary<int, byte[]>> Content in Context)
                {
                    int Bits = Content.Key;
                    foreach (KeyValuePair<int, byte[]> Data in Content.Value)
                        Builder.AppendLine($"{string.Join(", ", Data.Value.Select(i => $"{i:X2}"))} : {Convert.ToString(Data.Key, 2).PadLeft(Bits, '0')}");
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

    }
}