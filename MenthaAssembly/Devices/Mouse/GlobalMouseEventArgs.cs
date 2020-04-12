using System;

namespace MenthaAssembly.Devices
{
    public class GlobalMouseEventArgs : EventArgs
    {
        public bool Handled { set; get; }

        public Int32Point Position { get; }

        public MouseKey ChangedButton { get; }

        public GlobalMouseEventArgs(Int32Point Position, MouseKey ChangedButton)
        {
            this.Position = Position;
            this.ChangedButton = ChangedButton;
        }

    }
}
