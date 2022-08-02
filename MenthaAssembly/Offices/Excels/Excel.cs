using System;
using System.Buffers;
using System.Diagnostics;
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
            => TryParse(FilePath, null, -1, out Excel);
        public static bool TryParse(string FilePath, string Password, out IExcelWorkbook Excel)
            => TryParse(FilePath, () => Password, 1, out Excel);
        public static bool TryParse(string FilePath, Func<string> PasswordAction, int RetryCount, out IExcelWorkbook Excel)
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
                        Excel = new XlsWorkbook(XlsFileStream, PasswordAction, RetryCount);
                        return true;
                    }

                    if (TryGetEncryptedPackage(FileStream, Document, PasswordAction, RetryCount, out Stream XlsxFileStream))
                    {
                        using ZipArchive Archive = new ZipArchive(XlsxFileStream, ZipArchiveMode.Read);
                        Excel = new XlsxWorkbook(Archive);
                        return true;
                    }
                }
                else if (IsRawBiffStream(pHeader))
                {
                    FileStream.Seek(0, SeekOrigin.Begin);
                    Excel = new XlsWorkbook(FileStream, PasswordAction, RetryCount);
                    return true;
                }
                else if (IsPkZip(pHeader))
                {
                    FileStream.Seek(0, SeekOrigin.Begin);
                    using ZipArchive Archive = new ZipArchive(FileStream, ZipArchiveMode.Read);
                    Excel = new XlsxWorkbook(Archive);
                    return true;
                }
                else if (IsCsv(FilePath))
                {
                    Excel = null;
                    return true;
                }

                Excel = null;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Message : {ex.Message}");
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
            if (Document.Entries.Where(i => i.EntryType == 2)
                                .FirstOrDefault(i => i.EntryName == "Workbook" || i.EntryName == "Book") is CompoundDirectoryEntry Entry)
            {
                Stream = new CompoundStream(Document, FileStream, Entry, false);
                return true;
            }

            Stream = null;
            return false;
        }
        private static bool TryGetEncryptedPackage(Stream FileStream, CompoundDocument Document, Func<string> PasswordAction, int RetryCount, out Stream Stream)
        {
            if (Document.Entries.FirstOrDefault(i => i.EntryName == "EncryptedPackage") is not CompoundDirectoryEntry PackageEntry ||
                Document.Entries.FirstOrDefault(i => i.EntryName == "EncryptionInfo") is not CompoundDirectoryEntry InfoEntry)
            {
                Stream = null;
                return false;
            }

            using Stream EncryptionStream = new CompoundStream(Document, FileStream, InfoEntry, true);
            Encryption Encryption = Encryption.Create(EncryptionStream);

            string Password = null;

            // Magic password used for write-protected workbooks
            if (PasswordAction is null)
            {
                Password = "VelvetSweatshop";
                if (!Encryption.VerifyPassword(Password))
                {
                    Stream = null;
                    return false;
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

                    if (Encryption.VerifyPassword(Password))
                        break;
                }

                if (Try >= RetryCount)
                {
                    Stream = null;
                    return false;
                }
            }

            byte[] SecretKey = Encryption.GenerateSecretKey(Password);
            Stream PackageStream = new CompoundStream(Document, FileStream, PackageEntry, false);

            Stream = Encryption.CreateEncryptedPackageStream(PackageStream, SecretKey);
            return true;
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

        private static bool IsCsv(string FilePath)
            => FilePath.Length > 4 &&
               FilePath.Substring(FilePath.Length - 4, 4)
                       .ToLower()
                       .Equals(".csv");

    }
}
