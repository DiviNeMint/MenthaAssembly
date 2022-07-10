using System.IO;

namespace MenthaAssembly.Offices.Primitives
{
    public class XlsBiffReader : BiffReader
    {
        public XlsBiffReader(Stream Stream, bool LeaveOpen) : base(Stream, LeaveOpen)
        {
        }

        public override bool ReadVariable(out int ID)
        {
            if (!SkipVariable())
            {
                ID = -1;
                return false;
            }

            ID = Stream.Read<ushort>();
            VariableLength = Stream.Read<ushort>();
            return true;
        }

    }
}
