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
                ConstantExpression NullArg = Expression.Constant(null);
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
                ConstantExpression NullArg = Expression.Constant(null);
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

        public static bool TryParseExpression(string Formula, out ExpressionBlock Block)
        {
            StringBuilder Builder = new StringBuilder();
            int Index = 0,
                Length = Formula.Length;

            Block = new ExpressionBlock();
            while (TryParseExpressionElement(Formula, ref Index, Length, ref Builder, out IExpressionObject Element))
                Block.Contexts.Add(Element);

            if (Block.Contexts.Count == 0 || Index < Length)
            {
                Block = null;
                return false;
            }

            return true;
        }
        private static bool TryParseExpressionElement(string Formula, ref int Index, int Length, ref StringBuilder Builder, out IExpressionObject Element)
        {
            if (!ReaderHelper.MoveTo(Formula, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
            {
                Element = null;
                return false;
            }

            char c = Formula[Index];
            switch (c)
            {
                #region Block
                case '(':
                    {
                        Index++;

                        ExpressionBlock Block = new ExpressionBlock();
                        while (TryParseExpressionElement(Formula, ref Index, Length, ref Builder, out IExpressionObject Child))
                            Block.Contexts.Add(Child);

                        if (Index < Length)
                        {
                            c = Formula[Index];
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
                #region Number Constant
                case '.':
                    {
                        Index++;

                        try
                        {
                            Builder.Append(c);
                            Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out bool IsEnd, c => !c.IsArabicNumerals()));

                            if (IsEnd)
                            {
                                double DoubleConst = Builder.ToString().ToDoubleFast();
                                Element = new ExpressionConst(DoubleConst);
                                return true;
                            }

                            c = Formula[Index];
                            if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                            {
                                double DoubleConst = Builder.ToString().ToDoubleFast();
                                Element = new ExpressionConst(DoubleConst);
                                return true;
                            }

                            Index++;

                            string Remained = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out _, c => !c.IsVariableChars());
                            if (Remained.Length > 0)
                            {
                                Builder.Append(c);
                                Builder.Append(Remained);
                                Element = new ExpressionObject(Builder.ToString());
                                return true;
                            }

                            // Sepeified Types
                            switch (c)
                            {
                                case 'f':
                                case 'F':
                                    {
                                        float FloatConst = Builder.ToString().ToFloatFast();
                                        Element = new ExpressionConst(FloatConst);
                                        return true;
                                    }
                                case 'd':
                                case 'D':
                                    {
                                        double DoubleConst = Builder.ToString().ToDoubleFast();
                                        Element = new ExpressionConst(DoubleConst);
                                        return true;
                                    }
                                case 'm':
                                case 'M':
                                    {
                                        decimal DecimalConst = decimal.Parse(Builder.ToString());
                                        Element = new ExpressionConst(DecimalConst);
                                        return true;
                                    }
                            }

                            // Unknown
                            Builder.Append(c);
                            Builder.Append(Remained);
                            Element = new ExpressionObject(Builder.ToString());
                            return true;
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }
                case char n when n.IsArabicNumerals():
                    {
                        Index++;

                        try
                        {
                            Builder.Append(c);
                            Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out bool IsEnd, c => !c.IsArabicNumerals()));

                            // Integer
                            if (IsEnd)
                            {
                                int IntConst = Builder.ToString().ToInt32Fast();
                                Element = new ExpressionConst(IntConst);
                                return true;
                            }

                            c = Formula[Index];
                            if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                            {
                                int IntConst = Builder.ToString().ToInt32Fast();
                                Element = new ExpressionConst(IntConst);
                                return true;
                            }

                            Index++;

                            // Decimal
                            bool IsIntegerOrAllowSpecifiedType = true;
                            if (c == '.')
                            {
                                Builder.Append(c);
                                Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsArabicNumerals()));

                                if (IsEnd)
                                {
                                    double DoubleConst = Builder.ToString().ToDoubleFast();
                                    Element = new ExpressionConst(DoubleConst);
                                    return true;
                                }

                                c = Formula[Index];
                                if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                                {
                                    double DoubleConst = Builder.ToString().ToDoubleFast();
                                    Element = new ExpressionConst(DoubleConst);
                                    return true;
                                }

                                Index++;
                                IsIntegerOrAllowSpecifiedType = false;
                            }

                            // Specified Integer
                            else if (Builder.ToString() == "0")
                            {
                                Builder.Append(c);

                                // Hex
                                if (c == 'x' || c == 'X')
                                {
                                    // Format
                                    string Format = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsHexNumerals());
                                    if (Format.Length == 0)
                                    {
                                        Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars()));
                                        Element = new ExpressionObject(Builder.ToString());
                                        return true;
                                    }

                                    if (IsEnd)
                                    {
                                        int IntConst = int.Parse(Format, NumberStyles.HexNumber);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    c = Formula[Index];
                                    if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                                    {
                                        int IntConst = int.Parse(Format, NumberStyles.HexNumber);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    Index++;
                                    Builder.Append(Format);
                                    IsIntegerOrAllowSpecifiedType = false;
                                }

                                // Binary
                                else if (c == 'b' || c == 'B')
                                {
                                    // Format
                                    string Format = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsArabicNumerals());
                                    if (Format.Length == 0)
                                    {
                                        Builder.Append(ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars()));
                                        Element = new ExpressionObject(Builder.ToString());
                                        return true;
                                    }

                                    if (IsEnd)
                                    {
                                        int IntConst = Convert.ToInt32(Format, 2);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    c = Formula[Index];
                                    if (char.IsWhiteSpace(c) || c.IsIdentifierChars())
                                    {
                                        int IntConst = Convert.ToInt32(Format, 2);
                                        Element = new ExpressionConst(IntConst);
                                        return true;
                                    }

                                    Index++;
                                    Builder.Append(Format);
                                    IsIntegerOrAllowSpecifiedType = false;
                                }
                            }

                            string Remained = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out _, c => !c.IsVariableChars());
                            if (Remained.Length > 0)
                            {
                                Builder.Append(c);
                                Builder.Append(Remained);
                                Element = new ExpressionObject(Builder.ToString());
                                return true;
                            }

                            // Sepeified Types
                            switch (c)
                            {
                                case 'f':
                                case 'F':
                                    {
                                        float FloatConst = Builder.ToString().ToFloatFast();
                                        Element = new ExpressionConst(FloatConst);
                                        return true;
                                    }
                                case 'd':
                                case 'D':
                                    {
                                        double DoubleConst = Builder.ToString().ToDoubleFast();
                                        Element = new ExpressionConst(DoubleConst);
                                        return true;
                                    }
                                case 'm':
                                case 'M':
                                    {
                                        decimal DecimalConst = decimal.Parse(Builder.ToString());
                                        Element = new ExpressionConst(DecimalConst);
                                        return true;
                                    }
                                case 'l' when IsIntegerOrAllowSpecifiedType:
                                case 'L' when IsIntegerOrAllowSpecifiedType:
                                    {
                                        long Int64Const = long.Parse(Builder.ToString());
                                        Element = new ExpressionConst(Int64Const);
                                        return true;
                                    }
                                case 'u' when IsIntegerOrAllowSpecifiedType:
                                case 'U' when IsIntegerOrAllowSpecifiedType:
                                    {
                                        uint UIntConst = Builder.ToString().ToUInt32Fast();
                                        Element = new ExpressionConst(UIntConst);
                                        return true;
                                    }
                            }

                            // Unknown
                            Builder.Append(c);
                            Builder.Append(Remained);
                            Element = new ExpressionObject(Builder.ToString());
                            return true;
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }
                #endregion
                default:
                    {
                        try
                        {
                            Builder.Append(c);
                            ExpressionElement Paths = new ExpressionElement();

                            do
                            {
                                // Skip '.' or First appended char.
                                Index++;

                                string Variable = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out bool IsEnd, c => !c.IsVariableChars());
                                if (IsEnd)
                                {
                                    Builder.Append(Variable);
                                    Paths.Contexts.Add(new ExpressionMember(Builder.ToString()));
                                    break;
                                }

                                c = Formula[Index];

                                // Check Space
                                if (char.IsWhiteSpace(c))
                                {
                                    if (!ReaderHelper.MoveTo(Formula, ref Index, Length, false, c => !char.IsWhiteSpace(c)))
                                        break;

                                    // Tail Space
                                    if (string.IsNullOrEmpty(Variable))
                                    {
                                        Variable = ReaderHelper.ReadTo(Formula, ref Index, Length, false, out IsEnd, c => !c.IsVariableChars());
                                        ReaderHelper.MoveTo(Formula, ref Index, Length, false, c => !char.IsWhiteSpace(c));
                                    }

                                    c = Formula[Index];
                                }

                                Builder.Append(Variable);

                                // Method
                                if (c == '(')
                                {
                                    ExpressionMethod Method = new ExpressionMethod(Builder.ToString());
                                    if (string.IsNullOrEmpty(Method.Name))
                                        break;

                                    List<IExpressionObject> ArgContext = new List<IExpressionObject>();

                                    Builder.Clear();
                                    do
                                    {
                                        Index++;    // Skip ',' 、 '('

                                        while (TryParseExpressionElement(Formula, ref Index, Length, ref Builder, out IExpressionObject Arg))
                                            ArgContext.Add(Arg);

                                        if (ArgContext.Count == 0)
                                            break;

                                        Method.Parameters.Add(ArgContext.Count == 1 ? ArgContext[0] : new ExpressionBlock(ArgContext));
                                        if (Index >= Length)
                                        {
                                            Paths.Contexts.Add(Method);
                                            Element = Paths;
                                            return true;
                                        }

                                        c = Formula[Index];
                                        ArgContext.Clear();

                                    } while (c != ')');

                                    Index++;    // Skip ')'

                                    Paths.Contexts.Add(Method);
                                    c = Formula[Index];
                                }

                                // Member
                                else
                                {
                                    ExpressionMember Member = new ExpressionMember(Builder.ToString());
                                    if (string.IsNullOrEmpty(Member.Name))
                                        break;

                                    Paths.Contexts.Add(Member);
                                    Builder.Clear();
                                }

                            } while (c == '.');

                            Element = Paths;
                            return true;
                        }
                        finally
                        {
                            Builder.Clear();
                        }
                    }
            }
        }

        public static Expression CastExpression(this Expression This, Type Type)
            => This.Type == Type ? This : Expression.Convert(This, Type);

        public static bool IsVariableChars(this char This)
            => char.IsLetterOrDigit(This) || This == '_' || This == '@' || This == '#' || This == '$';

        public static bool IsIdentifierChars(this char This)
            => This == '+' || This == '-' || This == '*' || This == '/' || This == '%' || This == '^' ||
               This == '|' || This == '&' || This == '~' || This == '!' || This == '<' || This == '=' || This == '>' ||
               This == '(' || This == ')' || This == ',';

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

        public static Func<T, object> CreateExtractor(string Path)
        {
            Type ReturnType = typeof(object),
                 ParentType = typeof(T);
            StringBuilder Builder = new StringBuilder(128);

            try
            {
                int Length = Path.Length;
                char c;
                ConstantExpression NullArg = Expression.Constant(null);
                ParameterExpression Arg = Expression.Parameter(ParentType, "a");
                Expression Body = Arg;
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

                return Expression.Lambda<Func<T, object>>(Expression.Convert(Body, ReturnType), Arg)
                                 .Compile();
            }
            finally
            {
                Builder.Clear();
            }
        }
        public static Func<T, T2> CreateExtractor<T2>(string Path)
        {
            Type ReturnType = typeof(object),
                 ParentType = typeof(T);
            StringBuilder Builder = new StringBuilder(128);

            try
            {
                int Length = Path.Length;
                char c;
                ConstantExpression NullArg = Expression.Constant(null);
                ParameterExpression Arg = Expression.Parameter(ParentType, "a");
                Expression Body = Arg;
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

                return Expression.Lambda<Func<T, T2>>(ParentType.Equals(ReturnType) ? Body : Expression.Convert(Body, ReturnType), Arg)
                                 .Compile();
            }
            finally
            {
                Builder.Clear();
            }
        }

    }
}