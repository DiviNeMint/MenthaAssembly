using System.ComponentModel;
using System.Reflection;

namespace MenthaAssembly.Globalization
{
    public abstract class ILanguagePacket : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string LanguageName { internal protected set; get; } = "Default";

        public string this[string Name]
            => GetValue(Name);

        protected string GetValue(string Name)
        {
            if (this.GetType().GetProperty(Name) is PropertyInfo Info)
                return Info.GetValue(this).ToString();
            return null;
        }

        internal protected void OnPropertyChanged()
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

    }
}
