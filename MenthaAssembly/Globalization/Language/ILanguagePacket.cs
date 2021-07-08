using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MenthaAssembly.Globalization
{
    public abstract class ILanguagePacket : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected PropertyInfo[] _CachePropertyInfos;

        public string LanguageName { internal protected set; get; } = "Default";

        public virtual string this[string Name]
        {
            get
            {
                if (_CachePropertyInfos is null)
                    _CachePropertyInfos = this.GetType()
                                              .GetProperties()
                                              .Where(i => !i.Name.Equals(nameof(LanguageName)) && !i.Name.Equals("Item"))
                                              .ToArray();

                if (_CachePropertyInfos.FirstOrDefault(i => i.Name.Equals(Name)) is PropertyInfo Info)
                    return Info.GetValue(this).ToString();

                throw new KeyNotFoundException($"[{this.GetType().Name}]Not fount {Name}.");
            }
            internal set
            {
                if (_CachePropertyInfos is null)
                    _CachePropertyInfos = this.GetType()
                                              .GetProperties()
                                              .Where(i => !i.Name.Equals(nameof(LanguageName)) && !i.Name.Equals("Item"))
                                              .ToArray();

                if (_CachePropertyInfos.FirstOrDefault(i => i.Name.Equals(Name)) is PropertyInfo Info)
                    Info.SetValue(this, value);
            }
        }

        internal protected virtual IEnumerable<string> GetPropertyNames()
        {
            if (_CachePropertyInfos is null)
                _CachePropertyInfos = this.GetType()
                                          .GetProperties()
                                          .Where(i => !i.Name.Equals(nameof(LanguageName)) && !i.Name.Equals("Item"))
                                          .ToArray();

            return _CachePropertyInfos.Select(i => i.Name);
        }

        internal protected virtual void OnPropertyChanged()
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

    }
}
