using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;

namespace MenthaAssembly.Media.Barcode
{
    internal class Code39Ex : Code39
    {
        public static readonly Dictionary<char, string> Table = new Dictionary<char, string>
        {
            { '\x00', "%U" },   // NUL
            { '\x01', "$A" },   // SOH
            { '\x02', "$B" },   // STX
            { '\x03', "$C" },   // ETX
            { '\x04', "$D" },   // EOT
            { '\x05', "$E" },   // ENQ
            { '\x06', "$F" },   // ACK
            { '\x07', "$G" },   // BEL
            { '\x08', "$H" },   // BS
            { '\x09', "$I" },   // HT
            { '\x0A', "$J" },   // LF
            { '\x0B', "$K" },   // VT
            { '\x0C', "$L" },   // FF
            { '\x0D', "$M" },   // CR
            { '\x0E', "$N" },   // SO
            { '\x0F', "$O" },   // SI
            { '\x10', "$P" },   // DLE
            { '\x11', "$Q" },   // DC1
            { '\x12', "$R" },   // DC2
            { '\x13', "$S" },   // DC3
            { '\x14', "$T" },   // DC4
            { '\x15', "$U" },   // NAK
            { '\x16', "$V" },   // SYN
            { '\x17', "$W" },   // ETB
            { '\x18', "$X" },   // CAN
            { '\x19', "$Y" },   // EM
            { '\x1A', "$Z" },   // SUB
            { '\x1B', "%A" },   // ESC
            { '\x1C', "%B" },   // FS
            { '\x1D', "%C" },   // GS
            { '\x1E', "%D" },   // RS
            { '\x1F', "%E" },   // US
            { '\x20', " "  },   // ' '
            { '\x21', "/A" },   // !
            { '\x22', "/B" },   // "
            { '\x23', "/C" },   // #
            { '\x24', "/D" },   // $
            { '\x25', "/E" },   // %
            { '\x26', "/F" },   // &
            { '\x27',"/G"  },   // \
            { '\x28', "/H" },   // (
            { '\x29', "/I" },   // )
            { '\x2A', "/J" },   // *
            { '\x2B', "/K" },   // +
            { '\x2C', "/L" },   // ,
            { '\x2D', "-"  },   // -
            { '\x2E', "."  },   // .
            { '\x2F', "/O" },   // /
            { '\x30', "0"  },   // 0
            { '\x31', "1"  },   // 1
            { '\x32', "2"  },   // 2
            { '\x33', "3"  },   // 3
            { '\x34', "4"  },   // 4
            { '\x35', "5"  },   // 5
            { '\x36', "6"  },   // 6
            { '\x37', "7"  },   // 7
            { '\x38', "8"  },   // 8
            { '\x39', "9"  },   // 9
            { '\x3A', "/Z" },   // :
            { '\x3B', "%F" },   // ;
            { '\x3C', "%G" },   // <
            { '\x3D', "%H" },   // =
            { '\x3E', "%I" },   // >
            { '\x3F', "%J" },   // ?
            { '\x40', "%V" },   // @ 
            { '\x41', "A"  },   // A 
            { '\x42', "B"  },   // B 
            { '\x43', "C"  },   // C 
            { '\x44', "D"  },   // D 
            { '\x45', "E"  },   // E 
            { '\x46', "F"  },   // F 
            { '\x47', "G"  },   // G 
            { '\x48', "H"  },   // H 
            { '\x49', "I"  },   // I 
            { '\x4A', "J"  },   // J 
            { '\x4B', "K"  },   // K 
            { '\x4C', "L"  },   // L 
            { '\x4D', "M"  },   // M 
            { '\x4E', "N"  },   // N 
            { '\x4F', "O"  },   // O 
            { '\x50', "P"  },   // P 
            { '\x51', "Q"  },   // Q 
            { '\x52', "R"  },   // R 
            { '\x53', "S"  },   // S 
            { '\x54', "T"  },   // T 
            { '\x55', "U"  },   // U 
            { '\x56', "V"  },   // V 
            { '\x57', "W"  },   // W 
            { '\x58', "X"  },   // X 
            { '\x59', "Y"  },   // Y 
            { '\x5A', "Z"  },   // Z 
            { '\x5B', "%K" },   // [ 
            { '\x5C', "%L" },   // \ 
            { '\x5D', "%M" },   // ] 
            { '\x5E', "%N" },   // ^ 
            { '\x5F', "%O" },   // _ 
            { '\x60', "%W" },   // ` 
            { '\x61', "+A" },   // a
            { '\x62', "+B" },   // b
            { '\x63', "+C" },   // c
            { '\x64', "+D" },   // d
            { '\x65', "+E" },   // e
            { '\x66', "+F" },   // f
            { '\x67', "+G" },   // g
            { '\x68', "+H" },   // h
            { '\x69', "+I" },   // i
            { '\x6A', "+J" },   // j
            { '\x6B', "+K" },   // k
            { '\x6C', "+L" },   // l
            { '\x6D', "+M" },   // m
            { '\x6E', "+N" },   // n
            { '\x6F', "+O" },   // o
            { '\x70', "+P" },   // p
            { '\x71', "+Q" },   // q
            { '\x72', "+R" },   // r
            { '\x73', "+S" },   // s
            { '\x74', "+T" },   // t
            { '\x75', "+U" },   // u
            { '\x76', "+V" },   // v
            { '\x77', "+W" },   // w
            { '\x78', "+X" },   // x
            { '\x79', "+Y" },   // y
            { '\x7A', "+Z" },   // z
            { '\x7B', "%P" },   // {
            { '\x7C', "%Q" },   // |
            { '\x7D', "%R" },   // }
            { '\x7E', "%S" },   // ~
            { '\x7F', "%T" },   // DEL
        };

        public Code39Ex(bool EnableChecksum) : base(EnableChecksum)
        {
        }

        public override bool TryCreateContour(string Context, int X, int Y, int NarrowBarWidth, int WideBarWidth, int Height, double Theta, out ImageContour Contour)
        {
            ImageContour Result = new ImageContour();
            double Sin = Math.Sin(Theta),
                   Cos = Math.Cos(Theta),
                   BarcodeHeight = Height - 1,
                   Tx0 = X,
                   Ty0 = Y,
                   Tx1 = Tx0 - BarcodeHeight * Sin,
                   Ty1 = Ty0 + BarcodeHeight * Cos,
                   DLBx = WideBarWidth * Cos,
                   DLBy = WideBarWidth * Sin,
                   DSBx = NarrowBarWidth * Cos,
                   DSBy = NarrowBarWidth * Sin,
                   Tx2, Ty2, Tx3, Ty3;

            void Render(bool[] Datas)
            {
                bool IsBlock = true;
                foreach (bool IsWide in Datas)
                {
                    if (IsWide)
                    {
                        Tx2 = Tx1 + DLBx;
                        Ty2 = Ty1 + DLBy;
                        Tx3 = Tx0 + DLBx;
                        Ty3 = Ty0 + DLBy;
                    }
                    else
                    {
                        Tx2 = Tx1 + DSBx;
                        Ty2 = Ty1 + DSBy;
                        Tx3 = Tx0 + DSBx;
                        Ty3 = Ty0 + DSBy;
                    }

                    if (IsBlock)
                    {
                        ImageContour Bar = ImageContour.CreateFillRectangle((int)Math.Round(Tx0), (int)Math.Round(Ty0),
                                                                            (int)Math.Round(Tx1), (int)Math.Round(Ty1),
                                                                            (int)Math.Round(Tx2 - Cos), (int)Math.Round(Ty2 - Sin),
                                                                            (int)Math.Round(Tx3 - Cos), (int)Math.Round(Ty3 - Sin));
                        Result.Union(Bar);
                    }

                    IsBlock = !IsBlock;
                    Tx0 = Tx3;
                    Ty0 = Ty3;
                    Tx1 = Tx2;
                    Ty1 = Ty2;
                }
            }
            void SkipNarrowBar()
            {
                Tx0 += DSBx;
                Ty0 += DSBy;
                Tx1 += DSBx;
                Ty1 += DSBy;
            }

            // Start Char '*'
            Render(Values[43]);
            SkipNarrowBar();

            // Context
            int Sum = 0;
            foreach (char item in Context)
            {
                if (!Table.TryGetValue(item, out string Data))
                {
                    Contour = null;
                    return false;
                }

                foreach (char c in Data)
                {
                    int Index = Keys.IndexOf(c);
                    if (Index < 0)
                    {
                        Contour = null;
                        return false;
                    }

                    Sum += Index;
                    Render(Values[Index]);
                    SkipNarrowBar();
                }
            }

            if (EnableChecksum)
            {
                Render(Values[Sum % 43]);
                SkipNarrowBar();
            }

            // End Char '*'
            Render(Values[43]);

            Contour = Result;
            return true;
        }

        public override double GetBarcodeWidth(string Context, double NarrowBarWidth, double WideBarWidth)
        {
            int Length = 2; // Start & End Char
            foreach (char c in Context)
                if (Table.TryGetValue(c, out string Data))
                    Length += Data.Length;

            return (Length * 7 - 1) * NarrowBarWidth + Length * 3 * WideBarWidth;
        }
    }
}
