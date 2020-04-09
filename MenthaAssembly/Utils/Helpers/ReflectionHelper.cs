using System.Collections.Generic;

namespace System.Reflection
{
    public static class ReflectionHelper
    {
        public static readonly BindingFlags InternalFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        public static PropertyInfo GetInternalPropertyInfo(this object This, string PropertyName)
        {
            Type BaseType = This.GetType();
            PropertyInfo Result = BaseType?.GetProperty(PropertyName, InternalFlags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                    return null;

                Result = BaseType.GetProperty(PropertyName, InternalFlags);
            }

            return Result;
        }

        public static MethodInfo GetInternalMethodInfo(this object This, string MethodName)
        {
            Type BaseType = This.GetType();
            MethodInfo Result = BaseType?.GetMethod(MethodName, InternalFlags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                    return null;

                Result = BaseType.GetMethod(MethodName, InternalFlags);
            }

            return Result;
        }

        public static MulticastDelegate GetEventField(this object This, string EventName)
        {
            Type BaseType = This.GetType();
            FieldInfo EventField = BaseType?.GetField(EventName, InternalFlags);
            while (EventField is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                    return null;

                EventField = BaseType.GetField(EventName, InternalFlags);
            }

            if (!(EventField.GetValue(This) is MulticastDelegate Handler))
                return null;

            return Handler;
        }

        public static IEnumerable<PropertyInfo> GetProperties(this Type This, params string[] PropertyNames)
        {
            foreach (string Name in PropertyNames)
                if (This.GetProperty(Name) is PropertyInfo Info)
                    yield return Info;
        }

        public static T GetInternalValue<T>(this object This, string PropertyName)
        {
            Type BaseType = This.GetType();
            PropertyInfo Result = BaseType?.GetProperty(PropertyName, InternalFlags);
            while (Result is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                    return default;

                Result = BaseType.GetProperty(PropertyName, InternalFlags);
            }

            return Result.PropertyType.Equals(typeof(T)) ? (T)Result.GetValue(This) : default;
        }

        public static void InvokeInternalMethod(this object This, string MethodName, params object[] Args)
        {
            if (This.GetInternalMethodInfo(MethodName) is MethodInfo Method)
                Method.Invoke(This, Args);
        }

        public static void RaiseEvent(this object This, string EventName, params object[] Args)
        {
            if (!(This.GetEventField(EventName) is MulticastDelegate Handler))
                return;

            foreach (Delegate InvocationMethod in Handler.GetInvocationList())
                InvocationMethod.DynamicInvoke(new[] { This, Args });
        }

    }
}
