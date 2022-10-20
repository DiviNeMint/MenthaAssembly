using System;

namespace MenthaAssembly.Expressions
{
    [Flags]
    public enum ExpressionObjectType
    {
        Unknown = 0,

        Identifier = 1,

        Block = 2,

        Element = 4,

        Const = 8,

        Member = 16,

        Method = 32,

        MathIdentifier = 257,

        LogicIdentifier = 513,

    }
}