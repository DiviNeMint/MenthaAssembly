using System.ComponentModel;

namespace MenthaAssembly.Devices
{
    public class GlobalMouseEventArgs : HandledEventArgs
    {
        public Int32Point Position { get; }

        public MouseKey ChangedButton { get; }

        public GlobalMouseEventArgs(Int32Point Position, MouseKey ChangedButton)
        {
            this.Position = Position;
            this.ChangedButton = ChangedButton;
        }

    }
}
