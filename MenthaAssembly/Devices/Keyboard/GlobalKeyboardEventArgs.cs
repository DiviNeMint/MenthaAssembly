using System;

namespace MenthaAssembly.Devices
{
    public class GlobalKeyboardEventArgs : EventArgs
    {
        public bool Handled { set; get; }

        public KeyboardKey Key { get; }

        public GlobalKeyboardEventArgs(KeyboardKey Key)
        {
            this.Key = Key;
        }

    }
}
