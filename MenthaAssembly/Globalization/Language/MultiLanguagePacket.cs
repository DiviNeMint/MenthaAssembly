using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MenthaAssembly.Globalization
{
    public class MultiLanguagePacket : LanguagePacketBase
    {
        public event EventHandler<IEnumerable<LanguagePacketBase>> PacketAdded;
        public event EventHandler<IEnumerable<LanguagePacketBase>> PacketRemoved;

        private readonly ObservableCollection<LanguagePacketBase> _Packets;
        public IReadOnlyList<LanguagePacketBase> Packets => _Packets;

        public override string this[string Name]
        {
            get
            {
                if (Packets.FirstOrDefault(i => i.GetPropertyNames().Contains(Name)) is LanguagePacketBase Packet)
                    return Packet[Name];

                Debug.WriteLine($"[LanguagePacket]Not fount {Name}.");
                return null;
            }
            protected internal set
            {
                if (Packets.FirstOrDefault(i => i.GetPropertyNames().Contains(Name)) is LanguagePacketBase Packet)
                    Packet[Name] = value;
            }
        }

        public MultiLanguagePacket()
        {
            _Packets = new ObservableCollection<LanguagePacketBase>();
        }
        public MultiLanguagePacket(IEnumerable<LanguagePacketBase> Packets)
        {
            _Packets = new ObservableCollection<LanguagePacketBase>(Packets);
        }

        public void Add(LanguagePacketBase Packet)
        {
            AddHander(Packet);

            PacketAdded?.Invoke(this, new LanguagePacketBase[] { Packet });
            OnPropertyChanged();
        }
        public void Add(IEnumerable<LanguagePacketBase> Packets)
        {
            foreach (LanguagePacketBase p in Packets)
                AddHander(p);

            PacketAdded?.Invoke(this, Packets);
            OnPropertyChanged();
        }
        private void AddHander(LanguagePacketBase Packet)
        {
            Type t = Packet.GetType();
            int Index = _Packets.IndexOf(i => t.Equals(i.GetType()));
            if (Index == -1)
                _Packets.Add(Packet);
            else
                _Packets[Index] = Packet;
        }

        public void Remove(LanguagePacketBase Packet)
        {
            _Packets.Remove(Packet);

            PacketRemoved?.Invoke(this, new LanguagePacketBase[] { Packet });
            OnPropertyChanged();
        }

        protected internal override IEnumerable<string> GetPropertyNames()
        {
            foreach (LanguagePacketBase Packet in Packets)
                foreach (string Name in Packet.GetPropertyNames())
                    yield return Name;
        }

    }
}