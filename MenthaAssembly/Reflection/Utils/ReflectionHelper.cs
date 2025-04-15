using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace System.Reflection
{
    public static class ReflectionHelper
    {
        public const BindingFlags AllModifier = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                  AllStaticModifier = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                                  PublicModifier = BindingFlags.Public | BindingFlags.Instance,
                                  PublicStaticModifier = BindingFlags.Public | BindingFlags.Static,
                                  InternalModifier = BindingFlags.NonPublic | BindingFlags.Instance,
                                  InternalStaticModifier = BindingFlags.NonPublic | BindingFlags.Static;

        #region Property
        public static IEnumerable<PropertyInfo> GetProperties(this Type This, params string[] Names)
        {
            foreach (string Name in Names)
                if (This.GetProperty(Name) is PropertyInfo Info)
                    yield return Info;
        }

        public static bool TryGetProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), Name, PublicModifier, out Info);
        public static bool TryGetProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, Name, PublicModifier, out Info);

        public static bool TryGetStaticProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), Name, PublicStaticModifier, out Info);
        public static bool TryGetStaticProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, Name, PublicStaticModifier, out Info);

        public static bool TryGetInternalProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), Name, InternalModifier, out Info);
        public static bool TryGetInternalProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, Name, InternalModifier, out Info);

        public static bool TryGetStaticInternalProperty<T>(string Name, out PropertyInfo Info)
            => TryGetProperty(typeof(T), Name, InternalStaticModifier, out Info);
        public static bool TryGetStaticInternalProperty(this Type This, string Name, out PropertyInfo Info)
            => TryGetProperty(This, Name, InternalStaticModifier, out Info);

        public static bool TryGetProperty<T>(string Name, BindingFlags Modifier, out PropertyInfo Info)
            => TryGetProperty(typeof(T), Name, Modifier, out Info);
        public static bool TryGetProperty(this Type This, string Name, BindingFlags Modifier, out PropertyInfo Info)
        {
            Type BaseType = This;
            PropertyInfo Result = BaseType?.GetProperty(Name, Modifier);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Info = null;
                    return false;
                }

                Result = BaseType.GetProperty(Name, Modifier);
            }
            Info = Result;
            return true;
        }

        public static bool TrySetPropertyValue<T>(object This, string Name, T Value)
            => InternalTrySetPropertyValue(This?.GetType(), This, Name, PublicModifier, Value);
        public static bool TryGetPropertyValue<T>(object This, string Name, out T Value)
            => InternalTryGetPropertyValue(This?.GetType(), This, Name, PublicModifier, out Value);

        public static bool TrySetInternalPropertyValue<T>(object This, string Name, T Value)
            => InternalTrySetPropertyValue(This?.GetType(), This, Name, InternalModifier, Value);
        public static bool TryGetInternalPropertyValue<T>(object This, string Name, out T Value)
            => InternalTryGetPropertyValue(This?.GetType(), This, Name, InternalModifier, out Value);

        public static bool TrySetPropertyValue<T>(object This, string Name, BindingFlags Modifier, T Value)
            => InternalTrySetPropertyValue(This?.GetType(), This, Name, Modifier, Value);
        public static bool TryGetPropertyValue<T>(object This, string Name, BindingFlags Modifier, out T Value)
            => InternalTryGetPropertyValue(This?.GetType(), This, Name, Modifier, out Value);

        public static bool TrySetStaticPropertyValue<T>(this Type This, string Name, T Value)
            => InternalTrySetPropertyValue(This, null, Name, PublicStaticModifier, Value);
        public static bool TryGetStaticPropertyValue<T>(this Type This, string Name, out T Value)
            => InternalTryGetPropertyValue(This, null, Name, PublicStaticModifier, out Value);

        public static bool TrySetStaticInternalPropertyValue<T>(this Type This, string Name, T Value)
            => InternalTrySetPropertyValue(This, null, Name, InternalStaticModifier, Value);
        public static bool TryGetStaticInternalPropertyValue<T>(this Type This, string Name, out T Value)
            => InternalTryGetPropertyValue(This, null, Name, InternalStaticModifier, out Value);

        private static bool InternalTrySetPropertyValue<T>(Type Type, object Object, string Name, BindingFlags Modifier, T Value)
        {
            if (TryGetProperty(Type, Name, Modifier, out PropertyInfo Info) &&
                Info.CanWrite &&
                typeof(T).IsBaseOn(Info.PropertyType))
            {
                Info.SetValue(Object, Value);
                return true;
            }
            return false;
        }
        private static bool InternalTryGetPropertyValue<T>(Type Type, object Object, string Name, BindingFlags Modifier, out T Value)
        {
            if (TryGetProperty(Type, Name, Modifier, out PropertyInfo Info) &&
                Info.CanRead &&
                Info.PropertyType.IsBaseOn<T>())
            {
                Value = (T)Info.GetValue(Object);
                return true;
            }

            Value = default;
            return false;
        }

        #endregion

        #region Field
        public static bool IsBackingField(FieldInfo Field)
            => Field.Name.ToLower().Contains(">k__backingfield");

        public static bool TryGetField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), Name, PublicModifier, out Info);
        public static bool TryGetField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, Name, PublicModifier, out Info);

        public static bool TryGetStaticField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), Name, PublicStaticModifier, out Info);
        public static bool TryGetStaticField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, Name, PublicStaticModifier, out Info);

        public static bool TryGetInternalField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), Name, InternalModifier, out Info);
        public static bool TryGetInternalField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, Name, InternalModifier, out Info);

        public static bool TryGetStaticInternalField<T>(string Name, out FieldInfo Info)
            => TryGetField(typeof(T), Name, InternalStaticModifier, out Info);
        public static bool TryGetStaticInternalField(this Type This, string Name, out FieldInfo Info)
            => TryGetField(This, Name, InternalStaticModifier, out Info);

        public static bool TryGetField<T>(string Name, BindingFlags Modifier, out FieldInfo Info)
            => TryGetField(typeof(T), Name, Modifier, out Info);
        public static bool TryGetField(this Type This, string Name, BindingFlags Modifier, out FieldInfo Info)
        {
            Type BaseType = This;
            FieldInfo Result = BaseType?.GetField(Name, Modifier);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Info = null;
                    return false;
                }

                Result = BaseType.GetField(Name, Modifier);
            }
            Info = Result;
            return true;
        }

        public static bool TrySetFieldValue<T>(object This, string Name, T Value)
            => TrySetFieldValue(This, Name, PublicModifier, Value);
        public static bool TryGetFieldValue<T>(object This, string Name, out T Value)
            => TryGetFieldValue(This, Name, PublicModifier, out Value);

        public static bool TrySetInternalFieldValue<T>(object This, string Name, T Value)
            => TrySetFieldValue(This, Name, InternalModifier, Value);
        public static bool TryGetInternalFieldValue<T>(object This, string Name, out T Value)
            => TryGetFieldValue(This, Name, InternalModifier, out Value);

        public static bool TrySetFieldValue<T>(object This, string Name, BindingFlags Modifier, T Value)
            => InternalTrySetFieldValue(This?.GetType(), This, Name, Modifier, Value);
        public static bool TryGetFieldValue<T>(object This, string Name, BindingFlags Modifier, out T Value)
            => InternalTryGetFieldValue(This?.GetType(), This, Name, Modifier, out Value);

        public static bool TrySetStaticFieldValue<T>(this Type This, string Name, T Value)
            => InternalTrySetFieldValue(This?.GetType(), null, Name, PublicStaticModifier, Value);
        public static bool TryGetStaticFieldValue<T>(this Type This, string Name, out T Value)
            => InternalTryGetFieldValue(This?.GetType(), null, Name, PublicStaticModifier, out Value);

        public static bool TrySetStaticInternalFieldValue<T>(this Type This, string Name, T Value)
            => InternalTrySetFieldValue(This?.GetType(), null, Name, InternalStaticModifier, Value);
        public static bool TryGetStaticInternalFieldValue<T>(this Type This, string Name, out T Value)
            => InternalTryGetFieldValue(This?.GetType(), null, Name, InternalStaticModifier, out Value);

        private static bool InternalTrySetFieldValue<T>(Type Type, object Object, string Name, BindingFlags Modifier, T Value)
        {
            if (TryGetField(Type, Name, Modifier, out FieldInfo Info) &&
                Info.FieldType.IsBaseOn<T>())
            {
                Info.SetValue(Object, Value);
                return true;
            }

            return false;
        }
        private static bool InternalTryGetFieldValue<T>(Type Type, object Object, string Name, BindingFlags Modifier, out T Value)
        {
            if (TryGetField(Type, Name, Modifier, out FieldInfo Info) &&
                Info.FieldType.IsBaseOn<T>())
            {
                Value = (T)Info.GetValue(Object);
                return true;
            }

            Value = default;
            return false;
        }

        #endregion

        #region Indexer
        public static bool TryGetIndexer<T>(Type[] ParameterTypes, out PropertyInfo Info)
            => TryGetIndexer(typeof(T), PublicModifier, ParameterTypes, out Info);
        public static bool TryGetIndexer<T>(BindingFlags Modifier, Type[] ParameterTypes, out PropertyInfo Info)
            => TryGetIndexer(typeof(T), Modifier, ParameterTypes, out Info);
        public static bool TryGetIndexer(this Type This, Type[] ParameterTypes, out PropertyInfo Info)
            => TryGetIndexer(This, PublicModifier, ParameterTypes, out Info);
        public static bool TryGetIndexer(this Type This, BindingFlags Modifier, Type[] ParameterTypes, out PropertyInfo Info)
        {
            int ParameterCount = ParameterTypes.Length;
            while (This != null)
            {
                foreach (PropertyInfo Property in This.GetProperties(Modifier))
                {
                    ParameterInfo[] Params = Property.GetIndexParameters();
                    if (Params.Length != ParameterCount)
                        continue;

                    if (ParameterTypes.SequenceEqual(Params.Select(i => i.ParameterType)))
                    {
                        Info = Property;
                        return true;
                    }
                }

                This = This.BaseType;
            }

            Info = null;
            return false;
        }

        public static bool TryGetIndexerWithImplicitParameter<T>(Type[] ParameterTypes, out PropertyInfo Info, out Type[] DefinedParameterTypes)
            => TryGetIndexerWithImplicitParameter(typeof(T), PublicModifier, ParameterTypes, out Info, out DefinedParameterTypes);
        public static bool TryGetIndexerWithImplicitParameter<T>(BindingFlags Modifier, Type[] ParameterTypes, out PropertyInfo Info, out Type[] DefinedParameterTypes)
            => TryGetIndexerWithImplicitParameter(typeof(T), Modifier, ParameterTypes, out Info, out DefinedParameterTypes);
        public static bool TryGetIndexerWithImplicitParameter(this Type This, Type[] ParameterTypes, out PropertyInfo Info, out Type[] DefinedParameterTypes)
            => TryGetIndexerWithImplicitParameter(This, PublicModifier, ParameterTypes, out Info, out DefinedParameterTypes);
        public static bool TryGetIndexerWithImplicitParameter(this Type This, BindingFlags Modifier, Type[] ParameterTypes, out PropertyInfo Info, out Type[] DefinedParameterTypes)
        {
            int ParameterCount = ParameterTypes.Length;

            PropertyInfo MinorIndexer = null;
            Type[] MinorParameterTypes = null;
            int MinorScore = int.MaxValue;

            while (This != null)
            {
                foreach (PropertyInfo Property in This.GetProperties(Modifier))
                {
                    ParameterInfo[] Params = Property.GetIndexParameters();
                    if (Params.Length != ParameterCount)
                        continue;

                    Type[] TempDefinedParameterTypes = Params.Select(i => i.ParameterType).ToArray();
                    int Score = ScoreMatchingParameters(ParameterTypes, TempDefinedParameterTypes);
                    if (Score == 0)
                    {
                        Info = Property;
                        DefinedParameterTypes = ParameterTypes;
                        return true;
                    }
                    else if (Score > 0)
                    {
                        if (Score < MinorScore)
                        {
                            MinorScore = Score;
                            MinorIndexer = Property;
                            MinorParameterTypes = TempDefinedParameterTypes;
                        }
                    }
                }

                This = This.BaseType;
            }

            if (MinorIndexer is null)
            {
                Info = null;
                DefinedParameterTypes = null;
                return false;
            }

            Info = MinorIndexer;
            DefinedParameterTypes = MinorParameterTypes;
            return true;
        }

        #endregion

        #region Constant
        public static bool TryGetConstant<T>(this Type This, string Name, out T Value)
        {
            if (TryGetField(This, Name, PublicStaticModifier, out FieldInfo Field))
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
            => TryGetMethod(typeof(T), Name, PublicModifier, out Info);
        public static bool TryGetMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, PublicModifier, ParameterTypes, out Info);
        public static bool TryGetMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, PublicModifier, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, Name, PublicModifier, out Info);
        public static bool TryGetMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, PublicModifier, ParameterTypes, out Info);
        public static bool TryGetMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, PublicModifier, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetStaticMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, PublicStaticModifier, out Info);
        public static bool TryGetStaticMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, PublicStaticModifier, ParameterTypes, out Info);
        public static bool TryGetStaticMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, PublicStaticModifier, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetStaticMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, Name, PublicStaticModifier, out Info);
        public static bool TryGetStaticMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, PublicStaticModifier, ParameterTypes, out Info);
        public static bool TryGetStaticMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, PublicStaticModifier, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetInternalMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, InternalModifier, out Info);
        public static bool TryGetInternalMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, InternalModifier, ParameterTypes, out Info);
        public static bool TryGetInternalMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, InternalModifier, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetInternalMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, Name, InternalModifier, out Info);
        public static bool TryGetInternalMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, InternalModifier, ParameterTypes, out Info);
        public static bool TryGetInternalMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, InternalModifier, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetStaticInternalMethod<T>(string Name, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, InternalStaticModifier, out Info);
        public static bool TryGetStaticInternalMethod<T>(string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, InternalStaticModifier, ParameterTypes, out Info);
        public static bool TryGetStaticInternalMethod<T>(string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, InternalStaticModifier, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetStaticInternalMethod(this Type This, string Name, out MethodInfo Info)
            => TryGetMethod(This, Name, InternalStaticModifier, out Info);
        public static bool TryGetStaticInternalMethod(this Type This, string Name, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, InternalStaticModifier, ParameterTypes, out Info);
        public static bool TryGetStaticInternalMethod(this Type This, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(This, Name, InternalStaticModifier, GenericTypes, ParameterTypes, out Info);

        public static bool TryGetMethod<T>(string Name, BindingFlags Modifier, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, Modifier, out Info);
        public static bool TryGetMethod<T>(string Name, BindingFlags Modifier, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, Modifier, ParameterTypes, out Info);
        public static bool TryGetMethod<T>(string Name, BindingFlags Modifier, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethod(typeof(T), Name, Modifier, GenericTypes, ParameterTypes, out Info);
        public static bool TryGetMethod(this Type This, string Name, BindingFlags Modifier, out MethodInfo Info)
        {
            Type BaseType = This;
            MethodInfo Result = BaseType?.GetMethod(Name, Modifier);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Info = null;
                    return false;
                }

                Result = BaseType.GetMethod(Name, Modifier);
            }
            Info = Result;
            return true;
        }
        public static bool TryGetMethod(this Type This, string Name, BindingFlags Modifier, Type[] ParameterTypes, out MethodInfo Info)
        {
            while (This != null)
            {
                if (This.GetMethod(Name, Modifier, null, ParameterTypes, null) is MethodInfo Method)
                {
                    Info = Method;
                    return true;
                }

                This = This.BaseType;
            }

            Info = null;
            return false;
        }
        public static bool TryGetMethod(this Type This, string Name, BindingFlags Modifier, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
        {
            int GenericLength = GenericTypes.Length,
                ParameterLength = ParameterTypes.Length;
            bool IsGeneric = GenericLength > 0;

            List<Tuple<MethodInfo, Type[]>> MinorMethods = [];
            while (This != null)
            {
                foreach (MethodInfo Method in This.GetMethods(Modifier).Where(i => i.Name == Name))
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
                            bool IsMatch = true;
                            for (int i = 0; i < ParameterLength; i++)
                            {
                                Type DefinedType = DefinedParameterTypes[i];

                                // Generic parameters
                                // Ex. T arg1
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

                                // Generic type parameters
                                // Ex. Func<T> arg1
                                else if (DefinedType.IsGenericType)
                                {
                                    List<Type> SubDefinedParameterTypes = [];

                                    Type[] SubDefinedGenericTypes = DefinedType.GetGenericArguments();
                                    foreach (Type SubDefinedType in SubDefinedGenericTypes)
                                    {
                                        int Index = Array.IndexOf(DefinedGenericTypes, SubDefinedType);
                                        if (Index == -1)
                                        {
                                            IsMatch = false;
                                            break;
                                        }

                                        SubDefinedParameterTypes.Add(GenericTypes[Index]);
                                    }

                                    if (!IsMatch)
                                        break;

                                    DefinedType = DefinedType.GetGenericTypeDefinition();
                                    DefinedParameterTypes[i] = DefinedType.MakeGenericType([.. SubDefinedParameterTypes]);
                                }
                            }

                            if (IsMatch &&
                                ParameterTypes.SequenceEqual(DefinedParameterTypes))
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
            => TryGetMethodWithImplicitParameter(Base, Name, PublicModifier | BindingFlags.Static, GenericTypes, ParameterTypes, out Info, out _);
        public static bool TryGetMethodWithImplicitParameter(Type Base, string Name, BindingFlags Modifier, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info)
            => TryGetMethodWithImplicitParameter(Base, Name, Modifier, GenericTypes, ParameterTypes, out Info, out _);
        internal static bool TryGetMethodWithImplicitParameter(Type Base, string Name, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes)
            => TryGetMethodWithImplicitParameter(Base, Name, PublicModifier | BindingFlags.Static, GenericTypes, ParameterTypes, out Info, out DefinedParameterTypes);
        internal static bool TryGetMethodWithImplicitParameter(Type Base, string Name, BindingFlags Modifier, Type[] GenericTypes, Type[] ParameterTypes, out MethodInfo Info, out Type[] DefinedParameterTypes)
        {
            int GenericLength = GenericTypes.Length,
                ParameterLength = ParameterTypes.Length;
            bool IsGeneric = GenericLength > 0;

            MethodInfo MinorMethod = null;
            Type[] MinorParameterTypes = null;
            int MinorScore = int.MaxValue;
            while (Base != null)
            {
                foreach (MethodInfo Method in Base.GetMethods(Modifier).Where(i => i.Name == Name))
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
                            bool IsMatch = true;
                            for (int i = 0; i < ParameterLength; i++)
                            {
                                Type DefinedType = TempDefinedParameterTypes[i];

                                // Generic parameters
                                // Ex. T arg1
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

                                // Generic type parameters
                                // Ex. Func<T> arg1
                                else if (DefinedType.IsGenericType)
                                {
                                    List<Type> SubDefinedParameterTypes = [];

                                    Type[] SubDefinedGenericTypes = DefinedType.GetGenericArguments();
                                    foreach (Type SubDefinedType in SubDefinedGenericTypes)
                                    {
                                        int Index = Array.IndexOf(DefinedGenericTypes, SubDefinedType);
                                        if (Index == -1)
                                        {
                                            IsMatch = false;
                                            break;
                                        }

                                        SubDefinedParameterTypes.Add(GenericTypes[Index]);
                                    }

                                    if (!IsMatch)
                                        break;

                                    DefinedType = DefinedType.GetGenericTypeDefinition();
                                    TempDefinedParameterTypes[i] = DefinedType.MakeGenericType([.. SubDefinedParameterTypes]);
                                }
                            }

                            if (!IsMatch)
                                continue;

                            int Score = ScoreMatchingParameters(ParameterTypes, TempDefinedParameterTypes);
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
                            int Score = ScoreMatchingParameters(ParameterTypes, TempDefinedParameterTypes);
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

        public static IEnumerable<MethodInfo> GetImplicits(this Type This)
        {
            Type BaseType = This;
            while (BaseType != null)
            {
                foreach (MethodInfo Implicit in BaseType.GetMethods(PublicStaticModifier).Where(i => i.Name == "op_Implicit"))
                    yield return Implicit;

                BaseType = BaseType.BaseType;
            }
        }
        public static IEnumerable<MethodInfo> GetExplicits(this Type This)
        {
            Type BaseType = This;
            while (BaseType != null)
            {
                foreach (MethodInfo Implicit in BaseType.GetMethods(PublicStaticModifier).Where(i => i.Name == "op_Explicit"))
                    yield return Implicit;

                BaseType = BaseType.BaseType;
            }
        }

        public static bool TryInvokeMethod(object This, string Name, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, PublicModifier, Type.EmptyTypes, Args);
        public static bool TryInvokeMethod(object This, string Name, Type[] GenericTypes, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, PublicModifier, GenericTypes, Args);
        public static bool TryInvokeMethod<T>(object This, string Name, out T Result, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, PublicModifier, Type.EmptyTypes, out Result, Args);
        public static bool TryInvokeMethod<T>(object This, string Name, Type[] GenericTypes, out T Result, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, PublicModifier, GenericTypes, out Result, Args);

        public static bool TryInvokeInternalMethod(object This, string Name, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, InternalModifier, Type.EmptyTypes, Args);
        public static bool TryInvokeInternalMethod(object This, string Name, Type[] GenericTypes, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, InternalModifier, GenericTypes, Args);
        public static bool TryInvokeInternalMethod<T>(object This, string Name, out T Result, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, InternalModifier, Type.EmptyTypes, out Result, Args);
        public static bool TryInvokeInternalMethod<T>(object This, string Name, Type[] GenericTypes, out T Result, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, InternalModifier, GenericTypes, out Result, Args);

        public static bool TryInvokeMethod(object This, string Name, BindingFlags Modifier, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, Modifier, Type.EmptyTypes, Args);
        public static bool TryInvokeMethod(object This, string Name, BindingFlags Modifier, Type[] GenericTypes, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, Modifier, GenericTypes, Args);
        public static bool TryInvokeMethod<T>(object This, string Name, BindingFlags Modifier, out T Result, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, Modifier, Type.EmptyTypes, out Result, Args);
        public static bool TryInvokeMethod<T>(object This, string Name, BindingFlags Modifier, Type[] GenericTypes, out T Result, params object[] Args)
            => InternalTryInvokeMethod(This?.GetType(), This, Name, Modifier, GenericTypes, out Result, Args);

        private static bool InternalTryInvokeMethod(Type Type, object Object, string Name, BindingFlags Modifier, Type[] GenericTypes, params object[] Args)
        {
            if (TryGetMethodWithImplicitParameter(Type, Name, Modifier, GenericTypes, Args.Select(i => i?.GetType()).ToArray(), out MethodInfo Method))
            {
                Method.Invoke(Object, Args);
                return true;
            }

            return false;
        }
        private static bool InternalTryInvokeMethod<T>(Type Type, object Object, string Name, BindingFlags Modifier, Type[] GenericTypes, out T Result, params object[] Args)
        {
            if (TryGetMethodWithImplicitParameter(Type, Name, Modifier, GenericTypes, Args.Select(i => i?.GetType()).ToArray(), out MethodInfo Method) &&
                Method.ReturnType.IsConvertibleTo<T>())
            {
                Result = (T)Method.Invoke(Object, Args);
                return true;
            }

            Result = default;
            return false;
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type This)
            => This.GetMethods()
                   .Concat(This.GetInterfaces()
                               .SelectMany(i => i.GetMethods()));

        #endregion

        #region Event
        public static bool TryGetEventField(object This, string Name, out MulticastDelegate Delegates)
            => TryGetInternalFieldValue(This, Name, out Delegates) && Delegates != null;

        public static bool TryGetStaticEventField(this Type This, string Name, out MulticastDelegate Delegates)
            => TryGetStaticInternalFieldValue(This, Name, out Delegates) && Delegates != null;

        public static void RaiseEvent(object This, string Name, params object[] Args)
        {
            if (TryGetEventField(This, Name, out MulticastDelegate Handler))
            {
                object[] Arguments = new object[Args.Length + 1];
                Arguments[0] = This;
                Args.CopyTo(Arguments, 1);

                foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                    InvocationMethod.DynamicInvoke(Arguments);
            }
        }

        public static void RaiseStaticEvent(object This, string Name, params object[] Args)
        {
            if (TryGetStaticEventField(This?.GetType(), Name, out MulticastDelegate Handler))
                foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                    InvocationMethod.DynamicInvoke([This, Args]);
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
                if (This.GetMethods(PublicStaticModifier)
                        .Where(i => i.Name is "op_Implicit" or "op_Explicit")
                        .Any(t => t.ReturnType == Type))
                    return true;

                This = This.BaseType;
            } while (This != null);

            return false;
        }

        /// <summary>
        /// Try to get the interface type that inherits the specified generic interface in the specified type.
        /// </summary>
        /// <param name="Type">The specified type.</param>
        /// <param name="GenericInterface">The specified type of generic interface.</param>
        /// <param name="InheritedInterface">The interface type that inherits <paramref name="GenericInterface"/>.</param>
        public static bool TryGetInheritedGenericInterfaceType(Type Type, Type GenericInterface, out Type InheritedInterface)
        {
            if (Type.IsGenericType && Type.GetGenericTypeDefinition() == GenericInterface)
            {
                InheritedInterface = Type;
                return true;
            }

            // Check all the interfaces implemented by this type
            foreach (Type Interface in Type.GetInterfaces())
            {
                if (Interface.IsGenericType && Interface.GetGenericTypeDefinition() == GenericInterface)
                {
                    InheritedInterface = Interface;
                    return true;
                }
            }

            InheritedInterface = null;
            return false;
        }

        #endregion

        #region Operator

        private static HashSet<Type> BuiltInBitwiseTypes =
        [
            typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char)
        ];

        /// <summary>
        /// Determines whether this type supports "&amp;" operator.
        /// </summary>
        public static bool SupportsBitwiseAnd(this Type Type)
        {
            Type NonNullable = Nullable.GetUnderlyingType(Type) ?? Type;
            return BuiltInBitwiseTypes.Contains(NonNullable) || HasOperator(NonNullable, "op_BitwiseAnd");
        }

        /// <summary>
        /// Determines whether this type supports "|" operator.
        /// </summary>
        public static bool SupportsBitwiseOr(this Type Type)
        {
            Type NonNullable = Nullable.GetUnderlyingType(Type) ?? Type;
            return BuiltInBitwiseTypes.Contains(NonNullable) || HasOperator(NonNullable, "op_BitwiseOr");
        }

        /// <summary>
        /// Determines whether this type supports "^" operator.
        /// </summary>
        public static bool SupportsBitwiseXor(this Type Type)
        {
            Type NonNullable = Nullable.GetUnderlyingType(Type) ?? Type;
            return BuiltInBitwiseTypes.Contains(NonNullable) || HasOperator(NonNullable, "op_ExclusiveOr");
        }

        /// <summary>
        /// Determines whether this type supports "&amp;&amp;" operator.
        /// </summary>
        public static bool SupportsLogicalAnd(this Type Type)
            => Type == typeof(bool) ||
               (HasOperator(Type, "op_True") &&
                HasOperator(Type, "op_False") &&
                (HasOperator(Type, "op_BitwiseAnd") || HasOperator(Type, "op_And")));

        /// <summary>
        /// Determines whether this type supports "||" operator.
        /// </summary>
        public static bool SupportsLogicalOr(this Type Type)
            => Type == typeof(bool) ||
               (HasOperator(Type, "op_True") &&
                HasOperator(Type, "op_False") &&
                (HasOperator(Type, "op_BitwiseOr") || HasOperator(Type, "op_Or")));

        /// <summary>
        /// Determines whether this type supports "!" operator.
        /// </summary>
        public static bool SupportsLogicalNot(this Type Type)
            => Type == typeof(bool) || HasOperator(Type, "op_LogicalNot") || HasOperator(Type, "op_Not");

        /// <summary>
        /// Determines whether this type supports "==" operators.
        /// </summary>
        public static bool SupportsComparisonEquality(this Type Type)
            => Type == typeof(bool) || HasOperator(Type, "op_Equality");

        /// <summary>
        /// Determines whether this type supports "!=" operators.
        /// </summary>
        public static bool SupportsComparisonInequality(this Type Type)
            => Type == typeof(bool) || HasOperator(Type, "op_Inequality");

        /// <summary>
        /// Determines whether this type supports "&lt;" operator.
        /// </summary>
        public static bool SupportsComparisonLessThan(this Type Type)
            => HasOperator(Type, "op_LessThan");

        /// <summary>
        /// Determines whether this type supports "&lt;=" operator.
        /// </summary>
        public static bool SupportsComparisonLessThanOrEqual(this Type Type)
            => HasOperator(Type, "op_LessThanOrEqual");

        /// <summary>
        /// Determines whether this type supports "&gt;" operator.
        /// </summary>
        public static bool SupportsComparisonGreaterThan(this Type Type)
            => HasOperator(Type, "op_GreaterThan");

        /// <summary>
        /// Determines whether this type supports "&gt;=" operator.
        /// </summary>
        public static bool SupportsComparisonGreaterThanOrEqual(this Type Type)
            => HasOperator(Type, "op_GreaterThanOrEqual");

        /// <summary>
        /// Checks whether the specified operator exists.
        /// </summary>
        private static bool HasOperator(Type Type, string OperatorName)
            => TryGetMethod(Type, OperatorName, PublicStaticModifier, out _);

        #endregion

        #region NumberType
        internal static readonly Dictionary<Type, byte> NumberTypes = new()
        {
            { typeof(byte)    , 0  },
            { typeof(ushort)  , 1  },
            { typeof(uint)    , 2  },
            { typeof(ulong)   , 3  },

            { typeof(sbyte)   , 4  },
            { typeof(short)   , 5  },
            { typeof(int)     , 6  },
            { typeof(long)    , 7  },

            { typeof(float)   , 8  },
            { typeof(double)  , 9  },
            { typeof(decimal) , 10 }
        };
        private static readonly Dictionary<Type, sbyte> NumberTypeScores = new()
        {
            { typeof(sbyte)   , 0  },
            { typeof(byte)    , 1  },

            { typeof(short)   , 2  },
            { typeof(ushort)  , 3  },

            { typeof(int)     , 4  },
            { typeof(uint)    , 5  },

            { typeof(long)    , 6  },
            { typeof(ulong)   , 7  },

            { typeof(float)   , 8  },
            { typeof(double)  , 9  },
            { typeof(decimal) , 10 },
        };

#if NET7_0_OR_GREATER
        internal static readonly Type NumberType = typeof(INumber<>);
        internal static readonly Type FloatingPointType = typeof(IFloatingPoint<>);

        /// <summary>
        /// Determines whether the current type is <see cref="INumber&lt;&gt;"/>.
        /// </summary>
        public static bool IsNumberType(this Type This)
        {
            return This.GetInterfaces()
                       .Any(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == NumberType);
        }
        /// <summary>
        /// Determines whether the current type is <see cref="IFloatingPoint&lt;&gt;"/>.
        /// </summary>
        public static bool IsDecimalType(this Type This)
        {
            return This.GetInterfaces()
                       .Any(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == FloatingPointType);
        }

#else

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

#endif

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

        internal static readonly Dictionary<string, Type> TypeAlias = new()
        {
            { "bool"    , typeof(bool)    },
            { "byte"    , typeof(byte)    },
            { "char"    , typeof(char)    },
            { "decimal" , typeof(decimal) },
            { "double"  , typeof(double)  },
            { "float"   , typeof(float)   },
            { "int"     , typeof(int)     },
            { "long"    , typeof(long)    },
            { "object"  , typeof(object)  },
            { "sbyte"   , typeof(sbyte)   },
            { "short"   , typeof(short)   },
            { "string"  , typeof(string)  },
            { "uint"    , typeof(uint)    },
            { "ulong"   , typeof(ulong)   },
            { "void"    , typeof(void)    }
        };
        public static bool TryGetType(string Route, out Type Type)
            => TryGetType(Route, Type.EmptyTypes, out Type);
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
            => TryGetType(Name, Namespace, Type.EmptyTypes, out Type);
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

        /// <summary>
        /// Determines whether the specified type is a structure type.
        /// </summary>
        /// <param name="This">The specified type.</param>
        /// <param name="CheckGeneric">Determines whether to check together with generic parameters.</param>
        public static bool IsStruct(this Type This, bool CheckGeneric)
        {
            if (!This.IsValueType)
                return false;

            if (!This.IsGenericType)
                return true;

            // Generic Types
            if (CheckGeneric)
            {
                Type[] GenericTypes = This.GetGenericArguments();
                foreach (Type GenericType in GenericTypes)
                    if (!IsStruct(GenericType, true))
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Scores the matching of the types.
        /// </summary>
        /// <returns>The score of matching.<para/>
        /// If <paramref name="Types"/> are equal to <paramref name="DefinedTypes"/>, return 0.<para/>
        /// If <paramref name="Types"/> and <paramref name="DefinedTypes"/> are completely different, return <see cref="int.MinValue"/>.</returns>
        private static int ScoreMatchingParameters(Type[] Types, Type[] DefinedTypes)
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
                    if (Type is null)
                    {
                        if (DefinedType.IsValueType)
                            return int.MinValue;
                    }
                    else
                    {
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
            }

            return Score;
        }

        private static Action<Action> InvokeAction;
        public static void InvokeOnUIThread(Action Callback)
        {
            if (InvokeAction is null)
            {
                AssemblyName[] References = Assembly.GetEntryAssembly()?
                                                    .GetReferencedAssemblies()
                                                    .Where(AssemblyHelper.IsDotNetAssembly)
                                                    .ToArray();

                // WPF
                if (References?.FirstOrDefault(i => i.Name == "PresentationFramework") is AssemblyName Name)
                {
                    Assembly Assembly = Assembly.Load(Name);
                    if (Assembly.GetType("System.Windows.Application", false) is Type AppType &&
                        AppType.TryGetStaticPropertyValue("Current", out object Current) &&
                        TryGetPropertyValue(Current, "Dispatcher", out object Dispatcher) &&
                        TryGetMethod(Dispatcher.GetType(), "Invoke", [typeof(Action)], out MethodInfo Method))
                        InvokeAction = a => Method.Invoke(Dispatcher, [a]);
                }

                // Winform
                if (References?.FirstOrDefault(i => i.Name == "System.Windows.Forms") is AssemblyName WinformAssemblyName)
                {
                    Assembly Assembly = Assembly.Load(WinformAssemblyName);
                    if (Assembly.GetType("System.Windows.Forms.Application", false) is Type AppType &&
                        AppType.TryGetStaticPropertyValue("OpenForms", out IEnumerable Current) &&
                        Current.FirstOrNull() is object Form &&
                        TryGetMethod(Form.GetType(), "Invoke", [typeof(Action)], out MethodInfo Method))
                        InvokeAction = a => Method.Invoke(Form, [a]);
                }

                if (InvokeAction is null)
                    throw new EntryPointNotFoundException("Unable to locate a supported UI thread dispatcher.\r\n" +
                                                          "This method requires either a WPF (PresentationFramework) or WinForms (System.Windows.Forms) application context.");
            }

            InvokeAction.Invoke(Callback);
        }

        private static readonly ConcurrentDictionary<Type, Func<object[], object>> InvokeFuncs = [];
        public static T InvokeOnUIThread<T>(Func<T> Callback)
        {
            Type Key = typeof(T);
            if (!InvokeFuncs.TryGetValue(Key, out Func<object[], object> Func))
            {
                AssemblyName[] References = Assembly.GetEntryAssembly()?
                                                    .GetReferencedAssemblies()
                                                    .Where(AssemblyHelper.IsDotNetAssembly)
                                                    .ToArray();

                // WPF
                if (References?.FirstOrDefault(i => i.Name == "PresentationFramework") is AssemblyName WPFAssemblyName)
                {
                    Assembly Assembly = Assembly.Load(WPFAssemblyName);
                    if (Assembly.GetType("System.Windows.Application", false) is Type AppType &&
                        AppType.TryGetStaticPropertyValue("Current", out object Current) &&
                        TryGetPropertyValue(Current, "Dispatcher", out object Dispatcher) &&
                        TryGetMethod(Dispatcher.GetType(), "Invoke", [typeof(T)], [typeof(Func<T>)], out MethodInfo Method))
                        Func = Arg => Method.Invoke(Dispatcher, Arg);
                }

                // Winform
                else if (References?.FirstOrDefault(i => i.Name == "System.Windows.Forms") is AssemblyName WinformAssemblyName)
                {
                    Assembly Assembly = Assembly.Load(WinformAssemblyName);
                    if (Assembly.GetType("System.Windows.Forms.Application", false) is Type AppType &&
                        AppType.TryGetStaticPropertyValue("OpenForms", out IEnumerable Current) &&
                        Current.FirstOrNull() is object Form &&
                        TryGetMethod(Form.GetType(), "Invoke", [typeof(T)], [typeof(Func<T>)], out MethodInfo Method))
                        Func = Arg => Method.Invoke(Form, Arg);
                }

                if (Func is null)
                    throw new EntryPointNotFoundException("Unable to locate a supported UI thread dispatcher.\r\n" +
                                                          "This method requires either a WPF (PresentationFramework) or WinForms (System.Windows.Forms) application context.");

                InvokeFuncs.TryAdd(Key, Func);
            }

            return (T)Func.Invoke([Callback]);
        }

        private static Action<Action> BeginInvokeAction;
        public static void BeginInvokeOnUIThread(Action Callback)
        {
            if (BeginInvokeAction is null)
            {
                AssemblyName[] References = Assembly.GetEntryAssembly()?
                                                    .GetReferencedAssemblies()
                                                    .Where(AssemblyHelper.IsDotNetAssembly)
                                                    .ToArray();

                // WPF
                if (References?.FirstOrDefault(i => i.Name == "PresentationFramework") is AssemblyName WPFAssemblyName)
                {
                    Assembly Assembly = Assembly.Load(WPFAssemblyName);
                    if (Assembly.GetType("System.Windows.Application", false) is Type AppType &&
                        AppType.TryGetStaticPropertyValue("Current", out object Current) &&
                        TryGetPropertyValue(Current, "Dispatcher", out object Dispatcher) &&
                        TryGetMethod(Dispatcher.GetType(), "BeginInvoke", [typeof(Delegate), typeof(object[])], out MethodInfo Method))
                        BeginInvokeAction = a => Method.Invoke(Dispatcher, [a, null]);
                }

                // WinForms
                if (References?.FirstOrDefault(i => i.Name == "System.Windows.Forms") is AssemblyName WinformsAssemblyName)
                {
                    Assembly Assembly = Assembly.Load(WinformsAssemblyName);
                    if (Assembly.GetType("System.Windows.Forms.Application", false) is Type AppType &&
                        AppType.TryGetStaticPropertyValue("OpenForms", out IEnumerable Current) &&
                        Current.FirstOrNull() is object Form &&
                        TryGetMethod(Form.GetType(), "BeginInvoke", [typeof(Delegate), typeof(object[])], out MethodInfo Method))
                        BeginInvokeAction = a => Method.Invoke(Form, [a, null]);
                }

                if (BeginInvokeAction is null)
                    throw new EntryPointNotFoundException("Unable to locate a supported UI thread dispatcher for BeginInvoke.\r\n" +
                                                          "This method requires either a WPF (PresentationFramework) or WinForms (System.Windows.Forms) application context.");
            }

            BeginInvokeAction.Invoke(Callback);
        }

    }
}