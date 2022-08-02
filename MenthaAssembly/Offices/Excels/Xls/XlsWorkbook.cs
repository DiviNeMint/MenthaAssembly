using MenthaAssembly.Offices.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public XlsWorkbook(Stream Stream, Func<string> PasswordAction, int RetryCount)
        {
            using XlsBiffReader Reader = new XlsBiffReader(Stream, true);

            if (!Reader.ReadVariable(out int ID, out _) || (ID != 0x009 && ID != 0x0209 && ID != 0x0409 && ID != 0x0809))
                throw new InvalidDataException();

            int BiffVersion = Reader.Read<ushort>(),
                BiffType = Reader.Read<ushort>();

            // Encoding.GetEncoding(1252);

            if (BiffType == 0x0005)
            {
                List<XlsSheetInfo> SheetInfos = new List<XlsSheetInfo>();
                int LastID = -1,
                    SSTCount = 0;
                while (Reader.ReadVariable(out ID, out int Length) && ID != 0x0010)
                {
                    switch (ID)
                    {
                        #region FilePass
                        case 0x002F when Reader.Encryption is null:
                            {
                                #region Build Encryption Validator
                                if (2 <= BiffVersion && BiffVersion <= 5)
                                {
                                    // XOR obfuscation
                                    ushort Key = Reader.Read<ushort>(),
                                           Hash = Reader.Read<ushort>();

                                    Reader.Encryption = new XorEncryption(Key, Hash);
                                }
                                else
                                {
                                    ushort EncryptionType = Reader.Read<ushort>();
                                    switch (EncryptionType)
                                    {
                                        // XOR obfuscation
                                        case 0x00:
                                            {
                                                ushort Key = Reader.Read<ushort>(),
                                                       Hash = Reader.Read<ushort>();

                                                Reader.Encryption = new XorEncryption(Key, Hash);
                                                break;
                                            }
                                        // Encryption
                                        case 0x01:
                                            {
                                                using Stream Memory = new MemoryStream(Reader.ReadBuffer(Length - 2));
                                                Reader.Encryption = Encryption.Create(Memory);
                                                break;
                                            }
                                        default:
                                            throw new NotSupportedException($"Unknown encryption type: {EncryptionType}");
                                    }
                                }
                                #endregion

                                string Password = null;
                                #region Validate
                                // Magic password used for write-protected workbooks
                                if (PasswordAction is null)
                                {
                                    Password = "VelvetSweatshop";
                                    if (!Reader.Encryption.VerifyPassword(Password))
                                    {
                                        Dispose();
                                        return;
                                    }
                                }
                                else
                                {
                                    int Try = 0;
                                    string LastPassword = null;
                                    for (; Try < RetryCount; Try++)
                                    {
                                        Password = PasswordAction();
                                        if (Password == LastPassword)
                                            continue;

                                        if (Reader.Encryption.VerifyPassword(Password))
                                            break;
                                    }

                                    if (Try >= RetryCount)
                                    {
                                        Dispose();
                                        return;
                                    }
                                }
                                #endregion

                                Reader.SecretKey = Reader.Encryption.GenerateSecretKey(Password);

                                Stream.Position = 0;
                                break;
                            }
                        #endregion
                        #region SheetInfo
                        case 0x0085:    // BoundSheet
                            {
                                uint Position = Reader.Read<uint>();
                                bool Visible = Reader.Read<byte>() == 0;
                                int SheetType = Reader.Read<byte>();

                                // Name
                                int cch = Reader.Read<byte>();
                                byte Option = Reader.Read<byte>();
                                bool fHighByte = (Option & 0x01) > 0;

                                byte[] Datas = Reader.ReadBuffer(fHighByte ? cch << 1 : cch);
                                string SheetName = fHighByte ? Encoding.Unicode.GetString(Datas) :
                                                               Encoding.UTF8.GetString(Datas);

                                SheetInfos.Add(new XlsSheetInfo(Position, SheetName, SheetType, Visible));
                                break;
                            }
                        #endregion
                        #region SharedStrings
                        case 0x00FC:
                            {
                                int cstTotal = Reader.Read<int>();
                                SSTCount = Reader.Read<int>();

                                while (SharedStrings.Count < SSTCount &&
                                       Reader.TryRead(out ushort cch))
                                {
                                    byte OptionDatas = Reader.Read<byte>();
                                    bool fHighByte = (OptionDatas & 0x01) > 0,
                                         fExtSt = (OptionDatas & 0x04) > 0,
                                         fRichSt = (OptionDatas & 0x08) > 0;

                                    ushort cRun = fRichSt ? Reader.Read<ushort>() : ushort.MinValue;
                                    int cbExtRst = fExtSt ? Reader.Read<int>() : 0;

                                    byte[] Datas = Reader.ReadBuffer(fHighByte ? cch << 1 : cch);

                                    string Context = fHighByte ? Encoding.Unicode.GetString(Datas) :
                                                                 Encoding.UTF8.GetString(Datas);

                                    SharedStrings.Add(Context);

                                    // Skip FormatRun & ExtRst
                                    Reader.Skip((cRun << 2) + cbExtRst);
                                }

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
                        #region Continue
                        case 0x003C:
                            {
                                switch (LastID)
                                {
                                    case 0x00FC:
                                        {
                                            throw new NotImplementedException("Continue & SharedStrings");
                                        }
                                    default:
                                        break;
                                }
                                continue;
                            }
                            #endregion
                    }
                    LastID = ID;
                }

                // Sheet
                foreach (XlsSheetInfo Info in SheetInfos)
                    _Sheets.Add(new XlsSheet(this, Info, Stream, Reader.Encryption, Reader.SecretKey));

            }
            //else if (BiffType == 0x0010)
            //{
            //    XlsSheetInfo Info = new XlsSheetInfo(0, "Sheet", 0, true);

            //}
            else
            {
                throw new InvalidDataException("Error reading Workbook Globals.");
            }
        }

        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            _Sheets.ForEach(i => i.Dispose());
            _Sheets.Clear();
            _Sheets = null;

            SharedStrings.Clear();
            SharedStrings = null;

            IsDisposed = true;
        }

    }
}
