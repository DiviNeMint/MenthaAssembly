using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MenthaAssembly.Globalization
{
    public class MultiLanguagePacket : ILanguagePacket
    {
        public IList<ILanguagePacket> Packets { get; }

        public MultiLanguagePacket() : this(new ILanguagePacket[0])
        {
        }
        public MultiLanguagePacket(IEnumerable<ILanguagePacket> Packets)
        {
            ObservableCollection<ILanguagePacket> TempPackets = new ObservableCollection<ILanguagePacket>(Packets);
            TempPackets.CollectionChanged += (s, e) => this.OnPropertyChanged();
            this.Packets = TempPackets;
        }

        public override string this[string Name]
        {
            get
            {
                if (Packets.FirstOrDefault(i => i.GetPropertyNames().Contains(Name)) is ILanguagePacket Packet)
                    return Packet[Name];

                Debug.WriteLine($"[LanguagePacket]Not fount {Name}.");
                return null;
            }

            internal set
            {
                if (Packets.FirstOrDefault(i => i.GetPropertyNames().Contains(Name)) is ILanguagePacket Packet)
                    Packet[Name] = value;
            }
        }

        protected internal override IEnumerable<string> GetPropertyNames()
        {
            foreach (ILanguagePacket Packet in Packets)
                foreach (string Name in Packet.GetPropertyNames())
                    yield return Name;
        }


    }
}
