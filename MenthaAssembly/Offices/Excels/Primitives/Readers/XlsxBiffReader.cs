using System;
using System.IO;

namespace MenthaAssembly.Offices.Primitives
{
    public class XlsxBiffReader : BiffReader
    {
        public XlsxBiffReader(Stream Stream) : base(Stream)
        {
        }

        public override bool ReadVariable(out int ID)
        {
            if (!SkipVariable())
            {
                ID = -1;
                return false;
            }

            ID = 0;
            for (int i = 0; i < 4; i++)
            {
                if (Stream.Read(Buffer, 0, 1) == 0)
                {
                    ID = -1;
                    return false;
                }

                byte Data = Buffer[0];
                ID |= (Data & 0x7F) << (7 * i);

                if ((Data & 0x80) == 0)
                    break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (Stream.Read(Buffer, 0, 1) == 0)
                {
                    ID = -1;
                    return false;
                }

                byte Data = Buffer[0];
                VariableLength |= (Data & 0x7F) << (7 * i);

                if ((Data & 0x80) == 0)
                    break;
            }

            return true;
        }

        public object ReadRkNumber()
        {
            int Data = Read<int>();

            bool fx100 = (Data & 0b01) != 0,
                 fInt = (Data & 0b10) != 0;

            Data >>= 2;
            if (fInt)
                return fx100 ? Data / 100 : Data;

            double FloatValue = BitConverter.Int64BitsToDouble(((long)Data) << 34);
            return fx100 ? FloatValue / 100d : FloatValue;
        }

    }
}
