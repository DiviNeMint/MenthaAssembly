namespace MenthaAssembly.Media.Imaging
{
    public sealed class ConstThreshold : ImageThreshold
    {
        public byte Threshold { get; }

        public ConstThreshold(byte Threshold) : base(true)
        {
            this.Threshold = Threshold;
        }

        protected internal override bool InternalPredict(int X, int Y, byte Gray)
            => Threshold <= Gray;

        public override ImageThreshold Clone()
            => this;

    }
}