using System;
using System.IO;
using System.Security.Cryptography;

namespace MenthaAssembly.Offices.Primitives
{
    /// <summary>
    /// Base class for the various encryption schemes used by Excel
    /// </summary>
    internal abstract class Encryption
    {
        public abstract byte[] GenerateSecretKey(string Password);

        public abstract byte[] GenerateBlockKey(int BlockNumber, byte[] SecretKey);

        public abstract Stream CreateEncryptedPackageStream(Stream Stream, byte[] SecretKey);

        public abstract bool VerifyPassword(string password);

        public abstract SymmetricAlgorithm CreateCipher();

        public static byte[] Decrypt(SymmetricAlgorithm Cipher, byte[] Datas, byte[] Key, byte[] IV)
        {
            using ICryptoTransform Transform = Cipher.CreateDecryptor(Key, IV);
            return Decrypt(Transform, Datas);
        }
        public static byte[] Decrypt(ICryptoTransform Transform, byte[] Datas)
        {
            using MemoryStream Memory = new MemoryStream(Datas);
            using CryptoStream Decrypt = new CryptoStream(Memory, Transform, CryptoStreamMode.Read);
            return Decrypt.Read(Datas.Length);
        }

        public static Encryption Create(Stream Stream)
        {
            // TODO Does this work on a big endian system?
            ushort Major = Stream.Read<ushort>(),
                   Minor = Stream.Read<ushort>();

            if (Major == 1 && Minor == 1)
                return new RC4Encryption(Stream);

            // 2.3.4.5 \EncryptionInfo Stream (Standard Encryption)
            else if ((Major == 2 || Major == 3 || Major == 4) && Minor == 2)
                return new StandardEncryption(Stream);

            //// 2.3.4.6 \EncryptionInfo Stream (Extensible Encryption)
            //else if ((Major == 3 || Major == 4) && Minor == 3)
            //    throw new NotSupportedException("Not supported Extensible Encryption.");

            // 2.3.4.10 \EncryptionInfo Stream (Agile Encryption)
            else if (Major == 4 && Minor == 4)
                return new AgileEncryption(Stream);

            throw new NotSupportedException($"Not supported EncryptionInfo version {Major}.{Minor}");
        }

    }
}
