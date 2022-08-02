namespace MenthaAssembly.Offices
{
    internal class XlsSheetInfo
    {
        public string Name { set; get; }

        /// <summary>
        /// Worksheet = 0x0,
        /// MacroSheet = 0x1,
        /// Chart = 0x2,
        /// VBModule = 0x6  // ReSharper disable once InconsistentNaming
        /// </summary>
        public int Type { set; get; }

        public bool Visible { set; get; }

        public uint Position { set; get; }

        public XlsSheetInfo(uint Position, string Name, int Type, bool Visible)
        {
            this.Name = Name;
            this.Type = Type;
            this.Visible = Visible;
            this.Position = Position;
        }

    }
}
