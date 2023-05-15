using System;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly
{
    public static class CRC32
    {
        private static uint[] _Table;
        private static uint[] Table
        {
            get
            {
                if (_Table is null)
                {
                    _Table = new uint[256];
                    unchecked
                    {
                        uint dwCrc;
                        byte i = 0;
                        do
                        {
                            dwCrc = i;
                            for (byte j = 8; j > 0; j--)
                            {

                                if ((dwCrc & 1) == 1)
                                {
                                    dwCrc = (dwCrc >> 1) ^ 0xEDB88320;
                                }
                                else
                                {
                                    dwCrc >>= 1;
                                }
                            }
                            _Table[i] = dwCrc;
                            i++;
                        } while (i != 0);
                    }
                }
                return _Table;
            }
        }

        public static void Calculate(byte Data, out uint NewRegister, uint Register = 0xFFFFFFFFU)
            => NewRegister = (Register >> 8) ^ Table[(Register & 0x000000FF) ^ Data];
        public static void Calculate(byte Data, out int CRCResult, uint Register = 0xFFFFFFFFU)
        {
            Register = (Register >> 8) ^ Table[(Register & 0x000000FF) ^ Data];
            CRCResult = unchecked((int)~Register);
        }

        public static void Calculate(IEnumerable<byte> Datas, out uint NewRegister, uint Register = 0xFFFFFFFFU)
        {
            if (Datas is null ||
                Datas.Count() == 0)
                throw new ArgumentNullException("The Data buffer must not be null.");

            NewRegister = Register;
            // BZip Algorithm
            foreach (byte Data in Datas)
                NewRegister = (NewRegister >> 8) ^ Table[(NewRegister & 0x000000FF) ^ Data];

        }
        public static void Calculate(IEnumerable<byte> Datas, out int CRCResult, uint Register = 0xFFFFFFFFU)
        {
            if (Datas is null ||
                Datas.Count() == 0)
                throw new ArgumentNullException("The Data buffer must not be null.");

            // BZip Algorithm
            foreach (byte Data in Datas)
                Register = (Register >> 8) ^ Table[(Register & 0x000000FF) ^ Data];

            CRCResult = unchecked((int)~Register);
        }

#if NETSTANDARD2_1
        public static void Calculate(ReadOnlySpan<byte> SpanDatas, out uint NewRegister, uint Register = 0xFFFFFFFFU)
        {
            if (SpanDatas.IsEmpty ||
                SpanDatas.Length == 0)
                throw new ArgumentNullException("The Data buffer must not be null.");

            NewRegister = Register;
            // BZip Algorithm
            foreach (byte Data in SpanDatas)
                NewRegister = (NewRegister >> 8) ^ Table[(NewRegister & 0x000000FF) ^ Data];

        }
        public static void Calculate(ReadOnlySpan<byte> SpanDatas, out int CRCResult, uint Register = 0xFFFFFFFFU)
        {
            if (SpanDatas.IsEmpty ||
                SpanDatas.Length == 0)
                throw new ArgumentNullException("The Data buffer must not be null.");

            // BZip Algorithm
            foreach (byte Data in SpanDatas)
                Register = (Register >> 8) ^ Table[(Register & 0x000000FF) ^ Data];

            CRCResult = unchecked((int)~Register);
        }
#endif

        public static void Calculate(byte[] Datas, int Offset, int Length, out uint NewRegister, uint Register = 0xFFFFFFFFU)
        {
            if (Datas is null ||
                Datas.Count() == 0)
                throw new ArgumentNullException("The Data buffer must not be null.");

            NewRegister = Register;
            // BZip Algorithm
            for (int i = Offset; i < Offset + Length; i++)
                NewRegister = (NewRegister >> 8) ^ Table[(NewRegister & 0x000000FF) ^ Datas[i]];
        }
        public static void Calculate(byte[] Datas, int Offset, int Length, out int CRCResult, uint Register = 0xFFFFFFFFU)
        {
            if (Datas is null ||
                Datas.Count() == 0)
                throw new ArgumentNullException("The Data buffer must not be null.");

            // BZip Algorithm
            for (int i = Offset; i < Offset + Length; i++)
                Register = (Register >> 8) ^ Table[(Register & 0x000000FF) ^ Datas[i]];

            CRCResult = unchecked((int)~Register);
        }

    }
}