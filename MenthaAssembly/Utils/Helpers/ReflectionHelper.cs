using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
    public static class ReflectionHelper
    {
        private static readonly BindingFlags InternalFlags = BindingFlags.Instance | BindingFlags.NonPublic,
                                             StaticInternalFlags = InternalFlags | BindingFlags.Static,
                                             PublicFlags = BindingFlags.Instance | BindingFlags.Public,
                                             StaticFlags = BindingFlags.Public | BindingFlags.Static;

        #region Property
        public static IEnumerable<PropertyInfo> GetProperties(this Type This, params string[] Names)
        {
            foreach (string Name in Names)
                if (This.GetProperty(Name) is PropertyInfo Info)
                    yield return Info;
        }

        public static bool TryGetProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), PublicFlags, Name, out Info);
        public static bool TryGetProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, PublicFlags, Name, out Info);

        public static bool TryGetStaticProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), StaticFlags, Name, out Info);
        public static bool TryGetStaticProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, StaticFlags, Name, out Info);

        public static bool TryGetInternalProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), InternalFlags, Name, out Info);
        public static bool TryGetInternalProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, InternalFlags, Name, out Info);

        public static bool TryGetStaticInternalProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), StaticInternalFlags, Name, out Info);
        public static bool TryGetStaticInternalProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, StaticInternalFlags, Name, out Info);

        public static bool TryGetProperty<T>(BindingFlags Flags, string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), Flags, Name, out Info);
        public static bool TryGetProperty(this Type This, BindingFlags Flags, string Name, out PropertyInfo Info)
        {
            Type BaseType = This;
            PropertyInfo Result = BaseType?.GetProperty(Name, Flags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Info = null;
                    return false;
                }

                Result = BaseType.GetProperty(Name, Flags);
            }
            Info = Result;
            return true;
        }

        public static bool TrySetPropertyValue<T>(this object This, string Name, T Value)
            => TrySetPropertyValue(This, PublicFlags, Name, Value);
        public static bool TryGetPropertyValue<T>(this object This, string Name, out T Value)
            => TryGetPropertyValue(This, PublicFlags, Name, out Value);

        public static bool TrySetStaticPropertyValue<T>(this object This, string Name, T Value)
            => TrySetPropertyValue(This, StaticFlags, Name, Value);
        public static bool TryGetStaticPropertyValue<T>(this object This, string Name, out T Value)
            => TryGetPropertyValue(This, StaticFlags, Name, out Value);

        public static bool TrySetInternalPropertyValue<T>(this object This, string Name, T Value)
            => TrySetPropertyValue(This, InternalFlags, Name, Value);
        public static bool TryGetInternalPropertyValue<T>(this object This, string Name, out T Value)
            => TryGetPropertyValue(This, InternalFlags, Name, out Value);

        public static bool TrySetStaticInternalPropertyValue<T>(this object This, string Name, T Value)
            => TrySetPropertyValue(This, StaticInternalFlags, Name, Value);
        public static bool TryGetStaticInternalPropertyValue<T>(this object This, string Name, out T Value)
            => TryGetPropertyValue(This, StaticInternalFlags, Name, out Value);

        public static bool TrySetPropertyValue<T>(this object This, BindingFlags Flags, string Name, T Value)
        {
            if (TryGetProperty(This?.GetType(), Flags, Name, out PropertyInfo Info) &&
                typeof(T).IsBaseOn(Info.PropertyType))
            {
                Info.SetValue(This, Value);
                return true;
            }
            return false;
        }
        public static bool TryGetPropertyValue<T>(this object This, BindingFlags Flags, string Name, out T Value)
        {
            if (TryGetProperty(This?.GetType(), Flags, Name, out PropertyInfo Info) &&
                Info.PropertyType.IsBaseOn<T>())
            {
                Value = (T)Info.GetValue(This);
                return true;
            }

            Value = default;
            return false;
        }

        #endregion

        #region Field
        public static bool TryGetField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), PublicFlags, Name, out Info);
        public static bool TryGetField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, PublicFlags, Name, out Info);

        public static bool TryGetStaticField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), StaticFlags, Name, out Info);
        public static bool TryGetStaticField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, StaticFlags, Name, out Info);

        public static bool TryGetInternalField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), InternalFlags, Name, out Info);
        public static bool TryGetInternalField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, InternalFlags, Name, out Info);

        public static bool TryGetStaticInternalField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), StaticInternalFlags, Name, out Info);
        public static bool TryGetStaticInternalField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, StaticInternalFlags, Name, out Info);

        public static bool TryGetField<T>(BindingFlags Flags, string Name, out FieldInfo Info)
            => TryGetField(typeof(T), Flags, Name, out Info);
        public static bool TryGetField(this Type This, BindingFlags Flags, string Name, out FieldInfo Info)
        {
            Type BaseType = This;
            FieldInfo Result = BaseType?.GetField(Name, Flags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Info = null;
                    return false;
                }

                Result = BaseType.GetField(Name, Flags);
            }
            Info = Result;
            return true;
        }

        public static bool TrySetFieldValue<T>(this object This, string Name, T Value)
            => TrySetFieldValue(This, PublicFlags, Name, Value);
        public static bool TryGetFieldValue<T>(this object This, string Name, out T Value)
            => TryGetFieldValue(This, PublicFlags, Name, out Value);

        public static bool TrySetStaticFieldValue<T>(this object This, string Name, T Value)
            => TrySetFieldValue(This, StaticFlags, Name, Value);
        public static bool TryGetStaticFieldValue<T>(this object This, string Name, out T Value)
            => TryGetFieldValue(This, StaticFlags, Name, out Value);

        public static bool TrySetInternalFieldValue<T>(this object This, string Name, T Value)
            => TrySetFieldValue(This, InternalFlags, Name, Value);
        public static bool TryGetInternalFieldValue<T>(this object This, string Name, out T Value)
            => TryGetFieldValue(This, InternalFlags, Name, out Value);

        public static bool TrySetStaticInternalFieldValue<T>(this object This, string Name, T Value)
            => TrySetFieldValue(This, StaticInternalFlags, Name, Value);
        public static bool TryGetStaticInternalFieldValue<T>(this object This, string Name, out T Value)
            => TryGetFieldValue(This, StaticInternalFlags, Name, out Value);

        public static bool TrySetFieldValue<T>(this object This, BindingFlags Flags, string Name, T Value)
        {
            if (TryGetField(This?.GetType(), Flags, Name, out FieldInfo Info) &&
                typeof(T).IsBaseOn(Info.FieldType))
            {
                Info.SetValue(This, Value);
                return true;
            }
            return false;
        }
        public static bool TryGetFieldValue<T>(this object This, BindingFlags Flags, string Name, out T Value)
        {
            if (TryGetField(This?.GetType(), Flags, Name, out FieldInfo Info) &&
                Info.FieldType.IsBaseOn<T>())
            {
                Value = (T)Info.GetValue(This);
                return true;
            }

            Value = default;
            return false;
        }

        #endregion

        #region Constant
        public static bool TryGetConstant<T>(this Type This, string Name, out T Value)
        {
            if (TryGetField(This, StaticFlags, Name, out FieldInfo Field))
            {
                Value = (T)Field.GetValue(null);
                return true;
            }

            Value = default;
            return false;
        }

        #endregion

        #region Method
        public static bool TryGetMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), PublicFlags, Name, out Info);
        public static bool TryGetMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), PublicFlags, Name, ParameterTypes, out Info);
        public static bool TryGetMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), PublicFlags, Name, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, PublicFlags, Name, out Info);
        public static bool TryGetMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, PublicFlags, Name, ParameterTypes, out Info);
        public static bool TryGetMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, PublicFlags, Name, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetStaticMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), StaticFlags, Name, out Info);
        public static bool TryGetStaticMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), StaticFlags, Name, ParameterTypes, out Info);
        public static bool TryGetStaticMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), StaticFlags, Name, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetStaticMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, StaticFlags, Name, out Info);
        public static bool TryGetStaticMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, StaticFlags, Name, ParameterTypes, out Info);
        public static bool TryGetStaticMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, StaticFlags, Name, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetInternalMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), InternalFlags, Name, out Info);
        public static bool TryGetInternalMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), InternalFlags, Name, ParameterTypes, out Info);
        public static bool TryGetInternalMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), InternalFlags, Name, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetInternalMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, InternalFlags, Name, out Info);
        public static bool TryGetInternalMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, InternalFlags, Name, ParameterTypes, out Info);
        public static bool TryGetInternalMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, InternalFlags, Name, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetStaticInternalMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), StaticInternalFlags, Name, out Info);
        public static bool TryGetStaticInternalMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), StaticInternalFlags, Name, ParameterTypes, out Info);
        public static bool TryGetStaticInternalMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), StaticInternalFlags, Name, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetStaticInternalMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, StaticInternalFlags, Name, out Info);
        public static bool TryGetStaticInternalMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, StaticInternalFlags, Name, ParameterTypes, out Info);
        public static bool TryGetStaticInternalMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, StaticInternalFlags, Name, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetMethod<T>(BindingFlags Flags, string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), Flags, Name, out Info);
        public static bool TryGetMethod<T>(BindingFlags Flags, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Flags, Name, ParameterTypes, out Info);
        public static bool TryGetMethod<T>(BindingFlags Flags, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Flags, Name, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetMethod(this Type This, BindingFlags Flags, string Name, out MethodInfo Info)
        {
            Type BaseType = This;
            MethodInfo Result = BaseType?.GetMethod(Name, Flags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Info = null;
                    return false;
                }

                Result = BaseType.GetMethod(Name, Flags);
            }
            Info = Result;
            return true;
        }
        public static bool TryGetMethod(this Type This, BindingFlags Flags, string Name, Type[] ParameterTypes, out MethodInfo Info)
        {
            while (This != null)
            {
                if (This.GetMethod(Name, Flags, null, ParameterTypes, null) is MethodInfo Method)
                {
                    Info = Method;
                    return true;
                }

                This = This.BaseType;
            }

            Info = null;
            return false;
        }
        public static bool TryGetMethod(this Type This, BindingFlags Flags, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
        {
            int GenericLength = GenericTypes.Length,
                ParameterLength = ParameterTypes.Length;
            bool IsGeneric = GenericLength > 0;

            List<Tuple<MethodInfo, Type[]>> MinorMethods = new List<Tuple<MethodInfo, Type[]>>();
            while (This != null)
            {
                foreach (MethodInfo Method in This.GetMethods(Flags).Where(i => i.Name == Name))
                {
                    ParameterInfo[] Parameter = Method.GetParameters();
                    if (Parameter.Length == ParameterLength)
                    {
                        if (IsGeneric)
                        {
                            if (!Method.IsGenericMethodDefinition)
                                continue;

                            Type[] DefinedGenericTypes = Method.GetGenericArguments();
                            if (DefinedGenericTypes.Length != GenericLength)
                                continue;

                            Type[] DefinedParameterTypes = Parameter.Select(i => i.ParameterType).ToArray();

                            // Implement Generic Parameter Types.
                            Type DefinedType;
                            for (int i = 0; i < ParameterLength; i++)
                            {
                                DefinedType = DefinedParameterTypes[i];
                                if (DefinedType.IsGenericParameter)
                                {
                                    for (int j = 0; j < GenericLength; j++)
                                    {
                                        if (DefinedType == DefinedGenericTypes[j])
                                        {
                                            DefinedParameterTypes[i] = GenericTypes[j];
                                            break;
                                        }
                                    }
                                }
                            }

                            if (ParameterTypes.SequenceEqual(DefinedParameterTypes))
                            {
                                Info = Method.MakeGenericMethod(GenericTypes);
                                return true;
                            }
                        }
                        else if (ParameterTypes.SequenceEqual(Parameter.Select(i => i.ParameterType)))
                        {
                            Info = Method;
                            return true;
                        }
                    }
                }

                This = This.BaseType;
            }

            Info = null;
            return false;
        }

        public static bool TryGetMethodWithImplicitParameter(Type Base, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethodWithImplicitParameter(Base, PublicFlags | BindingFlags.Static, Name, GenericTypes, ParameterTypes, out Info, out _);
        public static bool TryGetMethodWithImplicitParameter(Type Base, BindingFlags Flags, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethodWithImplicitParameter(Base, Flags, Name, GenericTypes, ParameterTypes, out Info, out _);
        internal static bool TryGetMethodWithImplicitParameter(Type Base, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes)
            => TryGetMethodWithImplicitParameter(Base, PublicFlags | BindingFlags.Static, Name, GenericTypes, ParameterTypes, out Info, out DefinedParameterTypes);
        internal static bool TryGetMethodWithImplicitParameter(Type Base, BindingFlags Flags, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes)
        {
            int GenericLength = GenericTypes.Length,
                ParameterLength = ParameterTypes.Length;
            bool IsGeneric = GenericLength > 0;

            MethodInfo MinorMethod = null;
            Type[] MinorParameterTypes = null;
            int MinorScore = int.MaxValue;
            while (Base != null)
            {
                foreach (MethodInfo Method in Base.GetMethods(Flags).Where(i => i.Name == Name))
                {
                    ParameterInfo[] Parameter = Method.GetParameters();
                    if (Parameter.Length == ParameterLength)
                    {
                        if (IsGeneric)
                        {
                            if (!Method.IsGenericMethodDefinition)
                                continue;

                            Type[] DefinedGenericTypes = Method.GetGenericArguments();
                            if (DefinedGenericTypes.Length != GenericLength)
                                continue;

                            Type[] TempDefinedParameterTypes = Parameter.Select(i => i.ParameterType).ToArray();

                            // Implement Generic Parameter Types.
                            Type DefinedType;
                            for (int i = 0; i < ParameterLength; i++)
                            {
                                DefinedType = TempDefinedParameterTypes[i];
                                if (DefinedType.IsGenericParameter)
                                {
                                    for (int j = 0; j < GenericLength; j++)
                                    {
                                        if (DefinedType == DefinedGenericTypes[j])
                                        {
                                            TempDefinedParameterTypes[i] = GenericTypes[j];
                                            break;
                                        }
                                    }
                                }
                            }

                            int Score = MatchParameters(ParameterTypes, TempDefinedParameterTypes);
                            if (Score == 0)
                            {
                                Info = Method.MakeGenericMethod(GenericTypes);
                                DefinedParameterTypes = ParameterTypes;
                                return true;
                            }
                            else if (Score > 0)
                            {
                                if (Score < MinorScore)
                                {
                                    MinorScore = Score;
                                    MinorMethod = Method;
                                    MinorParameterTypes = TempDefinedParameterTypes;
                                }
                            }
                        }
                        else
                        {
                            Type[] TempDefinedParameterTypes = Parameter.Select(i => i.ParameterType).ToArray();
                            int Score = MatchParameters(ParameterTypes, TempDefinedParameterTypes);
                            if (Score == 0)
                            {
                                Info = Method;
                                DefinedParameterTypes = ParameterTypes;
                                return true;
                            }
                            else if (Score > 0)
                            {
                                if (Score < MinorScore)
                                {
                                    MinorScore = Score;
                                    MinorMethod = Method;
                                    MinorParameterTypes = TempDefinedParameterTypes;
                                }
                            }
                        }
                    }
                }

                Base = Base.BaseType;
            }

            if (MinorMethod is null)
            {
                Info = null;
                DefinedParameterTypes = null;
                return false;
            }

            Info = IsGeneric ? MinorMethod.MakeGenericMethod(GenericTypes) : MinorMethod;
            DefinedParameterTypes = MinorParameterTypes;
            return true;
        }

        /// <summary>
        /// Scores the matching of the types.
        /// </summary>
        /// <returns>The score of matching.<para/>
        /// If <paramref name="Types"/> are equal to <paramref name="DefinedTypes"/>, return 0.<para/>
        /// If <paramref name="Types"/> and <paramref name="DefinedTypes"/> are completely different, return <see cref="int.MinValue"/>.</returns>
        private static int MatchParameters(Type[] Types, Type[] DefinedTypes)
        {
            Type Type, DefinedType;
            int Score = 0;
            for (int i = 0; i < Types.Length; i++)
            {
                DefinedType = DefinedTypes[i];
                Type = Types[i];
                if (Type != DefinedType)
                {
                    Score++;
                    if (!Type.IsConvertibleTo(DefinedType))
                        return int.MinValue;

                    if (NumberTypeScores.TryGetValue(Type, out sbyte Score1) &&
                        NumberTypeScores.TryGetValue(DefinedType, out sbyte Score2))
                    {
                        int Delta = Score2 - Score1;
                        Score += Delta < 0 ? 11 - Score1 : Delta;
                    }
                }
            }

            return Score;
        }

        public static IEnumerable<MethodInfo> GetImplicits(this Type This)
        {
            Type BaseType = This;
            while (BaseType != null)
            {
                foreach (MethodInfo Implicit in BaseType.GetMethods(StaticFlags).Where(i => i.Name == "op_Implicit"))
                    yield return Implicit;

                BaseType = BaseType.BaseType;
            }
        }
        public static IEnumerable<MethodInfo> GetExplicits(this Type This)
        {
            Type BaseType = This;
            while (BaseType != null)
            {
                foreach (MethodInfo Implicit in BaseType.GetMethods(StaticFlags).Where(i => i.Name == "op_Explicit"))
                    yield return Implicit;

                BaseType = BaseType.BaseType;
            }
        }

        public static bool TryInvokeMethod(this object This, string Name, params object[] Args)
            => TryInvokeMethod(This, PublicFlags, Name, Args);
        public static bool TryInvokeMethod<T>(this object This, string Name, out T Result, params object[] Args)
            => TryInvokeMethod(This, PublicFlags, Name, out Result, Args);
        public static bool TryInvokeMethod<T>(this object This, string Name, Type[] GenericTypes, out T Result, params object[] Args)
            => TryInvokeMethod(This, PublicFlags, Name, GenericTypes, out Result, Args);

        public static bool TryInvokeInternalMethod(this object This, string Name, params object[] Args)
            => TryInvokeMethod(This, InternalFlags, Name, Args);
        public static bool TryInvokeInternalMethod<T>(this object This, string Name, out T Result, params object[] Args)
            => TryInvokeMethod(This, InternalFlags, Name, out Result, Args);
        public static bool TryInvokeInternalMethod<T>(this object This, string Name, Type[] GenericTypes, out T Result, params object[] Args)
            => TryInvokeMethod(This, InternalFlags, Name, GenericTypes, out Result, Args);

        public static bool TryInvokeMethod(this object This, BindingFlags Flags, string Name, params object[] Args)
        {
            if (TryGetMethodWithImplicitParameter(This?.GetType(), Flags, Name, new Type[0], Args.Select(i => i.GetType()).ToArray(), out MethodInfo Method))
            {
                Method.Invoke(This, Args);
                return true;
            }

            return false;
        }
        public static bool TryInvokeMethod<T>(this object This, BindingFlags Flags, string Name, out T Result, params object[] Args)
        {
            if (TryGetMethodWithImplicitParameter(This?.GetType(), Flags, Name, new Type[0], Args.Select(i => i.GetType()).ToArray(), out MethodInfo Method) &&
                Method.ReturnType.IsConvertibleTo<T>())
            {
                Result = (T)Method.Invoke(This, Args);
                return true;
            }

            Result = default;
            return false;
        }
        public static bool TryInvokeMethod<T>(this object This, BindingFlags Flags, string Name, Type[] GenericTypes, out T Result, params object[] Args)
        {
            if (TryGetMethodWithImplicitParameter(This?.GetType(), Flags, Name, GenericTypes, Args.Select(i => i.GetType()).ToArray(), out MethodInfo Method) &&
                Method.ReturnType.IsConvertibleTo<T>())
            {
                Result = (T)Method.Invoke(This, Args);
                return true;
            }

            Result = default;
            return false;
        }

        #endregion

        #region Event
        public static bool TryGetEventField(this object This, string Name, out MulticastDelegate Delegates)
            => TryGetInternalFieldValue(This, Name, out Delegates) && Delegates != null;

        public static bool TryGetStaticEventField(this object This, string Name, out MulticastDelegate Delegates)
            => TryGetStaticInternalFieldValue(This, Name, out Delegates) && Delegates != null;

        public static void RaiseEvent(this object This, string Name, params object[] Args)
        {
            if (TryGetEventField(This, Name, out MulticastDelegate Handler))
                foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                    InvocationMethod.DynamicInvoke(new[] { This, Args });
        }

        public static void RaiseStaticEvent(this object This, string Name, params object[] Args)
        {
            if (TryGetStaticEventField(This, Name, out MulticastDelegate Handler))
                foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                    InvocationMethod.DynamicInvoke(new[] { This, Args });
        }

        #endregion

        #region Inherited and Convert
        /// <summary>
        /// Determines whether the current type inherited from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to compare with the current type.</typeparam>
        public static bool IsBaseOn<T>(this Type This)
            => IsBaseOn(This, typeof(T));
        /// <summary>
        /// Determines whether the current type inherited from the specified type.
        /// </summary>
        /// <param name="Type">The type to compare with the current type.</param>
        public static bool IsBaseOn(this Type This, Type Type)
            => Type.IsAssignableFrom(This);

        /// <summary>
        /// Determines whether an instance of the current type is convertible to an instance of a specified type.
        /// </summary>
        /// <typeparam name="T">The type to compare with the current type.</typeparam>
        public static bool IsConvertibleTo<T>(this Type This)
            => IsConvertibleTo(This, typeof(T));
        /// <summary>
        /// Determines whether an instance of the current type is convertible to an instance of a specified type.
        /// </summary>
        /// <param name="Type">The type to compare with the current type.</param>
        public static bool IsConvertibleTo(this Type This, Type Type)
        {
            if (Type.IsAssignableFrom(This))
                return true;

            if (NumberTypes.ContainsKey(This) && NumberTypes.ContainsKey(Type))
                return true;

            do
            {
                if (This.GetMethods(StaticFlags)
                        .Where(i => i.Name == "op_Implicit" || i.Name == "op_Explicit")
                        .Any(t => t.ReturnType == Type))
                    return true;

                This = This.BaseType;
            } while (This != null);

            return false;
        }

        #endregion

        #region NumberType
        internal static readonly Dictionary<Type, byte> NumberTypes = new Dictionary<Type, byte>
        {
            { typeof(byte), 0 },
            { typeof(ushort), 1 },
            { typeof(uint), 2 },
            { typeof(ulong), 3 },

            { typeof(sbyte), 4 },
            { typeof(short), 5 },
            { typeof(int), 6 },
            { typeof(long), 7 },

            { typeof(float), 8 },
            { typeof(double), 9 },
            { typeof(decimal), 10 }
        };
        private static readonly Dictionary<Type, sbyte> NumberTypeScores = new Dictionary<Type, sbyte>
        {
            { typeof(sbyte), 0 },
            { typeof(byte), 1 },

            { typeof(short), 2 },
            { typeof(ushort), 3 },

            { typeof(int), 4 },
            { typeof(uint), 5 },

            { typeof(long), 6 },
            { typeof(ulong), 7 },

            { typeof(float), 8 },
            { typeof(double), 9 },
            { typeof(decimal), 10 },
        };

        /// <summary>
        /// Determines whether the current type is
        /// <see cref="byte"/>、<see cref="ushort"/>、<see cref="uint"/>、<see cref="ulong"/>、
        /// <see cref="sbyte"/>、<see cref="short"/>、<see cref="int"/>、<see cref="long"/>、
        /// <see cref="float"/>、<see cref="double"/>、<see cref="decimal"/>.
        /// </summary>
        public static bool IsNumberType(this Type This)
            => NumberTypes.ContainsKey(This);
        /// <summary>
        /// Determines whether the current type is <see cref="float"/>、<see cref="double"/>、<see cref="decimal"/>.
        /// </summary>
        public static bool IsDecimalType(this Type This)
            => NumberTypes.TryGetValue(This, out byte Value) && 7 < Value;
        /// <summary>
        /// Determines whether the current type is
        /// <see cref="byte"/>、<see cref="ushort"/>、<see cref="uint"/>、<see cref="ulong"/>、
        /// <see cref="sbyte"/>、<see cref="short"/>、<see cref="int"/>、<see cref="long"/>、
        /// </summary>
        public static bool IsIntegerType(this Type This)
            => NumberTypes.TryGetValue(This, out byte Value) && Value < 8;
        /// <summary>
        /// Determines whether the current type is
        /// <see cref="sbyte"/>、<see cref="short"/>、<see cref="int"/>、<see cref="long"/>.
        /// </summary>
        public static bool IsSignedIntegerType(this Type This)
            => NumberTypes.TryGetValue(This, out byte Value) && 3 < Value && Value < 8;
        /// <summary>
        /// Determines whether the current type is
        /// <see cref="byte"/>、<see cref="ushort"/>、<see cref="uint"/>、<see cref="ulong"/>.
        /// </summary>
        public static bool IsUnsignedIntegerType(this Type This)
            => NumberTypes.TryGetValue(This, out byte Value) && Value < 4;

        #endregion

        internal static readonly Dictionary<string, Type> TypeAlias = new Dictionary<string, Type>
        {
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "char", typeof(char) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "long", typeof(long) },
            { "object", typeof(object) },
            { "sbyte", typeof(sbyte) },
            { "short", typeof(short) },
            { "string", typeof(string) },
            { "uint", typeof(uint) },
            { "ulong", typeof(ulong) },
            { "void", typeof(void) }
        };
        public static bool TryGetType(string Route, out Type Type)
            => TryGetType(Route, new Type[0], out Type);
        public static bool TryGetType(string Route, Type[] GenericTypes, out Type Type)
        {
            int Length = GenericTypes.Length;
            if (Length > 0)
                Route = $"{Route}`{GenericTypes.Length}";

            if (Route.Contains('.'))
            {
                if (AppDomain.CurrentDomain.GetAssemblies()
                                           .Select(i => i.GetType(Route, false))
                                           .FirstOrDefault(i => i != null) is Type Result)
                {
                    Type = Length > 0 ? Result.MakeGenericType(GenericTypes) : Result;
                    return true;
                }

                Type = null;
                return false;
            }

            if (TypeAlias.TryGetValue(Route, out Type))
                return true;

            Type[] Types = AppDomain.CurrentDomain.GetAssemblies()
                                                  .TrySelectMany(i => i.GetTypes())
                                                  .Where(i => i.Name == Route)
                                                  .ToArray();
            switch (Types.Length)
            {
                case 1:
                    {
                        Type = Length > 0 ? Types[0].MakeGenericType(GenericTypes) : Types[0];
                        return true;
                    }
                case 0:
                default:
                    {
                        Type = null;
                        return false;
                    }
            }
        }
        public static bool TryGetType(string Name, string Namespace, out Type Type)
            => TryGetType(Name, Namespace, new Type[0], out Type);
        public static bool TryGetType(string Name, string Namespace, Type[] GenericTypes, out Type Type)
        {
            int Length = GenericTypes.Length;
            if (Length > 0)
                Name = $"{Name}`{GenericTypes.Length}";

            if (string.IsNullOrEmpty(Namespace))
            {
                if (TypeAlias.TryGetValue(Name, out Type))
                    return true;

                Type[] Types = AppDomain.CurrentDomain.GetAssemblies()
                                                      .TrySelectMany(i => i.GetTypes())
                                                      .Where(i => i.Name == Name)
                                                      .ToArray();
                switch (Types.Length)
                {
                    case 1:
                        {
                            Type = Length > 0 ? Types[0].MakeGenericType(GenericTypes) : Types[0];
                            return true;
                        }
                    case 0:
                    default:
                        {
                            Type = null;
                            return false;
                        }
                }
            }

            string Route = $"{Namespace}.{Name}";
            if (AppDomain.CurrentDomain.GetAssemblies()
                                            .Select(i => i.GetType(Route, false))
                                            .FirstOrDefault(i => i != null) is Type Result)
            {
                Type = Length > 0 ? Result.MakeGenericType(GenericTypes) : Result;
                return true;
            }

            Type = null;
            return false;
        }

    }
}