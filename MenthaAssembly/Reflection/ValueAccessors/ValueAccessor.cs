using System;

namespace MenthaAssembly.Reflection
{
    internal abstract class ValueAccessor
    {
        public abstract string Name { get; }

        public abstract Type ValueType { get; }

        public abstract void SetValue(object obj, object value);

        public abstract object GetValue(object obj);

        public override string ToString()
            => $"{Name}, {ValueType}";

    }
}