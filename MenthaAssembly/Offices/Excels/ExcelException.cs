using System;

namespace MenthaAssembly.Offices
{
    public class ExcelException : Exception
    {
        public static ExcelException StreamWorkbookNotFound => new ExcelException("Neither stream 'Workbook' nor 'Book' was found in file.");
        public static ExcelException WorkbookIsNotStream => new ExcelException("Workbook directory entry is not a Stream.");
        public static ExcelException WorkbookGlobalsInvalidData => new ExcelException("Error reading Workbook Globals - Stream has invalid data.");
        public static ExcelException FatBadSector => new ExcelException("Error reading as FAT table : There's no such sector in FAT.");
        public static ExcelException FatRead => new ExcelException("Error reading stream from FAT area.");
        public static ExcelException EndOfFile => new ExcelException("The excel file may be corrupt or truncated. We've read past the end of the file.");
        public static ExcelException CyclicSectorChain => new ExcelException("Cyclic sector chain in compound document.");
        public static ExcelException HeaderSignature => new ExcelException("Invalid file signature.");
        public static ExcelException HeaderOrder => new ExcelException("Invalid byte order specified in header.");
        public static ExcelException BiffRecordSize => new ExcelException("Buffer size is less than minimum BIFF record size.");
        public static ExcelException BiffIlegalBefore => new ExcelException("BIFF Stream error: Moving before stream start.");
        public static ExcelException BiffIlegalAfter => new ExcelException("BIFF Stream error: Moving after stream end.");

        public static ExcelException DirectoryEntryArray => new ExcelException("Directory Entry error: Array is too small.");
        public static ExcelException CompoundNoOpenXml => new ExcelException("Detected compound document, but not a valid OpenXml file.");
        public static ExcelException ZipNoOpenXml => new ExcelException("Detected ZIP file, but not a valid OpenXml file.");
        public static ExcelException InvalidPassword => new ExcelException("Invalid password.");

        public ExcelException(string Message) : base(Message)
        {
        }

    }
}
