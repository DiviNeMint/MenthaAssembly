using System;
using System.Collections.Generic;
using System.Text;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum LoadLibraryFlag : uint
    {
        None = 0,
        DONT_RESOLVE_DLL_REFERENCES = 0x0001,

        LOAD_LIBRARY_AS_DATAFILE = 0x0002,

        LOAD_WITH_ALTERED_SEARCH_PATH = 0x0008,

        LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x0010,

        LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x0020,

        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x0040,

        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x0100,

        LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x0200,

        LOAD_LIBRARY_SEARCH_USER_DIRS = 0x0400,

        LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x0800,

        LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x1000,

    }
}
