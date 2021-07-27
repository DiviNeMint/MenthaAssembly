using System.ComponentModel;

namespace MenthaAssembly
{
    public class ChangedEventArgs<T> : HandledEventArgs
    {
        public T OldValue { get; }

        public T NewValue { get; }

        public ChangedEventArgs(T OldValue, T NewValue)
        {
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }
        public ChangedEventArgs(object OldValue, object NewValue)
        {
            this.OldValue = OldValue is T o ? o : default;
            this.NewValue = NewValue is T n ? n : default;
        }
    }
}
