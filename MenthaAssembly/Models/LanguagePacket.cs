using MenthaAssembly.Interfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MenthaAssembly.Models
{
    public class LanguagePacket : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                this.OnPropertyChanged(nameof(Name));
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        Data = null;
                        return;
                    }

                    if (Data is null)
                    {
                        if (!(AppDomain.CurrentDomain.GetAssemblies()
                                                      .SelectMany(i => i.GetTypes())
                                                      .FirstOrDefault(i => i.IsClass && !i.IsAbstract && i.GetInterface(nameof(ILanguageData)) != null) is Type DataType))
                            return;
                        Data = (ILanguageData)Activator.CreateInstance(DataType);
                    }
                    Data.Load(value);
                    Data.OnPropertyChanged("");
                }
                catch
                {
                    Data = null;
                }
            }
        }

        private ILanguageData _Data;
        public ILanguageData Data
        {
            get => _Data;
            protected set
            {
                _Data = value;
                this.OnPropertyChanged(nameof(Data));
            }
        }

        public string this[string Name] => GetValue(Name);

        public string GetValue(string Name)
        {
            if (Data.GetType().GetProperty(Name) is PropertyInfo Info)
                return Info.GetValue(Data).ToString();
            return string.Empty;
        }

        public void Export(string DirectoryPath)
            => Data?.Export(Name, DirectoryPath);


        public void SetLanguageData(string Name, ILanguageData Data)
        {
            _Name = Name;
            _Data = Data;
            OnPropertyChanged();
        }

        public void OnPropertyChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));

    }

}
