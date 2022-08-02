using MenthaAssembly.Offices.Primitives;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace MenthaAssembly.Offices
{
    internal class RC4Encryption : Encryption
    {
        public byte[] Salt { get; }

        public byte[] Verifier { get; }

        public byte[] Hash { get; }

        public RC4Encryption(Stream Stream)
        {
            Salt = Stream.Read(16);
            Verifier = Stream.Read(16);
            Hash = Stream.Read(16);
        }

        public override SymmetricAlgorithm CreateCipher()
            => new RC4Managed();

        public override Stream CreateEncryptedPackageStream(Stream Stream, byte[] SecretKey)
            => throw new NotImplementedException();

        public override byte[] GenerateBlockKey(int BlockNumber, byte[] SecretKey)
        {
            int Length = SecretKey.Length;
            byte[] Datas = new byte[Length + 4];
            Array.Copy(SecretKey, Datas, Length);
            PointerHelper.Copy(BlockNumber, Datas, Length);

            using HashAlgorithm Hash = MD5.Create();
            return Hash.ComputeHash(Datas);
        }

        public override byte[] GenerateSecretKey(string Password)
        {
            if (Password.Length > 16)
                Password = Password.Substring(0, 16);

            using HashAlgorithm Hash = MD5.Create();
            byte[] Buffer = new byte[336];

            byte[] h = Hash.ComputeHash(System.Text.Encoding.Unicode.GetBytes(Password));
            int PasswordHashLength = Math.Min(h.Length, 5);
            for (int i = 0, Offset = 0; i < 16; i++, Offset += 21)
            {
                Array.Copy(h, 0, Buffer, Offset, PasswordHashLength);
                Array.Copy(Salt, 0, Buffer, Offset + 5, 16);
            }

            h = Hash.ComputeHash(Buffer);
            Array.Resize(ref h, 5);

            return h;
        }

        public override bool VerifyPassword(string Password)
        {
            byte[] SecretKey = GenerateSecretKey(Password);
            byte[] BlockKey = GenerateBlockKey(0, SecretKey);

            using ICryptoTransform Transform = new RC4Transform(BlockKey);
            byte[] DecryptedVerifier = Decrypt(Transform, Verifier),
                   DecryptedHash = Decrypt(Transform, Hash);

            using HashAlgorithm HashBuilder = MD5.Create();
            byte[] VerifierHash = HashBuilder.ComputeHash(DecryptedVerifier);

            for (int i = 0; i < 16; ++i)
                if (DecryptedHash[i] != VerifierHash[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Minimal RC4 decryption compatible with System.Security.Cryptography.SymmetricAlgorithm.
        /// </summary>
        internal class RC4Managed : SymmetricAlgorithm
        {
            public override ICryptoTransform CreateDecryptor(byte[] Key, byte[] IV)
                => new RC4Transform(Key);

            public override ICryptoTransform CreateEncryptor(byte[] Key, byte[] IV)
                => throw new NotImplementedException();

            public override void GenerateIV()
                => throw new NotImplementedException();

            public override void GenerateKey()
                => throw new NotImplementedException();

        }

        private class RC4Transform : ICryptoTransform
        {
            private readonly byte[] Table = new byte[256];

            public RC4Transform(byte[] Key)
            {
                this.Key = Key;

                for (int i = 0; i < Table.Length; i++)
                    Table[i] = (byte)i;

                int Length = Key.Length;
                for (int i = 0, j = 0; i < 256; i++)
                {
                    j = (j + Key[i % Length] + Table[i]) & 255;
                    MathHelper.Swap(ref Table[i], ref Table[j]);
                }
            }

            public int InputBlockSize => 1024;

            public int OutputBlockSize => 1024;

            public bool CanTransformMultipleBlocks => false;

            public bool CanReuseTransform => false;

            public byte[] Key { get; }

            public int TransformBlock(byte[] InputBuffer, int InputOffset, int InputLength, byte[] OutputBuffer, int OutputOffset)
            {
                for (int i = 0; i < InputLength; i++)
                    OutputBuffer[OutputOffset + i] = (byte)(InputBuffer[InputOffset + i] ^ Output());

                return InputLength;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                byte[] result = new byte[inputCount];
                TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
                return result;
            }

            private int Index1, Index2;
            private byte Output()
            {
                Index1 = (Index1 + 1) & 0xFF;
                Index2 = (Index2 + Table[Index1]) & 0xFF;

                MathHelper.Swap(ref Table[Index1], ref Table[Index2]);

                return Table[(Table[Index1] + Table[Index2]) & 0xFF];
            }

            public void Dispose()
            {
            }

        }

    }
}
