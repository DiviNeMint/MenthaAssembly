namespace MenthaAssembly.Expressions
{
    public sealed class ExpressionObject : IExpressionObject
    {
        ExpressionObjectType IExpressionObject.Type
            => ExpressionObjectType.Unknown;

        public string Context { get; }

        public ExpressionObject(string Context)
        {
            this.Context = Context;
        }

        public override string ToString()
            => Context;

    }
}