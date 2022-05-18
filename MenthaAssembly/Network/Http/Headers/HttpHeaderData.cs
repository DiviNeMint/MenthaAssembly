using System;

namespace MenthaAssembly.Network.Primitives
{
    public class HttpHeaderData
    {
        public string Key { get; }

        public string Value { get; set; }

        public HttpHeaderData(string Key)
        {
            if (string.IsNullOrEmpty(Key))
                throw new ArgumentNullException(nameof(Key));

            this.Key = Key;
        }
        public HttpHeaderData(string Key, string Value) : this(Key)
        {
            this.Value = Value;
        }

        public override string ToString()
            => $"{Key}: {Value}";

    }
}