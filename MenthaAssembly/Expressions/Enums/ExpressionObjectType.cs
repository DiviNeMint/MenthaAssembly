using System;

namespace MenthaAssembly.Expressions
{
    [Flags]
    public enum ExpressionObjectType
    {
        Unknown = 0,
        Identifier = 1,
        Element = 2,
        Block = 4,
        Method = 8,

        MathIdentifier = 257,
        LogicIdentifier = 513,

    }
}