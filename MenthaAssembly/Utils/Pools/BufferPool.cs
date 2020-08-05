namespace MenthaAssembly.Utils
{
    public class BufferPool : Pool<byte[]>
    {
        private readonly int BufferSize;
        public BufferPool(int BufferSize)
        {
            this.BufferSize = BufferSize;
        }

        public virtual byte[] Dequeue()
            => base.TryDequeue(out byte[] Buffer) ? Buffer : new byte[BufferSize];

        public override bool TryDequeue(out byte[] Item)
        {
            if (!base.TryDequeue(out Item))
                Item = new byte[BufferSize];

            return true;
        }

    }
}
