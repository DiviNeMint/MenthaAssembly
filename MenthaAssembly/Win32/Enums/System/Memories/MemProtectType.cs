﻿using System;

namespace MenthaAssembly.Win32
{
    /// <summary>
    /// Memory Protection Constants
    /// </summary>
    [Flags]
    internal enum MemProtectType : uint
    {
        /// <summary>
        ///	Disables all access to the committed region of pages. 
        ///	An attempt to read from, write to, or execute the committed region results in an access violation.
        ///	This flag is not supported by the CreateFileMapping function.
        /// </summary>
        Page_NoAccess = 0x01,
        /// <summary>	
        ///	Enables read-only access to the committed region of pages. 
        ///	An attempt to write to the committed region results in an access violation. 
        ///	If Data Execution Prevention is enabled, an attempt to execute code in the committed region results in an access violation.
        /// </summary>	
        Page_ReadOnly = 0x02,
        /// <summary>	
        ///	Enables read-only or read/write access to the committed region of pages. 
        ///	If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
        /// </summary>	
        Page_ReadWrite = 0x04,
        /// <summary>	
        ///	Enables read-only or copy-on-write access to a mapped view of a file mapping object. 
        ///	An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process. 
        ///	The private page is marked as PAGE_READWRITE, and the change is written to the new page. 
        ///	If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
        ///	This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.
        /// </summary>	
        Page_WriteCopy = 0x08,

        /// <summary>
        /// Enables execute access to the committed region of pages. 
        /// An attempt to write to the committed region results in an access violation.
        /// This flag is not supported by the CreateFileMapping function.
        /// </summary>
        Page_Execute = 0x10,
        /// <summary>
        /// Enables execute or read-only access to the committed region of pages. 
        /// An attempt to write to the committed region results in an access violation.
        /// Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function until Windows XP with SP2 and Windows Server 2003 with SP1.
        /// </summary>
        Page_Execute_Read = 0x20,
        /// <summary>
        /// Enables execute, read-only, or read/write access to the committed region of pages.
        /// Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function until Windows XP with SP2 and Windows Server 2003 with SP1.
        /// </summary>
        Page_Execute_ReadWrite = 0x40,
        /// <summary>
        /// Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. 
        /// An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process.
        /// The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
        /// This flag is not supported by the VirtualAlloc or VirtualAllocEx functions. 
        /// Windows Vista, Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function until Windows Vista with SP1 and Windows Server 2008.
        /// </summary>
        Page_Execute_WriteCopy = 0x80,

        /// <summary>
        /// Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. 
        /// Guard pages thus act as a one-time access alarm.
        /// For more information, see Creating Guard Pages.
        ///	When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
        ///	If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
        ///	This value cannot be used with PAGE_NOACCESS.
        ///	This flag is not supported by the CreateFileMapping function.
        /// </summary>
        Page_Guard = 0x100,
        /// <summary>
        ///	Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a device. 
        ///	Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///	The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.
        ///	The PAGE_NOCACHE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx, or VirtualAllocExNuma functions.
        ///	To enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag when calling the CreateFileMapping function.
        /// </summary>
        Page_NoCache = 0x200,
        /// <summary>
        ///	Sets all pages to be write-combined.
        ///	Applications should not use this attribute except when explicitly required for a device. 
        ///	Using the interlocked functions with memory that is mapped as write-combined can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///	The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags.
        ///	The PAGE_WRITECOMBINE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx, or VirtualAllocExNuma functions.
        ///	To enable write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when calling the CreateFileMapping function.
        ///	Windows Server 2003 and Windows XP: This flag is not supported until Windows Server 2003 with SP1.
        /// </summary>
        Page_WriteCombine = 0x400,

        /// <summary>
        /// Sets all locations in the pages as invalid targets for CFG. 
        /// Used along with any execute page protection like PAGE_EXECUTE, PAGE_EXECUTE_READ, PAGE_EXECUTE_READWRITE and PAGE_EXECUTE_WRITECOPY. 
        /// Any indirect call to locations in those pages will fail CFG checks and the process will be terminated. The default behavior for executable pages allocated is to be marked valid call targets for CFG.
        /// This flag is not supported by the VirtualProtect or CreateFileMapping functions.
        /// </summary>
        Page_Targets_Invalid = 0x40000000,
        /// <summary>
        /// Pages in the region will not have their CFG information updated while the protection changes for VirtualProtect.
        /// For example, if the pages in the region was allocated using PAGE_TARGETS_INVALID, then the invalid information will be maintained while the page protection changes. 
        /// This flag is only valid when the protection changes to an executable type like PAGE_EXECUTE, PAGE_EXECUTE_READ, PAGE_EXECUTE_READWRITE and PAGE_EXECUTE_WRITECOPY. 
        /// The default behavior for VirtualProtect protection change to executable is to mark all locations as valid call targets for CFG.
        /// </summary>
        Page_Targets_No_Update = 0x40000000,
    }
}
