using MenthaAssembly.Offices.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MenthaAssembly.Offices
{
    /// <summary>
    /// Represents the binary "Standard Encryption" header used in XLS and XLSX.
    /// XLS uses RC4+SHA1. XLSX uses AES+SHA1.
    /// </summary>
    internal class StandardEncryption : Encryption
    {
        #region Enums
        private enum StandardCipher
        {
            Default = 0x0000,
            AES128 = 0x660E,
            AES192 = 0x660F,
            AES256 = 0x6610,
            RC4 = 0x6801
        }

        private enum StandardHash
        {
            Default = 0x0000,
            SHA1 = 0x8004,
        }

        private enum EncryptionHeaderFlags : uint
        {
            CryptoAPI = 0x04,
            DocProps = 0x08,
            External = 0x10,
            AES = 0x20,
        }
        #endregion

        public int KeySize { get; }

        public byte[] Salt { get; }

        public byte[] Verifier { get; }

        public byte[] Hash { get; }

        public int VerifierHashBytesNeeded { get; }

        private EncryptionHeaderFlags Flags { get; }

        public StandardEncryption(Stream Stream)
        {
            Flags = Stream.Read<EncryptionHeaderFlags>();

            int HeaderSize = Stream.Read<int>();

            // Using ProviderType and KeySize instead
            Stream.Seek(8, SeekOrigin.Current);

            StandardCipher CipherAlgorithm = Stream.Read<StandardCipher>();

            //StandardHash HashAlgorithm = Stream.Read<StandardHash>();
            Stream.Seek(4, SeekOrigin.Current);
            HasherCreator = () => SHA1.Create();

            // ECMA-376: 0x00000080 (AES-128), 0x000000C0 (AES-192), or 0x00000100 (AES-256).
            // RC4: 0x00000028 – 0x00000080 (inclusive), 8-bits increments
            KeySize = Stream.Read<int>();

            //// Don't use this; is implementation-specific
            //StandardProvider providerType = Stream.Read<StandardProvider>();
            //Stream.Seek(8, SeekOrigin.Current);
            //CSPName = Stream.ReadString(HeaderSize - 32, Encoding.Unicode);

            // Skip providerType(4) & two reserved dwords(4 * 2) & CSPName(HeaderSize - 32)
            // 4 + 8 + HeaderSize - 32
            Stream.Seek(HeaderSize - 20, SeekOrigin.Current);

            int SaltSize = Stream.Read<int>();
            Salt = Stream.Read(SaltSize);
            Verifier = Stream.Read(16);

            // An unsigned integer that specifies the number of bytes needed to
            // contain the hash of the data used to generate the EncryptedVerifier field.
            VerifierHashBytesNeeded = Stream.Read<int>();

            // If the encryption algorithm is RC4, the length MUST be 20 bytes.
            // If the encryption algorithm is AES, the length MUST be 32 bytes
            int VerifierHashSize;
            switch (CipherAlgorithm)
            {
                case StandardCipher.AES128:
                case StandardCipher.AES192:
                case StandardCipher.AES256:
                case StandardCipher.Default when (Flags & EncryptionHeaderFlags.AES) != 0:
                    {
                        VerifierHashSize = 32;
                        CipherCreator = () => new RijndaelManaged
                        {
                            KeySize = KeySize,
                            BlockSize = 128,
                            Mode = CipherMode.ECB,
                            Padding = PaddingMode.Zeros
                        };
                        break;
                    }
                case StandardCipher.RC4:
                case StandardCipher.Default when (Flags & EncryptionHeaderFlags.CryptoAPI) != 0:
                    {
                        VerifierHashSize = 20;
                        CipherCreator = () => new RC4Encryption.RC4Managed();
                        break;
                    }
                default:
                    {
                        VerifierHashSize = ((Flags & EncryptionHeaderFlags.AES) != 0) ? 32 : 20;
                        CipherCreator = () => throw new InvalidOperationException($"Unsupported encryption method : {CipherAlgorithm}, {Flags}.");
                        break;
                    }
            }

            Hash = Stream.Read(VerifierHashSize);
        }

        private readonly Func<SymmetricAlgorithm> CipherCreator;
        public override SymmetricAlgorithm CreateCipher()
            => CipherCreator();

        private readonly Func<HashAlgorithm> HasherCreator;
        private HashAlgorithm CreateHasher()
            => HasherCreator();

        public override Stream CreateEncryptedPackageStream(Stream Stream, byte[] SecretKey)
            => new EncryptedPackageStream(this, Stream, SecretKey);

        public override byte[] GenerateBlockKey(int BlockNumber, byte[] SecretKey)
        {
            if ((Flags & EncryptionHeaderFlags.AES) != 0)
                throw new NotImplementedException("Block key for ECMA-376 Standard Encryption not implemented");

            else if ((Flags & EncryptionHeaderFlags.CryptoAPI) != 0)
            {
                using HashAlgorithm Hash = CreateHasher();
                byte[] Datas = Hash.ComputeHash(SecretKey.Concat(BitConverter.GetBytes(BlockNumber)).ToArray());

                Array.Resize(ref Datas, KeySize >> 3);

                // 2.3.5.2: If keyLength is exactly 40 bits, the encryption key MUST be composed of the first 40 bits of Hfinal and 88 bits set to zero, creating a 128-bit key.
                if (KeySize == 40)
                    Array.Resize(ref Datas, 16);

                return Datas;
            }

            throw new InvalidOperationException("Unknown encryption type");
        }

        public override byte[] GenerateSecretKey(string Password)
        {
            // 2.3.4.7 ECMA-376 Document Encryption Key Generation (Standard Encryption)
            if ((Flags & EncryptionHeaderFlags.AES) != 0)
            {
                using HashAlgorithm Hasher = CreateHasher();

                static byte[] DeriveKey(byte[] hashValue, HashAlgorithm HashAlgorithm, int KeySize, int VerifierHashSize)
                {
                    // And one more hash to derive the key
                    byte[] DerivedKey = new byte[64];

                    // This is step 4a in 2.3.4.7 of MS_OFFCRYPT version 1.0
                    // and is required even though the notes say it should be 
                    // used only when the encryption algorithm key > hash length.
                    for (int i = 0; i < DerivedKey.Length; i++)
                        DerivedKey[i] = (byte)(i < hashValue.Length ? 0x36 ^ hashValue[i] : 0x36);

                    byte[] x1 = HashAlgorithm.ComputeHash(DerivedKey);

                    if (VerifierHashSize > KeySize / 8)
                        return x1;

                    for (int i = 0; i < DerivedKey.Length; i++)
                        DerivedKey[i] = (byte)(i < hashValue.Length ? 0x5C ^ hashValue[i] : 0x5C);

                    return x1.Concat(HashAlgorithm.ComputeHash(DerivedKey)).ToArray();
                }

                byte[] Hash = Hasher.ComputeHash(Salt.Concat(Encoding.Unicode.GetBytes(Password)).ToArray());
                for (int i = 0; i < 50000; i++)
                    Hash = Hasher.ComputeHash(BitConverter.GetBytes(i).Concat(Hash).ToArray());

                Hash = Hasher.ComputeHash(Hash.Concat(BitConverter.GetBytes(0)).ToArray());

                // The algorithm in this 'DeriveKey' function is the bit that's not clear from the documentation
                Hash = DeriveKey(Hash, Hasher, KeySize, VerifierHashBytesNeeded);
                Array.Resize(ref Hash, KeySize >> 3);

                return Hash;
            }

            // 2.3.5.2 RC4 CryptoAPI Encryption Key Generation
            else if ((Flags & EncryptionHeaderFlags.CryptoAPI) != 0)
            {
                byte[] Datas = Salt.Concat(Encoding.Unicode.GetBytes(Password)).ToArray();
                using HashAlgorithm Hash = CreateHasher();
                return Hash.ComputeHash(Datas);
            }

            throw new InvalidOperationException("Unknown encryption type");
        }

        public override bool VerifyPassword(string Password)
        {
            byte[] SecretKey = GenerateSecretKey(Password),
                   BlockKey = ((Flags & EncryptionHeaderFlags.AES) != 0) ? SecretKey : GenerateBlockKey(0, SecretKey);

            using SymmetricAlgorithm Cipher = CreateCipher();
            using ICryptoTransform Transform = Cipher.CreateDecryptor(BlockKey, Salt);

            byte[] DecryptedVerifier = Decrypt(Transform, Verifier),
                   DecryptedVerifierHash = Decrypt(Transform, this.Hash);

            using HashAlgorithm Hash = CreateHasher();
            byte[] VerifierHash = Hash.ComputeHash(DecryptedVerifier);

            for (int i = 0; i < 16; ++i)
                if (DecryptedVerifierHash[i] != VerifierHash[i])
                    return false;

            return true;
        }

        private class EncryptedPackageStream : Stream
        {
            public EncryptedPackageStream(StandardEncryption Encryption, Stream UnderlyingStream, byte[] SecretKey)
            {
                Cipher = Encryption.CreateCipher();
                Decryptor = Cipher.CreateDecryptor(SecretKey, Encryption.Salt);
                DecryptedLength = UnderlyingStream.Read<int>();

                UnderlyingStream.Seek(4, SeekOrigin.Current);

                // Wrap CryptoStream to override the length and dispose the cipher and transform 
                // Zip readers scan backwards from the end for the central zip directory, and could fail if its too far away
                // CryptoStream is forward-only, so assume the zip readers read everything to memory
                BaseStream = new CryptoStream(UnderlyingStream, Decryptor, CryptoStreamMode.Read);
            }

            public override bool CanRead => BaseStream.CanRead;

            public override bool CanSeek => BaseStream.CanSeek;

            public override bool CanWrite => BaseStream.CanWrite;

            public override long Length => DecryptedLength;

            public override long Position
            {
                get => BaseStream.Position;
                set => BaseStream.Position = value;
            }

            private Stream BaseStream { get; set; }

            private SymmetricAlgorithm Cipher { get; set; }

            private ICryptoTransform Decryptor { get; set; }

            private long DecryptedLength { get; }

            public override void Flush() => BaseStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

            public override void SetLength(long value) => BaseStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Decryptor?.Dispose();
                    Decryptor = null;

                    ((IDisposable)Cipher)?.Dispose();
                    Cipher = null;

                    BaseStream?.Dispose();
                    BaseStream = null;
                }

                base.Dispose(disposing);
            }
        }

    }
}
