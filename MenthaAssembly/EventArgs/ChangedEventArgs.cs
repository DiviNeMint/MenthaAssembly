using System;

namespace MenthaAssembly
{
    public class ChangedEventArgs<T> : EventArgs
    {
        public bool Handled { get; set; }

        public T OldValue { get; }

        public T NewValue { get; }

        public ChangedEventArgs(T OldValue, T NewValue)
        {
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }
    }
}
