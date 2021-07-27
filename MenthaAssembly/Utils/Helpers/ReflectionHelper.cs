using System.Collections.Generic;

namespace System.Reflection
{
    public static class ReflectionHelper
    {
        private static readonly BindingFlags InternalFlags = BindingFlags.Instance | BindingFlags.NonPublic,
                                             PublicFlags = BindingFlags.Instance | BindingFlags.Public,
                                             ConstFlags = BindingFlags.Public | BindingFlags.Static;

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

        public static bool TryGetInternalProperty<T>(string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(typeof(T), InternalFlags, PropertyName, out PropertyInfo);
        public static bool TryGetInternalProperty(this Type This, string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetProperty(This, InternalFlags, PropertyName, out PropertyInfo);

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

        public static bool TrySetInternalPropertyValue<T>(this object This, string PropertyName, T Value)
            => TrySetPropertyValue(This, InternalFlags, PropertyName, Value);
        public static bool TryGetInternalPropertyValue<T>(this object This, string PropertyName, out T Value)
            => TryGetPropertyValue(This, InternalFlags, PropertyName, out Value);

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

        public static bool TryGetInternalField<T>(string FieldName, out FieldInfo FieldInfo)
            => TryGetField(typeof(T), InternalFlags, FieldName, out FieldInfo);
        public static bool TryGetInternalField(this Type This, string FieldName, out FieldInfo FieldInfo)
            => TryGetField(This, InternalFlags, FieldName, out FieldInfo);

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

        public static bool TrySetInternalFieldValue<T>(this object This, string FieldName, T Value)
            => TrySetFieldValue(This, InternalFlags, FieldName, Value);
        public static bool TryGetInternalFieldValue<T>(this object This, string FieldName, out T Value)
            => TryGetFieldValue(This, InternalFlags, FieldName, out Value);

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
            if (TryGetField(This, ConstFlags, ConstantName, out FieldInfo Field))
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
        public static bool TryGetMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(This, PublicFlags, MethodName, out MethodInfo);

        public static bool TryGetInternalMethod<T>(string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), InternalFlags, MethodName, out MethodInfo);
        public static bool TryGetInternalMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(This, InternalFlags, MethodName, out MethodInfo);

        public static bool TryGetMethod<T>(BindingFlags Flags, string MethodName, out MethodInfo MethodInfo)
            => TryGetMethod(typeof(T), Flags, MethodName, out MethodInfo);
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
            if (TryGetMethod(This?.GetType(), Flags, MethodName, out MethodInfo Method))
            {
                Method.Invoke(This, Args);
                return true;
            }

            return false;
        }
        public static bool TryInvokeMethod<T>(this object This, BindingFlags Flags, string MethodName, out T Result, params object[] Args)
        {
            if (TryGetMethod(This?.GetType(), Flags, MethodName, out MethodInfo Method) &&
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
            => TryGetInternalFieldValue(This, EventName, out Delegates) &&
               Delegates != null;

        public static void RaiseEvent(this object This, string EventName, params object[] Args)
        {
            if (TryGetEventField(This, EventName, out MulticastDelegate Handler))
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

    }
}
