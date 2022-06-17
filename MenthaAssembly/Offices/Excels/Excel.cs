using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using MenthaAssembly.Offices.Primitives;

namespace MenthaAssembly.Offices
{
    public static unsafe class Excel
    {

        public static bool TryParse(string FilePath, out IExcelWorkbook Excel)
        {
            using FileStream FileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            byte[] Header = ArrayPool<byte>.Shared.Rent(8);
            byte* pHeader = Header.ToPointer();
            try
            {
                FileStream.ReadBuffer(Header, 8);

                if (IsCompoundDocument(pHeader))
                {
                    CompoundDocument Document = new CompoundDocument(FileStream);
                    if (TryGetWorkbook(FileStream, Document, out Stream XlsFileStream))
                    {
                        Excel = new XlsWorkbook(XlsFileStream);
                        return true;
                    }

                    if (TryGetEncryptedPackage(FileStream, Document, null, out Stream XlsxFileStream))
                    {
                        using ZipArchive Archive = new ZipArchive(XlsxFileStream, ZipArchiveMode.Read);
                        Excel = new XlsxWorkbook(Archive);
                        return true;
                    }
                }
                else if (IsRawBiffStream(pHeader))
                {
                    FileStream.Seek(0, SeekOrigin.Begin);
                    Excel = new XlsWorkbook(FileStream);
                    return true;
                }
                else if (IsPkZip(pHeader))
                {
                    FileStream.Seek(0, SeekOrigin.Begin);
                    using ZipArchive Archive = new ZipArchive(FileStream, ZipArchiveMode.Read);
                    Excel = new XlsxWorkbook(Archive);
                    return true;
                }
                else
                {
                    string Extension = FilePath.Substring(FilePath.Length - 4, 4)
                                               .ToUpper();
                    if (Extension.Equals(".CSV"))
                    {
                        Excel = null;
                        return true;
                    }
                }

                Excel = null;
                return false;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(Header);
            }
        }

        private static bool TryGetWorkbook(Stream FileStream, CompoundDocument Document, out Stream Stream)
        {
            if (Document.Entries.FirstOrDefault(i => i.EntryType == 2 && i.EntryName.Equals("Workbook") || i.EntryName.Equals("Book")) is CompoundDirectoryEntry Entry)
            {
                Stream = new CompoundStream(Document, FileStream, Entry, false);
                return true;
            }

            Stream = null;
            return false;
        }
        private static bool TryGetEncryptedPackage(Stream FileStream, CompoundDocument Document, string Password, out Stream Stream)
        {
            if (Document.Entries.FirstOrDefault(i => i.EntryName.Equals("EncryptedPackage")) is not CompoundDirectoryEntry PackageEntry)
            {
                Stream = null;
                return false;
            }

            if (Document.Entries.FirstOrDefault(i => i.EntryName.Equals("EncryptionInfo")) is not CompoundDirectoryEntry InfoEntry)
            {
                Stream = null;
                return false;
            }


            Stream = null;
            return false;




            //var infoBytes = Document.ReadStream(FileStream, encryptionInfo.StreamFirstSector, (int)encryptionInfo.StreamSize, encryptionInfo.IsEntryMiniStream);
            //var encryption = EncryptionInfo.Create(infoBytes);

            //if (encryption.VerifyPassword("VelvetSweatshop"))
            //{
            //    // Magic password used for write-protected workbooks
            //    Password = "VelvetSweatshop";
            //}
            //else if (Password == null || !encryption.VerifyPassword(Password))
            //{
            //    throw new InvalidPasswordException(Errors.ErrorInvalidPassword);
            //}

            //var secretKey = encryption.GenerateSecretKey(Password);
            //var packageStream = new CompoundStream(Document, FileStream, encryptedPackage.StreamFirstSector, (int)encryptedPackage.StreamSize, encryptedPackage.IsEntryMiniStream, false);

            //Stream = encryption.CreateEncryptedPackageStream(packageStream, secretKey);
            //return true;
        }

        private static bool IsRawBiffStream(byte* pHeader)
        {
            ushort* pUShort = (ushort*)pHeader;
            switch (*pUShort++)
            {
                case 0x0009: // BIFF2
                    {
                        if (*pUShort++ != 4)
                            return false;

                        ushort Type = ++*pUShort;
                        if (Type != 0x10 && Type != 0x20 && Type != 0x40)
                            return false;

                        return true;
                    }
                case 0x0209: // BIFF3
                case 0x0409: // BIFF4
                    {
                        if (*pUShort++ != 6)
                            return false;

                        ushort Type = ++*pUShort;
                        if (Type != 0x10 && Type != 0x20 && Type != 0x40 && Type != 0x0100)
                            return false;

                        ////removed this additional check to keep the probe at 8 bytes
                        //ushort notUsed = BitConverter.Toushort(bytes, 8);
                        //if (notUsed != 0x00)
                        //    return false;

                        return true;
                    }
                case 0x0809: // BIFF5 / BIFF8
                    {
                        if (*pUShort++ < 4)
                            return false;

                        ushort BofVersion = *pUShort++;
                        if (BofVersion != 0 && BofVersion != 0x0200 && BofVersion != 0x0300 && BofVersion != 0x0400 && BofVersion != 0x0500 && BofVersion != 0x600)
                            return false;

                        ushort Type = *pUShort;
                        if (Type != 0x5 && Type != 0x6 && Type != 0x10 && Type != 0x20 && Type != 0x40 && Type != 0x0100)
                            return false;

                        ////removed this additional check to keep the probe at 8 bytes
                        //ushort identifier = BitConverter.Toushort(bytes, 10);
                        //if (identifier == 0)
                        //    return false;

                        return true;
                    }
            }

            return false;
        }

        private static bool IsPkZip(byte* pHeader)
            => *(int*)pHeader == 0x04034b50;

        private static bool IsCompoundDocument(byte* pHeader)
            => *(ulong*)pHeader == 0xE11AB1A1E011CFD0;

    }
}
