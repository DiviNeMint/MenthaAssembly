namespace MenthaAssembly.Devices
{
    public class PrinterPaper
    {
        public string Name { get; }

        public PaperTypes Type { get; }

        public LengthUnit Unit { get; }

        public double Width { get; }

        public double Height { get; }

        internal PrinterPaper(string Name, PaperTypes Type, double Width, double Height, LengthUnit Unit)
        {
            this.Name = Name;
            this.Type = Type;
            this.Width = Width;
            this.Height = Height;
            this.Unit = Unit;
        }
        public PrinterPaper(string Name, double Width, double Height, LengthUnit Unit)
        {
            this.Name = Name;
            Type = PaperTypes.Custom;
            this.Width = Width;
            this.Height = Height;
            this.Unit = Unit;
        }

        public override string ToString()
            => $"Name : {Name}, Kind : {Type}, Size : {Width} x {Height} {Unit}";

    }
}
