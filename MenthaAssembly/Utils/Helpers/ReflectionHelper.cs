using System.Collections.Generic;

namespace System.Reflection
{
    public static class ReflectionHelper
    {
        public static readonly BindingFlags InternalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        public static bool TryGetInternalProperty<T>(string PropertyName, out PropertyInfo PropertyInfo)
            => TryGetInternalProperty(typeof(T), PropertyName, out PropertyInfo);
        public static bool TryGetInternalProperty(this Type This, string PropertyName, out PropertyInfo PropertyInfo)
        {
            Type BaseType = This;
            PropertyInfo Result = BaseType?.GetProperty(PropertyName, InternalFlags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    PropertyInfo = null;
                    return false;
                }

                Result = BaseType.GetProperty(PropertyName, InternalFlags);
            }
            PropertyInfo = Result;
            return true;
        }

        public static bool TryGetInternalField<T>(string FieldName, out FieldInfo PropertyInfo)
            => TryGetInternalField(typeof(T), FieldName, out PropertyInfo);
        public static bool TryGetInternalField(this Type This, string FieldName, out FieldInfo PropertyInfo)
        {
            Type BaseType = This;
            FieldInfo Result = BaseType?.GetField(FieldName, InternalFlags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    PropertyInfo = null;
                    return false;
                }

                Result = BaseType.GetField(FieldName, InternalFlags);
            }
            PropertyInfo = Result;
            return true;
        }

        public static bool TryGetInternalMethod<T>(string MethodName, out MethodInfo MethodInfo)
            => TryGetInternalMethod(typeof(T), MethodName, out MethodInfo);
        public static bool TryGetInternalMethod(this Type This, string MethodName, out MethodInfo MethodInfo)
        {
            Type BaseType = This;
            MethodInfo Result = BaseType?.GetMethod(MethodName, InternalFlags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    MethodInfo = null;
                    return false;
                }

                Result = BaseType.GetMethod(MethodName, InternalFlags);
            }

            MethodInfo = Result;
            return true;
        }

        public static bool TryGetEventField<T>(string EventName, out MulticastDelegate Delegates)
            => TryGetEventField(typeof(T), EventName, out Delegates);
        public static bool TryGetEventField(this Type This, string EventName, out MulticastDelegate Delegates)
        {
            Type BaseType = This;
            FieldInfo EventField = BaseType?.GetField(EventName, InternalFlags);
            while (EventField is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                {
                    Delegates = null;
                    return false;
                }

                EventField = BaseType.GetField(EventName, InternalFlags);
            }

            if (!(EventField.GetValue(This) is MulticastDelegate Handler))
            {
                Delegates = null;
                return false;
            }

            Delegates = Handler;
            return true;
        }

        public static IEnumerable<PropertyInfo> GetProperties(this Type This, params string[] PropertyNames)
        {
            foreach (string Name in PropertyNames)
                if (This.GetProperty(Name) is PropertyInfo Info)
                    yield return Info;
        }

        public static bool TrySetInternalPropertyValue<T>(this object This, string PropertyName, T Value)
        {
            if (TryGetInternalProperty(This?.GetType(), PropertyName, out PropertyInfo Info) &&
                typeof(T).IsBaseOn(Info.PropertyType))
            {
                Info.SetValue(This, Value);
                return true;
            }
            return false;
        }
        public static bool TryGetInternalPropertyValue<T>(this object This, string PropertyName, out T Value)
        {
            if (TryGetInternalProperty(This?.GetType(), PropertyName, out PropertyInfo Info) &&
                Info.PropertyType.IsBaseOn<T>())
            {
                Value = (T)Info.GetValue(This);
                return true;
            }

            Value = default;
            return false;
        }

        public static bool TrySetInternalFieldValue<T>(this object This, string FieldName, T Value)
        {
            if (TryGetInternalField(This?.GetType(), FieldName, out FieldInfo Info) &&
                typeof(T).IsBaseOn(Info.FieldType))
            {
                Info.SetValue(This, Value);
                return true;
            }
            return false;
        }
        public static bool TryGetInternalFieldValue<T>(this object This, string FieldName, out T Value)
        {
            if (TryGetInternalField(This?.GetType(), FieldName, out FieldInfo Info) &&
                Info.FieldType.IsBaseOn<T>())
            {
                Value = (T)Info.GetValue(This);
                return true;
            }

            Value = default;
            return false;
        }

        public static bool TryInvokeInternalMethod(this object This, string MethodName, params object[] Args)
        {
            if (TryGetInternalMethod(This?.GetType(), MethodName, out MethodInfo Method))
            {
                Method.Invoke(This, Args);
                return true;
            }

            return false;
        }
        public static bool TryInvokeInternalMethod<T>(this object This, string MethodName, out T Result, params object[] Args)
        {
            if (TryGetInternalMethod(This?.GetType(), MethodName, out MethodInfo Method) &&
                Method.ReturnType.IsBaseOn<T>())
            {
                Result = (T)Method.Invoke(This, Args);
                return true;
            }

            Result = default;
            return false;
        }

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

        public static void RaiseEvent(this object This, string EventName, params object[] Args)
        {
            if (!TryGetEventField(This?.GetType(), EventName, out MulticastDelegate Handler))
                return;

            foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                InvocationMethod.DynamicInvoke(new[] { This, Args });
        }

    }
}
