using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;

namespace MenthaAssembly
{

    public static class ConcurrentHelper
    {
        public static void OnPropertyChanged(this INotifyPropertyChanged This, string PropertyName)
        {
            if (!(This.GetEventField("PropertyChanged") is MulticastDelegate Handler))
                return;

            Delegate[] Invocations = Handler.GetInvocationList();
            if (Invocations.Length > 0)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(PropertyName);
                foreach (PropertyChangedEventHandler Event in Invocations)
                {
                    if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                    {
                        // Invoke handler in the target dispatcher's thread
                        DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                        continue;
                    }
                    Event(This, e);
                }
            }
        }

        public static void OnPropertyChanged(this INotifyPropertyChanged This, PropertyChangedEventArgs e)
        {
            if (!(This.GetEventField("PropertyChanged") is MulticastDelegate Handler))
                return;

            foreach (PropertyChangedEventHandler Event in Handler.GetInvocationList())
            {
                if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                {
                    // Invoke handler in the target dispatcher's thread
                    DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                    continue;
                }
                Event(This, e);
            }
        }


        public static void OnCollectionChanged(this INotifyCollectionChanged This, NotifyCollectionChangedEventArgs e)
        {
            if (!(This.GetEventField("CollectionChanged") is MulticastDelegate Handler))
                return;

            foreach (NotifyCollectionChangedEventHandler Event in Handler.GetInvocationList())
            {
                if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                {
                    //Invoke handler in the target dispatcher's thread
                    DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                    continue;
                }
                Event(This, e);
            }
        }

    }
}
