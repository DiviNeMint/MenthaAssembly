using MenthaAssembly.Offices.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace MenthaAssembly.Offices
{
    internal class XorEncryption : Encryption
    {
        public ushort Key { get; }

        public ushort Hash { get; }

        public XorEncryption(ushort Key, ushort Hash)
        {
            this.Key = Key;
            this.Hash = Hash;
        }

        public override SymmetricAlgorithm CreateCipher()
            => new XorManaged();

        public override Stream CreateEncryptedPackageStream(Stream Stream, byte[] SecretKey)
            => throw new NotImplementedException();

        public override byte[] GenerateBlockKey(int BlockNumber, byte[] SecretKey)
            => SecretKey;

        public override byte[] GenerateSecretKey(string Password)
        {
            byte[] passwordBytes = Encoding.ASCII.GetBytes(Password.Substring(0, Math.Min(Password.Length, 15)));
            return XorManaged.CreateXorArray_Method1(passwordBytes);
        }

        public override bool VerifyPassword(string Password)
        {
            byte[] PasswordBytes = Encoding.ASCII.GetBytes(Password.Substring(0, Math.Min(Password.Length, 15)));
            return XorManaged.CreatePasswordVerifier_Method1(PasswordBytes) == Hash;
        }

        /// <summary>
        /// Minimal Office "XOR Deobfuscation Method 1" implementation compatible
        /// with System.Security.Cryptography.SymmetricAlgorithm.
        /// </summary>
        private class XorManaged : SymmetricAlgorithm
        {
            private static readonly byte[] PadArray =
            {
                0xBB, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xB9, 0x80,
                0x00, 0xBE, 0x0F, 0x00, 0xBF, 0x0F, 0x00
            };

            private static readonly ushort[] InitialCode =
            {
                0xE1F0, 0x1D0F, 0xCC9C, 0x84C0, 0x110C,
                0x0E10, 0xF1CE, 0x313E, 0x1872, 0xE139,
                0xD40F, 0x84F9, 0x280C, 0xA96A, 0x4EC3
            };

            private static readonly ushort[] XorMatrix =
            {
                0xAEFC, 0x4DD9, 0x9BB2, 0x2745, 0x4E8A, 0x9D14, 0x2A09,
                0x7B61, 0xF6C2, 0xFDA5, 0xEB6B, 0xC6F7, 0x9DCF, 0x2BBF,
                0x4563, 0x8AC6, 0x05AD, 0x0B5A, 0x16B4, 0x2D68, 0x5AD0,
                0x0375, 0x06EA, 0x0DD4, 0x1BA8, 0x3750, 0x6EA0, 0xDD40,
                0xD849, 0xA0B3, 0x5147, 0xA28E, 0x553D, 0xAA7A, 0x44D5,
                0x6F45, 0xDE8A, 0xAD35, 0x4A4B, 0x9496, 0x390D, 0x721A,
                0xEB23, 0xC667, 0x9CEF, 0x29FF, 0x53FE, 0xA7FC, 0x5FD9,
                0x47D3, 0x8FA6, 0x0F6D, 0x1EDA, 0x3DB4, 0x7B68, 0xF6D0,
                0xB861, 0x60E3, 0xC1C6, 0x93AD, 0x377B, 0x6EF6, 0xDDEC,
                0x45A0, 0x8B40, 0x06A1, 0x0D42, 0x1A84, 0x3508, 0x6A10,
                0xAA51, 0x4483, 0x8906, 0x022D, 0x045A, 0x08B4, 0x1168,
                0x76B4, 0xED68, 0xCAF1, 0x85C3, 0x1BA7, 0x374E, 0x6E9C,
                0x3730, 0x6E60, 0xDCC0, 0xA9A1, 0x4363, 0x86C6, 0x1DAD,
                0x3331, 0x6662, 0xCCC4, 0x89A9, 0x0373, 0x06E6, 0x0DCC,
                0x1021, 0x2042, 0x4084, 0x8108, 0x1231, 0x2462, 0x48C4
            };

            public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
                => new XorTransform(rgbKey, 0);

            public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
                => throw new NotImplementedException();

            public override void GenerateIV()
                => throw new NotImplementedException();

            public override void GenerateKey()
                => throw new NotImplementedException();

            internal static ushort CreatePasswordVerifier_Method1(byte[] PasswordBytes)
            {
                List<byte> PasswordDatas = new List<byte> { (byte)PasswordBytes.Length };
                PasswordDatas.AddRange(PasswordBytes);

                int Verifier = 0,
                    MaxIndex = PasswordDatas.Count - 1;
                for (int i = 0; i < PasswordDatas.Count; ++i)
                    Verifier = ((((Verifier & 0x4000) == 0) ? 0 : 1) | ((Verifier << 1) & 0x7FFF)) ^ PasswordDatas[MaxIndex - i];

                return (ushort)(Verifier ^ 0xCE4B);
            }

            internal static ushort CreateXorKey_Method1(byte[] PasswordBytes)
            {
                int MaxPasswordIndex = PasswordBytes.Length - 1;
                ushort xorKey = InitialCode[MaxPasswordIndex];
                int currentElement = 0x68;

                for (int i = 0; i < PasswordBytes.Length; ++i)
                {
                    byte c = PasswordBytes[MaxPasswordIndex - i];
                    for (int j = 0; j < 7; ++j)
                    {
                        if ((c & 0x40) != 0)
                            xorKey ^= XorMatrix[currentElement];

                        c *= 2;
                        currentElement--;
                    }
                }

                return xorKey;
            }

            /// <summary>
            /// Generates a 16 byte obfuscation array based on the POI/LibreOffice implementations
            /// </summary>
            internal static byte[] CreateXorArray_Method1(byte[] PasswordBytes)
            {
                const int MaxLength = 16;
                byte[] Obfuscation = PasswordBytes.Concat(PadArray.Take(MaxLength - PasswordBytes.Length)).ToArray();
                ushort Key = CreateXorKey_Method1(PasswordBytes);
                byte[] baseKeyLE = new byte[] { (byte)(Key & 0xFF), (byte)((Key >> 8) & 0xFF) };
                for (int i = 0; i < MaxLength; i++)
                {
                    byte Element = Obfuscation[i];
                    Element ^= baseKeyLE[i & 1];
                    Element = (byte)((Element << 2) | (Element >> 6));
                    Obfuscation[i] = Element;
                }

                return Obfuscation;

                //byte[] obfuscationArray = new byte[16];
                //Array.Copy(PasswordBytes, 0, obfuscationArray, 0, PasswordBytes.Length);
                //Array.Copy(PadArray, 0, obfuscationArray, PasswordBytes.Length, PadArray.Length - PasswordBytes.Length + 1);

                //ushort Key = CreateXorKey_Method1(PasswordBytes);
                //byte[] baseKeyLE = new byte[] { (byte)(Key & 0xFF), (byte)((Key >> 8) & 0xFF) };
                //for (int i = 0; i < obfuscationArray.Length; i++)
                //{
                //    obfuscationArray[i] ^= baseKeyLE[i & 1];
                //    obfuscationArray[i] = (byte)((obfuscationArray[i] << 2) | (obfuscationArray[i] >> 6));
                //}

                //return obfuscationArray;
            }

        }

        internal class XorTransform : ICryptoTransform
        {
            public int InputBlockSize => 1024;

            public int OutputBlockSize => 1024;

            public bool CanTransformMultipleBlocks => false;

            public bool CanReuseTransform => false;

            /// <summary>
            /// Gets or sets the obfuscation array index. BIFF obfuscation uses a different XorArrayIndex per record.
            /// </summary>
            public int XorArrayIndex { get; set; }

            private byte[] XorArray { get; }

            public XorTransform(byte[] Key, int XorArrayIndex)
            {
                XorArray = Key;
                this.XorArrayIndex = XorArrayIndex;
            }

            public int TransformBlock(byte[] InputBuffer, int InputOffset, int InputLength, byte[] OutputBuffer, int OutputOffset)
            {
                for (int i = 0; i < InputLength; ++i)
                {
                    byte value = InputBuffer[InputOffset + i];
                    value = (byte)((value << 3) | (value >> 5));
                    value ^= XorArray[XorArrayIndex % 16];
                    OutputBuffer[OutputOffset + i] = value;
                    XorArrayIndex++;
                }

                return InputLength;
            }

            public byte[] TransformFinalBlock(byte[] InputBuffer, int InputOffset, int InputLength)
            {
                byte[] Buffer = new byte[InputLength];
                TransformBlock(InputBuffer, InputOffset, InputLength, Buffer, 0);
                return Buffer;
            }

            public void Dispose()
            {
            }

        }

    }
}
