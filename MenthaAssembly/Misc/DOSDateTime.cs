using System;

namespace MenthaAssembly
{
    public struct DOSDateTime
    {
        private ushort TimeData;
        private ushort DateData;

        public int Year
        {
            get => ((DateData >> 9) & 0b1111111) + 1980;
            set => DateData = (ushort)((((value - 1980) & 0b1111111) << 9) | (TimeData & 0b0000000111111111));
        }

        public int Month
        {
            get => (DateData >> 5) & 0b1111;
            set => DateData = (ushort)(((value & 0b1111) << 5) | (TimeData & 0b1111111000011111));
        }

        public int Day
        {
            get => DateData & 0b11111;
            set => DateData = (ushort)((value & 0b11111) | (DateData & 0b1111111111100000));
        }

        public int Hour
        {
            get => TimeData >> 11;
            set => TimeData = (ushort)(((value & 0b11111) << 11) | (TimeData & 0b0000011111111111));
        }

        public int Minute
        {
            get => (TimeData >> 5) & 0b111111;
            set => TimeData = (ushort)(((value & 0b111111) << 5) | (TimeData & 0b1111100000011111));
        }

        public int Second
        {
            get => (TimeData & 0b11111) << 1;
            set => TimeData = (ushort)(((value >> 1) & 0b11111) | (TimeData & 0b1111111111100000));
        }

        public static implicit operator DateTime(DOSDateTime This)
            => new DateTime(This.Year, This.Month, This.Day, This.Hour, This.Minute, This.Second);

    }
}