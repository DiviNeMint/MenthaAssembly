using MenthaAssembly.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public static class ExpressionHelper
    {
        public static object GetValue(this Expression This)
            => This.GetValue<object>();
        public static T GetValue<T>(this Expression This)
        {
            if (This is ConstantExpression CExpression)
                return (T)CExpression.Value;

            UnaryExpression objectMember = Expression.Convert(This, typeof(T));
            return Expression.Lambda<Func<T>>(objectMember)
                             .Compile()
                             .Invoke();
        }

        public static Func<object, object> CreateExtractor(Type ParentType, string Path)
        {
            Type t = typeof(object);
            StringBuilder Builder = new StringBuilder(128);

            try
            {
                int Length = Path.Length;
                char c;
                ParameterExpression Arg = Expression.Parameter(t, "a");
                Expression Body = Expression.Convert(Arg, ParentType);
                for (int i = 0; i < Length; i++)
                {
                    c = Path[i];

                    // Property
                    if (c.Equals('.'))
                    {
                        string PropertyName = Builder.ToString();
                        Builder.Clear();

                        if (string.IsNullOrEmpty(PropertyName))
                            continue;

                        if (ParentType.GetProperty(PropertyName) is PropertyInfo Property)
                        {
                            ParentType = Property.PropertyType;
                            Body = Expression.Property(Body, Property);
                            continue;
                        }

                        Debug.WriteLine($"[Parse]Unknown Property : {PropertyName}, Path : {Path}.");
                        return null;
                    }

                    // Method
                    if (c.Equals('('))
                    {
                        c = Path[++i];

                        if (!c.Equals(')'))
                        {
                            Debug.WriteLine($"[Parse]Unknown format path : {Path}.");
                            return null;
                        }

                        string MethodName = Builder.ToString();
                        Builder.Clear();

                        if (string.IsNullOrEmpty(MethodName))
                            continue;

                        if (ParentType.GetMethod(MethodName, new Type[0]) is MethodInfo Method)
                        {
                            ParentType = Method.ReturnType;
                            Body = Expression.Call(Body, Method);
                            continue;
                        }

                        Debug.WriteLine($"[Parse]Unknown Method : {MethodName}, Path : {Path}.");
                        return null;
                    }

                    // Indexer
                    if (c.Equals('['))
                    {
                        string PropertyName = Builder.ToString();
                        Builder.Clear();

                        if (!ParentType.TryGetProperty(string.IsNullOrEmpty(PropertyName) ? "Item" : PropertyName, out PropertyInfo Property))
                        {
                            Debug.WriteLine($"[Parse]Unknown Property : {PropertyName}, Path : {Path}.");
                            return null;
                        }

                        i++;
                        bool IsNumber = true;
                        for (; i < Length; i++)
                        {
                            c = Path[i];
                            if (c.Equals(']'))
                                break;

                            if (!char.IsNumber(c))
                            {
                                IsNumber = false;

                                if (c.Equals(@""""))
                                    continue;
                            }

                            Builder.Append(c);
                        }

                        string Data = Builder.ToString();
                        Builder.Clear();

                        ParentType = Property.PropertyType;
                        Body = IsNumber ? Expression.Property(Body, Property, Expression.Constant(Data.ToInt32Fast())) :
                                          Expression.Property(Body, Property, Expression.Constant(Data));

                        continue;
                    }

                    Builder.Append(c);
                }

                string LastPropertyName = Builder.ToString();
                Builder.Clear();

                if (!string.IsNullOrEmpty(LastPropertyName) &&
                    ParentType.GetProperty(LastPropertyName) is PropertyInfo LastProperty)
                {
                    ParentType = LastProperty.PropertyType;
                    Body = Expression.Property(Body, LastProperty);
                }

                return Expression.Lambda<Func<object, object>>(Expression.Convert(Body, t), Arg)
                                 .Compile();
            }
            finally
            {
                Builder.Clear();
            }
        }
        public static Func<object, T> CreateExtractor<T>(Type ParentType, string Path)
        {
            Type t = typeof(object);
            StringBuilder Builder = new StringBuilder(128);

            try
            {
                int Length = Path.Length;
                char c;
                ParameterExpression Arg = Expression.Parameter(t, "a");
                Expression Body = Expression.Convert(Arg, ParentType);
                for (int i = 0; i < Length; i++)
                {
                    c = Path[i];

                    // Property
                    if (c.Equals('.'))
                    {
                        string PropertyName = Builder.ToString();
                        Builder.Clear();

                        if (string.IsNullOrEmpty(PropertyName))
                            continue;

                        if (ParentType.GetProperty(PropertyName) is PropertyInfo Property)
                        {
                            ParentType = Property.PropertyType;
                            Body = Expression.Property(Body, Property);
                            continue;
                        }

                        Debug.WriteLine($"[Parse]Unknown Property : {PropertyName}, Path : {Path}.");
                        return null;
                    }

                    // Method
                    if (c.Equals('('))
                    {
                        c = Path[++i];

                        if (!c.Equals(')'))
                        {
                            Debug.WriteLine($"[Parse]Unknown format path : {Path}.");
                            return null;
                        }

                        string MethodName = Builder.ToString();
                        Builder.Clear();

                        if (string.IsNullOrEmpty(MethodName))
                            continue;

                        if (ParentType.GetMethod(MethodName, new Type[0]) is MethodInfo Method)
                        {
                            ParentType = Method.ReturnType;
                            Body = Expression.Call(Body, Method);
                            continue;
                        }

                        Debug.WriteLine($"[Parse]Unknown Method : {MethodName}, Path : {Path}.");
                        return null;
                    }

                    // Indexer
                    if (c.Equals('['))
                    {
                        string PropertyName = Builder.ToString();
                        Builder.Clear();

                        if (!ParentType.TryGetProperty(string.IsNullOrEmpty(PropertyName) ? "Item" : PropertyName, out PropertyInfo Property))
                        {
                            Debug.WriteLine($"[Parse]Unknown Property : {PropertyName}, Path : {Path}.");
                            return null;
                        }

                        i++;
                        bool IsNumber = true;
                        for (; i < Length; i++)
                        {
                            c = Path[i];
                            if (c.Equals(']'))
                                break;

                            if (!char.IsNumber(c))
                            {
                                IsNumber = false;

                                if (c.Equals(@""""))
                                    continue;
                            }

                            Builder.Append(c);
                        }

                        string Data = Builder.ToString();
                        Builder.Clear();

                        ParentType = Property.PropertyType;
                        Body = IsNumber ? Expression.Property(Body, Property, Expression.Constant(Data.ToInt32Fast())) :
                                          Expression.Property(Body, Property, Expression.Constant(Data));

                        continue;
                    }

                    Builder.Append(c);
                }

                string LastPropertyName = Builder.ToString();
                Builder.Clear();

                if (!string.IsNullOrEmpty(LastPropertyName) &&
                    ParentType.GetProperty(LastPropertyName) is PropertyInfo LastProperty)
                {
                    ParentType = LastProperty.PropertyType;
                    Body = Expression.Property(Body, LastProperty);
                }

                Type ReturnType = typeof(T);

                if (!ParentType.Equals(ReturnType))
                {
                    if ((ReturnType.IsIntegerType() || ReturnType.IsDecimalType()) &&
                        (ParentType.IsIntegerType() || ParentType.IsDecimalType()))
                        Body = Expression.Convert(Body, ReturnType);
                    else
                    {
                        if (typeof(Convert).TryGetStaticMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) }, out MethodInfo Method))
                            Body = Expression.Call(Method, Body, Expression.Constant(ReturnType));

                        Body = Expression.Convert(Body, ReturnType);
                    }
                }

                return Expression.Lambda<Func<object, T>>(Body, Arg)
                                 .Compile();
            }
            finally
            {
                Builder.Clear();
            }
        }

        public static bool TryParse(string Code, out ExpressionBlock Block)
        {
            StringBuilder Builder = new();
            int Index = 0,
                Length = Code.Length;

            Block = new ExpressionBlock();
            while (TryParseElement(Code, ref Index, Length, ref Builder, out IExpressionObject Element))
                Block.Contexts.Add(Element);

            if (Block.Contexts.Count == 0 || Index < Length)
            {
                Block = null;
                return false;
            }

            NormalizeConverter(Block);
            return true;
        }
        private static bool TryParseElement(string Code, ref int Index, int Length, ref StringBuilder Builder, out IExpressionObject Element)
        {
            if (!ReaderHelper.MoveTo(Code, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
            {
                Element = null;
                return false;
            }

            char c = Code[Index];
            switch (c)
            {
                #region Block
                case '(':
                    {
                        Index++;

                        ExpressionBlock Block = new();
                        while (TryParseElement(Code, ref Index, Length, ref Builder, out IExpressionObject Child))
                            Block.Contexts.Add(Child);

                        if (Index < Length)
                        {
                            c = Code[Index];
                            if (c == ')')
                            {
                                Index++;
                                Element = Block;
                                return true;
                            }
                        }

                        Debug.WriteLine($"[Expression][Parser]Unknown Element.");
                        Element = null;
                        return false;
                    }
                case ')':
                case ']':
                case '}':
                case ',':
                    {
                        Element = null;
                        return false;
                    }
                #endregion
                #region Identifier
                case '+':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("+", ExpressionType.Add);
                        return true;
                    }
                case '-':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("-", ExpressionType.Subtract);
                        return true;
                    }
                case '*':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("*", ExpressionType.Multiply);
                        return true;
                    }
                case '/':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("/", ExpressionType.Divide);
                        return true;
                    }
                case '%':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("%", ExpressionType.Modulo);
                        return true;
                    }
                case '＆':
                case '&':
                    {
                        Index++;
                        if (Index < Length)
                        {
                            c = Code[Index];
                            switch (c)
                            {
                                case '＆':
                                case '&':
                                    {
                                        Index++;
                                        Element = new ExpressionIdentifier("&&", ExpressionType.AndAlso);
                                        return true;
                                    }
                            }
                        }

                        Element = new ExpressionIdentifier("&", ExpressionType.And);
                        return true;
                    }
                case '|':
                    {
                        Index++;
                        if (Index < Length)
                        {
                            c = Code[Index];
                            if (c == '|')
                            {
                                Index++;
                                Element = new ExpressionIdentifier("||", ExpressionType.OrElse);
                                return true;
                            }
                        }

                        Element = new ExpressionIdentifier("|", ExpressionType.Or);
                        return true;
                    }
                case '^':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("^", ExpressionType.ExclusiveOr);
                        return true;
                    }
                case '!':
                    {
                        Index++;
                        if (Index < Length)
                        {
                            c = Code[Index];
                            if (c == '=')
                            {
                                Index++;
                                Element = new ExpressionIdentifier("!=", ExpressionType.NotEqual);
                                return true;
                            }
                        }

                        Element = new ExpressionIdentifier("!", ExpressionType.Not);
                        return true;
                    }
                case '~':
                    {
                        Index++;
                        Element = new ExpressionIdentifier("~", ExpressionType.OnesComplement);
                        return true;
                    }
                case '＜':
                case '<':
                    {
                        Index++;
                        if (Index < Length)
                        {
                            c = Code[Index];
                            switch (c)
                            {
                                case '=':
                                    {
                                        Index++;
                                        Element = new ExpressionIdentifier("<=", ExpressionType.LessThanOrEqual);
                                        return true;
                                    }
                                case '＜':
                                case '<':
                                    {
                                        Index++;
                                        Element = new ExpressionIdentifier("<<", ExpressionType.LeftShift);
                                        return true;
                                    }
                            }
                        }

                        Element = new ExpressionIdentifier("<", ExpressionType.LessThan);
                        return true;
                    }
                case '＞':
                case '>':
                    {
                        Index++;
                        if (Index < Length)
                        {
                            c = Code[Index];
                            switch (c)
                            {
                                case '=':
                                    {
                                        Index++;
                                        Element = new ExpressionIdentifier(">=", ExpressionType.GreaterThanOrEqual);
                                        return true;
                                    }
                                case '＞':
                                case '>':
                                    {
                                        Index++;
                                        Element = new ExpressionIdentifier(">>", ExpressionType.RightShift);
                                        return true;
                                    }
                            }
                        }

                        Element = new ExpressionIdentifier(">", ExpressionType.GreaterThan);
                        return true;
                    }
                case '=':
                    {
                        Index++;
                        if (Index < Length)
                        {
                            c = Code[Index];
                            if (c == '=')
                            {
                                Index++;
                                Element = new ExpressionIdentifier("==", ExpressionType.Equal);
                                return true;
                            }
                        }

                        Element = null;
                        return false;
                    }
                #endregion
                #region Constant
                // String
                case '"':
                    {
                        Index++;    // Skip start char '"'
                        string Content = ReaderHelper.ReadTo(Code, ref Index, Length, false, out bool IsEnd, '"');
                        if (IsEnd)
                        {
                            Element = new ExpressionObject(Content);
                            return true;
                        }

                        Index++;    // Skip End char '"'
                        Element = new ExpressionConst(Content);
                        return true;
                    }

                // Decimal
                case '.':
                    {
                        // Append decimal point '.'.
                        Index++;
                        Builder.Append(c);

                        Element = ParseDecimalNumber(Code, ref Index, Length, ref Builder);
                        return true;
                    }
                // Number
                case char n when n.IsArabicNumerals():
                    {
                        // Append the first number.
                        Index++;
                        Builder.Append(c);

                        Element = ParseNumber(Code, ref Index, Length, ref Builder);
                        return true;
                    }
                #endregion
                default:
                    {
                        Element = ParseRoute(Code, ref Index, Length, ref Builder);
                        return true;
                    }
            }
        }
        private static void NormalizeConverter(ExpressionBlock Block)
            => NormalizeConverter(Block.Contexts);
        private static void NormalizeConverter(ExpressionRoute Route)
        {
            IExpressionRoute Context = Route.Contexts[Route.Contexts.Count - 1];
            if (Context is ExpressionMethod Method)
            {
                NormalizeConverter(Method.Parameters);
            }

            else if (Context is ExpressionIndexer Indexer)
            {
                NormalizeConverter(Indexer.Parameters);
            }
        }
        private static void NormalizeConverter(List<IExpressionObject> Contexts)
        {
            for (int i = 0; i < Contexts.Count;)
            {
                IExpressionObject Curt = Contexts[i++];
                if (Curt is ExpressionBlock Group)
                {
                    // Check if it could be a converter.
                    if (!TryGetConverterType(Group, out ExpressionRoute Type))
                    {
                        NormalizeConverter(Group);
                        continue;
                    }

                    // Checks end.
                    if (i >= Contexts.Count)
                        break;

                    // Checks if the it is a variable by checking if next one is an operator.
                    if (Contexts[i].Type == ExpressionObjectType.Identifier)
                        continue;

                    Contexts.Insert(i - 1, new ExpressionConvert(Type));
                    Contexts.RemoveAt(i);
                }
                else if (Curt is ExpressionRoute Route)
                {
                    NormalizeConverter(Route);
                }
            }
        }
        private static bool TryGetConverterType(ExpressionBlock Block, out ExpressionRoute Type)
        {
            if (Block.Contexts.Count != 1 ||
                Block.Contexts[0] is not ExpressionRoute Route ||
                Route.Contexts.Any(i => i.Type != ExpressionObjectType.Member))
            {
                Type = null;
                return false;
            }

            Type = Route;
            return true;
        }

        public static IEnumerable<string> EnumParameterNames(ExpressionBlock Block)
            => EnumParameterNames(Block.Contexts);
        private static IEnumerable<string> EnumParameterNames(ExpressionRoute Route)
        {
            if (Route.Contexts.Count == 1)
            {
                IExpressionRoute Path = Route.Contexts[0];
                if (Path.Type == ExpressionObjectType.Member)
                    yield return Path.Name;

                yield break;
            }

            IExpressionRoute Context = Route.Contexts[Route.Contexts.Count - 1];
            if (Context is ExpressionMethod Method)
            {
                foreach (string Name in EnumParameterNames(Method.Parameters))
                    yield return Name;
            }

            else if (Context is ExpressionIndexer Indexer)
            {
                foreach (string Name in EnumParameterNames(Indexer.Parameters))
                    yield return Name;
            }
        }
        private static IEnumerable<string> EnumParameterNames(List<IExpressionObject> Contents)
        {
            foreach (IExpressionObject Content in Contents)
            {
                if (Content is ExpressionRoute Route)
                {
                    foreach (string Name in EnumParameterNames(Route))
                        yield return Name;
                }

                else if (Content is ExpressionBlock Block)
                {
                    foreach (string Name in EnumParameterNames(Block))
                        yield return Name;
                }
            }
        }

        /// <summary>
        /// Parses numbers.
        /// </summary>
        public static IExpressionObject ParseNumber(string Code, ref int Index, int Length, ref StringBuilder Builder)
        {
            try
            {
                ReaderHelper.ReadTo(Code, ref Index, Length, false, out bool IsEnd, ref Builder, c => !c.IsArabicNumerals());

                // Integer
                if (IsEnd)
                    return new ExpressionConst(Builder.ToString().ToUnsignInt32Fast());

                char c = Code[Index];
                if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                    return new ExpressionConst(Builder.ToString().ToUnsignInt32Fast());

                Index++;

                // Decimal
                if (c == '.')
                {
                    // Append decimal point '.'.
                    Builder.Append(c);
                    return ParseDecimalNumber(Code, ref Index, Length, ref Builder);
                }

                // Specified Integer
                bool AllowSpecifiedType = true;
                if (Builder.ToString() == "0")
                {
                    Builder.Append(c);

                    // Hex
                    if (c is 'x' or 'X')
                    {
                        // Format
                        string Format = ReaderHelper.ReadTo(Code, ref Index, Length, false, out IsEnd, c => !c.IsHexNumerals());
                        if (Format.Length == 0)
                        {
                            Builder.Append(ReaderHelper.ReadTo(Code, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars()));
                            return new ExpressionObject(Builder.ToString());
                        }

                        if (IsEnd)
                            return new ExpressionConst(int.Parse(Format, NumberStyles.HexNumber));

                        c = Code[Index];
                        if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                            return new ExpressionConst(int.Parse(Format, NumberStyles.HexNumber));

                        Index++;
                        Builder.Append(Format);
                        AllowSpecifiedType = false;
                    }

                    // Binary
                    else if (c is 'b' or 'B')
                    {
                        // Format
                        string Format = ReaderHelper.ReadTo(Code, ref Index, Length, false, out IsEnd, c => !c.IsArabicNumerals());
                        if (Format.Length == 0)
                        {
                            Builder.Append(ReaderHelper.ReadTo(Code, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars()));
                            return new ExpressionObject(Builder.ToString());
                        }

                        if (IsEnd)
                            return new ExpressionConst(Convert.ToInt32(Format, 2));

                        c = Code[Index];
                        if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                            return new ExpressionConst(Convert.ToInt32(Format, 2));

                        Index++;
                        Builder.Append(Format);
                        AllowSpecifiedType = false;
                    }
                }

                string Remained = ReaderHelper.ReadTo(Code, ref Index, Length, false, out _, c => !c.IsVariableChars());
                if (!string.IsNullOrEmpty(Remained))
                {
                    Builder.Append(c);
                    Builder.Append(Remained);
                    return new ExpressionObject(Builder.ToString());
                }

                // Sepeified Types
                if (AllowSpecifiedType)
                {
                    switch (c)
                    {
                        case 'f':
                        case 'F':
                            return new ExpressionConst(Builder.ToString().ToUnsignFloatFast());
                        case 'd':
                        case 'D':
                            return new ExpressionConst(Builder.ToString().ToUnsignDoubleFast());
                        case 'm':
                        case 'M':
                            return new ExpressionConst(Builder.ToString().ToUnsignDecimalFast());
                        case 'l':
                        case 'L':
                            return new ExpressionConst(Builder.ToString().ToUnsignInt64Fast());
                        case 'u':
                        case 'U':
                            return new ExpressionConst(Builder.ToString().ToUInt32Fast());
                    }
                }

                // Unknown
                Builder.Append(c);
                return new ExpressionObject(Builder.ToString());
            }
            finally
            {
                Builder.Clear();
            }
        }
        /// <summary>
        /// Parses decimals. (Start at the right of the decimal point)
        /// </summary>
        public static IExpressionObject ParseDecimalNumber(string Code, ref int Index, int Length, ref StringBuilder Builder)
        {
            try
            {
                ReaderHelper.ReadTo(Code, ref Index, Length, false, out bool IsEnd, ref Builder, c => !c.IsArabicNumerals());

                if (IsEnd)
                    return new ExpressionConst(Builder.ToString().ToDoubleFast());

                char c = Code[Index];
                if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                    return new ExpressionConst(Builder.ToString().ToDoubleFast());

                Index++;

                string Remained = ReaderHelper.ReadTo(Code, ref Index, Length, false, out _, c => !c.IsVariableChars());
                if (!string.IsNullOrEmpty(Remained))
                {
                    Builder.Append(c);
                    Builder.Append(Remained);
                    return new ExpressionObject(Builder.ToString());
                }

                // Sepeified Types
                switch (c)
                {
                    case 'f':
                    case 'F':
                        return new ExpressionConst(Builder.ToString().ToFloatFast());
                    case 'd':
                    case 'D':
                        return new ExpressionConst(Builder.ToString().ToDoubleFast());
                    case 'm':
                    case 'M':
                        return new ExpressionConst(decimal.Parse(Builder.ToString()));
                }

                // Unknown
                Builder.Append(c);
                return new ExpressionObject(Builder.ToString());
            }
            finally
            {
                Builder.Clear();
            }
        }

        /// <summary>
        /// Parses route. (Variable or Method or Member)
        /// </summary>
        public static IExpressionObject ParseRoute(string Code, ref int Index, int Length, ref StringBuilder Builder)
        {
            ExpressionRoute Route = new();
            List<ExpressionTypeInfo> GenericTypes = null;
            char c;
            while (TryParseName(Code, ref Index, Length, ref Builder, out bool IsEnd, out string Name))
            {
                if (IsEnd ||
                    !ReaderHelper.MoveTo(Code, ref Index, Length, false, c => !char.IsWhiteSpace(c)))   // Skips all trailing spaces.
                {
                    Route.Contexts.Add(new ExpressionMember(Name));
                    return Route;
                }

                c = Code[Index];

                // Generic
                if (c == '<')
                {
                    if (!TryParseGenericTypes(Code, ref Index, Length, ref Builder, out GenericTypes))
                        break;

                    // Skips all trailing spaces.
                    if (!ReaderHelper.MoveTo(Code, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
                    {
                        Route.Contexts.Add(new ExpressionMember(Name, GenericTypes));
                        return Route;
                    }

                    c = Code[Index];
                }

                // Method
                if (c == '(')
                {
                    if (!TryParseCollectionContents(Code, ref Index, Length, ref Builder, out List<IExpressionObject> Parameters))
                        break;

                    ExpressionMethod Method = new ExpressionMethod(Name);
                    if (GenericTypes != null)
                    {
                        Method.GenericTypes.AddRange(GenericTypes);
                        GenericTypes = null;
                    }

                    Method.Parameters.AddRange(Parameters);
                    Route.Contexts.Add(Method);

                    // Skips all trailing spaces.
                    if (!ReaderHelper.MoveTo(Code, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
                        return Route;

                    c = Code[Index];

                    // Indexer
                    if (c == '[')
                    {
                        if (!TryParseCollectionContents(Code, ref Index, Length, ref Builder, out Parameters) ||
                            Parameters.Count == 0)
                            return Route;

                        ExpressionIndexer Indexer = new ExpressionIndexer(Parameters);
                        Route.Contexts.Add(Indexer);

                        // Skips all trailing spaces.
                        if (!ReaderHelper.MoveTo(Code, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
                            return Route;

                        c = Code[Index];
                    }

                    if (c != '.')
                        return Route;

                    // Skip '.'
                    Index++;
                    continue;
                }

                // Member
                ExpressionMember Member = new(Name);
                if (GenericTypes != null)
                {
                    Member.GenericTypes.AddRange(GenericTypes);
                    GenericTypes = null;
                }

                Route.Contexts.Add(Member);

                // Indexer
                if (c == '[')
                {
                    if (!TryParseCollectionContents(Code, ref Index, Length, ref Builder, out List<IExpressionObject> Parameters) ||
                        Parameters.Count == 0)
                        return Route;

                    ExpressionIndexer Indexer = new ExpressionIndexer(Parameters);
                    Route.Contexts.Add(Indexer);

                    // Skips all trailing spaces.
                    if (!ReaderHelper.MoveTo(Code, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
                        return Route;

                    c = Code[Index];
                }

                if (c != '.')
                    break;

                // Skip '.'
                Index++;
            }

            if (Route.Contexts.Count > 0)
                return Route;

            if (Index < Length)
                throw new IndexOutOfRangeException();

            try
            {
                c = Code[Index];
                Builder.Append(c);
                ReaderHelper.ReadTo(Code, ref Index, Length, false, out _, ref Builder, c => !c.IsVariableChars());
                return new ExpressionObject(Builder.ToString());
            }
            finally
            {
                Builder.Clear();
            }
        }

        /// <summary>
        /// Parses the contents of collection.<para/>
        /// A return value indicates whether the parsing succeeded.
        /// </summary>
        public static bool TryParseCollectionContents(string Code, ref int Index, int Length, ref StringBuilder Builder, out List<IExpressionObject> Contents)
        {
            int i = Index;
            char[] EndChar;

            // Checks EncChar and Skips StartChar.
            switch (Code[i])
            {
                case '(':
                    {
                        i++;
                        EndChar = [')'];
                        break;
                    }
                case '[':
                    {
                        i++;
                        EndChar = [']'];
                        break;
                    }
                case '{':
                    {
                        i++;
                        EndChar = ['}'];
                        break;
                    }
                default:
                    EndChar = [')', ']', '}'];
                    break;
            }

            Contents = [];

            char c;
            do
            {
                List<IExpressionObject> Contexts = [];
                while (TryParseElement(Code, ref i, Length, ref Builder, out IExpressionObject Context))
                    Contexts.Add(Context);

                if (i >= Length)
                {
                    Contents = null;
                    return false;
                }

                c = Code[i++];
                switch (Contexts.Count)
                {
                    case 0:
                        {
                            if (EndChar.Contains(c))
                            {
                                Index = i;
                                return true;
                            }

                            Contents = null;
                            return false;
                        }
                    case 1:
                        {
                            Contents.Add(Contexts[0]);
                            break;
                        }
                    default:
                        {
                            ExpressionBlock Block = new();
                            Block.Contexts.AddRange(Contexts);
                            Contents.Add(Block);
                            break;
                        }
                }

                if (c == ',')
                    continue;

                else if (EndChar.Contains(c))
                {
                    Index = i;
                    return true;
                }

                Contents = null;
                return false;

            } while (true);
        }

        /// <summary>
        /// Parses generic type names.<para/>
        /// A return value indicates whether the parsing succeeded.
        /// </summary>
        public static bool TryParseGenericTypes(string Code, ref int Index, int Length, ref StringBuilder Builder, out List<ExpressionTypeInfo> Types)
        {
            int i = Index;

            // Skip '<'
            if (Code[i] == '<')
                i++;

            string Namespace = null;
            Types = [];
            while (TryParseName(Code, ref i, Length, ref Builder, out bool IsEnd, out string Name))
            {
                if (IsEnd ||
                    !ReaderHelper.MoveTo(Code, ref i, Length, false, c => !char.IsWhiteSpace(c)))   // Skips all trailing spaces.
                {
                    Types = null;
                    return false;
                }

                switch (Code[i++])
                {
                    case '.':
                        {
                            Namespace = string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
                            continue;
                        }
                    case '<':
                        {
                            ExpressionTypeInfo Type = new(Name, Namespace);
                            Namespace = null;

                            if (!TryParseGenericTypes(Code, ref i, Length, ref Builder, out List<ExpressionTypeInfo> SubTypes))
                            {
                                Types = null;
                                return false;
                            }

                            Type.GenericTypes.AddRange(SubTypes);
                            Types.Add(Type);

                            // Skips all trailing spaces.
                            if (!ReaderHelper.MoveTo(Code, ref i, Length, false, c => !char.IsWhiteSpace(c)))
                                return false;

                            switch (Code[i++])
                            {
                                case '>':
                                    {
                                        Index = i;
                                        return true;
                                    }
                                case ',':
                                    continue;
                                default:
                                    return false;
                            }
                        }
                    case ',':
                        {
                            Types.Add(new ExpressionTypeInfo(Name, Namespace));
                            Namespace = null;
                            continue;
                        }
                    case '>':
                        {
                            Types.Add(new ExpressionTypeInfo(Name, Namespace));

                            Index = i;
                            return true;
                        }
                    default:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Parses name.<para/>
        /// A return value indicates whether the parsing succeeded.
        /// </summary>
        public static bool TryParseName(string Code, ref int Index, int Length, ref StringBuilder Builder, out bool IsEnd, out string Name)
        {
            int i = Index;

            // Skips all leading spaces.
            if (!ReaderHelper.MoveTo(Code, ref i, Length, false, c => !char.IsWhiteSpace(c)))
            {
                Name = null;
                IsEnd = true;
                return false;
            }

            // Checks number and symbol.
            char c = Code[i++];
            if (!c.IsVariableChars() || c.IsArabicNumerals())
            {
                Name = null;
                IsEnd = false;
                return false;
            }

            try
            {
                Builder.Append(c);
                ReaderHelper.ReadTo(Code, ref i, Length, false, out IsEnd, ref Builder, c => !c.IsVariableChars());

                Index = i;
                Name = Builder.ToString();
                return true;
            }
            finally
            {
                Builder.Clear();
            }
        }

        private static readonly MethodInfo ConvertMethod = typeof(Convert).GetMethod("ChangeType", [typeof(object), typeof(Type)]);
        public static Expression Cast(this Expression This, Type Type)
        {
            Type ObjType = This.Type;
            if (ObjType == Type)
                return This;

            if (Type == typeof(object))
                return Expression.Convert(This, Type);

            if (ObjType.IsBaseOn(Type))
                return This;

            if (ObjType.IsConvertibleTo(Type))
                return Expression.Convert(This, Type);

            This = Expression.Call(ConvertMethod, This, Expression.Constant(Type));
            return Expression.Convert(This, Type);
        }

        public static bool IsVariableChars(this char This)
            => char.IsLetterOrDigit(This) || This == '_' || This == '@';

        public static bool IsIdentifierChars(this char This)
            => This is '+' or '-' or '*' or '/' or '%' or '^' or
               '|' or '&' or '＆' or '~' or '!' or '<' or '＜' or '=' or '>' or '＞' or
               '(' or ')' or '[' or ']' or '{' or '}' or ',' or
               '?' or ':' or
               ';' or
               '"';

    }
}