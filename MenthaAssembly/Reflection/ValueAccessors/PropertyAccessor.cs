using System;
using System.Reflection;

namespace MenthaAssembly.Reflection
{
    internal sealed class PropertyAccessor(PropertyInfo Property) : ValueAccessor
    {
        public override string Name
            => Property.Name;

        public override Type ValueType
            => Property.PropertyType;

        public override object GetValue(object obj)
            => Property.GetValue(obj);

        public override void SetValue(object obj, object value)
            => Property.SetValue(obj, value);

    }
}