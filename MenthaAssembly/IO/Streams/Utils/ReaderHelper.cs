using System;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class ReaderHelper
    {
        /// <summary>
        /// Moves the index to the next occurrence of any character in the specified array of end characters.
        /// </summary>
        /// <param name="Content">The content to search.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="Length">The length of the content.</param>
        /// <param name="SkipEndChar">Specifies whether to skip the end character.</param>
        /// <param name="EndChars">The array of end characters to search for.</param>
        /// <returns>True if the index is moved to the next occurrence of any character in the end characters array; otherwise, false.</returns>
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
        /// <summary>
        /// Moves the index to the next occurrence of a character that satisfies the specified predicate.
        /// </summary>
        /// <param name="Content">The content to search.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="Length">The length of the content.</param>
        /// <param name="SkipEndChar">Specifies whether to skip the end character.</param>
        /// <param name="Predicate">The predicate function to determine if a character satisfies the condition.</param>
        /// <returns>True if the index is moved to the next occurrence of a character that satisfies the predicate; otherwise, false.</returns>
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
        /// <summary>
        /// Moves the index to the next occurrence of any character in the specified array of end characters in the buffer.
        /// </summary>
        /// <param name="This">The TextReader instance.</param>
        /// <param name="Buffer">The character buffer.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="BufferSize">The size of the buffer.</param>
        /// <param name="SkipEndChar">Specifies whether to skip the end character.</param>
        /// <param name="EndChars">The array of end characters to search for.</param>
        /// <returns>True if the index is moved to the next occurrence of any character in the end characters array; otherwise, false.</returns>
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
        /// <summary>
        /// Moves the index to the next occurrence of a character that satisfies the specified predicate in the buffer.
        /// </summary>
        /// <param name="This">The TextReader instance.</param>
        /// <param name="Buffer">The character buffer.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="BufferSize">The size of the buffer.</param>
        /// <param name="SkipEndChar">Specifies whether to skip the end character.</param>
        /// <param name="Predicate">The predicate function to determine if a character satisfies the condition.</param>
        /// <returns>True if the index is moved to the next occurrence of a character that satisfies the predicate; otherwise, false.</returns>
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

        /// <summary>
        /// Reads a character from the TextReader and advances the index.
        /// </summary>
        /// <param name="This">The TextReader instance.</param>
        /// <param name="Buffer">The character buffer.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="BufferSize">The size of the buffer.</param>
        /// <returns>The character read from the TextReader.</returns>
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

        /// <summary>
        /// Reads characters from the content until an end character is encountered or the end of the content is reached.
        /// </summary>
        /// <param name="Content">The content to read from.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="Length">The length of the content.</param>
        /// <param name="ContainEndChar">Specifies whether to include the end character in the result.</param>
        /// <param name="IsEnd">Indicates whether the end of the content is reached.</param>
        /// <param name="EndChars">The array of end characters to search for.</param>
        /// <returns>The substring of the content read until an end character is encountered or the end of the content is reached.</returns>
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
        /// <summary>
        /// Reads characters from the content until a character that satisfies the specified predicate is encountered or the end of the content is reached.
        /// </summary>
        /// <param name="Content">The content to read from.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="Length">The length of the content.</param>
        /// <param name="ContainEndChar">Specifies whether to include the end character in the result.</param>
        /// <param name="IsEnd">Indicates whether the end of the content is reached.</param>
        /// <param name="Predicate">The predicate function to determine if a character satisfies the condition.</param>
        /// <returns>The substring of the content read until a character that satisfies the predicate is encountered or the end of the content is reached.</returns>
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
        /// <summary>
        /// Reads characters from the content until a break character is encountered or the end of the content is reached.
        /// </summary>
        /// <param name="Content">The content to read from.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="Length">The length of the content.</param>
        /// <param name="ContainEndChar">Specifies whether to include the end character in the result.</param>
        /// <param name="IsEnd">Indicates whether the end of the content is reached.</param>
        /// <param name="Builder">The StringBuilder instance to store the read characters.</param>
        /// <param name="IsBreak">The predicate function to determine if a character is a break character.</param>
        public static void ReadTo(string Content, ref int Index, int Length, bool ContainEndChar, out bool IsEnd, ref StringBuilder Builder, Predicate<char> IsBreak)
        {
            char c;
            for (; Index < Length; Index++)
            {
                c = Content[Index];
                if (IsBreak(c))
                {
                    if (ContainEndChar)
                    {
                        Builder.Append(c);
                        Index++;
                    }

                    IsEnd = false;
                    return;
                }

                Builder.Append(c);
            }

            IsEnd = true;
        }
        /// <summary>
        /// Reads characters from the content until a break character is encountered or the end of the content is reached, ignoring the ignored characters.
        /// </summary>
        /// <param name="Content">The content to read from.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="Length">The length of the content.</param>
        /// <param name="ContainEndChar">Specifies whether to include the end character in the result.</param>
        /// <param name="IsEnd">Indicates whether the end of the content is reached.</param>
        /// <param name="Builder">The StringBuilder instance to store the read characters.</param>
        /// <param name="IsIgnored">The predicate function to determine if a character is an ignored character.</param>
        /// <param name="IsBreak">The predicate function to determine if a character is a break character.</param>
        public static void ReadTo(string Content, ref int Index, int Length, bool ContainEndChar, out bool IsEnd, ref StringBuilder Builder, Predicate<char> IsIgnored, Predicate<char> IsBreak)
        {
            char c;
            for (; Index < Length; Index++)
            {
                c = Content[Index];
                if (IsIgnored(c))
                    continue;

                if (IsBreak(c))
                {
                    if (ContainEndChar)
                    {
                        Builder.Append(c);
                        Index++;
                    }

                    IsEnd = false;
                    return;
                }

                Builder.Append(c);
            }

            IsEnd = true;
        }

        /// <summary>
        /// Reads characters from the TextReader until an end character is encountered or the end of the content is reached.
        /// </summary>
        /// <param name="This">The TextReader instance.</param>
        /// <param name="Buffer">The character buffer.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="BufferSize">The size of the buffer.</param>
        /// <param name="Builder">The StringBuilder instance to store the read characters.</param>
        /// <param name="ContainEndChar">Specifies whether to include the end character in the result.</param>
        /// <param name="EndChars">The array of end characters to search for.</param>
        /// <returns>The substring of the content read until an end character is encountered or the end of the content is reached.</returns>
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
        /// <summary>
        /// Reads characters from the TextReader until an end character is encountered or the end of the content is reached, with a maximum length limit.
        /// </summary>
        /// <param name="This">The TextReader instance.</param>
        /// <param name="Buffer">The character buffer.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="BufferSize">The size of the buffer.</param>
        /// <param name="Builder">The StringBuilder instance to store the read characters.</param>
        /// <param name="ContainEndChar">Specifies whether to include the end character in the result.</param>
        /// <param name="MaxLength">The maximum length of the substring to read.</param>
        /// <param name="EndChars">The array of end characters to search for.</param>
        /// <returns>The substring of the content read until an end character is encountered or the end of the content is reached, with a maximum length limit.</returns>
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

        /// <summary>
        /// Checks if the TextReader can read characters from the buffer.
        /// </summary>
        /// <param name="This">The TextReader instance.</param>
        /// <param name="Buffer">The character buffer.</param>
        /// <param name="Index">The current index position.</param>
        /// <param name="BufferSize">The size of the buffer.</param>
        /// <returns>True if the TextReader can read characters from the buffer; otherwise, false.</returns>
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