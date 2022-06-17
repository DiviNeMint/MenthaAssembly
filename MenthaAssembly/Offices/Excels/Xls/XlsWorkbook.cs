using MenthaAssembly.Offices.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenthaAssembly.Offices
{
    // https://interoperability.blob.core.windows.net/files/MS-XLS/%5bMS-XLS%5d.pdf
    internal class XlsWorkbook : IExcelWorkbook
    {
        public IExcelSheet this[int Index]
            => Index < _Sheets.Count ? _Sheets[Index] : null;
        public IExcelSheet this[string Name]
            => _Sheets.FirstOrDefault(i => i.Name.Equals(Name));

        private List<XlsSheet> _Sheets = new List<XlsSheet>();
        public IReadOnlyList<IExcelSheet> Sheets => _Sheets;

        private List<string> SharedStrings = new List<string>();
        List<string> IExcelWorkbook.SharedStrings
            => SharedStrings;

        public bool IsDate1904 { get; }

        public XlsWorkbook(Stream Stream)
        {
            using XlsBiffReader Reader = new XlsBiffReader(Stream, true);

            if (!Reader.ReadVariable(out int ID) || (ID != 0x009 && ID != 0x0209 && ID != 0x0409 && ID != 0x0809))
                throw new InvalidDataException();

            int BiffVersion = Reader.Read<ushort>(),
                BiffType = Reader.Read<ushort>();

            // Encoding.GetEncoding(1252);

            if (BiffType == 0x0005)
            {
                while (Reader.ReadVariable(out ID))
                {
                    switch (ID)
                    {
                        #region FilePass
                        case 0x002F:
                            {
                                //if (2 <= BiffVersion && BiffVersion <= 5)
                                //{
                                //    // XOR obfuscation
                                //    ushort EncryptionKey = Stream.Read<ushort>(),
                                //           HashValue = Stream.Read<ushort>();



                                //    EncryptionInfo = Encryption.Create(EncryptionKey, HashValue);
                                //}
                                //else
                                //{
                                //    ushort EncryptionType = Stream.Read<ushort>();
                                //    switch (EncryptionType)
                                //    {
                                //        // XOR obfuscation
                                //        case 0x00:
                                //            {
                                //                ushort EncryptionKey = Stream.Read<ushort>(),
                                //                       HashValue = Stream.Read<ushort>();
                                //                EncryptionInfo = Encryption.Create(encryptionKey, hashValue);
                                //                break;
                                //            }
                                //        // RC4 encryption
                                //        case 0x01:
                                //            {
                                //                var encryptionInfo = new byte[bytes.Length - 6]; // 6 = 4 + 2 = biffVersion header + filepass enryptiontype
                                //                Array.Copy(bytes, 6, encryptionInfo, 0, bytes.Length - 6);
                                //                EncryptionInfo = Encryption.Create(encryptionInfo);
                                //                break;
                                //            }
                                //        default:
                                //            throw new NotSupportedException($"Unknown encryption type: {EncryptionType}");
                                //    }
                                //}
                                break;
                            }
                        #endregion
                        #region Sheet

                        #endregion
                        #region SharedStrings
                        case 0x00FC:
                            {
                                uint cstTotal = Reader.Read<uint>(),
                                     cstUnique = Reader.Read<uint>();





                                break;
                            }
                        case 0x003C:
                            {
                                break;
                            }
                        #endregion
                        #region IsDate1904
                        case 0x0022:
                            {
                                IsDate1904 = Reader.Read<ushort>() == 1;
                                break;
                            }
                            #endregion
                    }
                }
            }
            else
            {

            }

        }

        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed)
                return;




            IsDisposed = true;
        }
    }
}
