using System.Collections.Generic;
using System.Text;
#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

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
        private readonly int BlobHeapOffset;
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
            MetadataSize = (int)metadataSize;

            // Metadata Root
            int md = MetadataOffset;
            if (ReadUInt32(md) != 0x424A5342) // BSJB
                throw new BadImageFormatException("Invalid CLR metadata root.");

            int versionLength = ReadInt32(md + 12);
            int streamCountOffset = Align4(md + 16 + versionLength);
            ushort streamCount = ReadUInt16(streamCountOffset + 2);

            int streamHeaderOffset = streamCountOffset + 4;

            int stringsHeapOffset = -1;
            int blobHeapOffset = -1;
            int tablesHeapOffset = -1;

            for (int i = 0; i < streamCount; i++)
            {
                uint offset = ReadUInt32(streamHeaderOffset);
                uint size = ReadUInt32(streamHeaderOffset + 4);

                int nameOffset = streamHeaderOffset + 8;
                string name = ReadZeroTerminatedAscii(nameOffset, out int consumedNameBytes);
                int alignedNameBytes = Align4(consumedNameBytes);

                switch (name)
                {
                    case "#Strings":
                        stringsHeapOffset = MetadataOffset + (int)offset;
                        break;
                    case "#Blob":
                        blobHeapOffset = MetadataOffset + (int)offset;
                        break;
                    case "#~":
                    case "#-":
                        tablesHeapOffset = MetadataOffset + (int)offset;
                        break;
                }

                streamHeaderOffset += 8 + alignedNameBytes;
            }

            if (stringsHeapOffset < 0 || blobHeapOffset < 0 || tablesHeapOffset < 0)
                throw new BadImageFormatException("Required metadata streams not found.");

            StringsHeapOffset = stringsHeapOffset;
            BlobHeapOffset = blobHeapOffset;
            TablesHeapOffset = tablesHeapOffset;

            // Tables Stream
            int tables = TablesHeapOffset;
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

            List<string> results = new();

            int rowCount = checked((int)TableRowCounts[CustomAttributeTable]);
            for (uint rid = 1; rid <= rowCount; rid++)
            {
                int rowOffset = GetTableRowOffset(CustomAttributeTable, rid);

                uint parent = ReadIndex(rowOffset, GetCodedIndexSize(HasCustomAttributeTables, 5));
                rowOffset += GetCodedIndexSize(HasCustomAttributeTables, 5);

                uint type = ReadIndex(rowOffset, GetCodedIndexSize(CustomAttributeTypeTables, 3));

                if (parent != assemblyParentToken)
                    continue;

                if (TryResolveCustomAttributeTypeFullName(type, out string fullName) &&
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
                    if (TryGetMethodDefOwnerTypeDefRid(rid, out uint ownerTypeRid))
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
                    if (TryGetMethodDefOwnerTypeDefRid(rid, out uint ownerTypeRid))
                        return TryGetTypeDefFullName(ownerTypeRid, out FullName);
                    break;
            }

            FullName = null;
            return false;
        }

        private bool TryGetMethodDefOwnerTypeDefRid(uint methodDefRid, out uint TypeDefRid)
        {
            const int TypeDefTable = 2;

            uint typeCount = TableRowCounts[TypeDefTable];
            if (typeCount == 0 || methodDefRid == 0)
            {
                TypeDefRid = 0;
                return false;
            }

            for (uint typeRid = 1; typeRid <= typeCount; typeRid++)
            {
                int rowOffset = GetTableRowOffset(TypeDefTable, typeRid);

                rowOffset += 4; // Flags
                rowOffset += GetStringIndexSize();
                rowOffset += GetStringIndexSize();
                rowOffset += GetCodedIndexSize(TypeDefOrRefTables, 2);
                rowOffset += GetTableIndexSize(4); // Field

                uint methodList = ReadIndex(rowOffset, GetTableIndexSize(6));
                uint nextMethodList;

                if (typeRid < typeCount)
                {
                    int nextRowOffset = GetTableRowOffset(TypeDefTable, typeRid + 1);
                    nextRowOffset += 4;
                    nextRowOffset += GetStringIndexSize();
                    nextRowOffset += GetStringIndexSize();
                    nextRowOffset += GetCodedIndexSize(TypeDefOrRefTables, 2);
                    nextRowOffset += GetTableIndexSize(4);
                    nextMethodList = ReadIndex(nextRowOffset, GetTableIndexSize(6));
                }
                else
                {
                    nextMethodList = TableRowCounts[6] + 1;
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

            if (enclosingRid > 0 && TryGetTypeDefFullName(enclosingRid, out string parentName))
            {
                string localName = ExtractTypeNameOnly(parentName) + "+" + name;
                string parentNs = ExtractNamespaceOnly(parentName);
                FullName = string.IsNullOrEmpty(parentNs) ? localName : $"{parentNs}.{localName}";
                return true;
            }

            FullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
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
                if (TryGetTypeRefFullName(scopeRid, out string parentName))
                {
                    string localName = ExtractTypeNameOnly(parentName) + "+" + name;
                    string parentNs = ExtractNamespaceOnly(parentName);
                    FullName = string.IsNullOrEmpty(parentNs) ? localName : $"{parentNs}.{localName}";
                    return true;
                }
            }

            FullName = string.IsNullOrEmpty(ns) ? name : $"{ns}.{name}";
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

        private void BuildTableLayout(int startOffset)
        {
            int currentOffset = startOffset;

            for (int table = 0; table < 64; table++)
            {
                if (((ValidTables >> table) & 1UL) == 0)
                    continue;

                TableOffsets[table] = currentOffset;
                TableRowSizes[table] = GetTableRowSize(table);
                currentOffset += checked((int)(TableRowCounts[table] * (uint)TableRowSizes[table]));
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
                    return 2 + 2 + GetStringIndexSize() + GetBlobIndexSize();

                case 6: // MethodDef
                    return 4 + 2 + 2 + GetStringIndexSize() + GetBlobIndexSize() + GetTableIndexSize(8);

                case 8: // Param
                    return 2 + 2 + GetStringIndexSize();

                case 10: // MemberRef
                    return GetCodedIndexSize(MemberRefParentTables, 3)
                         + GetStringIndexSize()
                         + GetBlobIndexSize();

                case 12: // CustomAttribute
                    return GetCodedIndexSize(HasCustomAttributeTables, 5)
                         + GetCodedIndexSize(CustomAttributeTypeTables, 3)
                         + GetBlobIndexSize();

                case 32: // Assembly
                    return 4 + 2 + 2 + 2 + 2 + 4 + GetBlobIndexSize() + GetStringIndexSize() + GetStringIndexSize();

                case 41: // NestedClass
                    return GetTableIndexSize(2) + GetTableIndexSize(2);

                default:
                    return ComputeUnsupportedTableRowSize(table);
            }
        }

        private int ComputeUnsupportedTableRowSize(int table)
        {
            // 只要這些表存在於 #~ 中，就需要正確跳過它們。
            // 這裡補上常見 metadata tables 的 row size 計算。
            switch (table)
            {
                case 0: return 2 + GetStringIndexSize() + GetGuidIndexSize() * 3;                                   // Module
                case 9: return GetTableIndexSize(2) + GetCodedIndexSize(TypeDefOrRefTables, 2);                    // InterfaceImpl
                case 11: return GetCodedIndexSize(HasConstantTables, 2) + GetBlobIndexSize();                        // Constant
                case 13: return GetCodedIndexSize(HasFieldMarshalTables, 1) + GetBlobIndexSize();                    // FieldMarshal
                case 14: return 2 + GetCodedIndexSize(HasDeclSecurityTables, 2) + GetBlobIndexSize();                // DeclSecurity
                case 15: return 2 + 4 + GetTableIndexSize(2);                                                        // ClassLayout
                case 16: return 4 + GetTableIndexSize(4);                                                            // FieldLayout
                case 17: return GetBlobIndexSize();                                                                   // StandAloneSig
                case 18: return GetTableIndexSize(2) + GetTableIndexSize(20);                                        // EventMap
                case 20: return 2 + GetStringIndexSize() + GetCodedIndexSize(TypeDefOrRefTables, 2);                // Event
                case 21: return GetTableIndexSize(2) + GetTableIndexSize(23);                                        // PropertyMap
                case 23: return 2 + GetStringIndexSize() + GetBlobIndexSize();                                       // Property
                case 24: return 2 + GetTableIndexSize(6) + GetCodedIndexSize(HasSemanticTables, 1);                 // MethodSemantics
                case 25: return GetTableIndexSize(2) + GetCodedIndexSize(MethodDefOrRefTables, 1) + GetCodedIndexSize(MethodDefOrRefTables, 1); // MethodImpl
                case 26: return GetStringIndexSize();                                                                 // ModuleRef
                case 27: return GetBlobIndexSize();                                                                   // TypeSpec
                case 28: return 2 + GetCodedIndexSize(MemberForwardedTables, 1) + GetStringIndexSize() + GetTableIndexSize(26); // ImplMap
                case 29: return 4 + GetTableIndexSize(4);                                                            // FieldRVA
                case 33: return 4;                                                                                   // AssemblyProcessor
                case 34: return 4 + 4 + 4;                                                                           // AssemblyOS
                case 35: return 2 + 2 + 2 + 2 + 4 + GetBlobIndexSize() + GetStringIndexSize() + GetStringIndexSize() + GetBlobIndexSize(); // AssemblyRef
                case 36: return 4 + GetTableIndexSize(35);                                                           // AssemblyRefProcessor
                case 37: return 4 + 4 + 4 + GetTableIndexSize(35);                                                   // AssemblyRefOS
                case 38: return 4 + GetStringIndexSize() + GetBlobIndexSize();                                       // File
                case 39: return 4 + 4 + GetStringIndexSize() + GetStringIndexSize() + GetCodedIndexSize(ImplementationTables, 2); // ExportedType
                case 40: return 4 + GetStringIndexSize() + GetCodedIndexSize(ImplementationTables, 2);              // ManifestResource
                case 42: return 2 + 2 + GetCodedIndexSize(TypeOrMethodDefTables, 1) + GetStringIndexSize();         // GenericParam
                case 43: return GetCodedIndexSize(MethodDefOrRefTables, 1) + GetBlobIndexSize();                     // MethodSpec
                case 44: return GetTableIndexSize(42) + GetCodedIndexSize(TypeDefOrRefTables, 2);                   // GenericParamConstraint
                default:
                    throw new NotSupportedException($"Unsupported metadata table: {table}");
            }
        }

        private uint EncodeHasCustomAttribute(int table, uint rid)
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

        private int GetTableRowOffset(int table, uint rid)
        {
            if (rid == 0 || rid > TableRowCounts[table])
                throw new ArgumentOutOfRangeException(nameof(rid));

            return TableOffsets[table] + checked((int)((rid - 1) * (uint)TableRowSizes[table]));
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
            => size == 2 ? ReadUInt16(offset) : ReadUInt32(offset);

        private string ReadString(uint index)
        {
            if (index == 0)
                return string.Empty;

            int offset = StringsHeapOffset + checked((int)index);
            int end = offset;
            while (end < RawData.Length && RawData[end] != 0)
                end++;

            return Encoding.UTF8.GetString(RawData, offset, end - offset);
        }

        private string ReadZeroTerminatedAscii(int offset, out int consumedBytes)
        {
            int start = offset;
            while (offset < RawData.Length && RawData[offset] != 0)
                offset++;

            string result = Encoding.ASCII.GetString(RawData, start, offset - start);

            // include terminator
            consumedBytes = (offset - start) + 1;
            return result;
        }

        private int RvaToOffset(uint rva)
        {
            for (int i = 0; i < Sections.Length; i++)
            {
                PESection section = Sections[i];
                uint size = Math.Max(section.VirtualSize, section.SizeOfRawData);

                if (section.VirtualAddress <= rva && rva < section.VirtualAddress + size)
                    return checked((int)(section.PointerToRawData + (rva - section.VirtualAddress)));
            }

            throw new BadImageFormatException($"Unable to map RVA 0x{rva:X8}.");
        }

        private static int Align4(int value)
            => (value + 3) & ~3;

        private ushort ReadUInt16(int offset)
            => BitConverter.ToUInt16(RawData, offset);

        private uint ReadUInt32(int offset)
            => BitConverter.ToUInt32(RawData, offset);

        private ulong ReadUInt64(int offset)
            => BitConverter.ToUInt64(RawData, offset);

        private int ReadInt32(int offset)
            => BitConverter.ToInt32(RawData, offset);

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

        private static readonly int[] ResolutionScopeTables = [0, 26, 35, 1];
        private static readonly int[] TypeDefOrRefTables = [2, 1, 27];
        private static readonly int[] HasConstantTables = [4, 8, 23];
        private static readonly int[] HasCustomAttributeTables = [6, 4, 1, 2, 8, 9, 10, 0, 14, 23, 20, 17, 26, 27, 32, 35, 38, 39, 40, 42, 44, 43];
        private static readonly int[] HasFieldMarshalTables = [4, 8];
        private static readonly int[] HasDeclSecurityTables = [2, 6, 32];
        private static readonly int[] MemberRefParentTables = [2, 1, 26, 6, 27];
        private static readonly int[] HasSemanticTables = [20, 23];
        private static readonly int[] MethodDefOrRefTables = [6, 10];
        private static readonly int[] MemberForwardedTables = [4, 6];
        private static readonly int[] ImplementationTables = [38, 35, 39];
        private static readonly int[] CustomAttributeTypeTables = [0, 0, 6, 10, 0];
        private static readonly int[] TypeOrMethodDefTables = [2, 6];
    }

}