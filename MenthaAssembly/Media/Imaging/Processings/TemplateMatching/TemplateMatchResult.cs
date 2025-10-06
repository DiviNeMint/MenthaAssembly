namespace MenthaAssembly.Media.Imaging
{
    public sealed class TemplateMatchResult(int X, int Y, double Score)
    {
        public int X { get; } = X;

        public int Y { get; } = Y;

        public double Score { get; } = Score;

        public override string ToString()
            => $"Location : ({X}, {Y}), Score : {Score}";
    }
}