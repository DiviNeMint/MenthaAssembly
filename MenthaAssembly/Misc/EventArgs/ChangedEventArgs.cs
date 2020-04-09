using System;

namespace MenthaAssembly
{
    public class ChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; }

        public T NewValue { get; }

        public bool Handled { set; get; }

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
