using System;
using System.Windows;
using System.Windows.Input;

namespace MenthaAssembly.Devices
{
    public class GlobalMouseEventArgs : EventArgs
    {
        public Int32Point Position { get; }

        public MouseButton ChangedButton { get; }

        public bool Handled { set; get; }

        public GlobalMouseEventArgs(Int32Point Position, MouseButton ChangedButton)
        {
            this.Position = Position;
            this.ChangedButton = ChangedButton;
        }

    }
}
