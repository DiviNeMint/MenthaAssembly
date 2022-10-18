using System;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class ReaderHelper
    {
        public static bool MoveTo(string Content, ref int Index, int Length, bool SkipEndChar, params char[] EndChars)
        {
            for (; Index < Length; Index++)
            {
                if (EndChars.Contains(Content[Index]))
                {
                    if (SkipEndChar)
                        Index++;

                    return true;
                }
            }

            return false;
        }
        public static bool MoveTo(string Content, ref int Index, int Length, bool SkipEndChar, Predicate<char> Predicate)
        {
            for (; Index < Length; Index++)
            {
                if (Predicate(Content[Index]))
                {
                    if (SkipEndChar)
                        Index++;

                    return true;
                }
            }

            return false;
        }
        public static bool MoveTo(this TextReader This, ref char[] Buffer, ref int Index, int BufferSize, bool SkipEndChar, params char[] EndChars)
        {
            do
            {
                for (; Index < BufferSize; Index++)
                {
                    if (EndChars.Contains(Buffer[Index]))
                    {
                        if (SkipEndChar)
                            Index++;

                        return true;
                    }
                }

                if (This.Read(Buffer, 0, BufferSize) > 0)
                    Index -= BufferSize;

            } while (Index < BufferSize);

            return false;
        }
        public static bool MoveTo(this TextReader This, ref char[] Buffer, ref int Index, int BufferSize, bool SkipEndChar, Predicate<char> Predicate)
        {
            do
            {
                for (; Index < BufferSize; Index++)
                {
                    if (Predicate(Buffer[Index]))
                    {
                        if (SkipEndChar)
                            Index++;

                        return true;
                    }
                }

                if (This.Read(Buffer, 0, BufferSize) > 0)
                    Index -= BufferSize;

            } while (Index < BufferSize);

            return false;
        }

        public static char Read(this TextReader This, ref char[] Buffer, ref int Index, int BufferSize)
        {
            Index++;

            while (BufferSize < Index)
            {
                if (This.Read(Buffer, 0, BufferSize) == 0)
                    throw new EndOfStreamException();

                Index -= BufferSize;
            }

            return Buffer[Index];
        }

        public static string ReadTo(string Content, ref int Index, int Length, bool ContainEndChar, out bool IsEnd, params char[] EndChars)
        {
            int Start = Index,
                SubLength = 0;

            for (; Index < Length; Index++, SubLength++)
            {
                if (EndChars.Contains(Content[Index]))
                {
                    if (ContainEndChar)
                    {
                        SubLength++;
                        Index++;
                    }

                    IsEnd = false;
                    return Content.Substring(Start, SubLength);
                }
            }

            IsEnd = true;
            return Content.Substring(Start, SubLength);
        }
        public static string ReadTo(string Content, ref int Index, int Length, bool ContainEndChar, out bool IsEnd, Predicate<char> Predicate)
        {
            int Start = Index,
                SubLength = 0;

            for (; Index < Length; Index++, SubLength++)
            {
                if (Predicate(Content[Index]))
                {
                    if (ContainEndChar)
                    {
                        SubLength++;
                        Index++;
                    }

                    IsEnd = false;
                    return Content.Substring(Start, SubLength);
                }
            }

            IsEnd = true;
            return Content.Substring(Start, SubLength);
        }
        public static string ReadTo(this TextReader This, ref char[] Buffer, ref int Index, int BufferSize, ref StringBuilder Builder, bool ContainEndChar, params char[] EndChars)
        {
            try
            {
                while (BufferSize < Index)
                {
                    if (This.Read(Buffer, 0, BufferSize) == 0)
                        return null;

                    Index -= BufferSize;
                }

                int Start = Index,
                    Length = 0;

                do
                {
                    for (; Index < BufferSize; Index++, Length++)
                    {
                        if (EndChars.Contains(Buffer[Index]))
                        {
                            if (ContainEndChar)
                            {
                                Length++;
                                Index++;
                            }

                            Builder.Append(Buffer, Start, Length);
                            return Builder.ToString();
                        }
                    }

                    Builder.Append(Buffer, Start, Length);

                    if (This.Read(Buffer, 0, BufferSize) > 0)
                    {
                        Index -= BufferSize;
                        Start = Index;
                        Length = 0;
                    }

                } while (Index < BufferSize);

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }
        public static string ReadTo(this TextReader This, ref char[] Buffer, ref int Index, int BufferSize, ref StringBuilder Builder, bool ContainEndChar, int MaxLength, params char[] EndChars)
        {
            try
            {
                while (BufferSize < Index)
                {
                    if (This.Read(Buffer, 0, BufferSize) == 0)
                        return null;

                    Index -= BufferSize;
                }

                int Start = Index,
                    Length = 0;

                do
                {
                    for (; Index < BufferSize; Index++, Length++)
                    {
                        if (MaxLength <= Length)
                        {
                            Builder.Append(Buffer, Start, Length);
                            return Builder.ToString();
                        }

                        if (EndChars.Contains(Buffer[Index]))
                        {
                            if (ContainEndChar)
                            {
                                Length++;
                                Index++;
                            }

                            Builder.Append(Buffer, Start, Length);
                            return Builder.ToString();
                        }
                    }

                    Builder.Append(Buffer, Start, Length);

                    if (This.Read(Buffer, 0, BufferSize) > 0)
                    {
                        Index -= BufferSize;
                        Start = Index;
                        MaxLength -= Length;
                        Length = 0;
                    }

                } while (Index < BufferSize);

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        public static bool CanRead(this TextReader This, ref char[] Buffer, ref int Index, int BufferSize)
        {
            while (BufferSize < Index)
            {
                if (This.Read(Buffer, 0, BufferSize) == 0)
                    return false;

                Index -= BufferSize;
            }

            return true;
        }

    }
}