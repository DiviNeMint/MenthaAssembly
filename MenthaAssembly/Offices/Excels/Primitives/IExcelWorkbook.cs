using System;
using System.Collections.Generic;

namespace MenthaAssembly.Offices
{
    public interface IExcelWorkbook : IDisposable
    {
        public IExcelSheet this[int Index] { get; }
        
        public IExcelSheet this[string Name] { get; }

        public IReadOnlyList<IExcelSheet> Sheets { get; }

        internal List<string> SharedStrings { get; }

    }
}
