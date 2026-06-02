using System.Runtime.InteropServices;

namespace System
{
    public static unsafe class BufferHelper
    {
        /// <summary>
        /// Copies bits from the source buffer to the destination buffer.
        /// </summary>
        /// <param name="Source">The source buffer.</param>
        /// <param name="SourceByteIndex">The byte index in the source buffer.</param>
        /// <param name="SourceBitOffset">The bit offset in the source byte.</param>
        /// <param name="Destination">The destination buffer.</param>
        /// <param name="DestinationByteIndex">The byte index in the destination buffer.</param>
        /// <param name="DestinationBitOffset">The bit offset in the destination byte.</param>
        /// <param name="BitCount">The number of bits to copy.</param>
        public static void CopyBits(this byte[] Source, int SourceByteIndex, int SourceBitOffset, byte[] Destination, int DestinationByteIndex, int DestinationBitOffset, int BitCount)
        {
            if (BitCount <= 0)
                return;
            if (SourceBitOffset < 0 || 7 < SourceBitOffset)
                throw new ArgumentOutOfRangeException(nameof(SourceBitOffset));
            if (DestinationBitOffset < 0 || 7 < DestinationBitOffset)
                throw new ArgumentOutOfRangeException(nameof(DestinationBitOffset));

            if (SourceBitOffset == 0 && DestinationBitOffset == 0)
            {
                int fullByteCount = BitCount >> 3,
                    tailBitCount = BitCount & 0x07;

                // Copy full bytes directly when both buffers are byte-aligned.
                if (fullByteCount > 0)
                    Array.Copy(Source, SourceByteIndex, Destination, DestinationByteIndex, fullByteCount);

                // Merge tail bits and preserve destination bits outside the copied range.
                if (tailBitCount > 0)
                {
                    byte mask = (byte)((1 << tailBitCount) - 1);
                    int destinationIndex = DestinationByteIndex + fullByteCount;
                    Destination[destinationIndex] = (byte)((Destination[destinationIndex] & ~mask) | (Source[SourceByteIndex + fullByteCount] & mask));
                }

                return;
            }

            if (SourceBitOffset == DestinationBitOffset)
            {
                int firstBitCount = Math.Min(8 - DestinationBitOffset, BitCount),
                    firstMask = ((1 << firstBitCount) - 1) << DestinationBitOffset;

                // Merge the leading partial byte until both buffers reach the next byte boundary.
                Destination[DestinationByteIndex] = (byte)((Destination[DestinationByteIndex] & ~firstMask) |
                                                          (Source[SourceByteIndex] & firstMask));

                SourceByteIndex++;
                DestinationByteIndex++;
                BitCount -= firstBitCount;
                if (BitCount <= 0)
                    return;

                int fullByteCount = BitCount >> 3,
                    tailBitCount = BitCount & 0x07;

                // Copy full bytes directly after same-phase alignment reaches byte boundaries.
                if (fullByteCount > 0)
                    Array.Copy(Source, SourceByteIndex, Destination, DestinationByteIndex, fullByteCount);

                // Merge tail bits and preserve destination bits outside the copied range.
                if (tailBitCount > 0)
                {
                    byte mask = (byte)((1 << tailBitCount) - 1);
                    int destinationIndex = DestinationByteIndex + fullByteCount;
                    Destination[destinationIndex] = (byte)((Destination[destinationIndex] & ~mask) | (Source[SourceByteIndex + fullByteCount] & mask));
                }

                return;
            }

            while (BitCount > 0)
            {
                int copyBits = Math.Min(8 - DestinationBitOffset, BitCount),
                    sourceData = Source[SourceByteIndex] >> SourceBitOffset;
                if (SourceBitOffset + copyBits > 8)
                    sourceData |= Source[SourceByteIndex + 1] << (8 - SourceBitOffset);

                int sourceMask = (1 << copyBits) - 1,
                    destinationMask = sourceMask << DestinationBitOffset;

                // Merge the current bit chunk and preserve destination bits outside it.
                Destination[DestinationByteIndex] = (byte)((Destination[DestinationByteIndex] & ~destinationMask) |
                                                          ((sourceData & sourceMask) << DestinationBitOffset));

                SourceBitOffset += copyBits;
                SourceByteIndex += SourceBitOffset >> 3;
                SourceBitOffset &= 0x07;

                DestinationBitOffset += copyBits;
                DestinationByteIndex += DestinationBitOffset >> 3;
                DestinationBitOffset &= 0x07;

                BitCount -= copyBits;
            }
        }

        /// <summary>
        /// Copies bits from the source buffer to the destination pointer.
        /// </summary>
        /// <param name="Source">The source buffer.</param>
        /// <param name="SourceByteIndex">The byte index in the source buffer.</param>
        /// <param name="SourceBitOffset">The bit offset in the source byte.</param>
        /// <param name="pDestination">The destination pointer.</param>
        /// <param name="DestinationBitOffset">The bit offset in the destination byte.</param>
        /// <param name="BitCount">The number of bits to copy.</param>
        public static void CopyBits(this byte[] Source, int SourceByteIndex, int SourceBitOffset, byte* pDestination, int DestinationBitOffset, int BitCount)
        {
            if (BitCount <= 0)
                return;
            if (SourceBitOffset < 0 || 7 < SourceBitOffset)
                throw new ArgumentOutOfRangeException(nameof(SourceBitOffset));
            if (DestinationBitOffset < 0 || 7 < DestinationBitOffset)
                throw new ArgumentOutOfRangeException(nameof(DestinationBitOffset));

            if (SourceBitOffset == 0 && DestinationBitOffset == 0)
            {
                int fullByteCount = BitCount >> 3,
                    tailBitCount = BitCount & 0x07;

                // Copy full bytes directly when both buffers are byte-aligned.
                if (fullByteCount > 0)
                    Marshal.Copy(Source, SourceByteIndex, (IntPtr)pDestination, fullByteCount);

                // Merge tail bits and preserve destination bits outside the copied range.
                if (tailBitCount > 0)
                {
                    byte mask = (byte)((1 << tailBitCount) - 1);
                    pDestination[fullByteCount] = (byte)((pDestination[fullByteCount] & ~mask) | (Source[SourceByteIndex + fullByteCount] & mask));
                }

                return;
            }

            if (SourceBitOffset == DestinationBitOffset)
            {
                int firstBitCount = Math.Min(8 - DestinationBitOffset, BitCount),
                    firstMask = ((1 << firstBitCount) - 1) << DestinationBitOffset;

                // Merge the leading partial byte until both buffers reach the next byte boundary.
                *pDestination = (byte)((*pDestination & ~firstMask) | (Source[SourceByteIndex] & firstMask));

                SourceByteIndex++;
                pDestination++;
                BitCount -= firstBitCount;
                if (BitCount <= 0)
                    return;

                int fullByteCount = BitCount >> 3,
                    tailBitCount = BitCount & 0x07;

                // Copy full bytes directly after same-phase alignment reaches byte boundaries.
                if (fullByteCount > 0)
                    Marshal.Copy(Source, SourceByteIndex, (IntPtr)pDestination, fullByteCount);

                // Merge tail bits and preserve destination bits outside the copied range.
                if (tailBitCount > 0)
                {
                    byte mask = (byte)((1 << tailBitCount) - 1);
                    pDestination[fullByteCount] = (byte)((pDestination[fullByteCount] & ~mask) | (Source[SourceByteIndex + fullByteCount] & mask));
                }

                return;
            }

            while (BitCount > 0)
            {
                int copyBits = Math.Min(8 - DestinationBitOffset, BitCount),
                    sourceData = Source[SourceByteIndex] >> SourceBitOffset;
                if (SourceBitOffset + copyBits > 8)
                    sourceData |= Source[SourceByteIndex + 1] << (8 - SourceBitOffset);

                int sourceMask = (1 << copyBits) - 1,
                    destinationMask = sourceMask << DestinationBitOffset;

                // Merge the current bit chunk and preserve destination bits outside it.
                *pDestination = (byte)((*pDestination & ~destinationMask) |
                                      ((sourceData & sourceMask) << DestinationBitOffset));

                SourceBitOffset += copyBits;
                SourceByteIndex += SourceBitOffset >> 3;
                SourceBitOffset &= 0x07;

                DestinationBitOffset += copyBits;
                pDestination += DestinationBitOffset >> 3;
                DestinationBitOffset &= 0x07;

                BitCount -= copyBits;
            }
        }
        /// <summary>
        /// Copies bits from the source pointer to the destination buffer.
        /// </summary>
        /// <param name="pSource">The source pointer.</param>
        /// <param name="SourceBitOffset">The bit offset in the source byte.</param>
        /// <param name="Destination">The destination buffer.</param>
        /// <param name="DestinationByteIndex">The byte index in the destination buffer.</param>
        /// <param name="DestinationBitOffset">The bit offset in the destination byte.</param>
        /// <param name="BitCount">The number of bits to copy.</param>
        public static void CopyBits(byte* pSource, int SourceBitOffset, byte[] Destination, int DestinationByteIndex, int DestinationBitOffset, int BitCount)
        {
            if (BitCount <= 0)
                return;
            if (SourceBitOffset < 0 || 7 < SourceBitOffset)
                throw new ArgumentOutOfRangeException(nameof(SourceBitOffset));
            if (DestinationBitOffset < 0 || 7 < DestinationBitOffset)
                throw new ArgumentOutOfRangeException(nameof(DestinationBitOffset));

            if (SourceBitOffset == 0 && DestinationBitOffset == 0)
            {
                int fullByteCount = BitCount >> 3,
                    tailBitCount = BitCount & 0x07;

                // Copy full bytes directly when both buffers are byte-aligned.
                if (fullByteCount > 0)
                    Marshal.Copy((IntPtr)pSource, Destination, DestinationByteIndex, fullByteCount);

                // Merge tail bits and preserve destination bits outside the copied range.
                if (tailBitCount > 0)
                {
                    byte mask = (byte)((1 << tailBitCount) - 1);
                    int destinationIndex = DestinationByteIndex + fullByteCount;
                    Destination[destinationIndex] = (byte)((Destination[destinationIndex] & ~mask) | (pSource[fullByteCount] & mask));
                }

                return;
            }

            if (SourceBitOffset == DestinationBitOffset)
            {
                int firstBitCount = Math.Min(8 - DestinationBitOffset, BitCount),
                    firstMask = ((1 << firstBitCount) - 1) << DestinationBitOffset;

                // Merge the leading partial byte until both buffers reach the next byte boundary.
                Destination[DestinationByteIndex] = (byte)((Destination[DestinationByteIndex] & ~firstMask) |
                                                          (pSource[0] & firstMask));

                pSource++;
                DestinationByteIndex++;
                BitCount -= firstBitCount;
                if (BitCount <= 0)
                    return;

                int fullByteCount = BitCount >> 3,
                    tailBitCount = BitCount & 0x07;

                // Copy full bytes directly after same-phase alignment reaches byte boundaries.
                if (fullByteCount > 0)
                    Marshal.Copy((IntPtr)pSource, Destination, DestinationByteIndex, fullByteCount);

                // Merge tail bits and preserve destination bits outside the copied range.
                if (tailBitCount > 0)
                {
                    byte mask = (byte)((1 << tailBitCount) - 1);
                    int destinationIndex = DestinationByteIndex + fullByteCount;
                    Destination[destinationIndex] = (byte)((Destination[destinationIndex] & ~mask) | (pSource[fullByteCount] & mask));
                }

                return;
            }

            while (BitCount > 0)
            {
                int copyBits = Math.Min(8 - DestinationBitOffset, BitCount),
                    sourceData = pSource[0] >> SourceBitOffset;
                if (SourceBitOffset + copyBits > 8)
                    sourceData |= pSource[1] << (8 - SourceBitOffset);

                int sourceMask = (1 << copyBits) - 1,
                    destinationMask = sourceMask << DestinationBitOffset;

                // Merge the current bit chunk and preserve destination bits outside it.
                Destination[DestinationByteIndex] = (byte)((Destination[DestinationByteIndex] & ~destinationMask) |
                                                          ((sourceData & sourceMask) << DestinationBitOffset));

                SourceBitOffset += copyBits;
                pSource += SourceBitOffset >> 3;
                SourceBitOffset &= 0x07;

                DestinationBitOffset += copyBits;
                DestinationByteIndex += DestinationBitOffset >> 3;
                DestinationBitOffset &= 0x07;

                BitCount -= copyBits;
            }
        }

    }
}