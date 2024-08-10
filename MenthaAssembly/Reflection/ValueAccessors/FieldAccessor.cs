using System;
using System.Reflection;

namespace MenthaAssembly.Reflection
{
    internal sealed class FieldAccessor(FieldInfo Field) : ValueAccessor
    {
        public override string Name
            => Field.Name;

        public override Type ValueType
            => Field.FieldType;

        public override object GetValue(object obj)
            => Field.GetValue(obj);

        public override void SetValue(object obj, object value)
            => Field.SetValue(obj, value);

    }
}