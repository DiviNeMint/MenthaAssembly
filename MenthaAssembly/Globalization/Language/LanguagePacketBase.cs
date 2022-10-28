using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MenthaAssembly.Globalization
{
    public abstract class LanguagePacketBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Dictionary<string, Func<string>> GetCaches;
        protected Dictionary<string, Action<string>> SetCaches;

        public string LanguageName { internal protected set; get; } = "Default";

        public virtual string this[string Name]
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return null;

                if (GetCaches is null)
                    CreateCache();

                if (GetCaches.TryGetValue(Name, out Func<string> GetValue))
                    return GetValue();

                Debug.WriteLine($"[{GetType().Name}]Not fount {Name}.");
                return null;
            }
            internal set
            {
                if (SetCaches is null)
                    CreateCache();

                if (SetCaches.TryGetValue(Name, out Action<string> SetValue))
                    SetValue(value);
            }
        }

        protected internal virtual IEnumerable<string> GetPropertyNames()
        {
            if (GetCaches is null)
                CreateCache();

            return GetCaches.Keys;
        }

        protected void CreateCache()
        {
            GetCaches = new Dictionary<string, Func<string>>();
            SetCaches = new Dictionary<string, Action<string>>();

            ConstantExpression This = Expression.Constant(this);
            foreach (PropertyInfo Info in GetType().GetProperties()
                                                   .Where(i => !i.Name.Equals(nameof(LanguageName)) && !i.Name.Equals("Item")))
            {
                MemberExpression Property = Expression.Property(This, Info);
                ParameterExpression Parameter = Expression.Parameter(typeof(string), "Value");
                BinaryExpression Assign = Expression.Assign(Property, Parameter);

                GetCaches.Add(Info.Name, Expression.Lambda<Func<string>>(Property).Compile());
                SetCaches.Add(Info.Name, Expression.Lambda<Action<string>>(Assign, Parameter).Compile());
            }
        }

        protected internal virtual void OnPropertyChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

    }
}