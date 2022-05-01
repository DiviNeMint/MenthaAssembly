using System.Runtime.InteropServices;

namespace MenthaAssembly.Network.Primitives
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TcpKeepAlive
    {
        [FieldOffset(0)]
        private uint EnableData;

        /// <summary>
        /// 是否啓用Keep-Alive
        /// </summary>
        public bool Enable
        {
            get => EnableData == 1;
            set => EnableData = value ? 1U : 0U;
        }

        [FieldOffset(4)]
        private uint _Time;

        /// <summary>
        /// 多長時間後開始第一次探測（單位：毫秒）
        /// </summary>
        public uint Time
        {
            get => _Time;
            set => _Time = value;
        }

        [FieldOffset(8)]
        private uint _Interval;

        /// <summary>
        /// 探測時間間隔（單位：毫秒）
        /// </summary>
        public uint Interval
        {
            get => _Interval;
            set => _Interval = value;
        }

    }
}
