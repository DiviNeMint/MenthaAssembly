using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MenthaAssembly.Offices
{
    internal class CsvWorkbook : IExcelWorkbook
    {
        public string FilePath { get; }

        public IExcelSheet this[int Index]
            => Index < _Sheets.Count ? _Sheets[Index] : null;
        public IExcelSheet this[string Name]
            => _Sheets.FirstOrDefault(i => i.Name.Equals(Name));

        private List<CsvSheet> _Sheets = new List<CsvSheet>();
        public IReadOnlyList<IExcelSheet> Sheets => _Sheets;

        List<string> IExcelWorkbook.SharedStrings
            => null;

        public CsvWorkbook(string FilePath, Stream Stream, Encoding Encoding)
        {
            this.FilePath = FilePath;
            _Sheets.Add(new CsvSheet(this, Stream, Encoding));
        }

        private bool IsDisposed = false;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            _Sheets.ForEach(i => i.Dispose());
            _Sheets.Clear();
            _Sheets = null;

            IsDisposed = true;
        }

    }
}