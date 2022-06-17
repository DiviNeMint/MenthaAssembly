using System.Text;
using System.Xml;

namespace MenthaAssembly.Offices.Primitives
{
    internal static class XmlReaderExcelHelper
    {
        public const string NsSpreadsheetMl = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        public const string NsDocumentRelationship = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public const string NsRelationship = "http://schemas.openxmlformats.org/package/2006/relationships";

        public const string ElementWorkbook = "workbook";
        public const string ElementWorkbookProperties = "workbookPr";
        public const string ElementSheets = "sheets";
        public const string ElementSheet = "sheet";

        public const string AttributeSheetId = "sheetId";
        public const string AttributeVisibleState = "state";
        public const string AttributeName = "name";
        public const string AttributeRelationshipId = "id";

        public const string ElementRelationship = "Relationship";
        public const string ElementRelationships = "Relationships";
        public const string AttributeId = "Id";
        public const string AttributeTarget = "Target";

        public const string ElementSst = "sst";
        public const string ElementStringItem = "si";

        public const string ElementStyleSheet = "styleSheet";

        public const string ANumFmtId = "numFmtId";

        public const string ElementCellCrossReference = "cellXfs";
        public const string ElementCellStyleCrossReference = "cellStyleXfs";
        public const string NXF = "xf";
        public const string AXFId = "xfId";
        public const string AApplyNumberFormat = "applyNumberFormat";
        public const string AApplyAlignment = "applyAlignment";
        public const string AApplyProtection = "applyProtection";

        public const string ElementNumberFormats = "numFmts";
        public const string NNumFmt = "numFmt";
        public const string AFormatCode = "formatCode";

        public const string NAlignment = "alignment";
        public const string AIndent = "indent";
        public const string AHorizontal = "horizontal";
        public const string NProtection = "protection";

        public const string NWorksheet = "worksheet";
        public const string NSheetData = "sheetData";
        public const string NRow = "row";
        public const string ARef = "ref";
        public const string AR = "r";
        public const string NV = "v";
        public const string NIs = "is";
        public const string AT = "t";
        public const string AS = "s";

        public const string NC = "c"; // cell

        public const string NMergeCells = "mergeCells";

        public const string NSheetProperties = "sheetPr";
        public const string NSheetFormatProperties = "sheetFormatPr";
        public const string ADefaultRowHeight = "defaultRowHeight";

        public const string NHeaderFooter = "headerFooter";
        public const string ADifferentFirst = "differentFirst";
        public const string ADifferentOddEven = "differentOddEven";
        public const string NFirstHeader = "firstHeader";
        public const string NFirstFooter = "firstFooter";
        public const string NOddHeader = "oddHeader";
        public const string NOddFooter = "oddFooter";
        public const string NEvenHeader = "evenHeader";
        public const string NEvenFooter = "evenFooter";

        public const string NCols = "cols";
        public const string NCol = "col";
        public const string AMin = "min";
        public const string AMax = "max";
        public const string ALocked = "locked";
        public const string AHidden = "hidden";
        public const string AWidth = "width";
        public const string ACustomWidth = "customWidth";

        public const string NMergeCell = "mergeCell";

        public const string ACustomHeight = "customHeight";
        public const string AHt = "ht";

        public const string ElementT = "t";
        public const string ElementR = "r";

        public static readonly XmlReaderSettings XmlSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            XmlResolver = null,
        };

        public static bool ReadFirstContent(this XmlReader This)
        {
            if (This.IsEmptyElement)
            {
                This.Read();
                return false;
            }

            This.MoveToContent();
            This.Read();
            return true;
        }

        public static string ReadStringItem(this XmlReader Reader)
        {
            if (!Reader.ReadFirstContent())
                return string.Empty;

            StringBuilder Builder = new StringBuilder();
            try
            {
                while (!Reader.EOF)
                {
                    if (Reader.IsStartElement(ElementT, NsSpreadsheetMl))
                    {
                        // There are multiple <t> in a <si>. Concatenate <t> within an <si>.
                        Builder.Append(Reader.ReadElementContentAsString());
                    }
                    else if (Reader.IsStartElement(ElementR, NsSpreadsheetMl))
                    {
                        Builder.Append(ReadRichTextRun(Reader));
                    }
                    else if (!Reader.SkipContent())
                    {
                        break;
                    }
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }
        private static string ReadRichTextRun(XmlReader Reader)
        {
            if (!Reader.ReadFirstContent())
                return string.Empty;

            StringBuilder Builder = new StringBuilder();
            try
            {
                while (!Reader.EOF)
                {
                    if (Reader.IsStartElement(ElementT, NsSpreadsheetMl))
                    {
                        Builder.Append(Reader.ReadElementContentAsString());
                    }
                    else if (!Reader.SkipContent())
                    {
                        break;
                    }
                }

                return Builder.ToString();
            }
            finally
            {
                Builder.Clear();
            }
        }

        public static ExcelHeaderFooter ReadHeaderFooter(this XmlReader Reader)
        {
            if (!Reader.ReadFirstContent())
                return null;

            ExcelHeaderFooter HeaderFooter = new ExcelHeaderFooter
            {
                _HasDifferentFirst = Reader.GetAttribute(ADifferentFirst) == "1",
                _HasDifferentOddEven = Reader.GetAttribute(ADifferentOddEven) == "1"
            };

            while (!Reader.EOF)
            {
                if (Reader.IsStartElement(NOddHeader, NsSpreadsheetMl))
                {
                    HeaderFooter._OddHeader = Reader.ReadElementContentAsString();
                }
                else if (Reader.IsStartElement(NOddFooter, NsSpreadsheetMl))
                {
                    HeaderFooter._OddFooter = Reader.ReadElementContentAsString();
                }
                else if (Reader.IsStartElement(NEvenHeader, NsSpreadsheetMl))
                {
                    HeaderFooter._EvenHeader = Reader.ReadElementContentAsString();
                }
                else if (Reader.IsStartElement(NEvenFooter, NsSpreadsheetMl))
                {
                    HeaderFooter._EvenFooter = Reader.ReadElementContentAsString();
                }
                else if (Reader.IsStartElement(NFirstHeader, NsSpreadsheetMl))
                {
                    HeaderFooter._FirstHeader = Reader.ReadElementContentAsString();
                }
                else if (Reader.IsStartElement(NFirstFooter, NsSpreadsheetMl))
                {
                    HeaderFooter._FirstFooter = Reader.ReadElementContentAsString();
                }
                else if (!Reader.SkipContent())
                {
                    break;
                }
            }

            return HeaderFooter;
        }

        public static bool SkipContent(this XmlReader This)
        {
            if (This.NodeType == XmlNodeType.EndElement)
            {
                This.Read();
                return false;
            }

            This.Skip();
            return true;
        }




    }
}
