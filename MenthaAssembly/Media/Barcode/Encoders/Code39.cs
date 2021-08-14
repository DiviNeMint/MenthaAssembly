using MenthaAssembly.Media.Imaging;
using System;

namespace MenthaAssembly.Media.Barcode
{
    internal class Code39 : IBarcode
    {
        public const string Keys = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
        public static readonly bool[][] Values =
        {
            new []{ false, false, false, true, true, false, true, false, false },   // 0
            new []{ true, false, false, true, false, false, false, false, true },   // 1
            new []{ false, false, true, true, false, false, false, false, true },   // 2
            new []{ true, false, true, true, false, false, false, false, false },   // 3
            new []{ false, false, false, true, true, false, false, false, true },   // 4
            new []{ true, false, false, true, true, false, false, false, false },   // 5
            new []{ false, false, true, true, true, false, false, false, false },   // 6
            new []{ false, false, false, true, false, false, true, false, true },   // 7
            new []{ true, false, false, true, false, false, true, false, false },   // 8
            new []{ false, false, true, true, false, false, true, false, false },   // 9
            new []{ true, false, false, false, false, true, false, false, true },   // A
            new []{ false, false, true, false, false, true, false, false, true },   // B
            new []{ true, false, true, false, false, true, false, false, false },   // C
            new []{ false, false, false, false, true, true, false, false, true },   // D
            new []{ true, false, false, false, true, true, false, false, false },   // E
            new []{ false, false, true, false, true, true, false, false, false },   // F
            new []{ false, false, false, false, false, true, true, false, true },   // G
            new []{ true, false, false, false, false, true, true, false, false },   // H
            new []{ false, false, true, false, false, true, true, false, false },   // I
            new []{ false, false, false, false, true, true, true, false, false },   // J
            new []{ true, false, false, false, false, false, false, true, true },   // K
            new []{ false, false, true, false, false, false, false, true, true },   // L
            new []{ true, false, true, false, false, false, false, true, false },   // M
            new []{ false, false, false, false, true, false, false, true, true },   // N
            new []{ true, false, false, false, true, false, false, true, false },   // O
            new []{ false, false, true, false, true, false, false, true, false },   // P
            new []{ false, false, false, false, false, false, true, true, true },   // Q
            new []{ true, false, false, false, false, false, true, true, false },   // R
            new []{ false, false, true, false, false, false, true, true, false },   // S
            new []{ false, false, false, false, true, false, true, true, false },   // T
            new []{ true, true, false, false, false, false, false, false, true },   // U
            new []{ false, true, true, false, false, false, false, false, true },   // V
            new []{ true, true, true, false, false, false, false, false, false },   // W
            new []{ false, true, false, false, true, false, false, false, true },   // X
            new []{ true, true, false, false, true, false, false, false, false },   // Y
            new []{ false, true, true, false, true, false, false, false, false },   // Z
            new []{ false, true, false, false, false, false, true, false, true },   // -
            new []{ true, true, false, false, false, false, true, false, false },   // .
            new []{ false, true, true, false, false, false, true, false, false },   //  
            new []{ false, true, false, true, false, true, false, false, false },   // $
            new []{ false, true, false, true, false, false, false, true, false },   // /
            new []{ false, true, false, false, false, true, false, true, false },   // +
            new []{ false, false, false, true, false, true, false, true, false },   // %
            new []{ false, true, false, false, true, false, true, false, false },   // *
        };

        public bool EnableChecksum { get; }

        public Code39(bool EnableChecksum)
        {
            this.EnableChecksum = EnableChecksum;
        }

        public virtual bool TryCreateContour(string Context, int X, int Y, int NarrowBarWidth, int WideBarWidth, int Height, double Theta, out ImageContour Contour)
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
            foreach (char c in Context)
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

        public virtual double GetBarcodeWidth(string Context, double NarrowBarWidth, double WideBarWidth)
        {
            int Length = Context.Length + 2; // Start & End Char
            return (Length * 7 - 1) * NarrowBarWidth + Length * 3 * WideBarWidth;
        }
    }
}
