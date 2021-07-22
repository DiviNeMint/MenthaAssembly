using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MenthaAssembly.Globalization
{
    public class MultiLanguagePacket : ILanguagePacket
    {
        public event EventHandler<IEnumerable<ILanguagePacket>> PacketAdded;

        private readonly ObservableCollection<ILanguagePacket> _Packets;
        public IReadOnlyList<ILanguagePacket> Packets => _Packets;

        internal bool NotifyCollectionChanged = true;

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

        public MultiLanguagePacket() : this(null)
        {
        }
        public MultiLanguagePacket(IEnumerable<ILanguagePacket> Packets)
        {
            ObservableCollection<ILanguagePacket> TempPackets = Packets is null ? new ObservableCollection<ILanguagePacket>() :
                                                                                  new ObservableCollection<ILanguagePacket>(Packets);
            TempPackets.CollectionChanged += (s, e) =>
            {
                if (NotifyCollectionChanged)
                    this.OnPropertyChanged();
            };
            this._Packets = TempPackets;
        }

        public void Add(ILanguagePacket Packet)
        {
            AddHander(Packet);
            PacketAdded?.Invoke(this, new ILanguagePacket[] { Packet });
        }
        public void Add(IEnumerable<ILanguagePacket> Packets)
        {
            try
            {
                NotifyCollectionChanged = false;

                foreach (ILanguagePacket p in Packets)
                    AddHander(p);

                PacketAdded?.Invoke(this, Packets);
                this.OnPropertyChanged();
            }
            finally
            {
                NotifyCollectionChanged = true;
            }

        }
        private void AddHander(ILanguagePacket Packet)
        {
            Type t = Packet.GetType();
            int Index = _Packets.IndexOf(i => t.Equals(i.GetType()));
            if (Index == -1)
                this._Packets.Add(Packet);
            else
                this._Packets[Index] = Packet;

        }

        public void Remove(ILanguagePacket Packet)
            => _Packets.Remove(Packet);

        protected internal override IEnumerable<string> GetPropertyNames()
        {
            foreach (ILanguagePacket Packet in Packets)
                foreach (string Name in Packet.GetPropertyNames())
                    yield return Name;
        }

    }
}
