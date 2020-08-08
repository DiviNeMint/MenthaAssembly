using System;

namespace MenthaAssembly.Win32
{
    [Flags]
    internal enum MemFreeType : uint
    {
        /// <summary>
        /// To coalesce two adjacent placeholders, specify Release | Coalesce_Placeholders. 
        /// When you coalesce placeholders, lpAddress and dwSize must exactly match those of the placeholder. 
        /// </summary>
        Coalesce_Placeholders = 0x01,
        /// <summary>
        /// Frees an allocation back to a placeholder (after you've replaced a placeholder with a private allocation using VirtualAlloc2 or Virtual2AllocFromApp). 
        /// To split a placeholder into two placeholders, specify Release | Preserve_Placeholder.
        /// </summary>
        Preserve_Placeholder = 0x02,

        /// <summary>
        /// Decommits the specified region of committed pages. 
        /// After the operation, the pages are in the reserved state. 
        /// The function does not fail if you attempt to decommit an uncommitted page.
        /// This means that you can decommit a range of pages without first determining the current commitment state.
        /// The MEM_DECOMMIT value is not supported when the lpAddress parameter provides the base address for an enclave.
        /// </summary>
        Decommit = 0x4000,
        /// <summary>
        /// Releases the specified region of pages, or placeholder (for a placeholder, the address space is released and available for other allocations). 
        /// After this operation, the pages are in the free state.
        /// If you specify this value, dwSize must be 0 (zero), and lpAddress must point to the base address returned by the VirtualAlloc function when the region is reserved.
        /// The function fails if either of these conditions is not met.
        /// If any pages in the region are committed currently, the function first decommits, and then releases them.
        /// The function does not fail if you attempt to release pages that are in different states, some reserved and some committed.
        /// This means that you can release a range of pages without first determining the current commitment state.
        /// </summary>
        Release = 0x8000,

    }
}
