using System;

namespace MenthaAssembly.Expressions
{
    [Flags]
    public enum ExpressionObjectType
    {
        Unknown = 0,

        Identifier = 1,
        
        Block = 2,
        
        Const = 4,

        Convert = 5,

        Route = 6,

        Member = 262,
        Indexer = 518,
        Method = 1030,

        MathIdentifier = 257,
        LogicIdentifier = 513,

    }
}