using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reflection
{
    internal sealed class PEImage
    {
        private struct PESection
        {
            public uint VirtualAddress;
            public uint VirtualSize;
            public uint SizeOfRawData;
            public uint PointerToRawData;
        }

        private readonly byte[] RawData;
        private readonly PESection[] Sections;

        private readonly int MetadataOffset;
        private readonly int MetadataSize;

        private readonly int StringsHeapOffset;
        private readonly int StringsHeapSize;

        private readonly int BlobHeapOffset;
        private readonly int BlobHeapSize;

        private readonly int TablesHeapOffset;

        private readonly bool StringIndexIsLarge;
        private readonly bool BlobIndexIsLarge;
        private readonly bool GuidIndexIsLarge;

        private readonly ulong ValidTables;
        private readonly uint[] TableRowCounts = new uint[64];
        private readonly int[] TableOffsets = new int[64];
        private readonly int[] TableRowSizes = new int[64];

        public PEImage(byte[] RawData)
        {
            this.RawData = RawData ?? throw new ArgumentNullException(nameof(RawData));

            // DOS Header
            if (ReadUInt16(0) != 0x5A4D) // MZ
                throw new BadImageFormatException("Invalid DOS header.");

            int peOffset = ReadInt32(0x3C);
            if (ReadUInt32(peOffset) != 0x00004550) // PE\0\0
                throw new BadImageFormatException("Invalid PE header.");

            int fileHeaderOffset = peOffset + 4;
            ushort numberOfSections = ReadUInt16(fileHeaderOffset + 2);
            ushort sizeOfOptionalHeader = ReadUInt16(fileHeaderOffset + 16);

            int optionalHeaderOffset = fileHeaderOffset + 20;
            ushort magic = ReadUInt16(optionalHeaderOffset);

            int dataDirectoryOffset;
            switch (magic)
            {
                case 0x10B: // PE32
                    dataDirectoryOffset = optionalHeaderOffset + 96;
                    break;
                case 0x20B: // PE32+
                    dataDirectoryOffset = optionalHeaderOffset + 112;
                    break;
                default:
                    throw new BadImageFormatException("Invalid optional header.");
            }

            // CLI Header = DataDirectory[14]
            int cliHeaderDirectoryOffset = dataDirectoryOffset + (14 * 8);
            uint cliHeaderRva = ReadUInt32(cliHeaderDirectoryOffset);
            uint cliHeaderSize = ReadUInt32(cliHeaderDirectoryOffset + 4);
            if (cliHeaderRva == 0 || cliHeaderSize == 0)
                throw new BadImageFormatException("Not a managed assembly.");

            int sectionHeaderOffset = optionalHeaderOffset + sizeOfOptionalHeader;
            Sections = new PESection[numberOfSections];
            for (int i = 0; i < numberOfSections; i++)
            {
                int offset = sectionHeaderOffset + (40 * i);

                uint virtualSize = ReadUInt32(offset + 8);
                uint virtualAddress = ReadUInt32(offset + 12);
                uint sizeOfRawData = ReadUInt32(offset + 16);
                uint pointerToRawData = ReadUInt32(offset + 20);

                Sections[i] = new PESection
                {
                    VirtualAddress = virtualAddress,
                    VirtualSize = virtualSize,
                    SizeOfRawData = sizeOfRawData,
                    PointerToRawData = pointerToRawData,
                };
            }

            int cliHeaderOffset = RvaToOffset(cliHeaderRva);
            uint metadataRva = ReadUInt32(cliHeaderOffset + 8);
            uint metadataSize = ReadUInt32(cliHeaderOffset + 12);

            MetadataOffset = RvaToOffset(metadataRva);
            MetadataSize = checked((int)metadataSize);

            if (MetadataOffset < 0 || MetadataSize <= 0 || MetadataOffset + MetadataSize > RawData.Length)
                throw new BadImageFormatException("Invalid CLR metadata range.");

            // Metadata Root
            int md = MetadataOffset;
            if (ReadUInt32(md) != 0x424A5342) // BSJB
                throw new BadImageFormatException("Invalid CLR metadata root.");

            int versionLength = ReadInt32(md + 12);
            int streamCountOffset = Align4(md + 16 + versionLength);
            ushort streamCount = ReadUInt16(streamCountOffset + 2);

            int streamHeaderOffset = streamCountOffset + 4;

            int stringsHeapOffset = -1;
            int stringsHeapSize = 0;

            int blobHeapOffset = -1;
            int blobHeapSize = 0;

            int tablesHeapOffset = -1;

            for (int i = 0; i < streamCount; i++)
            {
                uint offset = ReadUInt32(streamHeaderOffset);
                uint size = ReadUInt32(streamHeaderOffset + 4);

                int nameOffset = streamHeaderOffset + 8;
                string name = ReadZeroTerminatedAscii(nameOffset, out int consumedNameBytes);
                int alignedNameBytes = Align4(consumedNameBytes);

                int absoluteOffset = checked(MetadataOffset + (int)offset);
                int absoluteEnd = checked(absoluteOffset + (int)size);
                if (absoluteOffset < MetadataOffset || absoluteEnd > MetadataOffset + MetadataSize)
                    throw new BadImageFormatException($"Metadata stream '{name}' is out of metadata bounds.");

                switch (name)
                {
                    case "#Strings":
                        stringsHeapOffset = absoluteOffset;
                        stringsHeapSize = checked((int)size);
                        break;

                    case "#Blob":
                        blobHeapOffset = absoluteOffset;
                        blobHeapSize = checked((int)size);
                        break;

                    case "#~":
                    case "#-":
                        tablesHeapOffset = absoluteOffset;
                        break;
                }

                streamHeaderOffset += 8 + alignedNameBytes;
            }

            if (stringsHeapOffset < 0 || blobHeapOffset < 0 || tablesHeapOffset < 0)
                throw new BadImageFormatException("Required metadata streams not found.");

            StringsHeapOffset = stringsHeapOffset;
            StringsHeapSize = stringsHeapSize;

            BlobHeapOffset = blobHeapOffset;
            BlobHeapSize = blobHeapSize;

            TablesHeapOffset = tablesHeapOffset;

            // Tables Stream
            int tables = TablesHeapOffset;
            EnsureRange(tables, 24, "Metadata tables header");

            byte heapSizes = RawData[tables + 6];
            StringIndexIsLarge = (heapSizes & 0x01) != 0;
            GuidIndexIsLarge = (heapSizes & 0x02) != 0;
            BlobIndexIsLarge = (heapSizes & 0x04) != 0;

            ValidTables = ReadUInt64(tables + 8);

            int rowCountOffset = tables + 24;
            for (int table = 0; table < 64; table++)
            {
                if (((ValidTables >> table) & 1UL) != 0)
                {
                    TableRowCounts[table] = ReadUInt32(rowCountOffset);
                    rowCountOffset += 4;
                }
            }

            BuildTableLayout(rowCountOffset);
        }

        public bool TryReadAssemblyAttributeTypeFullNames(out string[] FullNames)
        {
            try
            {
                FullNames = ReadAssemblyAttributeTypeFullNames();
                return true;
            }
            catch
            {
                FullNames = Array.Empty<string>();
                return false;
            }
        }

        private string[] ReadAssemblyAttributeTypeFullNames()
        {
            const int AssemblyTable = 32;
            const int CustomAttributeTable = 12;

            if (TableRowCounts[AssemblyTable] == 0 || TableRowCounts[CustomAttributeTable] == 0)
                return Array.Empty<string>();

            uint assemblyRid = 1;
            uint assemblyParentToken = EncodeHasCustomAttribute(AssemblyTable, assemblyRid);

            List<string> results = new List<string>();

            int rowCount = checked((int)TableRowCounts[CustomAttributeTable]);
            for (uint rid = 1; rid <= rowCount; rid++)
            {
                int rowOffset = GetTableRowOffset(CustomAttributeTable, rid);

                int parentSize = GetCodedIndexSize(HasCustomAttributeTables, 5);
                uint parent = ReadIndex(rowOffset, parentSize);
                rowOffset += parentSize;

                int typeSize = GetCodedIndexSize(CustomAttributeTypeTables, 3);
                uint type = ReadIndex(rowOffset, typeSize);

                if (parent != assemblyParentToken)
                    continue;

                string fullName;
                if (TryResolveCustomAttributeTypeFullName(type, out fullName) &&
                    !string.IsNullOrEmpty(fullName) &&
                    !results.Contains(fullName))
                {
                    results.Add(fullName);
                }
            }

            return results.ToArray();
        }

        private bool TryResolveCustomAttributeTypeFullName(uint codedIndex, out string FullName)
        {
            // CustomAttributeType:
            // 2 = MethodDef
            // 3 = MemberRef
            uint tag = codedIndex & 0x7;
            uint rid = codedIndex >> 3;

            switch (tag)
            {
                case 2: // MethodDef
                    uint ownerTypeRid;
                    if (TryGetMethodDefOwnerTypeDefRid(rid, out ownerTypeRid))
                        return TryGetTypeDefFullName(ownerTypeRid, out FullName);
                    break;

                case 3: // MemberRef
                    return TryGetMemberRefOwnerTypeFullName(rid, out FullName);
            }

            FullName = null;
            return false;
        }

        private bool TryGetMemberRefOwnerTypeFullName(uint memberRefRid, out string FullName)
        {
            const int MemberRefTable = 10;

            if (memberRefRid == 0 || memberRefRid > TableRowCounts[MemberRefTable])
            {
                FullName = null;
                return false;
            }

            int rowOffset = GetTableRowOffset(MemberRefTable, memberRefRid);

            uint classIndex = ReadIndex(rowOffset, GetCodedIndexSize(MemberRefParentTables, 3));
            uint tag = classIndex & 0x7;
            uint rid = classIndex >> 3;

            switch (tag)
            {
                case 0: // TypeDef
                    return TryGetTypeDefFullName(rid, out FullName);

                case 1: // TypeRef
                    return TryGetTypeRefFullName(rid, out FullName);

                case 3: // MethodDef
                    uint ownerTypeRid;
                    if (TryGetMethodDefOwnerTypeDefRid(rid, out ownerTypeRid))
                        return TryGetTypeDefFullName(ownerTypeRid, out FullName);
                    break;
            }

            FullName = null;
            return false;
        }

        private bool TryGetMethodDefOwnerTypeDefRid(uint methodDefRid, out uint TypeDefRid)
        {
            const int TypeDefTable = 2;
            const int MethodDefTable = 6;

            uint typeCount = TableRowCounts[TypeDefTable];
            if (typeCount == 0 || methodDefRid == 0 || methodDefRid > TableRowCounts[MethodDefTable])
            {
                TypeDefRid = 0;
                return false;
            }

            int stringIndexSize = GetStringIndexSize();
            int extendsSize = GetCodedIndexSize(TypeDefOrRefTables, 2);
            int fieldIndexSize = GetTableIndexSize(4);
            int methodIndexSize = GetTableIndexSize(6);

            for (uint typeRid = 1; typeRid <= typeCount; typeRid++)
            {
                int rowOffset = GetTableRowOffset(TypeDefTable, typeRid);

                rowOffset += 4;               // Flags
                rowOffset += stringIndexSize; // Name
                rowOffset += stringIndexSize; // Namespace
                rowOffset += extendsSize;     // Extends
                rowOffset += fieldIndexSize;  // FieldList

                uint methodList = ReadIndex(rowOffset, methodIndexSize);
                uint nextMethodList;

                if (typeRid < typeCount)
                {
                    int nextRowOffset = GetTableRowOffset(TypeDefTable, typeRid + 1);
                    nextRowOffset += 4;
                    nextRowOffset += stringIndexSize;
                    nextRowOffset += stringIndexSize;
                    nextRowOffset += extendsSize;
                    nextRowOffset += fieldIndexSize;
                    nextMethodList = ReadIndex(nextRowOffset, methodIndexSize);
                }
                else
                {
                    nextMethodList = TableRowCounts[MethodDefTable] + 1;
                }

                if (methodList <= methodDefRid && methodDefRid < nextMethodList)
                {
                    TypeDefRid = typeRid;
                    return true;
                }
            }

            TypeDefRid = 0;
            return false;
        }

        private bool TryGetTypeDefFullName(uint rid, out string FullName)
        {
            const int TypeDefTable = 2;
            const int NestedClassTable = 41;

            if (rid == 0 || rid > TableRowCounts[TypeDefTable])
            {
                FullName = null;
                return false;
            }

            int rowOffset = GetTableRowOffset(TypeDefTable, rid);
            rowOffset += 4; // Flags

            uint nameIndex = ReadIndex(rowOffset, GetStringIndexSize());
            rowOffset += GetStringIndexSize();

            uint namespaceIndex = ReadIndex(rowOffset, GetStringIndexSize());

            string name = ReadString(nameIndex);
            string ns = ReadString(namespaceIndex);

            uint enclosingRid = 0;
            if (TableRowCounts[NestedClassTable] > 0)
                TryGetEnclosingTypeDefRid(rid, out enclosingRid);

            string parentName;
            if (enclosingRid > 0 && TryGetTypeDefFullName(enclosingRid, out parentName))
            {
                string localName = ExtractTypeNameOnly(parentName) + "+" + name;
                string parentNs = ExtractNamespaceOnly(parentName);
                FullName = string.IsNullOrEmpty(parentNs) ? localName : parentNs + "." + localName;
                return true;
            }

            FullName = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            return true;
        }

        private bool TryGetTypeRefFullName(uint rid, out string FullName)
        {
            const int TypeRefTable = 1;

            if (rid == 0 || rid > TableRowCounts[TypeRefTable])
            {
                FullName = null;
                return false;
            }

            int rowOffset = GetTableRowOffset(TypeRefTable, rid);

            uint resolutionScope = ReadIndex(rowOffset, GetCodedIndexSize(ResolutionScopeTables, 2));
            rowOffset += GetCodedIndexSize(ResolutionScopeTables, 2);

            uint nameIndex = ReadIndex(rowOffset, GetStringIndexSize());
            rowOffset += GetStringIndexSize();

            uint namespaceIndex = ReadIndex(rowOffset, GetStringIndexSize());

            string name = ReadString(nameIndex);
            string ns = ReadString(namespaceIndex);

            uint scopeTag = resolutionScope & 0x3;
            uint scopeRid = resolutionScope >> 2;

            if (scopeTag == 3 && scopeRid > 0) // TypeRef (nested)
            {
                string parentName;
                if (TryGetTypeRefFullName(scopeRid, out parentName))
                {
                    string localName = ExtractTypeNameOnly(parentName) + "+" + name;
                    string parentNs = ExtractNamespaceOnly(parentName);
                    FullName = string.IsNullOrEmpty(parentNs) ? localName : parentNs + "." + localName;
                    return true;
                }
            }

            FullName = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            return true;
        }

        private bool TryGetEnclosingTypeDefRid(uint nestedRid, out uint enclosingRid)
        {
            const int NestedClassTable = 41;

            int rowCount = checked((int)TableRowCounts[NestedClassTable]);
            int nestedIndexSize = GetTableIndexSize(2);

            for (uint rid = 1; rid <= rowCount; rid++)
            {
                int rowOffset = GetTableRowOffset(NestedClassTable, rid);

                uint nested = ReadIndex(rowOffset, nestedIndexSize);
                rowOffset += nestedIndexSize;

                uint enclosing = ReadIndex(rowOffset, nestedIndexSize);

                if (nested == nestedRid)
                {
                    enclosingRid = enclosing;
                    return true;
                }
            }

            enclosingRid = 0;
            return false;
        }

        public bool TryReadReferencedAssemblyNames(out string[] Names)
        {
            try
            {
                Names = ReadReferencedAssemblyNames();
                return true;
            }
            catch
            {
                Names = Array.Empty<string>();
                return false;
            }
        }

        public bool TryReadUnmanagedDependencyAssemblyNames(out string[] Names)
        {
            try
            {
                Names = ReadUnmanagedDependencyAssemblyNames();
                return true;
            }
            catch
            {
                Names = Array.Empty<string>();
                return false;
            }
        }

        private string[] ReadUnmanagedDependencyAssemblyNames()
        {
            const int ModuleRefTable = 26;
            const int ImplMapTable = 28;

            if (TableRowCounts[ModuleRefTable] == 0 || TableRowCounts[ImplMapTable] == 0)
                return Array.Empty<string>();

            List<string> results = new List<string>();

            int memberForwardedSize = GetCodedIndexSize(MemberForwardedTables, 1);
            int moduleRefIndexSize = GetTableIndexSize(ModuleRefTable);
            int rowCount = checked((int)TableRowCounts[ImplMapTable]);
            for (uint rid = 1; rid <= rowCount; rid++)
            {
                int rowOffset = GetTableRowOffset(ImplMapTable, rid);

                rowOffset += 2; // MappingFlags
                rowOffset += memberForwardedSize; // MemberForwarded
                rowOffset += GetStringIndexSize(); // ImportName

                uint moduleRefRid = ReadIndex(rowOffset, moduleRefIndexSize);
                if (moduleRefRid == 0 || moduleRefRid > TableRowCounts[ModuleRefTable])
                    continue;

                int moduleRefRowOffset = GetTableRowOffset(ModuleRefTable, moduleRefRid);
                uint nameIndex = ReadIndex(moduleRefRowOffset, GetStringIndexSize());
                string name = ReadString(nameIndex);
                if (!string.IsNullOrWhiteSpace(name) &&
                    !results.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    results.Add(name);
                }
            }

            return results.ToArray();
        }

        private string[] ReadReferencedAssemblyNames()
        {
            const int AssemblyRefTable = 35;

            if (TableRowCounts[AssemblyRefTable] == 0)
                return Array.Empty<string>();

            List<string> results = new List<string>();

            int rowCount = checked((int)TableRowCounts[AssemblyRefTable]);
            for (uint rid = 1; rid <= rowCount; rid++)
            {
                int rowOffset = GetTableRowOffset(AssemblyRefTable, rid);

                rowOffset += 2; // MajorVersion
                rowOffset += 2; // MinorVersion
                rowOffset += 2; // BuildNumber
                rowOffset += 2; // RevisionNumber
                rowOffset += 4; // Flags

                rowOffset += GetBlobIndexSize();   // PublicKeyOrToken

                uint nameIndex = ReadIndex(rowOffset, GetStringIndexSize());
                rowOffset += GetStringIndexSize();

                rowOffset += GetStringIndexSize(); // Culture
                rowOffset += GetBlobIndexSize();   // HashValue

                string name = ReadString(nameIndex);
                if (!string.IsNullOrWhiteSpace(name) &&
                    !results.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    results.Add(name);
                }
            }

            return results.ToArray();
        }

        private void BuildTableLayout(int startOffset)
        {
            int currentOffset = startOffset;
            int metadataEnd = checked(MetadataOffset + MetadataSize);

            for (int table = 0; table < 64; table++)
            {
                if (((ValidTables >> table) & 1UL) == 0)
                    continue;

                TableOffsets[table] = currentOffset;
                TableRowSizes[table] = GetTableRowSize(table);

                long nextOffset = (long)currentOffset + ((long)TableRowCounts[table] * TableRowSizes[table]);
                if (nextOffset > metadataEnd)
                    throw new BadImageFormatException($"Metadata table {table} exceeds metadata bounds.");

                currentOffset = checked((int)nextOffset);
            }
        }

        private int GetTableRowSize(int table)
        {
            switch (table)
            {
                case 1: // TypeRef
                    return GetCodedIndexSize(ResolutionScopeTables, 2)
                         + GetStringIndexSize()
                         + GetStringIndexSize();

                case 2: // TypeDef
                    return 4
                         + GetStringIndexSize()
                         + GetStringIndexSize()
                         + GetCodedIndexSize(TypeDefOrRefTables, 2)
                         + GetTableIndexSize(4)
                         + GetTableIndexSize(6);

                case 4: // Field
                    return 2
                         + GetStringIndexSize()
                         + GetBlobIndexSize();

                case 6: // MethodDef
                    return 4
                         + 2
                         + 2
                         + GetStringIndexSize()
                         + GetBlobIndexSize()
                         + GetTableIndexSize(8);

                case 8: // Param
                    return 2
                         + 2
                         + GetStringIndexSize();

                case 10: // MemberRef
                    return GetCodedIndexSize(MemberRefParentTables, 3)
                         + GetStringIndexSize()
                         + GetBlobIndexSize();

                case 12: // CustomAttribute
                    return GetCodedIndexSize(HasCustomAttributeTables, 5)
                         + GetCodedIndexSize(CustomAttributeTypeTables, 3)
                         + GetBlobIndexSize();

                case 32: // Assembly
                    return 4
                         + 2
                         + 2
                         + 2
                         + 2
                         + 4
                         + GetBlobIndexSize()
                         + GetStringIndexSize()
                         + GetStringIndexSize();

                case 41: // NestedClass
                    return GetTableIndexSize(2)
                         + GetTableIndexSize(2);

                default:
                    return ComputeUnsupportedTableRowSize(table);
            }
        }

        private int ComputeUnsupportedTableRowSize(int table)
        {
            switch (table)
            {
                case 0:  // Module
                    return 2 + GetStringIndexSize() + (GetGuidIndexSize() * 3);

                case 9:  // InterfaceImpl
                    return GetTableIndexSize(2) + GetCodedIndexSize(TypeDefOrRefTables, 2);

                case 11: // Constant
                    return 2 + GetCodedIndexSize(HasConstantTables, 2) + GetBlobIndexSize();

                case 13: // FieldMarshal
                    return GetCodedIndexSize(HasFieldMarshalTables, 1) + GetBlobIndexSize();

                case 14: // DeclSecurity
                    return 2 + GetCodedIndexSize(HasDeclSecurityTables, 2) + GetBlobIndexSize();

                case 15: // ClassLayout
                    return 2 + 4 + GetTableIndexSize(2);

                case 16: // FieldLayout
                    return 4 + GetTableIndexSize(4);

                case 17: // StandAloneSig
                    return GetBlobIndexSize();

                case 18: // EventMap
                    return GetTableIndexSize(2) + GetTableIndexSize(20);

                case 20: // Event
                    return 2 + GetStringIndexSize() + GetCodedIndexSize(TypeDefOrRefTables, 2);

                case 21: // PropertyMap
                    return GetTableIndexSize(2) + GetTableIndexSize(23);

                case 23: // Property
                    return 2 + GetStringIndexSize() + GetBlobIndexSize();

                case 24: // MethodSemantics
                    return 2 + GetTableIndexSize(6) + GetCodedIndexSize(HasSemanticTables, 1);

                case 25: // MethodImpl
                    return GetTableIndexSize(2)
                         + GetCodedIndexSize(MethodDefOrRefTables, 1)
                         + GetCodedIndexSize(MethodDefOrRefTables, 1);

                case 26: // ModuleRef
                    return GetStringIndexSize();

                case 27: // TypeSpec
                    return GetBlobIndexSize();

                case 28: // ImplMap
                    return 2
                         + GetCodedIndexSize(MemberForwardedTables, 1)
                         + GetStringIndexSize()
                         + GetTableIndexSize(26);

                case 29: // FieldRVA
                    return 4 + GetTableIndexSize(4);

                case 33: // AssemblyProcessor
                    return 4;

                case 34: // AssemblyOS
                    return 4 + 4 + 4;

                case 35: // AssemblyRef
                    return 2
                         + 2
                         + 2
                         + 2
                         + 4
                         + GetBlobIndexSize()
                         + GetStringIndexSize()
                         + GetStringIndexSize()
                         + GetBlobIndexSize();

                case 36: // AssemblyRefProcessor
                    return 4 + GetTableIndexSize(35);

                case 37: // AssemblyRefOS
                    return 4 + 4 + 4 + GetTableIndexSize(35);

                case 38: // File
                    return 4 + GetStringIndexSize() + GetBlobIndexSize();

                case 39: // ExportedType
                    return 4
                         + 4
                         + GetStringIndexSize()
                         + GetStringIndexSize()
                         + GetCodedIndexSize(ImplementationTables, 2);

                case 40: // ManifestResource
                    return 4
                         + 4
                         + GetStringIndexSize()
                         + GetCodedIndexSize(ImplementationTables, 2);

                case 42: // GenericParam
                    return 2
                         + 2
                         + GetCodedIndexSize(TypeOrMethodDefTables, 1)
                         + GetStringIndexSize();

                case 43: // MethodSpec
                    return GetCodedIndexSize(MethodDefOrRefTables, 1) + GetBlobIndexSize();

                case 44: // GenericParamConstraint
                    return GetTableIndexSize(42) + GetCodedIndexSize(TypeDefOrRefTables, 2);

                default:
                    throw new NotSupportedException($"Unsupported metadata table: {table}");
            }
        }

        private int GetTableRowOffset(int table, uint rid)
        {
            if (rid == 0 || rid > TableRowCounts[table])
                throw new ArgumentOutOfRangeException(nameof(rid));

            int rowSize = TableRowSizes[table];
            if (rowSize <= 0)
                throw new BadImageFormatException($"Invalid row size for table {table}.");

            int offset = checked(TableOffsets[table] + (int)((rid - 1) * (uint)rowSize));
            EnsureRange(offset, rowSize, $"Metadata table {table} row {rid}");
            return offset;
        }

        private int GetStringIndexSize()
            => StringIndexIsLarge ? 4 : 2;

        private int GetBlobIndexSize()
            => BlobIndexIsLarge ? 4 : 2;

        private int GetGuidIndexSize()
            => GuidIndexIsLarge ? 4 : 2;

        private int GetTableIndexSize(int table)
            => TableRowCounts[table] < 0x10000 ? 2 : 4;

        private int GetCodedIndexSize(int[] tables, int tagBits)
        {
            uint maxRows = 0;
            for (int i = 0; i < tables.Length; i++)
            {
                uint rows = TableRowCounts[tables[i]];
                if (rows > maxRows)
                    maxRows = rows;
            }

            return maxRows < (1u << (16 - tagBits)) ? 2 : 4;
        }

        private uint ReadIndex(int offset, int size)
        {
            EnsureRange(offset, size, "Metadata index");

            switch (size)
            {
                case 2:
                    return ReadUInt16(offset);
                case 4:
                    return ReadUInt32(offset);
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), "Index size must be 2 or 4.");
            }
        }

        private string ReadString(uint index)
        {
            if (index == 0)
                return string.Empty;

            if (index >= (uint)StringsHeapSize)
                throw new BadImageFormatException($"Invalid #Strings heap index: {index}.");

            int offset = checked(StringsHeapOffset + (int)index);
            int heapEnd = checked(StringsHeapOffset + StringsHeapSize);

            if (offset < StringsHeapOffset || offset >= heapEnd)
                throw new BadImageFormatException($"String heap offset out of range: {offset}.");

            int end = offset;
            while (end < heapEnd && RawData[end] != 0)
                end++;

            return Encoding.UTF8.GetString(RawData, offset, end - offset);
        }

        private string ReadZeroTerminatedAscii(int offset, out int consumedBytes)
        {
            EnsureRange(offset, 1, "Zero-terminated ASCII string");

            int start = offset;
            while (offset < RawData.Length && RawData[offset] != 0)
                offset++;

            if (offset >= RawData.Length)
                throw new BadImageFormatException("Unterminated ASCII string.");

            string result = Encoding.ASCII.GetString(RawData, start, offset - start);

            consumedBytes = (offset - start) + 1;
            return result;
        }

        private int RvaToOffset(uint rva)
        {
            for (int i = 0; i < Sections.Length; i++)
            {
                PESection section = Sections[i];
                uint mappedSize = Math.Max(section.VirtualSize, section.SizeOfRawData);

                if (section.VirtualAddress <= rva && rva < section.VirtualAddress + mappedSize)
                {
                    uint delta = rva - section.VirtualAddress;
                    if (delta >= section.SizeOfRawData)
                        throw new BadImageFormatException($"RVA 0x{rva:X8} maps outside section raw data.");

                    return checked((int)(section.PointerToRawData + delta));
                }
            }

            throw new BadImageFormatException($"Unable to map RVA 0x{rva:X8}.");
        }

        private static int Align4(int value)
            => (value + 3) & ~3;

        private void EnsureRange(int offset, int size, string name)
        {
            if (offset < 0 || size < 0 || offset > RawData.Length - size)
                throw new BadImageFormatException($"{name} is out of raw data bounds. Offset={offset}, Size={size}.");
        }

        private ushort ReadUInt16(int offset)
        {
            EnsureRange(offset, 2, "UInt16");
            return BitConverter.ToUInt16(RawData, offset);
        }

        private uint ReadUInt32(int offset)
        {
            EnsureRange(offset, 4, "UInt32");
            return BitConverter.ToUInt32(RawData, offset);
        }

        private ulong ReadUInt64(int offset)
        {
            EnsureRange(offset, 8, "UInt64");
            return BitConverter.ToUInt64(RawData, offset);
        }

        private int ReadInt32(int offset)
        {
            EnsureRange(offset, 4, "Int32");
            return BitConverter.ToInt32(RawData, offset);
        }

        private static uint EncodeHasCustomAttribute(int table, uint rid)
        {
            uint tag;
            switch (table)
            {
                case 6: tag = 0; break;  // MethodDef
                case 4: tag = 1; break;  // Field
                case 1: tag = 2; break;  // TypeRef
                case 2: tag = 3; break;  // TypeDef
                case 8: tag = 4; break;  // Param
                case 9: tag = 5; break;  // InterfaceImpl
                case 10: tag = 6; break;  // MemberRef
                case 0: tag = 7; break;  // Module
                case 14: tag = 8; break;  // DeclSecurity
                case 23: tag = 9; break;  // Property
                case 20: tag = 10; break; // Event
                case 17: tag = 11; break; // StandAloneSig
                case 26: tag = 12; break; // ModuleRef
                case 27: tag = 13; break; // TypeSpec
                case 32: tag = 14; break; // Assembly
                case 35: tag = 15; break; // AssemblyRef
                case 38: tag = 16; break; // File
                case 39: tag = 17; break; // ExportedType
                case 40: tag = 18; break; // ManifestResource
                case 42: tag = 19; break; // GenericParam
                case 44: tag = 20; break; // GenericParamConstraint
                case 43: tag = 21; break; // MethodSpec
                default:
                    throw new ArgumentOutOfRangeException(nameof(table));
            }

            return (rid << 5) | tag;
        }

        private static string ExtractNamespaceOnly(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            int plusIndex = fullName.IndexOf('+');
            string head = plusIndex >= 0 ? fullName.Substring(0, plusIndex) : fullName;

            int dotIndex = head.LastIndexOf('.');
            return dotIndex >= 0 ? head.Substring(0, dotIndex) : string.Empty;
        }

        private static string ExtractTypeNameOnly(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            int plusIndex = fullName.IndexOf('+');
            if (plusIndex >= 0)
                return fullName.Substring(plusIndex + 1);

            int dotIndex = fullName.LastIndexOf('.');
            return dotIndex >= 0 ? fullName.Substring(dotIndex + 1) : fullName;
        }

        private static readonly int[] ResolutionScopeTables = new int[] { 0, 26, 35, 1 };
        private static readonly int[] TypeDefOrRefTables = new int[] { 2, 1, 27 };
        private static readonly int[] HasConstantTables = new int[] { 4, 8, 23 };
        private static readonly int[] HasCustomAttributeTables = new int[] { 6, 4, 1, 2, 8, 9, 10, 0, 14, 23, 20, 17, 26, 27, 32, 35, 38, 39, 40, 42, 44, 43 };
        private static readonly int[] HasFieldMarshalTables = new int[] { 4, 8 };
        private static readonly int[] HasDeclSecurityTables = new int[] { 2, 6, 32 };
        private static readonly int[] MemberRefParentTables = new int[] { 2, 1, 26, 6, 27 };
        private static readonly int[] HasSemanticTables = new int[] { 20, 23 };
        private static readonly int[] MethodDefOrRefTables = new int[] { 6, 10 };
        private static readonly int[] MemberForwardedTables = new int[] { 4, 6 };
        private static readonly int[] ImplementationTables = new int[] { 38, 35, 39 };
        private static readonly int[] CustomAttributeTypeTables = new int[] { 0, 0, 6, 10, 0 };
        private static readonly int[] TypeOrMethodDefTables = new int[] { 2, 6 };

    }
}
