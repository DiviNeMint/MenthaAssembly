using MenthaAssembly.Offices.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using static MenthaAssembly.Offices.Primitives.XmlReaderExcelHelper;

namespace MenthaAssembly.Offices
{
    internal class XlsxWorkbook : IExcelWorkbook
    {
        internal static readonly XmlReaderSettings XmlSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            XmlResolver = null,
        };

        public string FilePath { get; }

        private bool _IsDate1904;
        public bool IsDate1904 => _IsDate1904;

        public IExcelSheet this[int Index]
            => Index < _Sheets.Count ? _Sheets[Index] : null;
        public IExcelSheet this[string Name]
            => _Sheets.FirstOrDefault(i => i.Name.Equals(Name));

        private List<XlsxSheet> _Sheets = new List<XlsxSheet>();
        public IReadOnlyList<IExcelSheet> Sheets => _Sheets;

        private List<string> SharedStrings = new List<string>();
        List<string> IExcelWorkbook.SharedStrings
            => SharedStrings;


        public XlsxWorkbook(string FilePath, ZipArchive Archive)
        {
            this.FilePath = FilePath;
            ZipArchiveEntry FindEntry(string Name)
                => Archive.Entries.FirstOrDefault(i => i.FullName.Equals(Name)) is ZipArchiveEntry Entry ? Entry : null;

            List<XlsxSheetInfo> SheetInfos = null;
            #region Load WorkbookInfo & SheetInfos
            {
                if (FindEntry("xl/workbook.xml") is ZipArchiveEntry XmlEntry)
                {
                    using Stream EntryStream = XmlEntry.Open();
                    using XmlReader Reader = XmlReader.Create(EntryStream, XmlSettings);
                    SheetInfos = LoadXmlSheetInfos(Reader).ToList();
                }
                else if (FindEntry("xl/workbook.bin") is ZipArchiveEntry BinEntry)
                {
                    using Stream EntryStream = BinEntry.Open();
                    using XlsxBiffReader Reader = new XlsxBiffReader(EntryStream);
                    SheetInfos = LoadBiffSheetInfos(Reader).ToList();
                }
                else
                    throw ExcelException.ZipNoOpenXml;
            }
            #endregion
            #region Load SheetRelationships
            {
                Stream Stream = FindEntry("xl/_rels/workbook.xml.rels") is ZipArchiveEntry XmlEntry ? XmlEntry.Open() :
                                FindEntry("xl/_rels/workbook.bin.rels") is ZipArchiveEntry BinEntry ? BinEntry.Open() :
                                null;
                if (Stream != null)
                {
                    try
                    {
                        using XmlReader Reader = XmlReader.Create(Stream);
                        LoadSheetRelationships(Reader, SheetInfos);
                    }
                    finally
                    {
                        Stream.Dispose();
                    }
                }
            }
            #endregion
            #region Load SharedStrings
            {
                if (FindEntry("xl/sharedStrings.xml") is ZipArchiveEntry XmlEntry)
                {
                    using Stream EntryStream = XmlEntry.Open();
                    using XmlReader Reader = XmlReader.Create(EntryStream, XmlSettings);
                    LoadXmlSharedStrings(Reader);
                }
                else if (FindEntry("xl/sharedStrings.bin") is ZipArchiveEntry BinEntry)
                {
                    using Stream EntryStream = BinEntry.Open();
                    using XlsxBiffReader Reader = new XlsxBiffReader(EntryStream);
                    LoadBiffSharedStrings(Reader);
                }
            }
            #endregion
            #region Load Style
            {
                //if (FindEntry("xl/styles.xml") is ZipArchiveEntry XmlEntry)
                //{
                //    using Stream EntryStream = XmlEntry.Open();
                //    using XmlReader Reader = XmlReader.Create(EntryStream, XmlSettings);
                //    LoadXmlStyles(Reader);
                //}
                //else if (FindEntry("xl/styles.bin") is ZipArchiveEntry BinEntry)
                //{
                //    using Stream EntryStream = BinEntry.Open();
                //    LoadBiffStyles(EntryStream);
                //}
            }
            #endregion
            #region Load Sheets
            foreach (XlsxSheetInfo Info in SheetInfos)
            {
                string SheetPath = Info.Path;

                // its possible sheetPath starts with /xl. in this case trim the /
                // see the test "Issue_11522_OpenXml"
                SheetPath = SheetPath.StartsWith("/xl/", StringComparison.OrdinalIgnoreCase) ? SheetPath.Substring(1) : $"xl/{SheetPath}";

                if (FindEntry(SheetPath) is ZipArchiveEntry Entry)
                {
                    string Extension = Path.GetExtension(SheetPath);
                    if (Extension.Equals(".xml"))
                    {
                        if (Entry.TryExtract(out Stream CompressedStream, out Func<Stream, Stream> GetDecompressor))
                        {
                            _Sheets.Add(new XlsxSheet(this, Info, CompressedStream, GetDecompressor, XmlSettings));
                        }
                        else
                        {
                            using Stream EntryStream = Entry.Open();
                            _Sheets.Add(new XlsxSheet(this, Info, EntryStream, null, XmlSettings));
                        }
                    }
                    else if (Extension.Equals(".bin"))
                    {
                        if (Entry.TryExtract(out Stream CompressedStream, out Func<Stream, Stream> GetDecompressor))
                        {
                            _Sheets.Add(new XlsxSheet(this, Info, CompressedStream, GetDecompressor));
                        }
                        else
                        {
                            using Stream EntryStream = Entry.Open();
                            _Sheets.Add(new XlsxSheet(this, Info, EntryStream, null));
                        }
                    }
                }
            }
            #endregion
        }

        private IEnumerable<XlsxSheetInfo> LoadXmlSheetInfos(XmlReader Reader)
        {
            if (!Reader.IsStartElement(ElementWorkbook, NsSpreadsheetMl))
                yield break;

            if (!Reader.ReadFirstContent())
                yield break;

            while (!Reader.EOF)
            {
                if (Reader.IsStartElement(ElementWorkbookProperties, NsSpreadsheetMl))
                {
                    // Workbook VBA CodeName: reader.GetAttribute("codeName");
                    _IsDate1904 = Reader.GetAttribute("date1904") == "1";
                    Reader.Skip();
                }
                else if (Reader.IsStartElement(ElementSheets, NsSpreadsheetMl))
                {
                    if (!Reader.ReadFirstContent())
                        continue;

                    while (!Reader.EOF)
                    {
                        if (Reader.IsStartElement(ElementSheet, NsSpreadsheetMl))
                        {
                            yield return new XlsxSheetInfo(Reader.GetAttribute(AttributeName),
                                                           uint.Parse(Reader.GetAttribute(AttributeSheetId)),
                                                           Reader.GetAttribute(AttributeRelationshipId, NsDocumentRelationship),
                                                           Reader.GetAttribute(AttributeVisibleState));
                            Reader.Skip();
                        }
                        else if (!Reader.SkipContent())
                        {
                            break;
                        }
                    }
                }
                else if (!Reader.SkipContent())
                {
                    yield break;
                }
            }
        }
        private IEnumerable<XlsxSheetInfo> LoadBiffSheetInfos(XlsxBiffReader Reader)
        {
            while (Reader.ReadVariable(out int ID, out _))
            {
                switch (ID)
                {
                    // WorkbookPr
                    case 0x99:
                        {
                            _IsDate1904 = (Reader.Read<byte>() & 0x01) == 1;
                            break;
                        }

                    // Sheet
                    case 0x9C:
                        {
                            uint VisibleState = Reader.Read<uint>(),
                                 SheetId = Reader.Read<uint>();
                            string Rid = Reader.ReadString(),
                                   Name = Reader.ReadString();

                            yield return new XlsxSheetInfo(Name, SheetId, Rid, VisibleState == 0);
                            break;
                        }
                }
            }
        }

        private void LoadSheetRelationships(XmlReader Reader, List<XlsxSheetInfo> SheetInfos)
        {
            if (!Reader.IsStartElement(ElementRelationships, NsRelationship))
                return;

            if (!Reader.ReadFirstContent())
                return;

            while (!Reader.EOF)
            {
                if (Reader.IsStartElement(ElementRelationship, NsRelationship))
                {
                    string Rid = Reader.GetAttribute(AttributeId);
                    if (SheetInfos.FirstOrDefault(i => i.Rid.Equals(Rid)) is XlsxSheetInfo Info)
                        Info.Path = Reader.GetAttribute(AttributeTarget);

                    Reader.Skip();
                }
                else if (!Reader.SkipContent())
                {
                    break;
                }
            }
        }

        private void LoadXmlSharedStrings(XmlReader Reader)
        {
            if (!Reader.IsStartElement(ElementSst, NsSpreadsheetMl))
                return;

            if (!Reader.ReadFirstContent())
                return;

            while (!Reader.EOF)
            {
                if (Reader.IsStartElement(ElementStringItem, NsSpreadsheetMl))
                {
                    SharedStrings.Add(Reader.ReadStringItem());
                }
                else if (!Reader.SkipContent())
                {
                    break;
                }
            }
        }
        private void LoadBiffSharedStrings(XlsxBiffReader Reader)
        {
            while (Reader.ReadVariable(out int ID, out _))
            {
                // String
                if (ID == 0x13)
                {
                    Reader.Skip(1);

                    string SharedString = Reader.ReadString();
                    SharedStrings.Add(SharedString);
                }
            }
        }

        private void LoadXmlStyles(XmlReader Reader)
        {
            //if (!Reader.IsStartElement(ElementStyleSheet, NsSpreadsheetMl))
            //    return;

            //if (!Reader.ReadFirstContent())
            //    return;

            //while (!Reader.EOF)
            //{
            //    if (Reader.IsStartElement(ElementCellCrossReference, NsSpreadsheetMl))
            //    {
            //        //foreach (var xf in ReadCellXfs())
            //        //    yield return new ExtendedFormatRecord(xf);
            //    }
            //    else if (Reader.IsStartElement(ElementCellStyleCrossReference, NsSpreadsheetMl))
            //    {
            //        //foreach (var xf in ReadCellXfs())
            //        //    yield return new CellStyleExtendedFormatRecord(xf);
            //    }
            //    else if (Reader.IsStartElement(ElementNumberFormats, NsSpreadsheetMl))
            //    {
            //        if (!Reader.ReadFirstContent())
            //            continue;

            //        while (!Reader.EOF)
            //        {
            //            if (Reader.IsStartElement(NNumFmt, NsSpreadsheetMl))
            //            {
            //                if (int.TryParse(Reader.GetAttribute(ANumFmtId), out int NumFmtId))
            //                {
            //                    string FormatCode = Reader.GetAttribute(AFormatCode);
            //                    //yield return new NumberFormatRecord(numFmtId, formatCode);
            //                }

            //                Reader.Skip();
            //            }
            //            else if (!Reader.SkipContent())
            //            {
            //                break;
            //            }
            //        }
            //    }
            //    else if (!Reader.SkipContent())
            //    {
            //        break;
            //    }
            //}
        }
        private void LoadBiffStyles(Stream Stream)
        {
            //new BiffWorkbookReader(Entry.Open());
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
