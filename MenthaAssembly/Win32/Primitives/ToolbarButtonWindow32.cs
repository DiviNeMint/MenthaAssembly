namespace MenthaAssembly.Win32.Primitives
{
    public class ToolbarButtonWindow32
    {
        public ToolbarWindow32 Parent { get; }

        public int Index { get; }

        public string Text { get; }

        public Int32Bound Bound { get; }

        internal ToolbarButtonWindow32(ToolbarWindow32 Parent, int Index, Int32Bound Bound, string Text)
        {
            this.Parent = Parent;
            this.Index = Index;
            this.Bound = Bound;
            this.Text = Text;
        }

    }

    public class ToolbarButtonWindow32<T> : ToolbarButtonWindow32
        where T : struct
    {
        public T DataContext { get; }

        internal ToolbarButtonWindow32(ToolbarWindow32 Parent, int Index, Int32Bound Bound, string Text, T DataContext) : base(Parent, Index, Bound, Text)
        {
            this.DataContext = DataContext;
        }

    }

}
