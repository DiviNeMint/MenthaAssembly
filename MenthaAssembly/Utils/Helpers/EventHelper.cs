using System;
using System.Reflection;

namespace MenthaAssembly
{
    public static class EventHelper
    {
        public static readonly BindingFlags EventFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        public static MulticastDelegate GetEventField(this object This, string EventName)
        {
            Type BaseType = This.GetType();
            FieldInfo EventField = BaseType?.GetField(EventName, EventFlags);
            while (EventField is null)
            {
                BaseType = BaseType?.BaseType;
                if (BaseType is null)
                    return null;

                EventField = BaseType.GetField(EventName, EventFlags);
            }

            if (!(EventField.GetValue(This) is MulticastDelegate Handler))
                return null;

            return Handler;
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
