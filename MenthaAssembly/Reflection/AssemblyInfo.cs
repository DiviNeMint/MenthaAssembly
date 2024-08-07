namespace System.Reflection
{
    public sealed class AssemblyInfo(string Name, string Location, LibraryType Type)
    {
        public string Name { get; } = Name;

        public string Location { get; } = Location;

        public LibraryType Type { get; } = Type;

    }
}