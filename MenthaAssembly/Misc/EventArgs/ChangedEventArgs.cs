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
            this.OldValue = (T)OldValue;
            this.NewValue = (T)NewValue;
        }
    }
}
