using System;

namespace MenthaAssembly
{
    [Flags]
    public enum KeyboardInputMode
    {
        All = 0,
        Alphabet = 1,
        Number = 2,
        Negative = 4,
        Dot = 8,

        NumberAndDot = Number | Dot,
        NegativeNumber = Number | Negative,
        NegativeNumberAndDot = Number | Negative | Dot,
    }
}
