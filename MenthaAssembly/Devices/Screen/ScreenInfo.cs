namespace MenthaAssembly.Devices
{
    public class ScreenInfo
    {
        public string DeviceID { get; }

        public Int32Bound Bound { get; }

        public Int32Bound WorkArea { get; }

        internal ScreenInfo(string DeviceID ,Int32Bound Bound, Int32Bound WorkArea)
        {
            this.DeviceID = DeviceID;
            this.Bound = Bound;
            this.WorkArea = WorkArea;
        }

    }
}
