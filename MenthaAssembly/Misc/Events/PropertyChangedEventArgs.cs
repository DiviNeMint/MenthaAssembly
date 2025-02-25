using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MenthaAssembly
{
    public sealed class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        public T OldValue { get; }

        public T NewValue { get; }

        public PropertyChangedEventArgs(string PropertyName, T OldValue, T NewValue) : base(PropertyName)
        {
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }
        public PropertyChangedEventArgs(string PropertyName, object OldValue, object NewValue) : base(PropertyName)
        {
            this.OldValue = OldValue is T o ? o : default;
            this.NewValue = NewValue is T n ? n : default;
        }

        public static PropertyChangedEventArgs Create(T OldValue, T NewValue, [CallerMemberName] string PropertyName = null)
            => new PropertyChangedEventArgs<T>(PropertyName, OldValue, NewValue);

    }
}