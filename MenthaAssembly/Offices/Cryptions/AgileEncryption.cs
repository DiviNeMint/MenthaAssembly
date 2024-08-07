using MenthaAssembly.Offices.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace MenthaAssembly.Offices
{
    /// <summary>
    /// Represents "Agile Encryption" used in XLSX (Office 2010 and newer)
    /// </summary>
    internal class AgileEncryption : Encryption
    {
        public int KeyBits { get; }

        public int BlockSize { get; }

        //public int HashSize { get; }

        public byte[] Salt { get; }

        public byte[] PasswordSalt { get; }

        public byte[] PasswordEncryptedKeyValue { get; }

        public byte[] PasswordEncryptedVerifierHashInput { get; }

        public byte[] PasswordEncryptedVerifierHashValue { get; }

        public int PasswordSpinCount { get; }

        public int PasswordKeyBits { get; }

        public AgileEncryption(Stream Stream)
        {
            Stream.Seek(4, SeekOrigin.Current); // Skip 4 bytes

            using MemoryStream Memory = new MemoryStream(Stream.Read((int)(Stream.Length - Stream.Position)));

            const string NsEncryption = "http://schemas.microsoft.com/office/2006/encryption";
            using XmlReader xmlReader = XmlReader.Create(Memory);

            if (!xmlReader.IsStartElement("encryption", NsEncryption))
                return;

            if (!xmlReader.ReadFirstContent())
                return;

            while (!xmlReader.EOF)
            {
                if (xmlReader.IsStartElement("keyData", NsEncryption))
                {
                    // <keyData saltSize="16" blockSize="16" keyBits="256" hashSize="64" cipherAlgorithm="AES" cipherChaining="ChainingModeCBC" hashAlgorithm="SHA512" saltValue="zYmgeIEW4PVmYPiNJItVCQ=="/>
                    // <dataIntegrity encryptedHmacKey="v11xCwbBfQ6Wq03h2M6Nh5Z9fwNnFQwEzu8vmBDps55kd+HfLDzrnuKzuQq4tlpxW0nX99VWh+n2X6ukU6v9FQ==" encryptedHmacValue="SvDwFQR4dNsXOzNstFWHqSHpAUWHQvAr63IhxlxhlQEAczDPIwCWD32aIEFipY7NOlW+LvYPaKC8zO1otxit2g=="/>
                    // <keyEncryptors><keyEncryptor uri="http://schemas.microsoft.com/office/2006/keyEncryptor/password"><p:encryptedKey spinCount="100000" saltSize="16" blockSize="16" keyBits="256" hashSize="64" cipherAlgorithm="AES" cipherChaining="ChainingModeCBC" hashAlgorithm="SHA512" saltValue="n37HW2mNfJuGwVxTeBY1LA==" encryptedVerifierHashInput="2Y2Oo+QDyMdo327gZUcejA==" encryptedVerifierHashValue="PmkCD5y5cHqMQqbgACUgxLRgISYZL6+jj3K0PSrFDWlEG+fjzFevIee1FubgdpY2P22IIM6W7C/bXE0ayAo8yg==" encryptedKeyValue="qzkvVPIBy2Bk/w2/fp+hhpq5sPReA8aUu414/Xh7494="/></keyEncryptor></keyEncryptors>
                    string CipherAlgorithm = xmlReader.GetAttribute("cipherAlgorithm");
                    string cipherChaining = xmlReader.GetAttribute("cipherChaining");
                    string HashAlgorithm = xmlReader.GetAttribute("hashAlgorithm");
                    string saltValue = xmlReader.GetAttribute("saltValue");

                    //int.TryParse(xmlReader.GetAttribute("saltSize"), out int saltSize);
                    int.TryParse(xmlReader.GetAttribute("blockSize"), out int blockSize);
                    int.TryParse(xmlReader.GetAttribute("keyBits"), out int keyBits);
                    //int.TryParse(xmlReader.GetAttribute("hashSize"), out int hashSize);

                    Salt = Convert.FromBase64String(saltValue);
                    //HashSize = hashSize; // given in bytes, also given implicitly by SHA512
                    KeyBits = keyBits;
                    BlockSize = blockSize;
                    CipherMode CipherChaining = ParseCipherMode(cipherChaining);

                    switch (CipherAlgorithm)
                    {
                        case "AES":
                            {
                                CipherCreator = () =>
                                {
                                    Aes Algorithm = Aes.Create();
                                    Algorithm.KeySize = KeyBits;
                                    Algorithm.BlockSize = BlockSize << 3;
                                    Algorithm.Mode = CipherChaining;
                                    Algorithm.Padding = PaddingMode.Zeros;
                                    return Algorithm;
                                };

                                //CipherCreator = () => new RijndaelManaged
                                //{
                                //    KeySize = KeyBits,
                                //    BlockSize = BlockSize << 3,
                                //    Mode = CipherChaining,
                                //    Padding = PaddingMode.Zeros
                                //};
                                break;
                            }
                        case "DES":
                            {
                                CipherCreator = () =>
                                {
                                    SymmetricAlgorithm Algorithm = DES.Create();
                                    Algorithm.KeySize = KeyBits;
                                    Algorithm.BlockSize = BlockSize << 3;
                                    Algorithm.Mode = CipherChaining;
                                    Algorithm.Padding = PaddingMode.Zeros;
                                    return Algorithm;
                                };
                                break;
                            }
                        case "3DES":
                            {
                                CipherCreator = () =>
                                {
                                    SymmetricAlgorithm Algorithm = TripleDES.Create();
                                    Algorithm.KeySize = KeyBits;
                                    Algorithm.BlockSize = BlockSize << 3;
                                    Algorithm.Mode = CipherChaining;
                                    Algorithm.Padding = PaddingMode.Zeros;
                                    return Algorithm;
                                };
                                break;
                            }
                        case "RC2":
                            {
                                CipherCreator = () =>
                                {
                                    SymmetricAlgorithm Algorithm = RC2.Create();
                                    Algorithm.KeySize = KeyBits;
                                    Algorithm.BlockSize = BlockSize << 3;
                                    Algorithm.Mode = CipherChaining;
                                    Algorithm.Padding = PaddingMode.Zeros;
                                    return Algorithm;
                                };
                                break;
                            }
                        default:
                            CipherCreator = () => throw new InvalidOperationException($"Unsupported encryption method : {CipherAlgorithm}.");
                            break;
                    }
                    switch (HashAlgorithm)
                    {
                        case "MD5":
                            {
                                HasherCreator = MD5.Create;
                                break;
                            }
                        case "SHA1":
                            {
                                HasherCreator = SHA1.Create;
                                break;
                            }
                        case "SHA256":
                            {
                                HasherCreator = SHA256.Create;
                                break;
                            }
                        case "SHA384":
                            {
                                HasherCreator = SHA384.Create;
                                break;
                            }
                        case "SHA512":
                            {
                                HasherCreator = SHA512.Create;
                                break;
                            }
                        default:
                            {
                                HasherCreator = () => throw new InvalidOperationException("Unsupported hash algorithm");
                                break;
                            }
                    }

                    xmlReader.Skip();
                }
                else if (xmlReader.IsStartElement("keyEncryptors", NsEncryption))
                {
                    if (!xmlReader.ReadFirstContent())
                        return;

                    while (!xmlReader.EOF)
                    {
                        if (xmlReader.IsStartElement("keyEncryptor", NsEncryption))
                        {
                            if (!xmlReader.ReadFirstContent())
                                return;

                            while (!xmlReader.EOF)
                            {
                                if (xmlReader.IsStartElement("encryptedKey", "http://schemas.microsoft.com/office/2006/keyEncryptor/password"))
                                {
                                    // <p:encryptedKey spinCount="100000" saltSize="16" blockSize="16" keyBits="256" hashSize="64" cipherAlgorithm="AES" cipherChaining="ChainingModeCBC" hashAlgorithm="SHA512" saltValue="n37HW2mNfJuGwVxTeBY1LA==" encryptedVerifierHashInput="2Y2Oo+QDyMdo327gZUcejA==" encryptedVerifierHashValue="PmkCD5y5cHqMQqbgACUgxLRgISYZL6+jj3K0PSrFDWlEG+fjzFevIee1FubgdpY2P22IIM6W7C/bXE0ayAo8yg==" encryptedKeyValue="qzkvVPIBy2Bk/w2/fp+hhpq5sPReA8aUu414/Xh7494="/></keyEncryptor></keyEncryptors>
                                    string CipherAlgorithm = xmlReader.GetAttribute("cipherAlgorithm");
                                    string cipherChaining = xmlReader.GetAttribute("cipherChaining");
                                    string HashAlgorithm = xmlReader.GetAttribute("hashAlgorithm");
                                    string saltValue = xmlReader.GetAttribute("saltValue");
                                    string encryptedVerifierHashInput = xmlReader.GetAttribute("encryptedVerifierHashInput");
                                    string encryptedVerifierHashValue = xmlReader.GetAttribute("encryptedVerifierHashValue");
                                    string encryptedKeyValue = xmlReader.GetAttribute("encryptedKeyValue");

                                    int.TryParse(xmlReader.GetAttribute("spinCount"), out int spinCount);
                                    //int.TryParse(xmlReader.GetAttribute("saltSize"), out int saltSize);
                                    int.TryParse(xmlReader.GetAttribute("blockSize"), out int blockSize);
                                    int.TryParse(xmlReader.GetAttribute("keyBits"), out int keyBits);
                                    //int.TryParse(xmlReader.GetAttribute("hashSize"), out int hashSize);

                                    PasswordSalt = Convert.FromBase64String(saltValue);
                                    CipherMode PasswordCipherChaining = ParseCipherMode(cipherChaining);
                                    PasswordEncryptedKeyValue = Convert.FromBase64String(encryptedKeyValue);
                                    PasswordEncryptedVerifierHashInput = Convert.FromBase64String(encryptedVerifierHashInput);
                                    PasswordEncryptedVerifierHashValue = Convert.FromBase64String(encryptedVerifierHashValue);
                                    PasswordSpinCount = spinCount;
                                    PasswordKeyBits = keyBits;
                                    int PasswordBlockSize = blockSize;

                                    switch (CipherAlgorithm)
                                    {
                                        case "AES":
                                            {
                                                PasswordCipherCreator = () =>
                                                {
                                                    Aes Algorithm = Aes.Create();
                                                    Algorithm.KeySize = PasswordKeyBits;
                                                    Algorithm.BlockSize = PasswordBlockSize << 3;
                                                    Algorithm.Mode = PasswordCipherChaining;
                                                    Algorithm.Padding = PaddingMode.Zeros;
                                                    return Algorithm;
                                                };

                                                //PasswordCipherCreator = () => new RijndaelManaged
                                                //{
                                                //    KeySize = PasswordKeyBits,
                                                //    BlockSize = PasswordBlockSize << 3,
                                                //    Mode = PasswordCipherChaining,
                                                //    Padding = PaddingMode.Zeros
                                                //};
                                                break;
                                            }
                                        case "DES":
                                            {
                                                PasswordCipherCreator = () =>
                                                {
                                                    SymmetricAlgorithm Algorithm = DES.Create();
                                                    Algorithm.KeySize = PasswordKeyBits;
                                                    Algorithm.BlockSize = PasswordBlockSize << 3;
                                                    Algorithm.Mode = PasswordCipherChaining;
                                                    Algorithm.Padding = PaddingMode.Zeros;
                                                    return Algorithm;
                                                };
                                                break;
                                            }
                                        case "3DES":
                                            {
                                                PasswordCipherCreator = () =>
                                                {
                                                    SymmetricAlgorithm Algorithm = TripleDES.Create();
                                                    Algorithm.KeySize = PasswordKeyBits;
                                                    Algorithm.BlockSize = PasswordBlockSize << 3;
                                                    Algorithm.Mode = PasswordCipherChaining;
                                                    Algorithm.Padding = PaddingMode.Zeros;
                                                    return Algorithm;
                                                };
                                                break;
                                            }
                                        case "RC2":
                                            {
                                                PasswordCipherCreator = () =>
                                                {
                                                    SymmetricAlgorithm Algorithm = RC2.Create();
                                                    Algorithm.KeySize = PasswordKeyBits;
                                                    Algorithm.BlockSize = PasswordBlockSize << 3;
                                                    Algorithm.Mode = PasswordCipherChaining;
                                                    Algorithm.Padding = PaddingMode.Zeros;
                                                    return Algorithm;
                                                };
                                                break;
                                            }
                                        default:
                                            PasswordCipherCreator = () => throw new InvalidOperationException($"Unsupported encryption method : {CipherAlgorithm}.");
                                            break;
                                    }
                                    switch (HashAlgorithm)
                                    {
                                        case "MD5":
                                            {
                                                PasswordHasherCreator = MD5.Create;
                                                break;
                                            }
                                        case "SHA1":
                                            {
                                                PasswordHasherCreator = SHA1.Create;
                                                break;
                                            }
                                        case "SHA256":
                                            {
                                                PasswordHasherCreator = SHA256.Create;
                                                break;
                                            }
                                        case "SHA384":
                                            {
                                                PasswordHasherCreator = SHA384.Create;
                                                break;
                                            }
                                        case "SHA512":
                                            {
                                                PasswordHasherCreator = SHA512.Create;
                                                break;
                                            }
                                        default:
                                            {
                                                PasswordHasherCreator = () => throw new InvalidOperationException("Unsupported hash algorithm");
                                                break;
                                            }
                                    }

                                    xmlReader.Skip();
                                }
                                else if (!xmlReader.SkipContent())
                                {
                                    break;
                                }
                            }
                        }
                        else if (!xmlReader.SkipContent())
                        {
                            break;
                        }
                    }
                }
                else if (!xmlReader.SkipContent())
                {
                    break;
                }
            }
        }

        private readonly Func<SymmetricAlgorithm> CipherCreator;
        public override SymmetricAlgorithm CreateCipher()
            => CipherCreator();

        private readonly Func<HashAlgorithm> HasherCreator;
        private HashAlgorithm CreateHasher()
            => HasherCreator();

        private readonly Func<SymmetricAlgorithm> PasswordCipherCreator;
        public SymmetricAlgorithm CreatePasswordCipher()
            => PasswordCipherCreator();

        private readonly Func<HashAlgorithm> PasswordHasherCreator;
        public HashAlgorithm CreatePasswordHasher()
            => PasswordHasherCreator();

        public override byte[] GenerateSecretKey(string Password)
        {
            using SymmetricAlgorithm Cipher = CreatePasswordCipher();
            using HashAlgorithm HashAlgorithm = CreatePasswordHasher();

            byte[] Hash = HashPassword(Password, PasswordSalt, HashAlgorithm, PasswordSpinCount),
                   Block3 = { 0x14, 0x6e, 0x0b, 0xe7, 0xab, 0xac, 0xd0, 0xd6 };
            Hash = HashAlgorithm.ComputeHash(Hash.Concat(Block3).ToArray());

            // Truncate or pad with 0x36
            int hashSize = Hash.Length;
            Array.Resize(ref Hash, PasswordKeyBits >> 3);
            for (int i = hashSize; i < PasswordKeyBits / 8; i++)
                Hash[i] = 0x36;

            // NOTE: the stored salt is padded to a multiple of the block size which affects AES-192
            byte[] DecryptedKeyValue = Decrypt(Cipher, PasswordEncryptedKeyValue, Hash, PasswordSalt);

            Array.Resize(ref DecryptedKeyValue, PasswordKeyBits >> 3);
            return DecryptedKeyValue;

        }

        public override byte[] GenerateBlockKey(int BlockNumber, byte[] SecretKey)
        {
            using HashAlgorithm Algorithm = HasherCreator();
            byte[] Datas = Algorithm.ComputeHash(SecretKey.Concat(BitConverter.GetBytes(BlockNumber)).ToArray());
            Array.Resize(ref Datas, BlockSize);
            return Datas;
        }

        public override Stream CreateEncryptedPackageStream(Stream Stream, byte[] SecretKey)
            => new EncryptedPackageStream(this, Stream, SecretKey, Salt);

        public override bool VerifyPassword(string Password)
        {
            using HashAlgorithm Algorithm = CreatePasswordHasher();

            byte[] Block1 = { 0xfe, 0xa7, 0xd2, 0x76, 0x3b, 0x4b, 0x9e, 0x79 },
                   Block2 = { 0xd7, 0xaa, 0x0f, 0x6d, 0x30, 0x61, 0x34, 0x4e },
                   SecretKey = HashPassword(Password, PasswordSalt, Algorithm, PasswordSpinCount),
                   InputBlockKey = Algorithm.ComputeHash(SecretKey.Concat(Block1).ToArray()),
                   ValueBlockKey = Algorithm.ComputeHash(SecretKey.Concat(Block2).ToArray());

            Array.Resize(ref InputBlockKey, PasswordKeyBits >> 3);
            Array.Resize(ref ValueBlockKey, PasswordKeyBits >> 3);

            using SymmetricAlgorithm Cipher = CreatePasswordCipher();

            byte[] DecryptedVerifier = Decrypt(Cipher, PasswordEncryptedVerifierHashInput, InputBlockKey, PasswordSalt),
                   DecryptedVerifierHash = Decrypt(Cipher, PasswordEncryptedVerifierHashValue, ValueBlockKey, PasswordSalt),
                   VerifierHash = Algorithm.ComputeHash(DecryptedVerifier);

            for (int i = 0; i < Math.Min(DecryptedVerifierHash.Length, VerifierHash.Length); ++i)
                if (DecryptedVerifierHash[i] != VerifierHash[i])
                    return false;

            return true;
        }

        private static byte[] HashPassword(string Password, byte[] Salt, HashAlgorithm Algorithm, int SpinCount)
        {
            byte[] Hash = Algorithm.ComputeHash(Salt.Concat(Encoding.Unicode.GetBytes(Password)).ToArray());
            for (int i = 0; i < SpinCount; i++)
                Hash = Algorithm.ComputeHash(BitConverter.GetBytes(i).Concat(Hash).ToArray());

            return Hash;
        }

        private CipherMode ParseCipherMode(string value)
        {
            if (value == "ChainingModeCBC")
                return CipherMode.CBC;
#if NET20 || NET45 || NETSTANDARD2_0
            else if (value == "ChainingModeCFB")
                return CipherMode.CFB;
#endif
            throw new ArgumentException("Invalid CipherMode " + value);
        }

        /// <summary>
        /// A seekable stream for reading an EncryptedPackage blob using OpenXml Agile Encryption. 
        /// </summary>
        private class EncryptedPackageStream : Stream
        {
            private const int SegmentLength = 4096;

            public override bool CanRead
                => true;

            public override bool CanSeek
                => true;

            public override bool CanWrite
                => false;

            public override long Length { get; }

            public override long Position
            {
                get => Offset - SegmentLength + SegmentOffset;
                set => Seek(value, SeekOrigin.Begin);
            }

            private Stream Stream { get; set; }

            private byte[] Key { get; }

            private byte[] IV { get; }

            private Encryption Encryption { get; }

            private int Offset { get; set; }

            public EncryptedPackageStream(Encryption Encryption, Stream Stream, byte[] Key, byte[] IV)
            {
                this.Stream = Stream;
                this.Key = Key;
                this.IV = IV;
                this.Encryption = Encryption;

                this.Stream.Read(SegmentBytes, 0, 8);
                Length = BitConverter.ToInt32(SegmentBytes, 0);

                ReadSegment();
            }

            public override void SetLength(long Value)
                => throw new NotImplementedException();

            private byte[] SegmentBytes { get; set; } = new byte[SegmentLength];
            private int SegmentOffset, SegmentIndex;
            public override long Seek(long Offset, SeekOrigin Origin)
            {
                switch (Origin)
                {
                    case SeekOrigin.Begin:
                        {
                            SegmentIndex = (int)(Offset / SegmentLength);
                            SegmentOffset = (int)(Offset % SegmentLength);

                            this.Offset = SegmentIndex * SegmentLength;
                            if (this.Offset < Length)
                                ReadSegment();

                            return Position;
                        }
                    case SeekOrigin.Current:
                        return Seek(Position + Offset, SeekOrigin.Begin);
                    case SeekOrigin.End:
                        return Seek(Length + Offset, SeekOrigin.Begin);
                    default:
                        return this.Offset;
                }
            }

            public override int Read(byte[] Buffer, int Offset, int Count)
            {
                if (Position >= Length)
                    throw new InvalidOperationException("Tried to read past the end of the encrypted stream");

                int index = 0;
                while (index < Count)
                {
                    if (SegmentOffset == SegmentBytes.Length)
                    {
                        ReadSegment();
                        SegmentOffset = 0;
                    }

                    int chunkSize = Math.Min(Count - index, SegmentBytes.Length - SegmentOffset);
                    Array.Copy(SegmentBytes, SegmentOffset, Buffer, Offset + index, chunkSize);
                    index += chunkSize;
                    SegmentOffset += chunkSize;
                }

                return index;
            }

            public override void Write(byte[] Buffer, int Offset, int Count)
                => throw new NotImplementedException();

            public override void Flush()
            {
            }

            private void ReadSegment()
            {
                byte[] Salt = Encryption.GenerateBlockKey(SegmentIndex, IV);

                // NOTE: +8 skips EncryptedPackage header
                Stream.Seek(8 + Offset, SeekOrigin.Begin);
                Stream.Read(SegmentBytes, 0, SegmentLength);

                using SymmetricAlgorithm Cipher = Encryption.CreateCipher();
                SegmentBytes = Decrypt(Cipher, SegmentBytes, Key, Salt);

                SegmentIndex++;
                Offset += SegmentLength;
            }

            protected override void Dispose(bool Disposing)
            {
                if (Disposing)
                {
                    Stream?.Dispose();
                    Stream = null;
                }

                base.Dispose(Disposing);
            }

        }

    }

}