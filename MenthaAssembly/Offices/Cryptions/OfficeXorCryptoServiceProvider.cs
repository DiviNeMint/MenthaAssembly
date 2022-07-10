﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;

//namespace MenthaAssembly.Offices.Cryptions
//{
//    internal class OfficeXorCryptoServiceProvider : SymmetricAlgorithm
//    {
//        private static readonly byte[] PadArray =
//        {
//            0xBB, 0xFF, 0xFF, 0xBA, 0xFF, 0xFF, 0xB9, 0x80,
//            0x00, 0xBE, 0x0F, 0x00, 0xBF, 0x0F, 0x00
//        };

//        private static readonly ushort[] initialCode =
//        {
//            0xE1F0, 0x1D0F, 0xCC9C, 0x84C0, 0x110C,
//            0x0E10, 0xF1CE, 0x313E, 0x1872, 0xE139,
//            0xD40F, 0x84F9, 0x280C, 0xA96A, 0x4EC3
//        };

//        private static readonly ushort[] xorMatrix =
//        {
//            0xAEFC, 0x4DD9, 0x9BB2, 0x2745, 0x4E8A, 0x9D14, 0x2A09,
//            0x7B61, 0xF6C2, 0xFDA5, 0xEB6B, 0xC6F7, 0x9DCF, 0x2BBF,
//            0x4563, 0x8AC6, 0x05AD, 0x0B5A, 0x16B4, 0x2D68, 0x5AD0,
//            0x0375, 0x06EA, 0x0DD4, 0x1BA8, 0x3750, 0x6EA0, 0xDD40,
//            0xD849, 0xA0B3, 0x5147, 0xA28E, 0x553D, 0xAA7A, 0x44D5,
//            0x6F45, 0xDE8A, 0xAD35, 0x4A4B, 0x9496, 0x390D, 0x721A,
//            0xEB23, 0xC667, 0x9CEF, 0x29FF, 0x53FE, 0xA7FC, 0x5FD9,
//            0x47D3, 0x8FA6, 0x0F6D, 0x1EDA, 0x3DB4, 0x7B68, 0xF6D0,
//            0xB861, 0x60E3, 0xC1C6, 0x93AD, 0x377B, 0x6EF6, 0xDDEC,
//            0x45A0, 0x8B40, 0x06A1, 0x0D42, 0x1A84, 0x3508, 0x6A10,
//            0xAA51, 0x4483, 0x8906, 0x022D, 0x045A, 0x08B4, 0x1168,
//            0x76B4, 0xED68, 0xCAF1, 0x85C3, 0x1BA7, 0x374E, 0x6E9C,
//            0x3730, 0x6E60, 0xDCC0, 0xA9A1, 0x4363, 0x86C6, 0x1DAD,
//            0x3331, 0x6662, 0xCCC4, 0x89A9, 0x0373, 0x06E6, 0x0DCC,
//            0x1021, 0x2042, 0x4084, 0x8108, 0x1231, 0x2462, 0x48C4
//        };

//        public OfficeXorCryptoServiceProvider(string Password)
//        {
//            IVValue = new byte[0];
//            GenerateKey(Password);
//            KeySizeValue = 64;
//        }



//        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

//        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV) => throw new NotImplementedException();

//        public override void GenerateIV() { }
//        public override void GenerateKey() { }
//        public void GenerateKey(string Password) 
//        {
//            if (Password.Length > 15)
//                Password = Password.Substring(0, 15);


//            byte[] KeyArray = Encoding.ASCII.GetBytes(Password),
//                   ObfuscationArray = new byte[16];

//            Array.Copy(KeyArray, 0, ObfuscationArray, 0, KeyArray.Length);
//            Array.Copy(PadArray, 0, ObfuscationArray, KeyArray.Length, PadArray.Length - KeyArray.Length + 1);

//            var xorKey = CreateXorKey_Method1(KeyArray);
//            byte[] baseKeyLE = new byte[] { (byte)(xorKey & 0xFF), (byte)((xorKey >> 8) & 0xFF) };
//            int nRotateSize = 2;
//            for (int i = 0; i < ObfuscationArray.Length; i++)
//            {
//                ObfuscationArray[i] ^= baseKeyLE[i & 1];
//                ObfuscationArray[i] = RotateLeft(ObfuscationArray[i], nRotateSize);
//            }

//            KeyValue = ObfuscationArray;
//        }


//        private ushort CreateXorKey_Method1(byte[] passwordBytes)
//        {
//            ushort xorKey = initialCode[passwordBytes.Length - 1];
//            var currentElement = 0x68;

//            for (var i = 0; i < passwordBytes.Length; ++i)
//            {
//                var c = passwordBytes[passwordBytes.Length - 1 - i];
//                for (var j = 0; j < 7; ++j)
//                {
//                    if ((c & 0x40) != 0)
//                    {
//                        xorKey ^= xorMatrix[currentElement];
//                    }

//                    c *= 2;
//                    currentElement--;
//                }
//            }

//            return xorKey;
//        }

//    }
//}
