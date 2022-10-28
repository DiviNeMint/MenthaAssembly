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

        private readonly ObservableCollection<LanguagePacketBase> _Packets;
        public IReadOnlyList<LanguagePacketBase> Packets => _Packets;

        internal bool NotifyCollectionChanged = true;

        public override string this[string Name]
        {
            get
            {
                if (Packets.FirstOrDefault(i => i.GetPropertyNames().Contains(Name)) is LanguagePacketBase Packet)
                    return Packet[Name];

                Debug.WriteLine($"[LanguagePacket]Not fount {Name}.");
                return null;
            }

            internal set
            {
                if (Packets.FirstOrDefault(i => i.GetPropertyNames().Contains(Name)) is LanguagePacketBase Packet)
                    Packet[Name] = value;
            }
        }

        public MultiLanguagePacket() : this(null)
        {
        }
        public MultiLanguagePacket(IEnumerable<LanguagePacketBase> Packets)
        {
            ObservableCollection<LanguagePacketBase> TempPackets = Packets is null ? new ObservableCollection<LanguagePacketBase>() :
                                                                                  new ObservableCollection<LanguagePacketBase>(Packets);
            TempPackets.CollectionChanged += (s, e) =>
            {
                if (NotifyCollectionChanged)
                    this.OnPropertyChanged();
            };
            this._Packets = TempPackets;
        }

        public void Add(LanguagePacketBase Packet)
        {
            AddHander(Packet);
            PacketAdded?.Invoke(this, new LanguagePacketBase[] { Packet });
        }
        public void Add(IEnumerable<LanguagePacketBase> Packets)
        {
            try
            {
                NotifyCollectionChanged = false;

                foreach (LanguagePacketBase p in Packets)
                    AddHander(p);

                PacketAdded?.Invoke(this, Packets);
                this.OnPropertyChanged();
            }
            finally
            {
                NotifyCollectionChanged = true;
            }

        }
        private void AddHander(LanguagePacketBase Packet)
        {
            Type t = Packet.GetType();
            int Index = _Packets.IndexOf(i => t.Equals(i.GetType()));
            if (Index == -1)
                this._Packets.Add(Packet);
            else
                this._Packets[Index] = Packet;

        }

        public void Remove(LanguagePacketBase Packet)
            => _Packets.Remove(Packet);

        protected internal override IEnumerable<string> GetPropertyNames()
        {
            foreach (LanguagePacketBase Packet in Packets)
                foreach (string Name in Packet.GetPropertyNames())
                    yield return Name;
        }

    }
}
