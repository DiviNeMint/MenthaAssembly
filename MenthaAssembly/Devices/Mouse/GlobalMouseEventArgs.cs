using System.ComponentModel;

namespace MenthaAssembly.Devices
{
    public class GlobalMouseEventArgs : HandledEventArgs
    {
        public Point<int> Position { get; }

        public MouseKey ChangedButton { get; }

        public GlobalMouseEventArgs(Point<int> Position, MouseKey ChangedButton)
        {
            this.Position = Position;
            this.ChangedButton = ChangedButton;
        }

    }
}
