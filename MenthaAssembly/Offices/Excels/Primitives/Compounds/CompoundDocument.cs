using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MenthaAssembly.Offices.Primitives
{
    // https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-cfb/53989ce4-7b05-4f8d-829b-d08d6148375b
    internal class CompoundDocument
    {
        private const uint FAT_EndOfChain = 0xFFFFFFFE;
        private const uint FAT_FreeSpace = 0xFFFFFFFF;

        public Guid CLSID { get; }

        public ushort Version { get; }

        public ushort DllVersion { get; }

        public int SectorSize { get; }

        public int MiniSectorSize { get; }

        public uint MiniStreamCutoff { get; }

        public List<uint> SectorTable { get; }

        public List<uint> MiniSectorTable { get; }

        public List<CompoundDirectoryEntry> Entries { get; }

        public CompoundDocument(Stream Stream)
        {
            CLSID = Stream.Read<Guid>();
            Version = Stream.Read<ushort>();
            DllVersion = Stream.Read<ushort>();
            ushort ByteOrder = Stream.Read<ushort>();

            // Some broken xls files uses 0xFFFF
            if (ByteOrder != 0xFFFE && ByteOrder != 0xFFFF)
                throw new InvalidDataException();

            SectorSize = 1 << Stream.Read<ushort>();
            MiniSectorSize = 1 << Stream.Read<ushort>();
            Stream.Seek(14, SeekOrigin.Current);
            // DirectorySectorCount = Stream.Read<int>();
            // FatSectorCount = Stream.Read<int>();
            uint RootDirectoryEntryStart = Stream.Read<uint>();
            Stream.Seek(4, SeekOrigin.Current);
            //TransactionSignature = Stream.Read<uint>();
            MiniStreamCutoff = Stream.Read<uint>();
            uint MiniFatFirstSector = Stream.Read<uint>();
            int MiniFatSectorCount = Stream.Read<int>();
            uint DiFatFirstSector = Stream.Read<uint>();
            int DiFatSectorCount = Stream.Read<int>();

            #region Sector Table
            {
                // first 109 FAT sector locations of the compound file.
                List<uint> Chain = new List<uint>();
                for (int i = 0; i < 109; ++i)
                    Chain.Add(Stream.Read<uint>());

                // DiFat Sector Location
                if (DiFatFirstSector != FAT_EndOfChain)
                {
                    for (int i = 0; i < DiFatSectorCount; ++i)
                    {
                        long Offset = GetSectorOffset(DiFatFirstSector);
                        Stream.Seek(DiFatFirstSector, SeekOrigin.Begin);
                        int Count = SectorSize >> 2 - 1;
                        for (int j = 0; j < Count; ++j)
                            Chain.Add(Stream.Read<uint>());

                        // The DIFAT sectors are linked together by the "Next DIFAT Sector Location" in each DIFAT sector:
                        DiFatFirstSector = Stream.Read<uint>();
                    }
                }

                // Trim FAT_FreeSpace & FAT_EndOfChain
                for (int i = Chain.Count - 1; i >= 0; i--)
                {
                    uint Sector = Chain[i];
                    if (Sector != FAT_EndOfChain && Sector != FAT_FreeSpace)
                        break;

                    Chain.RemoveAt(i);
                }

                this.SectorTable = ReadSectorTable(Stream, Chain);
            }
            #endregion
            #region Mini Sector Table
            {
                List<uint> Chain = GetSectorChain(MiniFatFirstSector);
                MiniSectorTable = ReadSectorTable(Stream, Chain);
            }
            #endregion
            #region Entries
            {
                List<uint> Chain = GetSectorChain(RootDirectoryEntryStart);
                Entries = new List<CompoundDirectoryEntry>();

                using (CompoundStream CompoundStream = new CompoundStream(this, Stream, Chain, Chain.Count * SectorSize, true))
                {
                    while (CompoundStream.Position < CompoundStream.Length)
                        Entries.Add(ReadDirectoryEntry(CompoundStream));
                }
            }
            #endregion
        }

        public List<uint> GetSectorChain(uint Sector)
        {
            List<uint> Chains = new List<uint>();
            int SectorTableCount = SectorTable.Count;
            while (Sector != FAT_EndOfChain)
            {
                Chains.Add(Sector);

                if (Sector >= SectorTableCount)
                    break;

                Sector = SectorTable[(int)Sector];

                if (Chains.Contains(Sector))
                    throw new InvalidDataException("CyclicSectorChain.");
            }

            // Trim FAT_FreeSpace
            for (int i = Chains.Count - 1; i >= 0; i--)
            {
                uint Chain = Chains[i];
                if (Chain != FAT_FreeSpace)
                    break;

                Chains.RemoveAt(i);
            }

            return Chains;
        }
        public List<uint> GetMiniSectorChain(uint Sector)
        {
            List<uint> Chains = new List<uint>();
            int MiniSectorTableCount = MiniSectorTable.Count;
            while (Sector != FAT_EndOfChain)
            {
                Chains.Add(Sector);

                if (Sector >= MiniSectorTableCount)
                    break;

                Sector = MiniSectorTable[(int)Sector];

                if (Chains.Contains(Sector))
                    throw new InvalidDataException("CyclicSectorChain.");
            }

            // Trim FAT_FreeSpace
            for (int i = Chains.Count - 1; i >= 0; i--)
            {
                uint Chain = Chains[i];
                if (Chain != FAT_FreeSpace)
                    break;

                Chains.RemoveAt(i);
            }

            return Chains;
        }

        private List<uint> ReadSectorTable(Stream Stream, IEnumerable<uint> Chain)
        {
            List<uint> Table = new List<uint>();
            foreach (uint Sector in Chain)
                Table.AddRange(ReadSectorLocations(Stream, Sector));

            // Trim FAT_FreeSpace
            for (int i = Table.Count - 1; i >= 0; i--)
            {
                uint Sector = Table[i];
                if (Sector != FAT_FreeSpace)
                    break;

                Table.RemoveAt(i);
            }

            return Table;
        }
        private IEnumerable<uint> ReadSectorLocations(Stream Stream, uint Sector)
        {
            Stream.Seek(GetSectorOffset(Sector), SeekOrigin.Begin);
            int Count = SectorSize >> 2;
            for (var i = 0; i < Count; ++i)
                yield return Stream.Read<uint>();
        }

        private CompoundDirectoryEntry ReadDirectoryEntry(Stream Stream)
        {
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(64);

            if (!Stream.ReadBuffer(Buffer, 64))
                throw new EndOfStreamException();

            ushort nameLength = Stream.Read<ushort>();

            CompoundDirectoryEntry Entry = new CompoundDirectoryEntry();
            if (nameLength > 0)
            {
                nameLength = Math.Min((ushort)64, nameLength);
                Entry.EntryName = Encoding.Unicode.GetString(Buffer, 0, nameLength).TrimEnd('\0');
            }
            ArrayPool<byte>.Shared.Return(Buffer);

            Entry.EntryType = Stream.Read<byte>();
            Entry.EntryColor = Stream.Read<byte>();
            Entry.LeftSiblingSid = Stream.Read<uint>();
            Entry.RightSiblingSid = Stream.Read<uint>();
            Entry.ChildSid = Stream.Read<uint>();
            Entry.ClassId = Stream.Read<Guid>();
            Entry.UserFlags = Stream.Read<uint>();
            Entry.CreationTime = GetFileTime(Stream.Read<long>());
            Entry.LastWriteTime = GetFileTime(Stream.Read<long>());
            Entry.StreamFirstSector = Stream.Read<uint>();
            Entry.StreamSize = Stream.Read<uint>();
            Entry.PropType = Stream.Read<uint>();
            Entry.IsEntryMiniStream = Entry.StreamSize < MiniStreamCutoff;
            return Entry;
        }

        // NOTE: DateTime.MaxValue.ToFileTime() fails on Unity in timezones with DST and +~6h offset, like Sidney Australia
        private readonly long SafeFileTimeMaxDate = DateTime.MaxValue.ToFileTimeUtc();
        private DateTime GetFileTime(long Time)
        {
            if (Time < 0 || Time > SafeFileTimeMaxDate)
                Time = 0;

            return DateTime.FromFileTime(Time);
        }

        public long GetSectorOffset(uint sector)
            => 512L + SectorSize * sector;
        public long GetMiniSectorOffset(uint sector)
            => MiniSectorSize * sector;

    }
}
