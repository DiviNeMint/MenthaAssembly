using System;
using System.Collections.Generic;
using System.IO;

namespace MenthaAssembly.Offices.Primitives
{
    internal class CompoundStream : Stream
    {
        public List<uint> SectorChain { get; }

        public List<uint> RootSectorChain { get; }

        public override bool CanRead
            => true;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => false;

        public override long Length { get; }

        public override long Position
        {
            get => Offset - SectorBytes.Length + SectorOffset;
            set => Seek(value, SeekOrigin.Begin);
        }

        private readonly bool IsMini;
        private readonly bool LeaveOpen;
        private Stream BaseStream;
        private CompoundDocument Document;
        public CompoundStream(CompoundDocument Document, Stream BaseStream, List<uint> SectorChain, int Length, bool LeaveOpen)
        {
            this.Document = Document;
            this.BaseStream = BaseStream;
            IsMini = false;
            this.LeaveOpen = LeaveOpen;
            this.Length = Length;
            this.SectorChain = SectorChain;
            ReadSector();
        }
        public CompoundStream(CompoundDocument document, Stream baseStream, CompoundDirectoryEntry Entry, bool leaveOpen)
        {
            Document = document;
            BaseStream = baseStream;
            IsMini = Entry.IsEntryMiniStream;
            Length = Entry.StreamSize;
            LeaveOpen = leaveOpen;

            if (IsMini)
            {
                SectorChain = Document.GetMiniSectorChain(Entry.StreamFirstSector);
                RootSectorChain = Document.GetSectorChain(Document.Entries[0].StreamFirstSector);
            }
            else
            {
                SectorChain = Document.GetSectorChain(Entry.StreamFirstSector);
            }

            ReadSector();
        }

        private byte[] SectorBytes { get; set; }
        private int SectorChainOffset, Offset, SectorOffset;
        public override int Read(byte[] buffer, int offset, int count)
        {
            int index = 0;
            while (index < count && Position < Length)
            {
                if (SectorOffset == SectorBytes.Length)
                {
                    ReadSector();
                    SectorOffset = 0;
                }

                int chunkSize = Math.Min(count - index, SectorBytes.Length - SectorOffset);
                Array.Copy(SectorBytes, SectorOffset, buffer, offset + index, chunkSize);
                index += chunkSize;
                SectorOffset += chunkSize;
            }

            return index;
        }
        private void ReadSector()
        {
            if (IsMini)
                ReadMiniSector();
            else
                ReadRegularSector();
        }
        private void ReadMiniSector()
        {
            uint sector = SectorChain[SectorChainOffset];
            int miniStreamOffset = (int)Document.GetMiniSectorOffset(sector);

            int rootSectorIndex = miniStreamOffset / Document.SectorSize;
            if (rootSectorIndex >= RootSectorChain.Count)
                throw new EndOfStreamException();

            uint rootSector = RootSectorChain[rootSectorIndex];
            int rootOffset = miniStreamOffset % Document.SectorSize;

            BaseStream.Seek(Document.GetSectorOffset(rootSector) + rootOffset, SeekOrigin.Begin);

            int ChunkSize = (int)Math.Min(Length - Offset, Document.MiniSectorSize);
            SectorBytes = new byte[ChunkSize];

            if (BaseStream.Read(SectorBytes, 0, ChunkSize) < ChunkSize)
                throw new EndOfStreamException();

            Offset += ChunkSize;
            SectorChainOffset++;
        }
        private void ReadRegularSector()
        {
            uint sector = SectorChain[SectorChainOffset];
            BaseStream.Seek(Document.GetSectorOffset(sector), SeekOrigin.Begin);

            int ChunkSize = (int)Math.Min(Length - Offset, Document.SectorSize);
            SectorBytes = new byte[ChunkSize];

            if (BaseStream.Read(SectorBytes, 0, ChunkSize) < ChunkSize)
                throw new EndOfStreamException();

            Offset += ChunkSize;
            SectorChainOffset++;
        }

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        int SectorSize = IsMini ? Document.MiniSectorSize : Document.SectorSize;
                        SectorChainOffset = (int)(offset / SectorSize);
                        Offset = SectorChainOffset * SectorSize;
                        SectorOffset = (int)(offset % SectorSize);

                        if (Offset < Length)
                            ReadSector();

                        return Position;
                    }
                case SeekOrigin.Current:
                    return Seek(Position + offset, SeekOrigin.Begin);
                case SeekOrigin.End:
                    return Seek(Length + offset, SeekOrigin.Begin);
                default:
                    return Offset;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!LeaveOpen)
                {
                    BaseStream?.Dispose();
                    BaseStream = null;
                }

                Document = null;
                SectorBytes = null;
            }

            base.Dispose(disposing);
        }

    }
}
