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
        public static IEnumerable<PropertyInfo> GetProperties(this Type This, params string[] PropertyNames)
        {
            foreach (string Name in PropertyNames)
                if (This.GetProperty(Name) is PropertyInfo Info)
                    yield return Info;
        }

        public static bool TryGetProperty<T>(string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(typeof(T), PublicFlags, PropertyName, out PropertyInfo);
        public static bool TryGetProperty(this Type This, string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(This, PublicFlags, PropertyName, out PropertyInfo);

        public static bool TryGetStaticProperty<T>(string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(typeof(T), StaticFlags, PropertyName, out PropertyInfo);
        public static bool TryGetStaticProperty(this Type This, string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(This, StaticFlags, PropertyName, out PropertyInfo);

        public static bool TryGetInternalProperty<T>(string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(typeof(T), InternalFlags, PropertyName, out PropertyInfo);
        public static bool TryGetInternalProperty(this Type This, string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(This, InternalFlags, PropertyName, out PropertyInfo);

        public static bool TryGetStaticInternalProperty<T>(string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(typeof(T), StaticInternalFlags, PropertyName, out PropertyInfo);
        public static bool TryGetStaticInternalProperty(this Type This, string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(This, StaticInternalFlags, PropertyName, out PropertyInfo);

        public static bool TryGetProperty<T>(BindingFlags Flags, string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(typeof(T), Flags, PropertyName, out PropertyInfo);
        public static bool TryGetProperty(this Type This, BindingFlags Flags, string PropertyName, out PropertyInfo PropertyInfo)
        {
            Type BaseType = This;
            PropertyInfo Result = BaseType?.GetProperty(PropertyName, Flags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    PropertyInfo = null;
                    return false;
                }

                Result = BaseType.GetProperty(PropertyName, Flags);
            }
            PropertyInfo = Result;
            return true;
        }

        public static bool TrySetPropertyValue<T>(this object This, string PropertyName, T Value)
            => TrySetPropertyValue(This, PublicFlags, PropertyName, Value);
        public static bool TryGetPropertyValue<T>(this object This, string PropertyName, out T Value)
            => TryGetPropertyValue(This, PublicFlags, PropertyName, out Value);

        public static bool TrySetStaticPropertyValue<T>(this object This, string PropertyName, T Value)
            => TrySetPropertyValue(This, StaticFlags, PropertyName, Value);
        public static bool TryGetStaticPropertyValue<T>(this object This, string PropertyName, out T Value)
            => TryGetPropertyValue(This, StaticFlags, PropertyName, out Value);

        public static bool TrySetInternalPropertyValue<T>(this object This, string PropertyName, T Value)
            => TrySetPropertyValue(This, InternalFlags, PropertyName, Value);
        public static bool TryGetInternalPropertyValue<T>(this object This, string PropertyName, out T Value)
            => TryGetPropertyValue(This, InternalFlags, PropertyName, out Value);

        public static bool TrySetStaticInternalPropertyValue<T>(this object This, string PropertyName, T Value)
            => TrySetPropertyValue(This, StaticInternalFlags, PropertyName, Value);
        public static bool TryGetStaticInternalPropertyValue<T>(this object This, string PropertyName, out T Value)
            => TryGetPropertyValue(This, StaticInternalFlags, PropertyName, out Value);

        public static bool TrySetPropertyValue<T>(this object This, BindingFlags Flags, string PropertyName, T Value)
        {
            if (TryGetProperty(This?.GetType(), Flags, PropertyName, out PropertyInfo Info) &&
                typeof(T).IsBaseOn(Info.PropertyType))
            {
                Info.SetValue(This, Value);
                return true;
            }
            return false;
        }
        public static bool TryGetPropertyValue<T>(this object This, BindingFlags Flags, string PropertyName, out T Value)
        {
            if (TryGetProperty(This?.GetType(), Flags, PropertyName, out PropertyInfo Info) &&
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
        public static bool TryGetField<T>(string FieldName, out FieldInfo FieldInfo)
            => TryGetField(typeof(T), PublicFlags, FieldName, out FieldInfo);
        public static bool TryGetField(this Type This, string FieldName, out FieldInfo FieldInfo)
            => TryGetField(This, PublicFlags, FieldName, out FieldInfo);

        public static bool TryGetStaticField<T>(string FieldName, out FieldInfo FieldInfo)
            => TryGetField(typeof(T), StaticFlags, FieldName, out FieldInfo);
        public static bool TryGetStaticField(this Type This, string FieldName, out FieldInfo FieldInfo)
            => TryGetField(This, StaticFlags, FieldName, out FieldInfo);

        public static bool TryGetInternalField<T>(string FieldName, out FieldInfo FieldInfo)
            => TryGetField(typeof(T), InternalFlags, FieldName, out FieldInfo);
        public static bool TryGetInternalField(this Type This, string FieldName, out FieldInfo FieldInfo)
            => TryGetField(This, InternalFlags, FieldName, out FieldInfo);

        public static bool TryGetStaticInternalField<T>(string FieldName, out FieldInfo FieldInfo)
            => TryGetField(typeof(T), StaticInternalFlags, FieldName, out FieldInfo);
        public static bool TryGetStaticInternalField(this Type This, string FieldName, out FieldInfo FieldInfo)
            => TryGetField(This, StaticInternalFlags, FieldName, out FieldInfo);

        public static bool TryGetField<T>(BindingFlags Flags, string FieldName, out FieldInfo FieldInfo)
            => TryGetField(typeof(T), Flags, FieldName, out FieldInfo);
        public static bool TryGetField(this Type This, BindingFlags Flags, string FieldName, out FieldInfo FieldInfo)
        {
            Type BaseType = This;
            FieldInfo Result = BaseType?.GetField(FieldName, Flags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    FieldInfo = null;
                    return false;
                }

                Result = BaseType.GetField(FieldName, Flags);
            }
            FieldInfo = Result;
            return true;
        }

        public static bool TrySetFieldValue<T>(this object This, string FieldName, T Value)
            => TrySetFieldValue(This, PublicFlags, FieldName, Value);
        public static bool TryGetFieldValue<T>(this object This, string FieldName, out T Value)
            => TryGetFieldValue(This, PublicFlags, FieldName, out Value);

        public static bool TrySetStaticFieldValue<T>(this object This, string FieldName, T Value)
            => TrySetFieldValue(This, StaticFlags, FieldName, Value);
        public static bool TryGetStaticFieldValue<T>(this object This, string FieldName, out T Value)
            => TryGetFieldValue(This, StaticFlags, FieldName, out Value);

        public static bool TrySetInternalFieldValue<T>(this object This, string FieldName, T Value)
            => TrySetFieldValue(This, InternalFlags, FieldName, Value);
        public static bool TryGetInternalFieldValue<T>(this object This, string FieldName, out T Value)
            => TryGetFieldValue(This, InternalFlags, FieldName, out Value);

        public static bool TrySetStaticInternalFieldValue<T>(this object This, string FieldName, T Value)
            => TrySetFieldValue(This, StaticInternalFlags, FieldName, Value);
        public static bool TryGetStaticInternalFieldValue<T>(this object This, string FieldName, out T Value)
            => TryGetFieldValue(This, StaticInternalFlags, FieldName, out Value);

        public static bool TrySetFieldValue<T>(this object This, BindingFlags Flags, string FieldName, T Value)
        {
            if (TryGetField(This?.GetType(), Flags, FieldName, out FieldInfo Info) &&
                typeof(T).IsBaseOn(Info.FieldType))
            {
                Info.SetValue(This, Value);
                return true;
            }
            return false;
        }
        public static bool TryGetFieldValue<T>(this object This, BindingFlags Flags, string FieldName, out T Value)
        {
            if (TryGetField(This?.GetType(), Flags, FieldName, out FieldInfo Info) &&
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
        public static bool TryGetConstant<T>(this Type This, string ConstantName, out T Value)
        {
            if (TryGetField(This, StaticFlags, ConstantName, out FieldInfo Field))
            {
                Value = (T)Field.GetValue(null);
                return true;
            }

            Value = default;
            return false;
        }

        #endregion

        #region Method
        public static bool TryGetMethod<T>(string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), PublicFlags, MethodName, out MethodInfo);
        public static bool TryGetMethod<T>(string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), PublicFlags, MethodName, ParameterTypes, out MethodInfo);
        public static bool TryGetMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(This, PublicFlags, MethodName, out MethodInfo);
        public static bool TryGetMethod(this Type This, string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(This, PublicFlags, MethodName, ParameterTypes, out MethodInfo);

        public static bool TryGetStaticMethod<T>(string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), StaticFlags, MethodName, out MethodInfo);
        public static bool TryGetStaticMethod<T>(string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), StaticFlags, MethodName, ParameterTypes, out MethodInfo);
        public static bool TryGetStaticMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(This, StaticFlags, MethodName, out MethodInfo);
        public static bool TryGetStaticMethod(this Type This, string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(This, StaticFlags, MethodName, ParameterTypes, out MethodInfo);

        public static bool TryGetInternalMethod<T>(string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), InternalFlags, MethodName, out MethodInfo);
        public static bool TryGetInternalMethod<T>(string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), InternalFlags, MethodName, ParameterTypes, out MethodInfo);
        public static bool TryGetInternalMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(This, InternalFlags, MethodName, out MethodInfo);
        public static bool TryGetInternalMethod(this Type This, string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(This, InternalFlags, MethodName, ParameterTypes, out MethodInfo);

        public static bool TryGetStaticInternalMethod<T>(string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), StaticInternalFlags, MethodName, out MethodInfo);
        public static bool TryGetStaticInternalMethod<T>(string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), StaticInternalFlags, MethodName, ParameterTypes, out MethodInfo);
        public static bool TryGetStaticInternalMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(This, StaticInternalFlags, MethodName, out MethodInfo);
        public static bool TryGetStaticInternalMethod(this Type This, string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(This, StaticInternalFlags, MethodName, ParameterTypes, out MethodInfo);

        public static bool TryGetMethod<T>(BindingFlags Flags, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), Flags, MethodName, out MethodInfo);
        public static bool TryGetMethod<T>(BindingFlags Flags, string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), Flags, MethodName, ParameterTypes, out MethodInfo);
        public static bool TryGetMethod(this Type This, BindingFlags Flags, string MethodName, out MethodInfo MethodInfo)
        {
            Type BaseType = This;
            MethodInfo Result = BaseType?.GetMethod(MethodName, Flags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    MethodInfo = null;
                    return false;
                }

                Result = BaseType.GetMethod(MethodName, Flags);
            }
            MethodInfo = Result;
            return true;
        }
        public static bool TryGetMethod(this Type This, BindingFlags Flags, string MethodName, Type[] ParameterTypes, out MethodInfo MethodInfo)
        {
            Type BaseType = This;
            MethodInfo Result = BaseType?.GetMethod(MethodName, Flags, null, ParameterTypes, null);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    MethodInfo = null;
                    return false;
                }

                Result = BaseType.GetMethod(MethodName, Flags, null, ParameterTypes, null);
            }
            MethodInfo = Result;
            return true;
        }

        public static bool TryInvokeMethod(this object This, string MethodName, params object[] Args)
            => TryInvokeMethod(This, PublicFlags, MethodName, Args);
        public static bool TryInvokeMethod<T>(this object This, string MethodName, out T Result, params object[] Args)
            => TryInvokeMethod(This, PublicFlags, MethodName, out Result, Args);

        public static bool TryInvokeInternalMethod(this object This, string MethodName, params object[] Args)
            => TryInvokeMethod(This, InternalFlags, MethodName, Args);
        public static bool TryInvokeInternalMethod<T>(this object This, string MethodName, out T Result, params object[] Args)
            => TryInvokeMethod(This, InternalFlags, MethodName, out Result, Args);

        public static bool TryInvokeMethod(this object This, BindingFlags Flags, string MethodName, params object[] Args)
        {
            if (TryGetMethod(This?.GetType(), Flags, MethodName, Args.Select(i => i.GetType()).ToArray(), out MethodInfo Method))
            {
                Method.Invoke(This, Args);
                return true;
            }

            return false;
        }
        public static bool TryInvokeMethod<T>(this object This, BindingFlags Flags, string MethodName, out T Result, params object[] Args)
        {
            if (TryGetMethod(This?.GetType(), Flags, MethodName, Args.Select(i => i.GetType()).ToArray(), out MethodInfo Method) &&
                Method.ReturnType.IsBaseOn<T>())
            {
                Result = (T)Method.Invoke(This, Args);
                return true;
            }

            Result = default;
            return false;
        }

        #endregion

        #region Event
        public static bool TryGetEventField(this object This, string EventName, out MulticastDelegate Delegates)
            => TryGetInternalFieldValue(This, EventName, out Delegates) && Delegates != null;

        public static bool TryGetStaticEventField(this object This, string EventName, out MulticastDelegate Delegates)
            => TryGetStaticInternalFieldValue(This, EventName, out Delegates) && Delegates != null;

        public static void RaiseEvent(this object This, string EventName, params object[] Args)
        {
            if (TryGetEventField(This, EventName, out MulticastDelegate Handler))
                foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                    InvocationMethod.DynamicInvoke(new[] { This, Args });
        }

        public static void RaiseStaticEvent(this object This, string EventName, params object[] Args)
        {
            if (TryGetStaticEventField(This, EventName, out MulticastDelegate Handler))
                foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                    InvocationMethod.DynamicInvoke(new[] { This, Args });
        }

        #endregion

        public static bool IsBaseOn<T>(this Type This)
            => IsBaseOn(This, typeof(T));
        public static bool IsBaseOn(this Type This, Type BaseOn)
        {
            Type BaseType = This;
            while (BaseType != BaseOn)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                    return false;
            }
            return true;
        }

        public static bool IsDecimalType(this Type This)
            => This.Name.Equals(nameof(Double)) ||
               This.Name.Equals(nameof(Single)) ||
               This.Name.Equals(nameof(Decimal));
        public static bool IsIntegerType(this Type This)
            => This.Name.Equals(nameof(SByte)) ||
               This.Name.Equals(nameof(Int16)) ||
               This.Name.Equals(nameof(Int32)) ||
               This.Name.Equals(nameof(Int64));
        public static bool IsPositiveIntegerType(this Type This)
            => This.Name.Equals(nameof(Byte)) ||
               This.Name.Equals(nameof(UInt16)) ||
               This.Name.Equals(nameof(UInt32)) ||
               This.Name.Equals(nameof(UInt64));

    }
}
