using System.Runtime.InteropServices;

namespace MenthaAssembly.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageImportDescriptor
    {
        /// <summary>
        /// 0 for terminating null import descriptor; RVA to original unbound IAT (PIMAGE_THUNK_DATA)
        /// </summary>
        public int Characteristics;

        /// <summary>
        /// 0 if not bound, -1 if bound, and real date\time stamp in IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT (new BIND); O.W. date/time stamp of DLL bound to (Old BIND)
        /// </summary>
        public uint TimeDateStamp;

        /// <summary>
        /// -1 if no forwarders
        /// </summary>
        public uint ForwarderChain;

        public int Name;

        /// <summary>
        /// RVA to IAT (if bound this IAT has actual addresses)
        /// </summary>
        public int FirstThunk;
    }
}