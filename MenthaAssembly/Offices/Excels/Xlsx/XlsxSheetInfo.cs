namespace MenthaAssembly.Offices
{
    internal class XlsxSheetInfo
    {
        public uint Id { set; get; }

        public string Rid { set; get; }

        public string Name { set; get; }

        public string Path { set; get; }

        public bool Visible { set; get; }

        public XlsxSheetInfo(string Name, uint Id, string Rid, string Visible)
        {
            this.Name = Name;
            this.Id = Id;
            this.Rid = Rid;
            this.Visible = string.IsNullOrEmpty(Visible) || Visible.ToLower().Equals("visible");
        }
        public XlsxSheetInfo(string Name, uint Id, string Rid, bool Visible)
        {
            this.Name = Name;
            this.Id = Id;
            this.Rid = Rid;
            this.Visible = Visible;
        }
    }
}
