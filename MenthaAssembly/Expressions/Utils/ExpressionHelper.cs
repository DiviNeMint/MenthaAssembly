using MenthaAssembly;
using MenthaAssembly.Expressions;
using System.Collections.Concurrent;
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
            StringBuilder Builder = new StringBuilder();
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

                        ExpressionBlock Block = new ExpressionBlock();
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
                #region Math Identifier
                case '+':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Add);
                        return true;
                    }
                case '-':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Subtract);
                        return true;
                    }
                case '*':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Multiply);
                        return true;
                    }
                case '/':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Divide);
                        return true;
                    }
                case '%':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Modulo);
                        return true;
                    }
                case '^':
                    {
                        Index++;
                        Element = new ExpressionIdentifier(ExpressionType.Power);
                        return true;
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
            ExpressionRoute Route = new ExpressionRoute();
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
                ExpressionMember Member = new ExpressionMember(Name);
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

            if (Index >= Length)
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
                        EndChar = new[] { ')' };
                        break;
                    }
                case '[':
                    {
                        i++;
                        EndChar = new[] { ']' };
                        break;
                    }
                case '{':
                    {
                        i++;
                        EndChar = new[] { '}' };
                        break;
                    }
                default:
                    EndChar = new[] { ')', ']', '}' };
                    break;
            }

            Contents = new List<IExpressionObject>();
            List<IExpressionObject> Contexts = new List<IExpressionObject>();

            char c;
            do
            {
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
                            ExpressionBlock Block = new ExpressionBlock();
                            Block.Contexts.AddRange(Contexts);
                            Contents.Add(Block);
                            Contexts.Clear();
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
            Types = new List<ExpressionTypeInfo>();
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
                            ExpressionTypeInfo Type = new ExpressionTypeInfo(Name, Namespace);
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

        private static readonly MethodInfo ConvertMethod = typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) });
        public static Expression Cast(this Expression This, Type Type)
        {
            Type ObjType = This.Type;
            if (ObjType == Type || ObjType.IsBaseOn(Type))
                return This;

            if (ObjType.IsConvertibleTo(Type))
                return Expression.Convert(This, Type);

            This = Expression.Call(ConvertMethod, This, Expression.Constant(Type));
            return Expression.Convert(This, Type);
        }

        public static bool IsVariableChars(this char This)
            => char.IsLetterOrDigit(This) || This == '_' || This == '@';

        public static bool IsIdentifierChars(this char This)
            => This == '+' || This == '-' || This == '*' || This == '/' || This == '%' || This == '^' ||
               This == '|' || This == '&' || This == '~' || This == '!' || This == '<' || This == '=' || This == '>' ||
               This == '(' || This == ')' || This == '[' || This == ']' || This == '{' || This == '}' || This == ',' ||
               This == '?' || This == ':' ||
               This == ';' ||
               This == '"';

    }

    public static class ExpressionHelper<T>
    {
        private static readonly Type t = typeof(T);

        private static Func<T, T> _Neg;
        public static Func<T, T> CreateNeg()
        {
            if (_Neg is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");

                try
                {
                    _Neg = Expression.Lambda<Func<T, T>>(Expression.Negate(Arg1), Arg1)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Neg = T => throw Ex;
                }
            }

            return _Neg;
        }

        private static Func<T, T, T> _Add, _Sub, _Mul, _Div;
        public static Func<T, T, T> CreateAdd()
        {
            if (_Add is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");
                try
                {
                    _Add = Expression.Lambda<Func<T, T, T>>(Expression.Add(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Add = (a, b) => throw Ex;
                }
            }

            return _Add;
        }
        public static Func<T, T, T> CreateSub()
        {
            if (_Sub is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Sub = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Sub = (a, b) => throw Ex;
                }
            }

            return _Sub;
        }
        public static Func<T, T, T> CreateMul()
        {
            if (_Mul is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Mul = Expression.Lambda<Func<T, T, T>>(Expression.Multiply(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Mul = (a, b) => throw Ex;
                }
            }

            return _Mul;
        }
        public static Func<T, T, T> CreateDiv()
        {
            if (_Div is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Div = Expression.Lambda<Func<T, T, T>>(Expression.Divide(Arg1, Arg2), Arg1, Arg2)
                                     .Compile();
                }
                catch (Exception Ex)
                {
                    _Div = (a, b) => throw Ex;
                }
            }

            return _Div;
        }

        public static readonly ConcurrentDictionary<object, Func<T, T>> _Muls = new(),
                                                                        _Divs = new();
        public static Func<T, T> CreateMul(object Constant)
        {
            if (_Muls.TryGetValue(Constant, out Func<T, T> MulFunc))
                return MulFunc;

            try
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                if (typeof(int).Equals(t))
                {
                    if (Constant is not int IntConst)
                        IntConst = Convert.ToInt32(Constant);

                    if (IntConst > 1 &&
                        (IntConst & (IntConst - 1)) == 0)
                    {
                        int Shift = 0;
                        while (IntConst > 1)
                        {
                            IntConst >>= 1;
                            Shift++;
                        }

                        MulFunc = Expression.Lambda<Func<T, T>>(Expression.LeftShift(Arg1, Expression.Constant(Shift)), Arg1)
                                            .Compile();
                    }
                    else
                    {
                        MulFunc = Expression.Lambda<Func<T, T>>(Expression.Multiply(Arg1, Expression.Constant(IntConst)), Arg1)
                                            .Compile();
                    }
                }
                else
                {
                    if (Constant is not T TConst)
                        TConst = (T)Convert.ChangeType(Constant, t);

                    MulFunc = Expression.Lambda<Func<T, T>>(Expression.Multiply(Arg1, Expression.Constant(TConst)), Arg1)
                                        .Compile();
                }

            }
            catch (Exception Ex)
            {
                MulFunc = a => throw Ex;
            }

            _Muls.AddOrUpdate(Constant, MulFunc, (k, v) => MulFunc);
            return MulFunc;
        }
        public static Func<T, T> CreateDiv(object Constant)
        {
            if (_Divs.TryGetValue(Constant, out Func<T, T> DivFunc))
                return DivFunc;

            try
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                if (typeof(int).Equals(t))
                {
                    if (Constant is not int IntConst)
                        IntConst = Convert.ToInt32(Constant);

                    if (IntConst > 1 &&
                        (IntConst & (IntConst - 1)) == 0)
                    {
                        int Shift = 0;
                        while (IntConst > 1)
                        {
                            IntConst >>= 1;
                            Shift++;
                        }

                        DivFunc = Expression.Lambda<Func<T, T>>(Expression.RightShift(Arg1, Expression.Constant(Shift)), Arg1)
                                            .Compile();
                    }
                    else
                    {
                        DivFunc = Expression.Lambda<Func<T, T>>(Expression.Divide(Arg1, Expression.Constant(IntConst)), Arg1)
                                            .Compile();
                    }
                }
                else
                {
                    if (Constant is not T TConst)
                        TConst = (T)Convert.ChangeType(Constant, t);

                    DivFunc = Expression.Lambda<Func<T, T>>(Expression.Divide(Arg1, Expression.Constant(TConst)), Arg1)
                                        .Compile();
                }

            }
            catch (Exception Ex)
            {
                DivFunc = a => throw Ex;
            }

            _Divs.AddOrUpdate(Constant, DivFunc, (k, v) => DivFunc);
            return DivFunc;
        }

        private static Func<T, T> _Abs;
        public static Func<T, T> CreateAbs()
        {
            if (_Abs is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");

                try
                {
                    if (t.Equals(typeof(int)) &&
                        typeof(MathHelper).TryGetStaticMethod(nameof(MathHelper.Abs), out MethodInfo IntAbsMethod))
                    {
                        _Abs = Expression.Lambda<Func<T, T>>(Expression.Call(IntAbsMethod, Arg1), Arg1)
                                         .Compile();
                    }
                    //else if (typeof(Math).TryGetStaticMethod(nameof(Math.Abs), new[] { t }, out MethodInfo CommonAbsMethod))
                    //{
                    //    _Abs = Expression.Lambda<Func<T, T>>(Expression.Call(CommonAbsMethod, Arg1), Arg1)
                    //                     .Compile();
                    //}
                    else
                    {
                        LabelTarget Label = Expression.Label(t);
                        ConstantExpression Arg2 = Expression.Constant(default(T));

                        BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2),
                                                                Expression.Return(Label, Arg1, typeof(T)),
                                                                Expression.Return(Label, Expression.Negate(Arg1), typeof(T))),
                                                                Expression.Label(Label, Expression.New(t)));

                        _Abs = Expression.Lambda<Func<T, T>>(Body, Arg1)
                                        .Compile();
                    }
                }
                catch (Exception Ex)
                {
                    _Abs = T => throw Ex;
                }
            }

            return _Abs;
        }

        private static Func<T, T, bool> _Equal, _GreaterThan, _LessThan, _GreaterThanOrEqual, _LessThanOrEqual;
        public static Func<T, T, bool> CreateEqual()
        {
            if (_Equal is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _Equal = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(Arg1, Arg2), Arg1, Arg2)
                                       .Compile();
                }
                catch (Exception Ex)
                {
                    _Equal = (a, b) => throw Ex;
                }
            }

            return _Equal;
        }
        public static Func<T, T, bool> CreateGreaterThan()
        {
            if (_GreaterThan is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _GreaterThan = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThan(Arg1, Arg2), Arg1, Arg2)
                                             .Compile();
                }
                catch (Exception Ex)
                {
                    _GreaterThan = (a, b) => throw Ex;
                }
            }

            return _GreaterThan;
        }
        public static Func<T, T, bool> CreateLessThan()
        {
            if (_LessThan is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _LessThan = Expression.Lambda<Func<T, T, bool>>(Expression.LessThan(Arg1, Arg2), Arg1, Arg2)
                                          .Compile();
                }
                catch (Exception Ex)
                {
                    _LessThan = (a, b) => throw Ex;
                }
            }

            return _LessThan;
        }
        public static Func<T, T, bool> CreateGreaterThanOrEqual()
        {
            if (_GreaterThanOrEqual is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _GreaterThanOrEqual = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThanOrEqual(Arg1, Arg2), Arg1, Arg2)
                                                    .Compile();
                }
                catch (Exception Ex)
                {
                    _GreaterThanOrEqual = (a, b) => throw Ex;
                }
            }

            return _GreaterThanOrEqual;
        }
        public static Func<T, T, bool> CreateLessThanOrEqual()
        {
            if (_LessThanOrEqual is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                    Arg2 = Expression.Parameter(t, "b");

                try
                {
                    _LessThanOrEqual = Expression.Lambda<Func<T, T, bool>>(Expression.LessThanOrEqual(Arg1, Arg2), Arg1, Arg2)
                                                 .Compile();
                }
                catch (Exception Ex)
                {
                    _LessThanOrEqual = (a, b) => throw Ex;
                }
            }

            return _LessThanOrEqual;
        }

        private static Predicate<T> _IsDefault;
        public static Predicate<T> CreateIsDefault()
        {
            if (_IsDefault is null)
            {
                ParameterExpression Arg1 = Expression.Parameter(t, "a");
                ConstantExpression Arg2 = Expression.Constant(default(T), t);

                try
                {
                    _IsDefault = Expression.Lambda<Predicate<T>>(Expression.Equal(Arg1, Arg2), Arg1)
                                           .Compile();
                }
                catch (Exception Ex)
                {
                    _IsDefault = a => throw Ex;
                }
            }

            return _IsDefault;
        }

        public static readonly ConcurrentDictionary<Type, Delegate> _Casts = new();
        public static Func<T, T2> CreateCast<T2>()
        {
            Type t2 = typeof(T2);
            if (_Casts.TryGetValue(t2, out Delegate CacheFunc))
                return (Func<T, T2>)CacheFunc;

            LabelTarget Label = Expression.Label(t2);
            ParameterExpression Arg1 = Expression.Parameter(t, "a");

            Func<T, T2> CastFunc;
            try
            {
                if (t.Equals(t2))
                {
                    CastFunc = Expression.Lambda<Func<T, T2>>(Expression.Block(Expression.Return(Label, Arg1, t2),
                                                                               Expression.Label(Label, Expression.New(t2))), Arg1)
                                         .Compile();
                }
                else if (t.IsDecimalType() && t2.IsIntegerType() &&
                         typeof(Math).TryGetStaticMethod(nameof(Math.Round), new[] { typeof(double) }, out MethodInfo Method))
                {
                    Type Dt = typeof(double);
                    MethodCallExpression RoundArg = Expression.Call(Method, t.Equals(Dt) ? Arg1 : Expression.Convert(Arg1, Dt));

                    CastFunc = Expression.Lambda<Func<T, T2>>(Expression.Convert(RoundArg, t2), Arg1)
                                         .Compile();
                }
                else
                {
                    CastFunc = Expression.Lambda<Func<T, T2>>(Expression.Convert(Arg1, t2), Arg1)
                                         .Compile();
                }
            }
            catch (Exception Ex)
            {
                CastFunc = a => throw Ex;
            }

            _Casts.AddOrUpdate(t2, CastFunc, (k, v) => CastFunc);
            return CastFunc;
        }

        public static Func<T, T, T> CreateMin()
        {
            LabelTarget Label = Expression.Label(t);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.LessThan(Arg1, Arg2), Expression.Return(Label, Arg1, typeof(T)), Expression.Return(Label, Arg2, typeof(T))),
                                                    Expression.Label(Label, Expression.New(t)));

            return Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                             .Compile();
        }
        public static Func<T, T, T> CreateMax()
        {
            LabelTarget Label = Expression.Label(t);
            ParameterExpression Arg1 = Expression.Parameter(t, "a"),
                                Arg2 = Expression.Parameter(t, "b");

            BlockExpression Body = Expression.Block(Expression.IfThenElse(Expression.GreaterThan(Arg1, Arg2), Expression.Return(Label, Arg1, typeof(T)), Expression.Return(Label, Arg2, typeof(T))),
                                                    Expression.Label(Label, Expression.New(t)));

            return Expression.Lambda<Func<T, T, T>>(Body, Arg1, Arg2)
                             .Compile();
        }

    }

}