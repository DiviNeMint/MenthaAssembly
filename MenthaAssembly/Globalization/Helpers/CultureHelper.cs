using MenthaAssembly.Win32;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MenthaAssembly.Globalization
{
    public static class CultureHelper
    {
        public static bool ExistsCulture(string CultureCode)
        {
            try
            {
                CultureInfo.GetCultureInfo(CultureCode);
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static Dictionary<string, uint> LoadStringTableFromMui(string FilePath)
        {
            if (!File.Exists(FilePath))
                return null;

            IntPtr hModule = Win32.System.LoadLibraryEx(FilePath, IntPtr.Zero, LoadLibraryFlag.LOAD_LIBRARY_AS_DATAFILE);
            if (hModule == IntPtr.Zero)
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                string Message;

#if NET7_0_OR_GREATER
                Message = Marshal.GetPInvokeErrorMessage(ErrorCode);
#else
                Win32Exception Ex = new(Marshal.GetLastWin32Error());
                Message = Ex.Message;
#endif

                Debug.WriteLine($"[{nameof(LoadStringTableFromMui)}][Win32Exception]{Message}");
                return null;
            }

            try
            {
                Dictionary<string, uint> Table = [];

                bool EnumResourceBlockCallback(nint hModule, nint lpszType, IntPtr lpszName, nint lParam)
                {
                    uint BlockID = (uint)lpszName.ToInt32();
                    IntPtr hResInfo = Win32.System.FindResource(hModule, BlockID, ResourceType.String);
                    if (hResInfo == IntPtr.Zero)
                        return true;

                    IntPtr hResData = Win32.System.LoadResource(hModule, hResInfo);
                    IntPtr pResData = Win32.System.LockResource(hResData);

                    int Size = Win32.System.SizeofResource(hModule, hResInfo);

                    byte[] Buffer = ArrayPool<byte>.Shared.Rent(Size);
                    try
                    {
                        Marshal.Copy(pResData, Buffer, 0, Size);

                        int Offset = 0;
                        for (uint i = 0; i < 16; i++)
                        {
                            int Length = BitConverter.ToUInt16(Buffer, Offset) << 1;
                            Offset += 2;

                            if (Length > 0)
                            {
                                string Text = Encoding.Unicode.GetString(Buffer, Offset, Length);
                                Offset += Length;

                                Table[Text] = ((BlockID - 1) << 4) + i;
                            }
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(Buffer);
                    }
                    return true;
                }

                Win32.System.EnumResourceNames(hModule, ResourceType.String, EnumResourceBlockCallback, IntPtr.Zero);

                return Table;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine($"[{nameof(LoadStringTableFromMui)}][Exception]{Ex.Message}");
            }
            finally
            {
                Win32.System.FreeLibrary(hModule);
            }

            return null;
        }

        public static string LoadStringFromMui(string FilePath, uint ResourceId)
        {
            IntPtr hModule = Win32.System.LoadLibraryEx(FilePath, IntPtr.Zero, LoadLibraryFlag.LOAD_LIBRARY_AS_DATAFILE);
            if (hModule == IntPtr.Zero)
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                string Message;

#if NET7_0_OR_GREATER
                Message = Marshal.GetPInvokeErrorMessage(ErrorCode);
#else
                Win32Exception Ex = new(Marshal.GetLastWin32Error());
                Message = Ex.Message;
#endif

                Debug.WriteLine($"[{nameof(LoadStringFromMui)}][Win32Exception]{Message}");
                return null;
            }

            StringBuilder Buffer = new(256);
            try
            {
                return Win32.System.LoadString(hModule, ResourceId, Buffer, Buffer.Capacity) > 0 ? Buffer.ToString() : null;
            }
            finally
            {
                Win32.System.FreeLibrary(hModule);
                Buffer.Clear();
            }
        }

    }
}