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

        Convert = Const | Identifier,

        Route = Const | Block,

        Member = 256 | Route,
        Indexer = 512 | Route,
        Method = 1024 | Route,

    }
}